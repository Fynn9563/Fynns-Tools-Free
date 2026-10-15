using UnityEngine;

namespace MatCapMaker
{
    public class MatCapMakerPreset : ScriptableObject
    {
        #region Base Settings
        public Color baseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Texture2D baseTexture;
        public float textureStrength = 1f;
        public int textureBlendMode = 2; 
        public float metallic = 0f;
        public float roughness = 0.5f;
        #endregion

        #region Lighting Settings
        public bool lightingEnabled = true;
        public int lightingBlendMode = 1; 
        public float lightingOpacity = 1f;

        public bool light1Enabled = true;
        public Vector3 light1Position = new Vector3(-0.6f, -0.57f, 1f);
        public float light1Intensity = 1.3f;
        public float light1Range = 2f;
        public float light1Blur = 0.5f;
        public Color light1Color = Color.white;
        public bool light1SpecularEnabled = true;
        public float light1SpecularIntensity = 0.5f;
        public float light1SpecularSize = 1f;

        public bool light2Enabled = false;
        public Vector3 light2Position = new Vector3(0.5f, 0.5f, 1f);
        public float light2Intensity = 1f;
        public float light2Range = 2f;
        public float light2Blur = 0.5f;
        public Color light2Color = Color.white;
        public bool light2SpecularEnabled = true;
        public float light2SpecularIntensity = 0.5f;
        public float light2SpecularSize = 1f;

        public bool light3Enabled = false;
        public Vector3 light3Position = new Vector3(0.5f, 0.5f, 1f);
        public float light3Intensity = 1f;
        public float light3Range = 2f;
        public float light3Blur = 0.5f;
        public Color light3Color = Color.white;
        public bool light3SpecularEnabled = true;
        public float light3SpecularIntensity = 0.5f;
        public float light3SpecularSize = 1f;

        public bool light4Enabled = false;
        public Vector3 light4Position = new Vector3(0.5f, 0.5f, 1f);
        public float light4Intensity = 1f;
        public float light4Range = 2f;
        public float light4Blur = 0.5f;
        public Color light4Color = Color.white;
        public bool light4SpecularEnabled = true;
        public float light4SpecularIntensity = 0.5f;
        public float light4SpecularSize = 1f;
        #endregion

        #region Fresnel Settings
        public bool fresnelEnabled = false;
        public float fresnelPower = 3f;
        public float fresnelIntensity = 1f;
        public Color fresnelColor = Color.white;
        public int fresnelBlendMode = 1; 
        public float fresnelOpacity = 1f;
        #endregion

        #region Rim Settings
        public bool rimEnabled = false;
        public float rimLightIntensity = 0.3f;
        public float rimLightBlur = 0.5f;
        public Color rimLightColor = Color.white;
        public float rimShadeIntensity = 0f;
        public float rimShadeBlur = 0.5f;
        public Color rimShadeColor = Color.black;
        public float emissive = 0f;
        public int rimBlendMode = 1; 
        public float rimOpacity = 1f;
        #endregion

        #region Reflection Settings
        public StudioSettings studio = new StudioSettings();
        public bool reflectionEnabled = false;
        public float reflectionSmoothness = 0.8f;
        public float reflectionIntensity = 1f;
        public Color reflectionColor = Color.white;
        public float reflectionFresnelBias = 0.1f;
        public float reflectionFresnelPower = 3f;
        public Color reflectionHorizonColor = new Color(0.5f, 0.5f, 0.6f);
        public Color reflectionGroundColor = new Color(0.2f, 0.15f, 0.1f);
        public int reflectionBlendMode = 1; 
        public float reflectionOpacity = 1f;
        #endregion

        #region Environment Settings
        public bool environmentEnabled = false;
        public Texture2D environmentMap;
        public Texture2D environmentNormalMap;
        public Cubemap environmentCubemap;
        public float environmentIntensity = 1f;
        public float environmentBlur = 0f;
        public float environmentRotation = 0f;
        public int environmentUVMode = 1; 
        public float environmentBorder = 0.5f;
        public Vector2 environmentOffset = Vector2.zero;
        public Vector2 environmentTiling = Vector2.one;
        public float environmentNormalStrength = 1f;
        public int environmentBlendMode = 1; 
        public float environmentOpacity = 1f;
        #endregion

        #region Noise Settings
        public bool noiseEnabled = false;
        public Texture2D noiseTexture;
        public float noiseStrength = 0.5f;
        public float noiseBlur = 0f;
        public Vector2 noiseTiling = Vector2.one;
        public int noiseBlendMode = 4; 
        public float noiseOpacity = 1f;
        #endregion

        #region Post-Process Settings
        public bool bloomEnabled;
        public float bloomStrength = .6f;
        public float bloomThreshold = .65f;
        public float bloomSpread = 5;

