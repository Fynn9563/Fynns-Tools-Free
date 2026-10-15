using System;
using UnityEngine;

namespace MatCapMaker
{
    #region Enums
    public enum BlendMode
    {
        Normal,
        Add,
        Multiply,
        Screen,
        Overlay,
        SoftLight,
        ColorDodge,
        ColorBurn
    }

    public enum EnvironmentUVMode
    {
        UTSStyle,       
        TopPinch,       
        DoubleSided,    
        Gradient,       
        DirectNormal,   
        Cubemap,
        SphericalReflection
    }
    #endregion

    #region Settings Classes
    [Serializable]
    public class BaseSettings
    {
        public Color baseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Texture2D baseTexture;
        public float textureStrength = 1f;
        public BlendMode textureBlendMode = BlendMode.Multiply;
        public float metallic = 0f;
        public float roughness = 0.5f;

        public BaseSettings() { }

        public BaseSettings(BaseSettings other)
        {
            baseColor = other.baseColor;
            baseTexture = other.baseTexture;
            textureStrength = other.textureStrength;
            textureBlendMode = other.textureBlendMode;
            metallic = other.metallic;
            roughness = other.roughness;
        }
    }

    [Serializable]
    public class LightSettings
    {
        public bool enabled = false;
        public Vector3 position = new Vector3(0.5f, 0.5f, 1f);
        public float intensity = 1f;
        public float range = 2f;
        public float blur = 0.5f;
        public Color color = Color.white;
        public bool specularEnabled = true;
        public float specularIntensity = 0.5f;
        public float specularSize = 1f;

        public LightSettings() { }

        public LightSettings(LightSettings other)
        {
            enabled = other.enabled;
            position = other.position;
            intensity = other.intensity;
            range = other.range;
            blur = other.blur;
            color = other.color;
            specularEnabled = other.specularEnabled;
            specularIntensity = other.specularIntensity;
            specularSize = other.specularSize;
        }
    }

    [Serializable]
    public class FresnelSettings
    {
        public bool enabled = false;
        public float power = 3f;
        public float intensity = 1f;
        public Color color = Color.white;
        public BlendMode blendMode = BlendMode.Add;
        public float opacity = 1f;

        public FresnelSettings() { }

        public FresnelSettings(FresnelSettings other)
        {
            enabled = other.enabled;
            power = other.power;
            intensity = other.intensity;
            color = other.color;
            blendMode = other.blendMode;
            opacity = other.opacity;
        }
    }

    [Serializable]
    public class RimSettings
    {
        public bool enabled = false;
        public float rimLightIntensity = 0.3f;
        public float rimLightBlur = 0.5f;
        public Color rimLightColor = Color.white;
        public float rimShadeIntensity = 0f;
        public float rimShadeBlur = 0.5f;
        public Color rimShadeColor = Color.black;
        public float emissive = 0f;
        public BlendMode blendMode = BlendMode.Add;
        public float opacity = 1f;

        public RimSettings() { }

        public RimSettings(RimSettings other)
        {
            enabled = other.enabled;
            rimLightIntensity = other.rimLightIntensity;
            rimLightBlur = other.rimLightBlur;
            rimLightColor = other.rimLightColor;
            rimShadeIntensity = other.rimShadeIntensity;
            rimShadeBlur = other.rimShadeBlur;
            rimShadeColor = other.rimShadeColor;
            emissive = other.emissive;
            blendMode = other.blendMode;
            opacity = other.opacity;
        }
    }

    public enum ReflectionShape { Softbox, Window, Ellipse, Ring, Hexagon, Star, Crescent }
    public enum SurfacePattern { Smooth, Fractal, Ridged, Nacre, Crystal, Brushed, Julia, Spiral, Lava, HexTiles, Scales, Shards, Flowers, Stone }

    [Serializable]
    public class StudioPanel
    {
        public bool enabled = true;
        public Vector3 direction = new Vector3(-0.8f, 0.9f, 0.6f);
        public Color color = Color.white;
        public float intensity = 2.4f;
        public Vector2 size = new Vector2(1.1f, 0.6f);
        public float rotation;
        public ReflectionShape shape;
    }

    [Serializable]
    public class StudioSettings
    {
        public bool enabled;
        public Color metalColor = new Color(0.3f, 0.45f, 0.55f);
        public Color shadowColor = new Color(0.025f, 0.035f, 0.05f);
        public float softness = 0.09f;
        public float rotation;
        public float distortion;
        public float grain = 0.035f;
        public float iridescence;
        public float satin = 0.25f;
        public SurfacePattern pattern;
        public float patternScale = 6;
        public float patternStrength = .4f;
        public int patternDetail = 5;
        public float patternSeed;
        public bool wrapPattern = true;
        public float patternWarp = .3f;
        public float patternGlow = .35f;
        public float patternRotation;
        public float patternContrast = 1;
        public Color patternColor = new Color(.2f,.7f,.8f);
        public StudioPanel[] panels = {
            new StudioPanel { shape = ReflectionShape.Window },
            new StudioPanel { direction = new Vector3(1.1f,0.1f,0.2f), size = new Vector2(0.45f,1.3f), intensity = 1.7f },
            new StudioPanel { direction = new Vector3(-0.2f,-1.2f,0.35f), size = new Vector2(1.4f,0.45f), intensity = 2f }
        };
        public StudioSettings Copy() { return JsonUtility.FromJson<StudioSettings>(JsonUtility.ToJson(this)); }
    }

    [Serializable]
    public class ReflectionSettings
    {
        public bool enabled = false;
        public StudioSettings studio = new StudioSettings();
        public float smoothness = 0.8f;           
        public float intensity = 1f;              
        public Color reflectionColor = Color.white;
        public float fresnelBias = 0.1f;          
        public float fresnelPower = 3f;           
        public Color horizonColor = new Color(0.5f, 0.5f, 0.6f);  
        public Color groundColor = new Color(0.2f, 0.15f, 0.1f);  
        public BlendMode blendMode = BlendMode.Add;
        public float opacity = 1f;

        public ReflectionSettings() { }

        public ReflectionSettings(ReflectionSettings other)
        {
            enabled = other.enabled;
            studio = other.studio != null ? other.studio.Copy() : new StudioSettings();
            smoothness = other.smoothness;
            intensity = other.intensity;
            reflectionColor = other.reflectionColor;
            fresnelBias = other.fresnelBias;
            fresnelPower = other.fresnelPower;
            horizonColor = other.horizonColor;
            groundColor = other.groundColor;
            blendMode = other.blendMode;
            opacity = other.opacity;
        }
    }

