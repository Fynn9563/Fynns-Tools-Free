using System;
using UnityEngine;
using FynnsToolkit;
namespace FynnsTextureTools {
 public static class NormalPipeline {
  public static RenderTexture Render(Texture2D input, Texture2NormalmapPreset p) {
   if (!input || !p) throw new ArgumentNullException("input");
   if (!SystemInfo.supportsComputeShaders) throw new NotSupportedException("Compute shaders are unavailable.");
   int w = input.width, h = input.height;
   TextureIO.ValidateSize(w, h);
   var previous = RenderTexture.active; bool srgb = GL.sRGBWrite;
   RenderTexture height = null, filtered = null, output = null; Material material = null;
   ComputeShader shader = null;
   try {
    GL.sRGBWrite = false;
    var heightShader = Resources.Load<Shader>("T2N_HeightFromChannel");
    if (!heightShader) throw new InvalidOperationException("Height shader is missing.");
    material = new Material(heightShader) { hideFlags = HideFlags.HideAndDontSave };
    material.SetFloat("_Channel", p.selectedChannel);
    height = Create(w, h, RenderTextureFormat.RHalf);
    Graphics.Blit(input, height, material);
    if (p.applyPreFilter) {
     var filter = Resources.Load<ComputeShader>("T2N_PreprocessCS");
     if (!filter) throw new InvalidOperationException("Pre-filter shader is missing.");
     filtered = Create(w, h, RenderTextureFormat.RHalf);
     int fk = filter.FindKernel("CSMain");
     filter.SetInt("width", w); filter.SetInt("height", h);
     filter.SetInt("kernelSize", Mathf.Clamp(p.preFilterKernelSize | 1, 3, 11));
     filter.SetFloat("sigma", Mathf.Max(0.01f, p.preFilterSigma));
     filter.SetTexture(fk, "Input", height); filter.SetTexture(fk, "Result", filtered);
     filter.Dispatch(fk, Mathf.CeilToInt(w / 8f), Mathf.CeilToInt(h / 8f), 1);
     height.Release(); UnityEngine.Object.DestroyImmediate(height); height = filtered; filtered = null;
    }
    output = Create(w, h, RenderTextureFormat.ARGB32);
            if (shader == null)
                shader = Resources.Load<ComputeShader>("T2N_NormalmapCS");
            int kernel = shader.FindKernel("CSMain");

            shader.SetInt("width", w);
            shader.SetInt("height", h);
            shader.SetInt("conversionAlgorithm", (int)p.conversionAlgorithm);
            shader.SetFloat("heightScale", p.heightScale);
            shader.SetBool("invertX", p.invertX);
            shader.SetBool("invertY", p.invertY);

            shader.SetFloat("sobelInnerWeight", p.sobelInnerWeight);
            shader.SetFloat("sobelOuterWeight", p.sobelOuterWeight);
            shader.SetFloat("prewittWeight", p.prewittWeight);
            shader.SetFloat("scharrLowWeight", p.scharrLowWeight);
            shader.SetFloat("scharrHighWeight", p.scharrHighWeight);
            shader.SetFloat("laplacianCenterWeight", p.laplacianCenterWeight);
            shader.SetFloat("laplacianNeighborWeight", p.laplacianNeighborWeight);
            shader.SetFloat("robertsScale", p.robertsScale);
            shader.SetFloat("kirschScale", p.kirschScale);
            shader.SetFloat("freiChenScale", p.freiChenScale);
            shader.SetFloat("multiScaleWeight1", p.multiScaleWeight1);
            shader.SetFloat("multiScaleWeight2", p.multiScaleWeight2);
            shader.SetFloat("contrastEnhancementFactor", p.contrastEnhancementFactor);
            shader.SetFloat("cannyLowThreshold", p.cannyLowThreshold);
            shader.SetFloat("cannyHighThreshold", p.cannyHighThreshold);
            shader.SetFloat("bilateralSigmaSpatial", p.bilateralSigmaSpatial);
            shader.SetFloat("bilateralSigmaRange", p.bilateralSigmaRange);
            shader.SetFloat("nlmH", p.nlmH);

            shader.SetTexture(kernel, "HeightTex", height);
            shader.SetTexture(kernel, "Result", output);

            shader.Dispatch(kernel, Mathf.CeilToInt(w / 8f), Mathf.CeilToInt(h / 8f), 1);


    var result = output; output = null; return result;
   } finally {
    RenderTexture.active = previous; GL.sRGBWrite = srgb;
    Dispose(height); Dispose(filtered); Dispose(output);
    if (material) UnityEngine.Object.DestroyImmediate(material);
   }
  }
  static RenderTexture Create(int w, int h, RenderTextureFormat format) {
   var result = new RenderTexture(w, h, 0, format, RenderTextureReadWrite.Linear) { enableRandomWrite = true, hideFlags = HideFlags.HideAndDontSave };
   result.Create(); return result;
  }
  public static void Dispose(RenderTexture texture) { if (texture) { texture.Release(); UnityEngine.Object.DestroyImmediate(texture); } }
  public static void Save(Texture2D input, Texture2NormalmapPreset settings, string path) {
   TextureIO.ValidateOutput(path);
   RenderTexture rt = null; Texture2D tex = null;
   try { rt = Render(input, settings); tex = TextureIO.Read(rt, rt.width, rt.height, true); TextureIO.Write(tex.EncodeToPNG(), path, true, true); }
   finally { if (tex) UnityEngine.Object.DestroyImmediate(tex); Dispose(rt); }
  }
 }
}