        public bool postProcessEnabled = false;
        public bool blurEnabled = false;
        public float blurRadius = 1f;
        public bool sharpenEnabled = false;
        public float sharpenStrength = 0.5f;
        public bool chromaticAberrationEnabled = false;
        public float chromaticAberrationIntensity = 0.5f;
        public bool filmGrainEnabled = false;
        public float filmGrainStrength = 0.3f;
        public float filmGrainScale = 1f;
        #endregion

        #region Scene Capture Settings
        public bool sceneCaptureEnabled = false;
        public int sceneCaptureMode = 0; 
        public int[] layerOrder;
        public Texture2D capturedTexture;
        public Material sceneCaptureMaterial;
        public int sceneCaptureResolution = 512;
        public float sceneCaptureScale = 1f;
        public Color sceneCaptureBackgroundColor = Color.clear;
        public bool sceneCaptureUseSceneLighting = true;
        public bool sceneCaptureUseReflectionProbe = true;
        public int sceneCaptureBlendMode = 0; 
        public float sceneCaptureOpacity = 1f;
        #endregion

        #region Methods
        public void ApplyToSettings(MatCapSettings settings)
        {
            settings.baseSettings.baseColor = baseColor;
            settings.baseSettings.baseTexture = baseTexture;
            settings.baseSettings.textureStrength = textureStrength;
            settings.baseSettings.textureBlendMode = (BlendMode)textureBlendMode;
            settings.baseSettings.metallic = metallic;
            settings.baseSettings.roughness = roughness;

            settings.lighting.enabled = lightingEnabled;
            settings.lighting.blendMode = (BlendMode)lightingBlendMode;
            settings.lighting.opacity = lightingOpacity;

            ApplyLightSettings(settings.lighting.lights[0], light1Enabled, light1Position, light1Intensity,
                light1Range, light1Blur, light1Color, light1SpecularEnabled, light1SpecularIntensity, light1SpecularSize);
            ApplyLightSettings(settings.lighting.lights[1], light2Enabled, light2Position, light2Intensity,
                light2Range, light2Blur, light2Color, light2SpecularEnabled, light2SpecularIntensity, light2SpecularSize);
            ApplyLightSettings(settings.lighting.lights[2], light3Enabled, light3Position, light3Intensity,
                light3Range, light3Blur, light3Color, light3SpecularEnabled, light3SpecularIntensity, light3SpecularSize);
            ApplyLightSettings(settings.lighting.lights[3], light4Enabled, light4Position, light4Intensity,
                light4Range, light4Blur, light4Color, light4SpecularEnabled, light4SpecularIntensity, light4SpecularSize);

            settings.fresnel.enabled = fresnelEnabled;
            settings.fresnel.power = fresnelPower;
            settings.fresnel.intensity = fresnelIntensity;
            settings.fresnel.color = fresnelColor;
            settings.fresnel.blendMode = (BlendMode)fresnelBlendMode;
            settings.fresnel.opacity = fresnelOpacity;

            settings.rim.enabled = rimEnabled;
            settings.rim.rimLightIntensity = rimLightIntensity;
            settings.rim.rimLightBlur = rimLightBlur;
            settings.rim.rimLightColor = rimLightColor;
            settings.rim.rimShadeIntensity = rimShadeIntensity;
            settings.rim.rimShadeBlur = rimShadeBlur;
            settings.rim.rimShadeColor = rimShadeColor;
            settings.rim.emissive = emissive;
            settings.rim.blendMode = (BlendMode)rimBlendMode;
            settings.rim.opacity = rimOpacity;

            settings.reflections.studio = studio != null ? studio.Copy() : new StudioSettings();
            settings.reflections.enabled = reflectionEnabled;
            settings.reflections.smoothness = reflectionSmoothness;
            settings.reflections.intensity = reflectionIntensity;
            settings.reflections.reflectionColor = reflectionColor;
            settings.reflections.fresnelBias = reflectionFresnelBias;
            settings.reflections.fresnelPower = reflectionFresnelPower;
            settings.reflections.horizonColor = reflectionHorizonColor;
            settings.reflections.groundColor = reflectionGroundColor;
            settings.reflections.blendMode = (BlendMode)reflectionBlendMode;
            settings.reflections.opacity = reflectionOpacity;

            settings.environment.enabled = environmentEnabled;
            settings.environment.envMap = environmentMap;
            settings.environment.normalMap = environmentNormalMap;
            settings.environment.cubemap = environmentCubemap;
            settings.environment.intensity = environmentIntensity;
            settings.environment.blur = environmentBlur;
            settings.environment.rotation = environmentRotation;
            settings.environment.uvMode = (EnvironmentUVMode)environmentUVMode;
            settings.environment.border = environmentBorder;
            settings.environment.offset = environmentOffset;
            settings.environment.tiling = environmentTiling;
            settings.environment.normalStrength = environmentNormalStrength;
            settings.environment.blendMode = (BlendMode)environmentBlendMode;
            settings.environment.opacity = environmentOpacity;

            settings.noise.enabled = noiseEnabled;
            settings.noise.noiseTex = noiseTexture;
            settings.noise.strength = noiseStrength;
            settings.noise.blur = noiseBlur;
            settings.noise.tiling = noiseTiling;
            settings.noise.blendMode = (BlendMode)noiseBlendMode;
            settings.noise.opacity = noiseOpacity;

            settings.postProcess.bloomEnabled = bloomEnabled;
            settings.postProcess.bloomStrength = bloomStrength;
            settings.postProcess.bloomThreshold = bloomThreshold;
            settings.postProcess.bloomSpread = bloomSpread;
            settings.postProcess.enabled = postProcessEnabled;
            settings.postProcess.blurEnabled = blurEnabled;
            settings.postProcess.blurRadius = blurRadius;
            settings.postProcess.sharpenEnabled = sharpenEnabled;
            settings.postProcess.sharpenStrength = sharpenStrength;
            settings.postProcess.chromaticAberrationEnabled = chromaticAberrationEnabled;
            settings.postProcess.chromaticAberrationIntensity = chromaticAberrationIntensity;
            settings.postProcess.filmGrainEnabled = filmGrainEnabled;
            settings.postProcess.filmGrainStrength = filmGrainStrength;
            settings.postProcess.filmGrainScale = filmGrainScale;

            settings.sceneCapture.enabled = sceneCaptureEnabled;
            settings.sceneCapture.captureMode = (SceneCaptureMode)sceneCaptureMode;
            settings.sceneCapture.captureMaterial = sceneCaptureMaterial;
            settings.sceneCapture.capturedTexture = capturedTexture;
            settings.sceneCapture.captureResolution = sceneCaptureResolution;
            settings.sceneCapture.sphereScale = sceneCaptureScale;
            settings.sceneCapture.backgroundColor = sceneCaptureBackgroundColor;
            settings.sceneCapture.useSceneLighting = sceneCaptureUseSceneLighting;
            settings.sceneCapture.useReflectionProbe = sceneCaptureUseReflectionProbe;
            settings.sceneCapture.blendMode = (BlendMode)sceneCaptureBlendMode;
            settings.sceneCapture.opacity = sceneCaptureOpacity;
        }