    [Serializable]
    public class EnvironmentSettings
    {
        public bool enabled = false;
        public Texture2D envMap;
        public Texture2D normalMap;
        public float normalStrength = 1f;
        public Cubemap cubemap;
        public float intensity = 1f;
        public float blur = 0f;
        public float rotation = 0f;
        public EnvironmentUVMode uvMode = EnvironmentUVMode.TopPinch;
        public float border = 0.5f;
        public Vector2 offset = Vector2.zero;
        public Vector2 tiling = Vector2.one;
        public BlendMode blendMode = BlendMode.Add;
        public float opacity = 1f;

        public EnvironmentSettings() { }

        public EnvironmentSettings(EnvironmentSettings other)
        {
            enabled = other.enabled;
            envMap = other.envMap;
            normalMap = other.normalMap;
            normalStrength = other.normalStrength;
            cubemap = other.cubemap;
            intensity = other.intensity;
            blur = other.blur;
            rotation = other.rotation;
            uvMode = other.uvMode;
            border = other.border;
            offset = other.offset;
            tiling = other.tiling;
            blendMode = other.blendMode;
            opacity = other.opacity;
        }
    }

    [Serializable]
    public class NoiseSettings
    {
        public bool enabled = false;
        public Texture2D noiseTex;
        public float strength = 0.5f;
        public float blur = 0f;
        public Vector2 tiling = Vector2.one;
        public BlendMode blendMode = BlendMode.Overlay;
        public float opacity = 1f;

        public NoiseSettings() { }

        public NoiseSettings(NoiseSettings other)
        {
            enabled = other.enabled;
            noiseTex = other.noiseTex;
            strength = other.strength;
            blur = other.blur;
            tiling = other.tiling;
            blendMode = other.blendMode;
            opacity = other.opacity;
        }
    }

    [Serializable]
    public class PostProcessSettings
    {
        public bool enabled = false;

        public bool bloomEnabled;
        public float bloomStrength = .6f;
        public float bloomThreshold = .65f;
        public float bloomSpread = 5;

        public bool blurEnabled = false;
        public float blurRadius = 1f;

        public bool sharpenEnabled = false;
        public float sharpenStrength = 0.5f;

        public bool chromaticAberrationEnabled = false;
        public float chromaticAberrationIntensity = 0.5f;

        public bool filmGrainEnabled = false;
        public float filmGrainStrength = 0.3f;
        public float filmGrainScale = 1f;

        public PostProcessSettings() { }

        public PostProcessSettings(PostProcessSettings other)
        {
            enabled = other.enabled;
            bloomEnabled = other.bloomEnabled;
            bloomStrength = other.bloomStrength;
            bloomThreshold = other.bloomThreshold;
            bloomSpread = other.bloomSpread;
            blurEnabled = other.blurEnabled;
            blurRadius = other.blurRadius;
            sharpenEnabled = other.sharpenEnabled;
            sharpenStrength = other.sharpenStrength;
            chromaticAberrationEnabled = other.chromaticAberrationEnabled;
            chromaticAberrationIntensity = other.chromaticAberrationIntensity;
            filmGrainEnabled = other.filmGrainEnabled;
            filmGrainStrength = other.filmGrainStrength;
            filmGrainScale = other.filmGrainScale;
        }
    }

    public enum SceneCaptureMode
    {
        ReflectionProbe,    
        Material            
    }

    [Serializable]
    public class SceneCaptureSettings
    {
        public bool enabled = false;
        public SceneCaptureMode captureMode = SceneCaptureMode.ReflectionProbe;
        public Texture2D capturedTexture;
        public Material captureMaterial;      
        public int captureResolution = 512;   
        public float sphereScale = 1f;        
        public Color backgroundColor = Color.clear;
        public bool useSceneLighting = true;  
        public bool useReflectionProbe = true; 
        public BlendMode blendMode = BlendMode.Normal;
        public float opacity = 1f;

        public SceneCaptureSettings() { }

        public SceneCaptureSettings(SceneCaptureSettings other)
        {
            enabled = other.enabled;
            captureMode = other.captureMode;
            captureMaterial = other.captureMaterial;
            capturedTexture = other.capturedTexture;
            captureResolution = other.captureResolution;
            sphereScale = other.sphereScale;
            backgroundColor = other.backgroundColor;
            useSceneLighting = other.useSceneLighting;
            useReflectionProbe = other.useReflectionProbe;
            blendMode = other.blendMode;
            opacity = other.opacity;
        }
    }

    [Serializable]
    public class LightingSettings
    {
        public bool enabled = true;
        public LightSettings[] lights = new LightSettings[4];
        public BlendMode blendMode = BlendMode.Add;
        public float opacity = 1.0f;

        public LightingSettings()
        {
            for (int i = 0; i < 4; i++)
            {
                lights[i] = new LightSettings();
            }
            lights[0].enabled = true;
            lights[0].position = new Vector3(-0.5f, -0.6f, 1f);
            lights[0].intensity = 0.6f;
            lights[0].blur = 0.3f;
            lights[0].specularIntensity = 0.5f;
            lights[0].specularSize = 0.3f;
        }

        public LightingSettings(LightingSettings other)
        {
            enabled = other.enabled;
            blendMode = other.blendMode;
            opacity = other.opacity;
            lights = new LightSettings[4];
            for (int i = 0; i < 4; i++)
            {
                lights[i] = new LightSettings(other.lights[i]);
            }
        }
    }

    [Serializable]
    public class MatCapSettings
    {
        public BaseSettings baseSettings = new BaseSettings();
        public LightingSettings lighting = new LightingSettings();
        public FresnelSettings fresnel = new FresnelSettings();
        public RimSettings rim = new RimSettings();
        public ReflectionSettings reflections = new ReflectionSettings();
        public EnvironmentSettings environment = new EnvironmentSettings();
        public NoiseSettings noise = new NoiseSettings();
        public PostProcessSettings postProcess = new PostProcessSettings();
        public SceneCaptureSettings sceneCapture = new SceneCaptureSettings();

        public MatCapSettings() { }

        public MatCapSettings(MatCapSettings other)
        {
            baseSettings = new BaseSettings(other.baseSettings);
            lighting = new LightingSettings(other.lighting);
            fresnel = new FresnelSettings(other.fresnel);
            rim = new RimSettings(other.rim);
            reflections = new ReflectionSettings(other.reflections);
            environment = new EnvironmentSettings(other.environment);
            noise = new NoiseSettings(other.noise);
            postProcess = new PostProcessSettings(other.postProcess);
            sceneCapture = new SceneCaptureSettings(other.sceneCapture);
        }

