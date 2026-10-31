using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.MultiUploader.Editor
{
    public sealed class QuestConverter
    {
        public const string CacheRoot = "Assets/FynnMultiUploaderGenerated";
        readonly MultiUploaderSettings settings;
        readonly Action<Object> save;
        readonly Func<Material, Material> findOriginal;
        readonly QuestConversionIgnore ignored;
        readonly Dictionary<Object, Object> copies = new Dictionary<Object, Object>();
        readonly Dictionary<Transform, Dictionary<Object, Object>> animationCopies = new Dictionary<Transform, Dictionary<Object, Object>>();
        Transform animationRoot;
        readonly List<string> messages = new List<string>();
        readonly HashSet<string> seenMessages = new HashSet<string>();
        int materialCount, textureCount, clipCount;
        bool hasBakedDecals;
        public bool HasErrors { get; private set; }
        public string Report => $"MultiUploader: {materialCount} materials, {textureCount} textures, {clipCount} animation clips.\n"
            + (messages.Count == 0 ? "No conversion warnings." : string.Join("\n", messages.OrderBy(m => m.StartsWith("ERROR:", StringComparison.Ordinal) ? 0 : 1)));
        public string ErrorSummary => "Quest conversion blocked:\n" + string.Join("\n", messages.Where(m => m.StartsWith("ERROR:", StringComparison.Ordinal)));
        int Size => Mathf.ClosestPowerOfTwo(Mathf.Clamp(settings.maxTextureSize, 256, 2048));
        static readonly HashSet<string> MobileShaders = new HashSet<string>
        {
            "VRChat/Mobile/Toon Standard"
        };

        public QuestConverter(MultiUploaderSettings settings, Action<Object> save, Func<Material, Material> findOriginal = null)
        {
            this.settings = settings;
            this.save = save;
            this.findOriginal = findOriginal ?? (m => m);
            ignored = new QuestConversionIgnore(settings.ignoredObjects);
        }

        void Issue(string message, bool unsafeResult = false)
        {
            bool error = unsafeResult && settings.blockUnsafeConversions;
            HasErrors |= error;
            string line = (error ? "ERROR: " : "WARNING: ") + message;
            if (seenMessages.Add(line)) messages.Add(line);
        }

        public void Convert(GameObject buildCopy)
        {
            foreach (var r in buildCopy.GetComponentsInChildren<Renderer>(true).Where(r => !ignored.Contains(r.transform)))
                r.sharedMaterials = r.sharedMaterials.Select(m => (Material)Remap(m)).ToArray();

            foreach (var component in buildCopy.GetComponentsInChildren<Component>(true))
            {
                if (component == null || component is MultiUploaderSettings || component is Renderer || ignored.Contains(component.transform)) continue;
                animationRoot = component is Animator || component is Animation ? component.transform : buildCopy.transform;
                RewriteReferences(component);
            }
            foreach (var r in buildCopy.GetComponentsInChildren<Renderer>(true).Where(r => !ignored.Contains(r.transform)))
                foreach (var m in r.sharedMaterials)
                    if (m != null && (m.shader == null || !MobileShaders.Contains(m.shader.name)))
                        Issue($"{r.name}: material {m.name} still has an unsupported shader.", true);
            if (hasBakedDecals)
                foreach (var context in animationCopies)
                foreach (var clip in context.Value.Keys.OfType<AnimationClip>())
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                        if (!ignored.ContainsBinding(context.Key, binding.path) &&
                            (binding.propertyName.StartsWith("material._Color", StringComparison.Ordinal)
                            || binding.propertyName.StartsWith("material._Decal", StringComparison.Ordinal)))
                            Issue($"{clip.name}: {binding.propertyName} animates color/decal settings on an avatar with baked decals; use a manually prepared Toon Standard override for affected materials.", true);
        }

        static bool IsAnimationAsset(Object value) => value is Motion || value is RuntimeAnimatorController
            || value is AnimatorState || value is AnimatorStateMachine || value is AnimatorTransitionBase
            || value is StateMachineBehaviour;

        void RewriteReferences(Object target)
        {
            var serialized = new SerializedObject(target);
            var p = serialized.GetIterator();
            while (p.Next(true))
            {
                if (p.propertyType != SerializedPropertyType.ObjectReference || p.propertyPath == "m_Script") continue;
                var value = p.objectReferenceValue;
                if (value == null || (!(value is Material) && !IsAnimationAsset(value))) continue;
                var replacement = Remap(value);
                if (replacement != value) p.objectReferenceValue = replacement;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        Object Remap(Object source)
        {
            if (source == null) return null;
            var cache = copies;
            if (IsAnimationAsset(source))
            {
                if (!animationCopies.TryGetValue(animationRoot, out cache))
                    animationCopies[animationRoot] = cache = new Dictionary<Object, Object>();
            }
            if (cache.TryGetValue(source, out var existing)) return existing;
            if (source is Texture2D texture) return CopyTexture(texture);
            if (source is Material material) return ConvertMaterial(material);
            if (!IsAnimationAsset(source)) return source;
            // Register empty graph copies before remapping cyclic references.
            Object copy = source switch
            {
                AnimatorController _ => new AnimatorController(),
                AnimatorOverrideController _ => new AnimatorOverrideController(),
                AnimatorStateMachine _ => new AnimatorStateMachine(),
                AnimatorState _ => new AnimatorState(),
                AnimatorStateTransition _ => new AnimatorStateTransition(),
                AnimatorTransition _ => new AnimatorTransition(),
                BlendTree _ => new BlendTree(),
                AnimationClip _ => new AnimationClip(),
                ScriptableObject _ => ScriptableObject.CreateInstance(source.GetType()),
                _ => throw new InvalidOperationException("Unsupported animation asset type: " + source.GetType().FullName)
            };
            EditorUtility.CopySerialized(source, copy);
            copy.name = source.name + " [Quest]";
            copy.hideFlags = HideFlags.None;
            cache.Add(source, copy);
            cache[copy] = copy;
            save(copy);
            if (copy is AnimationClip clip)
            {
                clipCount++;
                foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                {
                    if (ignored.ContainsBinding(animationRoot, binding.path)) continue;
                    var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    for (int i = 0; i < keys.Length; i++) keys[i].value = Remap(keys[i].value);
                    AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
                }
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    if (!ignored.ContainsBinding(animationRoot, binding.path) && binding.propertyName.StartsWith("material.", StringComparison.Ordinal)
                        && !IsSupportedAnimationProperty(binding.propertyName))
                        Issue($"{source.name}: shader animation {binding.propertyName} has no verified Toon Standard mapping; assign a Quest controller if this effect is needed.");
            }
            // Clip curves were handled by binding path; a generic walk would convert ignored keys.
            if (!(copy is AnimationClip)) RewriteReferences(copy);
            return copy;
        }

        Material ConvertMaterial(Material source)
        {
            var entry = (settings.overrides ?? Array.Empty<MaterialOverride>())
                .FirstOrDefault(x => x != null && (x.pcMaterial == source || x.pcMaterial == findOriginal(source)));
            Material chosen = entry != null ? entry.questMaterial : source;
            if (chosen == null) { Issue($"{source.name}: Quest override is empty.", true); chosen = source; }
            bool mobile = chosen.shader != null && MobileShaders.Contains(chosen.shader.name);
            if (entry != null && !mobile) Issue($"{source.name}: override must use VRChat/Mobile/Toon Standard.", true);

            var working = new Material(chosen) { name = source.name + " [Quest]", hideFlags = HideFlags.None };
            copies.Add(source, working);
            copies[working] = working;
            materialCount++;
            if (mobile)
            {
                foreach (var property in working.GetTexturePropertyNames())
                {
                    var texture = working.GetTexture(property);
                    if (texture is Texture2D t) working.SetTexture(property, CopyTexture(t));
                    else if (texture != null) Issue($"{source.name}: non-2D texture {property} was kept at its original size.");
                }
                save(working);
                return working;
            }

            try
            {
                // Poiyomi unlock can delete shaders shared by PC materials.
                var originalGuid = working.GetTag("OriginalShaderGUID", false, "");
                var originalName = working.GetTag("OriginalShader", false, "");
                var original = string.IsNullOrEmpty(originalGuid) ? null
                    : AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(originalGuid));
                if (original == null && !string.IsNullOrEmpty(originalName)) original = Shader.Find(originalName);
                if (original != null) working.shader = original;
                string shaderName = working.shader != null ? working.shader.name : "missing shader";
                bool poiyomi = shaderName.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) >= 0;
                bool lilToon = shaderName.IndexOf("lilToon", StringComparison.OrdinalIgnoreCase) >= 0;
                if (!poiyomi && !lilToon && shaderName != "Standard" && !shaderName.StartsWith("Unlit/", StringComparison.Ordinal))
                    if (!shaderName.StartsWith("VRChat/Mobile/", StringComparison.Ordinal))
                        Issue($"{source.name}: no automatic adapter for {shaderName}; assign a Quest override.", true);
                if (lilToon) Issue($"{source.name}: lilToon base texture and tint are mapped to Toon Standard; outlines, extra layers and lilToon-only effects are omitted.");
                if (working.renderQueue >= 2450 || working.GetTag("RenderType", false, "Opaque") != "Opaque")
                {
                    if (settings.approximateTransparencyAsOpaque)
                        Issue($"{source.name}: transparency/cutout is approximated as opaque for Quest; holes and see-through regions may be filled. Review the preview.");
                    else Issue($"{source.name}: transparency/cutout needs a Quest override, or enable Approximate transparency as opaque in Settings.", true);
                }
                if (F(working, "_MainTexUV") != 0 || F(working, "_MainTexStochastic") != 0
                    || V(working, "_MainTexPan") != Vector4.zero)
                    Issue($"{source.name}: alternate UVs, stochastic sampling or texture panning need a Quest override.", true);
                if (poiyomi)
                {
                    foreach (string feature in new[] { "_EnableAudioLink", "_EnableEmission1", "_EnableEmission2", "_EnableEmission3", "_EnableRimLighting", "_EnableGlitter", "_EnableMatcap", "_EnableDissolve", "_EnableOutline", "_EnableColorAdjust" })
                        if (F(working, feature) != 0) Issue($"{source.name}: {feature} is omitted by performance-first Toon Standard conversion.");
                    Issue($"{source.name}: Poiyomi lighting/reflections and shader-only effects are approximated by Toon Standard. Basic normal maps and emission are mapped when compatible.");
                }
                var texture = working.HasProperty("_MainTex") ? working.GetTexture("_MainTex") : null;
                Color tint = working.HasProperty("_Color") ? working.GetColor("_Color") : Color.white;
                Texture2D decalBake = null;
                try
                {
                    if (poiyomi) decalBake = BakeDecals(working, source.name);
                    if (decalBake != null) { texture = decalBake; tint = KeptTint(working); }
                    // Decal composites already contain the base tint.
                    var baked = decalBake != null ? BakeAlbedo(texture, Color.white)
                        : texture is Texture2D t ? CopyTexture(t) : null;
                    var scale = working.HasProperty("_MainTex") ? working.GetTextureScale("_MainTex") : Vector2.one;
                    var offset = working.HasProperty("_MainTex") ? working.GetTextureOffset("_MainTex") : Vector2.zero;
                    var shader = Shader.Find("VRChat/Mobile/Toon Standard");
                    if (shader == null) throw new InvalidOperationException("VRChat/Mobile/Toon Standard is missing. Install the VRChat Avatar SDK.");
                    var converted = new Material(shader) { name = working.name };
                    converted.SetTexture("_MainTex", baked);
                    converted.SetColor("_Color", tint);
                    converted.SetTextureScale("_MainTex", scale);
                    converted.SetTextureOffset("_MainTex", offset);
                    MapBasicFeatures(working, converted, poiyomi, source.name);
                    Object.DestroyImmediate(working);
                    working = converted;
                    copies[source] = working;
                    copies[working] = working;
                }
                finally { if (decalBake != null) Object.DestroyImmediate(decalBake); }
                save(working);
                return working;
            }
            catch { Object.DestroyImmediate(working); throw; }
        }

        static float F(Material m, string name) => m.HasProperty(name) ? m.GetFloat(name) : 0;
        static Vector4 V(Material m, string name) => m.HasProperty(name) ? m.GetVector(name) : Vector4.zero;
        static Type FindType(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

        static bool IsSupportedAnimationProperty(string name)
        {
            string property = name.Substring("material.".Length);
            return property == "_Color" || property.StartsWith("_Color.", StringComparison.Ordinal)
                || property.StartsWith("_MainTex_ST", StringComparison.Ordinal)
                || property == "_BumpScale" || property.StartsWith("_EmissionColor.", StringComparison.Ordinal)
                || property == "_EmissionStrength";
        }

        void MapBasicFeatures(Material source, Material target, bool poiyomi, string label)
        {
            target.SetFloat("_Culling", source.HasProperty("_Cull") ? source.GetFloat("_Cull") : 2);
            var ramp = Resources.Load<Texture2D>("VRChat/ShadowRampToon4Band");
            if (ramp != null) target.SetTexture("_Ramp", ramp);
            if (source.HasProperty("_BumpMap") && source.GetTexture("_BumpMap") is Texture2D normal)
            {
                if (F(source, "_BumpMapUV") == 0 && V(source, "_BumpMapPan") == Vector4.zero)
                {
                    target.SetTexture("_BumpMap", CopyTexture(normal));
                    target.SetTextureScale("_BumpMap", source.GetTextureScale("_BumpMap"));
                    target.SetTextureOffset("_BumpMap", source.GetTextureOffset("_BumpMap"));
                    target.SetFloat("_BumpScale", source.HasProperty("_BumpScale") ? F(source, "_BumpScale") : 1);
                    target.EnableKeyword("USE_NORMAL_MAPS");
                }
                else Issue($"{label}: normal map with alternate UVs or panning was omitted.");
            }
            bool emission = poiyomi ? F(source, "_EnableEmission") != 0 : source.IsKeywordEnabled("_EMISSION");
            if (!emission) return;
            if (F(source, "_EmissionMapUV") != 0 || V(source, "_EmissionMapPan") != Vector4.zero)
            {
                Issue($"{label}: emission with alternate UVs or panning was omitted.");
                return;
            }
            if (source.HasProperty("_EmissionColor")) target.SetColor("_EmissionColor", source.GetColor("_EmissionColor"));
            target.SetFloat("_EmissionStrength", poiyomi ? Mathf.Clamp(F(source, "_EmissionStrength"), 0, 2) : 1);
            if (source.HasProperty("_EmissionMap") && source.GetTexture("_EmissionMap") is Texture2D map)
            {
                target.SetTexture("_EmissionMap", CopyTexture(map));
                target.SetTextureScale("_EmissionMap", source.GetTextureScale("_EmissionMap"));
                target.SetTextureOffset("_EmissionMap", source.GetTextureOffset("_EmissionMap"));
            }
            if (poiyomi) Issue($"{label}: emission layer 0 is approximated; masks, blinking, themes and AudioLink modulation are not mapped.");
        }

        Texture2D BakeDecals(Material material, string label)
        {
            var slots = Enumerable.Range(0, 4).Select(i => "_DecalTexture" + (i == 0 ? "" : i.ToString()))
                .Where(p => material.HasProperty(p) && F(material, p.Replace("Texture", "Enabled")) != 0).ToList();
            if (slots.Count == 0) return null;
            if (!settings.bakeStaticPoiyomiDecals) { Issue($"{label}: active decals will be omitted because decal baking is disabled.", true); return null; }
            var type = FindType("Poi.Tools.PoiDecalBaker");
            var slotType = type?.GetNestedType("Slot", BindingFlags.Public);
            var render = type?.GetMethod("Render", new[] { typeof(Material), typeof(IList<string>), typeof(int), typeof(int) });
            var mainReasons = type?.GetMethod("MainBlockingReasons", BindingFlags.Static | BindingFlags.NonPublic);
            var slotReasons = type?.GetMethod("SlotBlockingReasons", BindingFlags.Static | BindingFlags.NonPublic);
            var unsupported = type?.GetMethod("UnbakedSettings", BindingFlags.Static | BindingFlags.NonPublic);
            if (render == null || slotType == null || mainReasons == null || slotReasons == null || unsupported == null)
            {
                Issue($"{label}: this Poiyomi version has no compatible safe decal baker; assign a pre-baked Quest override.", true);
                return null;
            }
            var reasons = ((IEnumerable<string>)mainReasons.Invoke(null, new object[] { material })).ToList();
            foreach (var slot in slots)
            {
                object s = Activator.CreateInstance(slotType, new object[] { slot });
                reasons.AddRange(((IEnumerable<string>)slotReasons.Invoke(null, new[] { (object)material, s })).Select(x => slot + ": " + x));
                reasons.AddRange(((IEnumerable<string>)unsupported.Invoke(null, new[] { (object)material, s })).Select(x => slot + ": " + x));
            }
            if (reasons.Count > 0)
            {
                Issue($"{label}: decal bake needs a manual override ({string.Join(", ", reasons)}). Active decals are omitted.", true);
                return null;
            }
            var main = material.GetTexture("_MainTex");
            Dimensions(main, out int width, out int height);
            var result = (Texture2D)render.Invoke(null, new object[] { material, slots, width, height });
            hasBakedDecals = true;
            messages.Add($"BAKED: {label}: {slots.Count} static decals into {width}×{height} albedo.");
            return result;
        }

        static Color KeptTint(Material m)
        {
            // The decal baker output already contains the other tint channels.
            Color c = m.GetColor("_Color"), kept = Color.white;
            for (int i = 0; i < 4; i++)
                if (c[i] > 1 || (i < 3 && F(m, "_ColorThemeIndex") != 0)
                    || F(m, "_TwoPassOverrideFirstPassColor") == 1
                    || (i == 3 && F(m, "_TwoPassOverrideFirstPassColor") == 2)) kept[i] = c[i];
            return kept;
        }

        void Dimensions(Texture texture, out int width, out int height)
        {
            int w = texture != null ? texture.width : Size, h = texture != null ? texture.height : Size;
            float ratio = Mathf.Min(1, (float)Size / Mathf.Max(w, h));
            width = Mathf.Max(4, Mathf.RoundToInt(w * ratio));
            height = Mathf.Max(4, Mathf.RoundToInt(h * ratio));
        }

        Texture2D BakeAlbedo(Texture source, Color tint)
        {
            var shader = Shader.Find("Hidden/FynnsTools/MultiUploader/Albedo");
            if (shader == null || !shader.isSupported) throw new InvalidOperationException("MultiUploader albedo bake shader is unavailable.");
            Dimensions(source, out int width, out int height);
            var previous = RenderTexture.active;
            bool previousSRGB = GL.sRGBWrite;
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var material = new Material(shader);
            Texture2D result = null;
            try
            {
                material.SetColor("_Color", tint);
                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                Graphics.Blit(source != null ? source : Texture2D.whiteTexture, rt, material);
                RenderTexture.active = rt;
                result = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply();
                byte[] png = result.EncodeToPNG();
                string sampler = source != null
                    ? $"{source.wrapModeU}:{source.wrapModeV}:{source.filterMode}:{source.anisoLevel}"
                    : "Repeat:Repeat:Bilinear:1";
                string path = CacheRoot + "/albedo_" + Hash(png) + "_" + Hash(Encoding.UTF8.GetBytes(sampler)).Substring(0, 12) + "_" + Size + ".png";
                EnsureCache();
                if (!File.Exists(path))
                {
                    File.WriteAllBytes(path, png);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    importer.sRGBTexture = true;
                    importer.wrapModeU = source != null ? source.wrapModeU : TextureWrapMode.Repeat;
                    importer.wrapModeV = source != null ? source.wrapModeV : TextureWrapMode.Repeat;
                    importer.filterMode = source != null ? source.filterMode : FilterMode.Bilinear;
                    importer.anisoLevel = source != null ? source.anisoLevel : 1;
                    ConfigureTexture(importer);
                }
                textureCount++;
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            finally
            {
                RenderTexture.active = previous;
                GL.sRGBWrite = previousSRGB;
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(material);
                if (result != null) Object.DestroyImmediate(result);
            }
        }

        Texture2D CopyTexture(Texture2D source)
        {
            if (copies.TryGetValue(source, out var cached)) return (Texture2D)cached;
            string sourcePath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(sourcePath) || !(AssetImporter.GetAtPath(sourcePath) is TextureImporter))
            {
                Issue($"{source.name}: generated/subasset texture retained; no safe importer to resize independently.");
                return source;
            }
            EnsureCache();
            string key = Hash(Encoding.UTF8.GetBytes(sourcePath + AssetDatabase.GetAssetDependencyHash(sourcePath) + Size));
            string path = CacheRoot + "/texture_" + key + Path.GetExtension(sourcePath);
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, path)) throw new IOException("Could not copy texture " + sourcePath);
                ConfigureTexture((TextureImporter)AssetImporter.GetAtPath(path));
            }
            var result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            copies[source] = result;
            copies[result] = result;
            textureCount++;
            return result;
        }

        void ConfigureTexture(TextureImporter importer)
        {
            importer.isReadable = false;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = true;
            importer.maxTextureSize = Size;
            var android = importer.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = Size;
            android.format = TextureImporterFormat.ASTC_6x6;
            android.crunchedCompression = false;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
        }

        static void EnsureCache()
        {
            if (!AssetDatabase.IsValidFolder(CacheRoot)) AssetDatabase.CreateFolder("Assets", "FynnMultiUploaderGenerated");
        }

        static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }
}