        public void CopyFromSettings(MatCapSettings settings)
        {
            baseColor = settings.baseSettings.baseColor;
            baseTexture = settings.baseSettings.baseTexture;
            textureStrength = settings.baseSettings.textureStrength;
            textureBlendMode = (int)settings.baseSettings.textureBlendMode;
            metallic = settings.baseSettings.metallic;
            roughness = settings.baseSettings.roughness;

            lightingEnabled = settings.lighting.enabled;
            lightingBlendMode = (int)settings.lighting.blendMode;
            lightingOpacity = settings.lighting.opacity;

            CopyLightSettings(settings.lighting.lights[0], ref light1Enabled, ref light1Position, ref light1Intensity,
                ref light1Range, ref light1Blur, ref light1Color, ref light1SpecularEnabled, ref light1SpecularIntensity, ref light1SpecularSize);
            CopyLightSettings(settings.lighting.lights[1], ref light2Enabled, ref light2Position, ref light2Intensity,
                ref light2Range, ref light2Blur, ref light2Color, ref light2SpecularEnabled, ref light2SpecularIntensity, ref light2SpecularSize);
            CopyLightSettings(settings.lighting.lights[2], ref light3Enabled, ref light3Position, ref light3Intensity,
                ref light3Range, ref light3Blur, ref light3Color, ref light3SpecularEnabled, ref light3SpecularIntensity, ref light3SpecularSize);
            CopyLightSettings(settings.lighting.lights[3], ref light4Enabled, ref light4Position, ref light4Intensity,
                ref light4Range, ref light4Blur, ref light4Color, ref light4SpecularEnabled, ref light4SpecularIntensity, ref light4SpecularSize);

            fresnelEnabled = settings.fresnel.enabled;
            fresnelPower = settings.fresnel.power;
            fresnelIntensity = settings.fresnel.intensity;
            fresnelColor = settings.fresnel.color;
            fresnelBlendMode = (int)settings.fresnel.blendMode;
            fresnelOpacity = settings.fresnel.opacity;

            rimEnabled = settings.rim.enabled;
            rimLightIntensity = settings.rim.rimLightIntensity;
            rimLightBlur = settings.rim.rimLightBlur;
            rimLightColor = settings.rim.rimLightColor;
            rimShadeIntensity = settings.rim.rimShadeIntensity;
            rimShadeBlur = settings.rim.rimShadeBlur;
            rimShadeColor = settings.rim.rimShadeColor;
            emissive = settings.rim.emissive;
            rimBlendMode = (int)settings.rim.blendMode;
            rimOpacity = settings.rim.opacity;

            studio = settings.reflections.studio.Copy();
            reflectionEnabled = settings.reflections.enabled;
            reflectionSmoothness = settings.reflections.smoothness;
            reflectionIntensity = settings.reflections.intensity;
            reflectionColor = settings.reflections.reflectionColor;
            reflectionFresnelBias = settings.reflections.fresnelBias;
            reflectionFresnelPower = settings.reflections.fresnelPower;
            reflectionHorizonColor = settings.reflections.horizonColor;
            reflectionGroundColor = settings.reflections.groundColor;
            reflectionBlendMode = (int)settings.reflections.blendMode;
            reflectionOpacity = settings.reflections.opacity;

            environmentEnabled = settings.environment.enabled;
            environmentMap = settings.environment.envMap;
            environmentNormalMap = settings.environment.normalMap;
            environmentCubemap = settings.environment.cubemap;
            environmentIntensity = settings.environment.intensity;
            environmentBlur = settings.environment.blur;
            environmentRotation = settings.environment.rotation;
            environmentUVMode = (int)settings.environment.uvMode;
            environmentBorder = settings.environment.border;
            environmentOffset = settings.environment.offset;
            environmentTiling = settings.environment.tiling;
            environmentNormalStrength = settings.environment.normalStrength;
            environmentBlendMode = (int)settings.environment.blendMode;
            environmentOpacity = settings.environment.opacity;

            noiseEnabled = settings.noise.enabled;
            noiseTexture = settings.noise.noiseTex;
            noiseStrength = settings.noise.strength;
            noiseBlur = settings.noise.blur;
            noiseTiling = settings.noise.tiling;
            noiseBlendMode = (int)settings.noise.blendMode;
            noiseOpacity = settings.noise.opacity;

            bloomEnabled = settings.postProcess.bloomEnabled;
            bloomStrength = settings.postProcess.bloomStrength;
            bloomThreshold = settings.postProcess.bloomThreshold;
            bloomSpread = settings.postProcess.bloomSpread;
            postProcessEnabled = settings.postProcess.enabled;
            blurEnabled = settings.postProcess.blurEnabled;
            blurRadius = settings.postProcess.blurRadius;
            sharpenEnabled = settings.postProcess.sharpenEnabled;
            sharpenStrength = settings.postProcess.sharpenStrength;
            chromaticAberrationEnabled = settings.postProcess.chromaticAberrationEnabled;
            chromaticAberrationIntensity = settings.postProcess.chromaticAberrationIntensity;
            filmGrainEnabled = settings.postProcess.filmGrainEnabled;
            filmGrainStrength = settings.postProcess.filmGrainStrength;
            filmGrainScale = settings.postProcess.filmGrainScale;

            sceneCaptureEnabled = settings.sceneCapture.enabled;
            sceneCaptureMode = (int)settings.sceneCapture.captureMode;
            sceneCaptureMaterial = settings.sceneCapture.captureMaterial;
            capturedTexture = settings.sceneCapture.capturedTexture;
            sceneCaptureResolution = settings.sceneCapture.captureResolution;
            sceneCaptureScale = settings.sceneCapture.sphereScale;
            sceneCaptureBackgroundColor = settings.sceneCapture.backgroundColor;
            sceneCaptureUseSceneLighting = settings.sceneCapture.useSceneLighting;
            sceneCaptureUseReflectionProbe = settings.sceneCapture.useReflectionProbe;
            sceneCaptureBlendMode = (int)settings.sceneCapture.blendMode;
            sceneCaptureOpacity = settings.sceneCapture.opacity;
        }

        private static void ApplyLightSettings(LightSettings light, bool enabled, Vector3 position, float intensity,
            float range, float blur, Color color, bool specEnabled, float specIntensity, float specSize)
        {
            light.enabled = enabled;
            light.position = position;
            light.intensity = intensity;
            light.range = range;
            light.blur = blur;
            light.color = color;
            light.specularEnabled = specEnabled;
            light.specularIntensity = specIntensity;
            light.specularSize = specSize;
        }

        private static void CopyLightSettings(LightSettings light, ref bool enabled, ref Vector3 position, ref float intensity,
            ref float range, ref float blur, ref Color color, ref bool specEnabled, ref float specIntensity, ref float specSize)
        {
            enabled = light.enabled;
            position = light.position;
            intensity = light.intensity;
            range = light.range;
            blur = light.blur;
            color = light.color;
            specEnabled = light.specularEnabled;
            specIntensity = light.specularIntensity;
            specSize = light.specularSize;
        }
        #endregion
    }
}