        public void CopyFrom(MatCapSettings other)
        {
            baseSettings = new BaseSettings(other.baseSettings);
            lighting = new LightingSettings(other.lighting);
            fresnel = new FresnelSettings(other.fresnel);
            rim = new RimSettings(other.rim);
            reflections = new ReflectionSettings(other.reflections);
            environment = new EnvironmentSettings(other.environment);
            noise = new NoiseSettings(other.noise);
            postProcess = new PostProcessSettings(other.postProcess);
            sceneCapture = new SceneCaptureSettings(other.sceneCapture);
        }
    }
    #endregion

    #region Built-in Presets
    public static class BuiltInPresets
    {
        private static void SetupLightingDefaults(MatCapSettings settings)
        {
            settings.lighting.blendMode = BlendMode.Add;
            settings.lighting.opacity = 1.0f;
        }

        private static MatCapSettings GetLegacyDefault()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            settings.baseSettings.metallic = 0.2f;
            settings.baseSettings.roughness = 0.4f;

            SetupLightingDefaults(settings);

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.5f, -0.6f, 1f);
            settings.lighting.lights[0].intensity = 0.6f;
            settings.lighting.lights[0].blur = 0.3f;
            settings.lighting.lights[0].specularIntensity = 0.5f;
            settings.lighting.lights[0].specularSize = 0.3f;

            settings.lighting.lights[1].enabled = true;
            settings.lighting.lights[1].position = new Vector3(0.7f, 0.2f, 0.8f);
            settings.lighting.lights[1].intensity = 0.2f;
            settings.lighting.lights[1].blur = 0.6f;
            settings.lighting.lights[1].specularEnabled = false;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 4f;
            settings.fresnel.intensity = 0.15f;
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.5f;

            return settings;
        }

        public static MatCapSettings GetGold()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.85f, 0.65f, 0.13f, 1f);
            settings.baseSettings.metallic = 0.95f;
            settings.baseSettings.roughness = 0.15f;

            SetupLightingDefaults(settings);

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.4f, -0.7f, 1f);
            settings.lighting.lights[0].intensity = 0.7f;
            settings.lighting.lights[0].blur = 0.2f;
            settings.lighting.lights[0].color = new Color(1f, 0.95f, 0.8f, 1f);
            settings.lighting.lights[0].specularIntensity = 1.0f;
            settings.lighting.lights[0].specularSize = 0.25f;

            settings.lighting.lights[1].enabled = true;
            settings.lighting.lights[1].position = new Vector3(0.8f, 0.3f, 0.5f);
            settings.lighting.lights[1].intensity = 0.25f;
            settings.lighting.lights[1].blur = 0.5f;
            settings.lighting.lights[1].color = new Color(1f, 0.85f, 0.5f, 1f);
            settings.lighting.lights[1].specularIntensity = 0.3f;
            settings.lighting.lights[1].specularSize = 0.5f;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 3.5f;
            settings.fresnel.intensity = 0.25f;
            settings.fresnel.color = new Color(1f, 0.9f, 0.5f, 1f);
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.5f;

            settings.rim.enabled = true;
            settings.rim.rimLightIntensity = 0.1f;
            settings.rim.rimLightBlur = 0.7f;
            settings.rim.rimLightColor = new Color(1f, 0.85f, 0.4f, 1f);
            settings.rim.blendMode = BlendMode.Add;
            settings.rim.opacity = 0.3f;

            return settings;
        }

        public static MatCapSettings GetGunmetal()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.2f, 0.22f, 0.28f, 1f);
            settings.baseSettings.metallic = 0.9f;
            settings.baseSettings.roughness = 0.15f;

            SetupLightingDefaults(settings);

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.5f, -0.55f, 1f);
            settings.lighting.lights[0].intensity = 0.6f;
            settings.lighting.lights[0].blur = 0.25f;
            settings.lighting.lights[0].color = new Color(0.85f, 0.9f, 1f, 1f);
            settings.lighting.lights[0].specularIntensity = 0.9f;
            settings.lighting.lights[0].specularSize = 0.3f;

            settings.lighting.lights[1].enabled = true;
            settings.lighting.lights[1].position = new Vector3(0.7f, 0.4f, 0.6f);
            settings.lighting.lights[1].intensity = 0.2f;
            settings.lighting.lights[1].blur = 0.5f;
            settings.lighting.lights[1].color = new Color(0.6f, 0.75f, 1f, 1f);
            settings.lighting.lights[1].specularIntensity = 0.3f;
            settings.lighting.lights[1].specularSize = 0.5f;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 3f;
            settings.fresnel.intensity = 0.2f;
            settings.fresnel.color = new Color(0.5f, 0.65f, 0.9f, 1f);
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.5f;

            settings.rim.enabled = true;
            settings.rim.rimShadeIntensity = 0.15f;
            settings.rim.rimShadeBlur = 0.8f;
            settings.rim.rimShadeColor = new Color(0.05f, 0.05f, 0.1f, 1f);
            settings.rim.blendMode = BlendMode.Multiply;
            settings.rim.opacity = 0.5f;

            return settings;
        }

        public static MatCapSettings GetStainless()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.75f, 0.75f, 0.78f, 1f);
            settings.baseSettings.metallic = 1f;
            settings.baseSettings.roughness = 0.05f;

            SetupLightingDefaults(settings);

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.45f, -0.65f, 1f);
            settings.lighting.lights[0].intensity = 0.8f;
            settings.lighting.lights[0].blur = 0.15f;
            settings.lighting.lights[0].color = Color.white;
            settings.lighting.lights[0].specularIntensity = 1.2f;
            settings.lighting.lights[0].specularSize = 0.2f;

            settings.lighting.lights[1].enabled = true;
            settings.lighting.lights[1].position = new Vector3(0.6f, 0.2f, 0.9f);
            settings.lighting.lights[1].intensity = 0.3f;
            settings.lighting.lights[1].blur = 0.4f;
            settings.lighting.lights[1].specularIntensity = 0.5f;
            settings.lighting.lights[1].specularSize = 0.4f;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 4.5f;
            settings.fresnel.intensity = 0.3f;
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.5f;

            return settings;
        }

        public static MatCapSettings GetStone()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.45f, 0.42f, 0.38f, 1f);
            settings.baseSettings.metallic = 0f;
            settings.baseSettings.roughness = 0.85f;

            SetupLightingDefaults(settings);
            settings.lighting.blendMode = BlendMode.SoftLight;
            settings.lighting.opacity = 0.8f;

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.35f, -0.5f, 1f);
            settings.lighting.lights[0].intensity = 0.5f;
            settings.lighting.lights[0].blur = 0.6f;
            settings.lighting.lights[0].specularEnabled = false;

            settings.lighting.lights[1].enabled = true;
            settings.lighting.lights[1].position = new Vector3(0.5f, 0.3f, 0.8f);
            settings.lighting.lights[1].intensity = 0.2f;
            settings.lighting.lights[1].blur = 0.8f;
            settings.lighting.lights[1].specularEnabled = false;

            settings.rim.enabled = true;
            settings.rim.rimShadeIntensity = 0.2f;
            settings.rim.rimShadeBlur = 1f;
            settings.rim.rimShadeColor = new Color(0.25f, 0.22f, 0.2f, 1f);
            settings.rim.blendMode = BlendMode.Multiply;
            settings.rim.opacity = 0.4f;

            return settings;
        }

        public static MatCapSettings GetMatteRed()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.7f, 0.12f, 0.1f, 1f);
            settings.baseSettings.metallic = 0f;
            settings.baseSettings.roughness = 0.7f;

            SetupLightingDefaults(settings);
            settings.lighting.blendMode = BlendMode.SoftLight;
            settings.lighting.opacity = 0.7f;

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.4f, -0.5f, 1f);
            settings.lighting.lights[0].intensity = 0.5f;
            settings.lighting.lights[0].blur = 0.4f;
            settings.lighting.lights[0].specularIntensity = 0.15f;
            settings.lighting.lights[0].specularSize = 0.8f;

            settings.lighting.lights[1].enabled = true;
            settings.lighting.lights[1].position = new Vector3(0.6f, 0.3f, 0.8f);
            settings.lighting.lights[1].intensity = 0.25f;
            settings.lighting.lights[1].blur = 0.7f;
            settings.lighting.lights[1].specularEnabled = false;

            settings.rim.enabled = true;
            settings.rim.rimShadeIntensity = 0.2f;
            settings.rim.rimShadeBlur = 0.9f;
            settings.rim.rimShadeColor = new Color(0.3f, 0.05f, 0.05f, 1f);
            settings.rim.blendMode = BlendMode.Multiply;
            settings.rim.opacity = 0.4f;

            return settings;
        }

        public static MatCapSettings GetChrome()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.8f, 0.8f, 0.82f, 1f);
            settings.baseSettings.metallic = 1f;
            settings.baseSettings.roughness = 0.02f;

            SetupLightingDefaults(settings);

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.5f, -0.6f, 1f);
            settings.lighting.lights[0].intensity = 1.0f;
            settings.lighting.lights[0].blur = 0.1f;
            settings.lighting.lights[0].color = Color.white;
            settings.lighting.lights[0].specularIntensity = 1.5f;
            settings.lighting.lights[0].specularSize = 0.15f;

            settings.lighting.lights[1].enabled = true;
            settings.lighting.lights[1].position = new Vector3(0.7f, 0.4f, 0.7f);
            settings.lighting.lights[1].intensity = 0.4f;
            settings.lighting.lights[1].blur = 0.2f;
            settings.lighting.lights[1].specularIntensity = 0.8f;
            settings.lighting.lights[1].specularSize = 0.25f;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 5f;
            settings.fresnel.intensity = 0.35f;
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.5f;

            return settings;
        }

        public static MatCapSettings GetCyanGlass()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.15f, 0.4f, 0.5f, 1f);
            settings.baseSettings.metallic = 0.3f;
            settings.baseSettings.roughness = 0.08f;

            SetupLightingDefaults(settings);

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.4f, -0.5f, 1f);
            settings.lighting.lights[0].intensity = 0.6f;
            settings.lighting.lights[0].blur = 0.25f;
            settings.lighting.lights[0].color = new Color(0.8f, 0.95f, 1f, 1f);
            settings.lighting.lights[0].specularIntensity = 0.7f;
            settings.lighting.lights[0].specularSize = 0.3f;

            settings.lighting.lights[1].enabled = true;
            settings.lighting.lights[1].position = new Vector3(0.6f, 0.3f, 0.6f);
            settings.lighting.lights[1].intensity = 0.2f;
            settings.lighting.lights[1].blur = 0.4f;
            settings.lighting.lights[1].color = new Color(0.3f, 0.8f, 1f, 1f);
            settings.lighting.lights[1].specularIntensity = 0.4f;
            settings.lighting.lights[1].specularSize = 0.5f;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 2.5f;
            settings.fresnel.intensity = 0.4f;
            settings.fresnel.color = new Color(0.4f, 0.85f, 1f, 1f);
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.6f;

            settings.rim.enabled = true;
            settings.rim.rimLightIntensity = 0.2f;
            settings.rim.rimLightBlur = 0.5f;
            settings.rim.rimLightColor = new Color(0.3f, 0.7f, 0.9f, 1f);
            settings.rim.blendMode = BlendMode.Add;
            settings.rim.opacity = 0.4f;

            return settings;
        }

        public static MatCapSettings GetIridescentBubble()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.05f, 0.05f, 0.08f, 1f);
            settings.baseSettings.metallic = 0.6f;
            settings.baseSettings.roughness = 0.02f;

            SetupLightingDefaults(settings);
            settings.lighting.opacity = 0.5f;

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.3f, -0.4f, 1f);
            settings.lighting.lights[0].intensity = 0.4f;
            settings.lighting.lights[0].blur = 0.2f;
            settings.lighting.lights[0].specularIntensity = 1.0f;
            settings.lighting.lights[0].specularSize = 0.2f;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 2f;
            settings.fresnel.intensity = 0.6f;
            settings.fresnel.color = new Color(0.8f, 0.9f, 1f, 1f);
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.7f;

            settings.environment.enabled = true;
            settings.environment.intensity = 1.2f;
            settings.environment.blur = 0f;
            settings.environment.uvMode = EnvironmentUVMode.DoubleSided; 
            settings.environment.border = 0.5f;
            settings.environment.blendMode = BlendMode.Add;
            settings.environment.opacity = 0.9f;

            return settings;
        }

        public static MatCapSettings GetSpeckledMetal()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.7f, 0.7f, 0.72f, 1f);
            settings.baseSettings.metallic = 0.8f;
            settings.baseSettings.roughness = 0.35f;

            SetupLightingDefaults(settings);

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.4f, -0.5f, 1f);
            settings.lighting.lights[0].intensity = 0.5f;
            settings.lighting.lights[0].blur = 0.35f;
            settings.lighting.lights[0].specularIntensity = 0.4f;
            settings.lighting.lights[0].specularSize = 0.5f;

            settings.noise.enabled = true;
            settings.noise.strength = 1.0f;
            settings.noise.blur = 0f;
            settings.noise.tiling = new Vector2(3f, 3f);
            settings.noise.blendMode = BlendMode.Overlay;
            settings.noise.opacity = 0.6f;

            return settings;
        }

        public static MatCapSettings GetDarkEmerald()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.02f, 0.04f, 0.02f, 1f);
            settings.baseSettings.metallic = 0.4f;
            settings.baseSettings.roughness = 0.15f;

            SetupLightingDefaults(settings);

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.2f, -0.3f, 1f);
            settings.lighting.lights[0].intensity = 0.8f;
            settings.lighting.lights[0].blur = 0.2f;
            settings.lighting.lights[0].color = new Color(0.2f, 1f, 0.3f, 1f); 
            settings.lighting.lights[0].specularIntensity = 1.8f;
            settings.lighting.lights[0].specularSize = 0.2f;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 3f;
            settings.fresnel.intensity = 0.35f;
            settings.fresnel.color = new Color(0.1f, 0.5f, 0.2f, 1f);
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.6f;

            return settings;
        }

        public static MatCapSettings GetMarble()
        {
            var settings = new MatCapSettings();
            settings.baseSettings.baseColor = new Color(0.85f, 0.82f, 0.78f, 1f); 
            settings.baseSettings.metallic = 0.1f;
            settings.baseSettings.roughness = 0.4f;

            SetupLightingDefaults(settings);
            settings.lighting.blendMode = BlendMode.SoftLight;
            settings.lighting.opacity = 0.6f;

            settings.lighting.lights[0].enabled = true;
            settings.lighting.lights[0].position = new Vector3(-0.4f, -0.5f, 1f);
            settings.lighting.lights[0].intensity = 0.5f;
            settings.lighting.lights[0].blur = 0.4f;
            settings.lighting.lights[0].specularIntensity = 0.3f;
            settings.lighting.lights[0].specularSize = 0.6f;

            settings.baseSettings.textureStrength = 0.35f;
            settings.baseSettings.textureBlendMode = BlendMode.Multiply;

            settings.fresnel.enabled = true;
            settings.fresnel.power = 4f;
            settings.fresnel.intensity = 0.15f;
            settings.fresnel.blendMode = BlendMode.Add;
            settings.fresnel.opacity = 0.3f;

            return settings;
        }

        public static MatCapSettings GetDefault() { return GetRefinedOriginal(0); }

        public static MatCapSettings GetRefinedOriginal(int index)
        {
            MatCapSettings settings;
            switch (index)
            {
                case 1: settings = GetGold(); break;
                case 2: settings = GetGunmetal(); break;
                case 3: settings = GetStainless(); break;
                case 4: settings = GetStone(); break;
                case 5: settings = GetMatteRed(); break;
                case 6: settings = GetChrome(); break;
                case 7: settings = GetCyanGlass(); break;
                case 8: settings = GetIridescentBubble(); break;
                case 9: settings = GetSpeckledMetal(); break;
                case 10: settings = GetDarkEmerald(); break;
                case 11: settings = GetMarble(); break;
                default: settings = GetLegacyDefault(); break;
            }
            settings.lighting.enabled = false;
            settings.environment.enabled = false;
            settings.fresnel.enabled = false;
            settings.rim.enabled = false;
            settings.reflections.enabled = true;
            settings.reflections.blendMode = BlendMode.Normal;
            settings.reflections.opacity = 1;
            var studio = settings.reflections.studio;
            studio.enabled = true;
            studio.metalColor = settings.baseSettings.baseColor;
            studio.shadowColor = studio.metalColor * .08f;
            studio.shadowColor.a = 1;
            studio.panels[0].shape = ReflectionShape.Softbox;
            studio.softness = .18f;
            studio.grain = .015f;
            switch (index)
            {
                case 0:
                    studio.metalColor = new Color(.38f,.45f,.55f);
                    studio.softness = .28f; studio.satin = .6f;
                    break;
                case 1:
                    studio.metalColor = new Color(1f,.62f,.15f);
                    studio.shadowColor = new Color(.12f,.035f,.003f);
                    studio.softness = .09f;
                    studio.panels[0].shape = ReflectionShape.Window;
                    studio.panels[0].color = new Color(1f,.87f,.5f);
                    studio.panels[1].color = new Color(1f,.65f,.18f);
                    studio.panels[2].color = new Color(1f,.78f,.3f);
                    break;
                case 2:
                    studio.metalColor = new Color(.13f,.18f,.24f);
                    studio.shadowColor = new Color(.008f,.015f,.025f);
                    studio.softness = .13f; studio.grain = .012f;
                    studio.panels[1].color = new Color(.45f,.68f,1f);
                    break;
                case 3:
                    studio.metalColor = new Color(.5f,.56f,.62f);
                    studio.softness = .2f; studio.satin = .65f; studio.grain = .008f;
                    studio.panels[0].size = new Vector2(1.6f,.35f);
                    break;
                case 4:
                    studio.metalColor = new Color(.48f,.44f,.37f);
                    studio.shadowColor = new Color(.12f,.1f,.08f);
                    studio.softness = .65f; studio.satin = 1; studio.grain = .025f;
                    foreach (var panel in studio.panels) panel.intensity = .5f;
                    break;
                case 5:
                    studio.metalColor = new Color(.75f,.025f,.04f);
                    studio.shadowColor = new Color(.12f,.003f,.008f);
                    studio.softness = .65f; studio.satin = 1; studio.grain = .01f;
                    foreach (var panel in studio.panels) { panel.intensity = .75f; panel.color = new Color(1f,.2f,.22f); }
                    break;
                case 6:
                    studio.metalColor = new Color(.42f,.48f,.56f);
                    studio.shadowColor = new Color(.008f,.012f,.02f);
                    studio.softness = .04f; studio.panels[0].shape = ReflectionShape.Window;
                    studio.panels[0].size = new Vector2(1.4f,.65f);
                    break;
                case 7:
                    studio.metalColor = new Color(.035f,.65f,.8f);
                    studio.shadowColor = new Color(.001f,.015f,.025f);
                    studio.softness = .08f;
                    studio.panels[1].color = new Color(.3f,1f,1f);
                    break;
                case 8:
                    studio.metalColor = new Color(.16f,.24f,.4f);
                    studio.shadowColor = new Color(.002f,.003f,.012f);
                    studio.iridescence = 1; studio.softness = .12f;
                    studio.panels[0].intensity = 1.2f;
                    studio.panels[0].size = new Vector2(.5f,.3f);
                    studio.panels[1].color = new Color(.4f,.8f,1f);
                    studio.panels[2].color = new Color(1f,.3f,.7f);
                    break;
                case 9:
                    studio.metalColor = new Color(.3f,.37f,.42f);
                    studio.softness = .16f; studio.grain = .015f;
                    settings.noise.opacity = .07f; settings.noise.blur = .25f;
                    settings.noise.tiling = new Vector2(1.4f,1.4f);
                    break;
                case 10:
                    studio.metalColor = new Color(.015f,.24f,.11f);
                    studio.shadowColor = new Color(.002f,.012f,.005f);
                    studio.softness = .1f;
                    studio.panels[1].color = new Color(.18f,1f,.45f);
                    break;
                case 11:
                    studio.metalColor = new Color(.8f,.79f,.73f);
                    studio.shadowColor = new Color(.22f,.2f,.17f);
                    studio.softness = .35f; studio.satin = .75f; studio.grain = .02f;
                    foreach (var panel in studio.panels) panel.intensity = .85f;
                    break;
            }
            switch (index)
            {
                case 0: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[1].enabled = false; break;
                case 2: studio.rotation = -35; studio.panels[0].size = new Vector2(.18f,1.6f); studio.panels[2].enabled = false; break;
                case 3: studio.pattern = SurfacePattern.Brushed; studio.patternStrength = .25f; studio.patternScale = 6; studio.rotation = 60; break;
                case 4:
                    studio.pattern = SurfacePattern.Stone; studio.patternStrength = .85f; studio.patternScale = 18;
                    studio.metalColor = new Color(.48f,.45f,.41f); studio.patternColor = new Color(.67f,.64f,.58f);
                    studio.patternGlow = 0; studio.patternWarp = .15f; studio.patternContrast = 1.3f;
                    studio.panels[0].direction = new Vector3(-.5f,.8f,1); studio.panels[1].enabled = false;
                    break;
                case 5: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].direction = new Vector3(-.5f,1,1); studio.panels[1].enabled = false; studio.panels[2].enabled = false; break;
                case 6: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].size = new Vector2(2.5f,1.5f); studio.panels[0].direction = new Vector3(0,1,.3f); studio.panels[1].enabled = false; studio.panels[2].enabled = false; break;
                case 7: studio.panels[0].shape = ReflectionShape.Crescent; studio.panels[0].direction = new Vector3(.2f,.5f,1); studio.panels[1].enabled = false; break;
                case 8: studio.panels[0].shape = ReflectionShape.Ring; studio.panels[0].direction = Vector3.forward; studio.panels[0].size = new Vector2(1.8f,1.8f); studio.panels[1].enabled = false; studio.panels[2].enabled = false; break;
                case 9: studio.panels[0].shape = ReflectionShape.Hexagon; studio.rotation = 30; break;
                case 10: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].size = new Vector2(.4f,.4f); studio.panels[1].enabled = false; studio.panels[2].enabled = false; break;
                case 11: studio.panels[0].direction = new Vector3(.4f,1,.5f); studio.panels[1].enabled = false; break;
            }
            switch (index)
            {
                case 0: studio.panels[2].enabled = false; studio.panels[0].size = new Vector2(1.5f,.9f); break;
                case 4: studio.panels[1].enabled = false; studio.panels[2].enabled = false; break;
                case 1: studio.panels[2].enabled = false; studio.panels[1].intensity = .8f; break;
                case 2: studio.softness = .3f; studio.panels[0].intensity = .85f; studio.panels[0].size = new Vector2(.4f,1.3f); studio.panels[0].direction = new Vector3(-.6f,.8f,1); studio.panels[1].enabled = false; break;
                case 3: studio.patternStrength = .12f; studio.panels[1].enabled = false; studio.panels[2].enabled = false; studio.softness = .3f; break;
                case 6: studio.softness = .22f; studio.panels[0].intensity = 1.5f; break;
                case 7: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].direction = new Vector3(-.7f,.8f,1); studio.panels[0].size = new Vector2(.8f,.4f); studio.panels[2].enabled = false; break;
                case 8: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].direction = new Vector3(-.8f,.9f,.7f); studio.panels[0].size = new Vector2(.65f,.35f); studio.panels[0].intensity = 1; studio.softness = .22f; break;
                case 9: studio.panels[2].enabled = false; studio.panels[1].intensity = .6f; studio.softness = .24f; break;
                case 11: studio.panels[2].enabled = false; studio.panels[0].intensity = .5f; break;
            }
            return settings;
        }

        public static MatCapSettings GetStudioPreset(int index)
        {
            var settings = new MatCapSettings();
            settings.lighting.enabled = false;
            settings.reflections.enabled = true;
            settings.reflections.blendMode = BlendMode.Normal;
            var studio = settings.reflections.studio;
            studio.enabled = true;
            switch (index)
            {
                case 0:
                    studio.metalColor = new Color(.24f,.32f,.44f);
                    studio.shadowColor = new Color(.008f,.004f,.025f);
                    studio.iridescence = .95f; studio.distortion = .07f; studio.grain = .015f;
                    studio.panels[1].color = new Color(.5f,.8f,1f);
                    studio.panels[2].color = new Color(1f,.6f,.9f);
                    break;
                case 1:
                    studio.metalColor = new Color(.83f,.32f,.22f);
                    studio.shadowColor = new Color(.12f,.025f,.018f);
                    studio.distortion = .12f; studio.softness = .14f; studio.grain = .015f;
                    studio.panels[0].color = new Color(1f,.82f,.65f);
                    studio.panels[1].color = new Color(1f,.7f,.55f);
                    break;
                case 2:
                    studio.metalColor = new Color(.85f,.34f,.12f);
                    studio.shadowColor = new Color(.13f,.035f,.012f);
                    studio.softness = .55f; studio.satin = .9f; studio.grain = .025f;
                    studio.panels[0].shape = ReflectionShape.Softbox; studio.panels[0].size = new Vector2(1.6f,.85f);
                    studio.panels[0].intensity = 1.3f;
                    studio.panels[1].enabled = false;
                    studio.panels[2].size = new Vector2(1.4f,.65f);
                    studio.panels[2].color = new Color(1f,.6f,.27f);
                    break;
                case 3:
                    studio.metalColor = new Color(.12f,.42f,.62f);
                    studio.shadowColor = new Color(.018f,.09f,.12f);
                    studio.distortion = .36f; studio.softness = .22f; studio.grain = .025f; studio.satin = .6f;
                    studio.panels[0].shape = ReflectionShape.Softbox;
                    studio.panels[0].color = new Color(1f,.87f,.75f);
                    studio.panels[1].size = new Vector2(.6f,1.1f);
                    break;
                case 4:
                    studio.metalColor = new Color(.055f,.035f,.09f);
                    studio.shadowColor = new Color(.003f,.004f,.009f);
                    studio.iridescence = 1f; studio.distortion = .25f; studio.grain = .015f;
                    studio.panels[0].intensity = 1.5f; studio.panels[0].size = new Vector2(.35f,.2f);
                    studio.panels[1].color = new Color(.12f,1f,.7f);
                    studio.panels[2].color = new Color(.65f,.2f,1f);
                    break;
                case 5:
                    studio.metalColor = new Color(.8f,.66f,.44f);
                    studio.shadowColor = new Color(.14f,.11f,.1f);
                    studio.softness = .32f; studio.satin = .7f; studio.iridescence = .15f; studio.grain = .02f;
                    studio.panels[0].shape = ReflectionShape.Softbox;
                    studio.panels[0].size = new Vector2(1.2f,.65f);
                    studio.panels[1].color = new Color(.65f,.8f,1f);
                    break;
                case 6:
                    studio.metalColor = new Color(.035f,.06f,.12f);
                    studio.shadowColor = new Color(.004f,.007f,.02f);
                    studio.iridescence = .65f; studio.rotation = 25;
                    studio.panels[0].shape = ReflectionShape.Softbox;
                    studio.panels[0].color = new Color(.12f,1f,1f);
                    studio.panels[0].size = new Vector2(.25f,1.4f);
                    studio.panels[1].color = new Color(1f,.1f,.65f);
                    studio.panels[2].color = new Color(.55f,.3f,1f);
                    break;
                case 7:
                    studio.metalColor = new Color(.55f,.63f,.7f);
                    studio.shadowColor = new Color(.03f,.04f,.05f);
                    studio.distortion = .16f; studio.softness = .045f; studio.grain = .008f;
                    studio.panels[0].size = new Vector2(1.1f,.45f);
                    break;
            }
            switch (index)
            {
                case 0: studio.rotation = -20; studio.panels[1].shape = ReflectionShape.Ellipse; studio.panels[1].direction = new Vector3(1,.8f,.6f); break;
                case 1: studio.rotation = 40; studio.panels[0].direction = new Vector3(-.6f,.5f,1); studio.panels[2].shape = ReflectionShape.Ellipse; break;
                case 2: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].direction = new Vector3(0,1,.2f); studio.panels[2].direction = new Vector3(0,-1,.15f); break;
                case 3: studio.pattern = SurfacePattern.Fractal; studio.patternScale = 5; studio.patternStrength = .45f; studio.patternColor = new Color(.55f,.85f,1); studio.panels[0].shape = ReflectionShape.Ellipse; break;
                case 4: studio.pattern = SurfacePattern.Ridged; studio.patternStrength = .6f; studio.patternColor = new Color(.03f,.8f,.45f); studio.panels[1].enabled = false; studio.panels[2].enabled = false; break;
                case 5: studio.panels[0].shape = ReflectionShape.Ring; studio.panels[0].direction = Vector3.forward; studio.panels[0].size = new Vector2(1.8f,1.8f); studio.panels[1].enabled = false; studio.panels[2].enabled = false; break;
                case 6: studio.panels[0].rotation = -30; studio.panels[1].rotation = 35; studio.panels[2].enabled = false; break;
                case 7: studio.panels[0].shape = ReflectionShape.Crescent; studio.panels[0].direction = Vector3.forward; studio.panels[0].size = new Vector2(1.7f,1.7f); studio.panels[1].enabled = false; break;
            }
            switch (index)
            {
                case 0: studio.panels[2].intensity = .75f; studio.softness = .16f; break;
                case 1: studio.panels[2].intensity = .65f; studio.softness = .22f; break;
                case 3: studio.panels[0].intensity = .55f; studio.panels[0].size = new Vector2(1.4f,.9f); studio.panels[1].intensity = .35f; studio.panels[2].enabled = false; studio.softness = .45f; studio.distortion = .12f; break;
                case 4: studio.panels[0].intensity = .7f; studio.softness = .22f; break;
                case 5: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].direction = new Vector3(-.5f,.8f,.6f); studio.panels[0].size = new Vector2(1.3f,.8f); studio.panels[0].intensity = 1; studio.softness = .5f; break;
                case 6:
                    studio.metalColor = new Color(.16f,.21f,.3f);
                    studio.iridescence = .3f; studio.grain = .015f; studio.softness = .3f;
                    studio.panels[0].shape = ReflectionShape.Ellipse;
                    studio.panels[0].direction = new Vector3(-.5f,.8f,1);
                    studio.panels[0].size = new Vector2(.9f,.25f); studio.panels[0].rotation = -20;
                    studio.panels[0].color = new Color(.25f,.9f,1); studio.panels[0].intensity = .8f;
                    studio.panels[1].shape = ReflectionShape.Ellipse;
                    studio.panels[1].direction = new Vector3(.8f,.1f,1);
                    studio.panels[1].size = new Vector2(.25f,.8f); studio.panels[1].rotation = 15;
                    studio.panels[1].color = new Color(1,.2f,.55f); studio.panels[1].intensity = 1.1f;
                    break;
                case 7: studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].direction = new Vector3(-.5f,.8f,1); studio.panels[0].size = new Vector2(.9f,.4f); studio.panels[0].intensity = 1.1f; studio.panels[2].enabled = false; studio.softness = .24f; studio.distortion = .035f; break;
            }
            return settings;
        }

        public static MatCapSettings GetPatternPreset(int index)
        {
            var settings = GetStudioPreset(index == 3 ? 4 : 3);
            var studio = settings.reflections.studio;
            studio.panels[0].shape = ReflectionShape.Ellipse;
            studio.panels[0].direction = new Vector3(-.6f,.8f,1);
            studio.panels[0].size = new Vector2(.4f,.35f);
            studio.panels[1].enabled = false;
            studio.panels[2].enabled = false;
            studio.distortion = .05f;
            studio.grain = .015f;
            studio.patternSeed = index * 7.13f;
            switch (index)
            {
                case 0:
                    studio.pattern = SurfacePattern.Nacre; studio.patternScale = 5; studio.patternStrength = .75f;
                    studio.metalColor = new Color(.025f,.28f,.18f); studio.patternColor = new Color(.1f,.55f,.38f);
                    studio.iridescence = .35f; studio.softness = .25f; studio.panels[0].intensity = .45f;
                    break;
                case 1:
                    studio.pattern = SurfacePattern.Crystal; studio.patternScale = 14; studio.patternStrength = .85f;
                    studio.patternWarp = .35f; studio.patternGlow = .65f; studio.patternDetail = 6;
                    studio.metalColor = new Color(.035f,.18f,.36f); studio.patternColor = new Color(.3f,.8f,1);
                    studio.iridescence = .08f; studio.grain = .005f; studio.softness = .2f;
                    studio.panels[0].shape = ReflectionShape.Ellipse; studio.panels[0].intensity = .8f;
                    studio.panels[0].size = new Vector2(.7f,.2f);
                    break;
                case 2:
                    studio.pattern = SurfacePattern.Fractal; studio.patternScale = 4; studio.patternStrength = .9f;
                    studio.metalColor = new Color(.04f,.025f,.12f); studio.patternColor = new Color(.45f,.15f,.8f);
                    studio.iridescence = .45f; studio.panels[0].intensity = .5f;
                    break;
                case 3:
                    studio.pattern = SurfacePattern.Smooth; studio.iridescence = .08f;
                    studio.metalColor = new Color(.025f,.05f,.08f); studio.shadowColor = new Color(.001f,.002f,.004f);
                    studio.panels[0].intensity = .75f; studio.softness = .12f; studio.satin = 1;
                    break;
            }
            return settings;
        }

        public static MatCapSettings GetExpressivePreset(int index)
        {
            var settings = GetPatternPreset(2);
            var studio = settings.reflections.studio;
            studio.pattern = (SurfacePattern)(6 + index);
            studio.patternStrength = 1;
            studio.patternScale = index == 0 ? 2 : 6;
            studio.patternGlow = .8f;
            studio.iridescence = .1f;
            studio.patternWarp = .25f;
            studio.patternDetail = 6;
            studio.panels[0].intensity = .5f;
            studio.metalColor = new Color(.025f,.025f,.06f);
            studio.patternColor = new Color(.4f,.12f,1);
            switch (index)
            {
                case 0:
                    studio.patternSeed = 24.36f; studio.patternScale = 1.15f; studio.wrapPattern = true;
                    studio.patternWarp = .08f; studio.patternRotation = 0; studio.patternContrast = 1.2f;
                    studio.metalColor = new Color(.24f,.025f,.5f); studio.patternColor = new Color(.08f,.75f,1);
                    studio.iridescence = 0; studio.panels[0].intensity = .15f;
                    break;
                case 1: studio.patternColor = new Color(.03f,.85f,.65f); studio.patternScale = 3; studio.patternRotation = 25; break;
                case 2: studio.patternColor = new Color(1,.16f,.005f); studio.metalColor = new Color(.035f,.025f,.02f); studio.patternScale = 9; studio.patternGlow = 2; break;
                case 3: studio.patternColor = new Color(.015f,.5f,1); studio.metalColor = new Color(.12f,.01f,.045f); studio.patternScale = 12; studio.patternWarp = 0; break;
                case 4: studio.iridescence = .8f; studio.patternColor = new Color(.9f,.3f,.8f); studio.patternScale = 16; studio.patternWarp = .15f; break;
                case 5:
                    studio.patternColor = new Color(.45f,.85f,1); studio.metalColor = new Color(.015f,.045f,.12f);
                    studio.patternScale = 8; studio.patternWarp = .06f; studio.patternRotation = 22;
                    studio.patternGlow = 1.1f; studio.iridescence = .03f; studio.softness = .12f;
                    studio.panels[0].intensity = .45f;
                    break;
                case 6:
                    studio.patternColor = new Color(1,.18f,.55f); studio.metalColor = new Color(.35f,.025f,.12f);
                    studio.shadowColor = new Color(.04f,.003f,.018f); studio.patternScale = 7;
                    studio.patternWarp = .35f; studio.patternGlow = .65f; studio.patternStrength = .8f;
                    studio.iridescence = .08f; studio.softness = .22f; studio.satin = .1f;
                    studio.panels[0].intensity = .8f; studio.panels[0].size = new Vector2(1.2f,.3f);
                    break;
            }
            settings.postProcess.enabled = true;
            settings.postProcess.bloomEnabled = true;
            settings.postProcess.bloomStrength = .35f;
            settings.postProcess.bloomThreshold = .55f;
            return settings;
        }

        public static string[] GetPresetNames()
        {
            return new string[]
            {
                "Default", "Gold", "Gunmetal", "Stainless", "Stone",
                "Matte Red", "Chrome", "Cyan Glass",
                "Iridescent Bubble", "Speckled Metal", "Dark Emerald", "Marble",
                "Prismatic Chrome", "Rose Copper", "Satin Copper", "Glacier Frost", "Black Opal", "Champagne Pearl", "Neon Titanium", "Liquid Silver", "Abalone Pearl", "Crystal Ice", "Fractal Nebula", "Black Pearl", "Julia Aurora", "Emerald Flow", "Molten Rock", "Neon Hex", "Dragon Scales", "Ice Shards", "Petal Glass"
            };
        }

        public static MatCapSettings GetPresetByIndex(int index)
        {
            switch (index)
            {
                case 0: return GetRefinedOriginal(0);
                case 1: return GetRefinedOriginal(1);
                case 2: return GetRefinedOriginal(2);
                case 3: return GetRefinedOriginal(3);
                case 4: return GetRefinedOriginal(4);
                case 5: return GetRefinedOriginal(5);
                case 6: return GetRefinedOriginal(6);
                case 7: return GetRefinedOriginal(7);
                case 8: return GetRefinedOriginal(8);
                case 9: return GetRefinedOriginal(9);
                case 10: return GetRefinedOriginal(10);
                case 11: return GetRefinedOriginal(11);
                case 24: case 25: case 26: case 27: case 28: case 29: case 30: return GetExpressivePreset(index - 24);
                case 20: case 21: case 22: case 23: return GetPatternPreset(index - 20);
                case 12: case 13: case 14: case 15: case 16: case 17: case 18: case 19: return GetStudioPreset(index - 12);
                default: return GetDefault();
            }
        }

        public static bool PresetRequiresTextures(int index)
        {
            return index == 8 || index == 9 || index == 11; 
        }

        public static void GetPresetTextureRequirements(int index, out bool needsEnvMap, out bool needsNoise, out bool needsBaseTexture)
        {
            needsEnvMap = false;
            needsNoise = false;
            needsBaseTexture = false;

            switch (index)
            {
                case 8: 
                    needsEnvMap = true;
                    break;
                case 9: 
                    needsNoise = true;
                    break;
                case 11: 
                    needsBaseTexture = true;
                    break;
            }
        }
    }
    #endregion
}
