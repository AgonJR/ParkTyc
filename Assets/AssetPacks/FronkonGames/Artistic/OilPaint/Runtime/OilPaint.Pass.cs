////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FronkonGames.Artistic.OilPaint
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class OilPaint
  {
    private sealed class RenderPass : ScriptableRenderPass
    {
      private readonly Settings settings;

      private RenderTargetIdentifier colorBuffer;

      private readonly List<int> temporalRTIDs = new();
      private int countIDs;

      private readonly Material material;

      private AnimationCurve curve = null;
      private Texture2D curveTexture;

      private static readonly ProfilerMarker ProfilerMarker = new($"{Constants.Asset.AssemblyName}.Pass.Execute");

      private const string CommandBufferName = Constants.Asset.AssemblyName;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int DeltaTime = Shader.PropertyToID("_DeltaTime");

        internal static readonly int Radius = Shader.PropertyToID("_Radius");
        internal static readonly int Sharpness = Shader.PropertyToID("_Sharpness");
        internal static readonly int Hardness = Shader.PropertyToID("_Hardness");
        internal static readonly int Alpha = Shader.PropertyToID("_Alpha");
        internal static readonly int ZeroCrossing = Shader.PropertyToID("_ZeroCrossing");
        internal static readonly int Blur = Shader.PropertyToID("_Blur");
        internal static readonly int DetailStrength = Shader.PropertyToID("_DetailStrength");
        internal static readonly int EmbossStrength = Shader.PropertyToID("_EmbossStrength");
        internal static readonly int EmbossAngle = Shader.PropertyToID("_EmbossAngle");
        internal static readonly int WaterColor = Shader.PropertyToID("_WaterColorStrength");
        internal static readonly int WaterColorBlend = Shader.PropertyToID("_WaterColorBlend");
        internal static readonly int SampleSky = Shader.PropertyToID("_SampleSky");
        internal static readonly int DepthCurve = Shader.PropertyToID("_DepthCurve");
        internal static readonly int DepthPower = Shader.PropertyToID("_DepthPower");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      private static class Textures
      {
        internal static readonly int TensorTexture = Shader.PropertyToID("_TensorTex");
        internal static readonly int CurveTexture = Shader.PropertyToID("_CurveTex");
      }

      private static class Keywords
      {
        internal static readonly string KawaharaLow = "KAWAHARA_LOW";
        internal static readonly string KawaharaMedium = "KAWAHARA_MEDIUM";
        internal static readonly string KawaharaHigh = "KAWAHARA_HIGH";
        internal static readonly string KawaharaCustom = "KAWAHARA_CUSTOM";
        internal static readonly string SymmetricNearestNeighbour = "SYMMETRIC_NEAREST_NEIGHBOUR";

        internal static readonly string DetailSharpen = "DETAIL_SHARPEN";
        internal static readonly string DetailEmboss = "DETAIL_EMBOSS";

        internal static readonly string WaterColor = "WATER_COLOR";

        internal static readonly string ProcessDepth = "PROCESS_DEPTH";
        internal static readonly string ViewDepth = "VIEW_DEPTH";
      }

      /// <summary> Shader passes. </summary>
      private enum Passes
      {
        KuwaharaBasic             = 0,
        KuwaharaGeneralized       = 1,
        KuwaharaDirectional       = 2,
        Tensor                    = 3,
        BlurHorizontal            = 4,
        BlurVertical              = 5,
        KuwaharaAnisotropic       = 6,
        TomitaTsuji               = 7,
        SymmetricNearestNeighbour = 8,
        Detail                    = 9,
      }

      private int GetTemporalRT(CommandBuffer cmd, int width, int height, FilterMode filter, int depthBuffer = 0, RenderTextureFormat format = RenderTextureFormat.ARGB32)
      {
        int id = Shader.PropertyToID($"_TemporalBuffer{countIDs++}{Constants.Asset.Name}");
        temporalRTIDs.Add(id);

        cmd.GetTemporaryRT(id, width, height, depthBuffer, filter, format);

        return id;
      }

      private void ReleaseTemporalRT(CommandBuffer cmd, int id)
      {
        if (temporalRTIDs.Contains(id) == true)
        {
          cmd.ReleaseTemporaryRT(id);
          temporalRTIDs.Remove(id);
        }
      }

      /// <summary> Update curve texture. </summary>
      public void UpdateCurveTexture()
      {
        curve = settings.depthCurve;

        const int width = 256;
        const int height = 4;
        curveTexture = new Texture2D(width, height, TextureFormat.RGB24, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };

        const float inv = 1.0f / (width - 1);
        for (int y = 0; y < height; ++y)
        {
          for (int x = 0; x < width; ++x)
            curveTexture.SetPixel(x, y, Color.white * curve.Evaluate(x * inv));
        }

        curveTexture.Apply();

        settings.forceCurveTextureUpdate = false;
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass(Settings settings)
      {
        this.settings = settings;

        string shaderPath = $"Shaders/{Constants.Asset.ShaderName}_URP";
        Shader shader = Resources.Load<Shader>(shaderPath);
        if (shader != null)
        {
          if (shader.isSupported == true)
            material = CoreUtils.CreateEngineMaterial(shader);
          else
            Log.Warning($"'{shaderPath}.shader' not supported");
        }
      }

      /// <inheritdoc/>
      public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
      {
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

#if UNITY_2022_1_OR_NEWER
        colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
#else
        colorBuffer = renderingData.cameraData.renderer.cameraColorTarget;
#endif        
      }

      /// <inheritdoc/>
      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
      {
        if (material == null ||
            renderingData.postProcessingEnabled == false ||
            settings.intensity == 0.0f ||
            settings.affectSceneView == false && renderingData.cameraData.isSceneViewCamera == true)
          return;

        CommandBuffer cmd = CommandBufferPool.Get(CommandBufferName);

        if (settings.enableProfiling == true)
          ProfilerMarker.Begin();

        material.shaderKeywords = null;
        material.SetFloat(ShaderIDs.Intensity, settings.intensity);
        material.SetFloat(ShaderIDs.DeltaTime, settings.ignoreDeltaTimeScale == true ? Time.unscaledDeltaTime : Time.deltaTime);

        material.SetFloat(ShaderIDs.Brightness, settings.brightness);
        material.SetFloat(ShaderIDs.Contrast, settings.contrast);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / settings.gamma);
        material.SetFloat(ShaderIDs.Hue, settings.hue);
        material.SetFloat(ShaderIDs.Saturation, settings.saturation);

        float renderSize = Mathf.Max(0.01f, settings.renderSize);

        material.SetInt(ShaderIDs.Radius, settings.radius);

        if (settings.processDepth == true)
        {
          material.EnableKeyword(Keywords.ProcessDepth);

          if (settings.forceCurveTextureUpdate == true || curve == null || curve != settings.depthCurve)
            UpdateCurveTexture();

#if UNITY_EDITOR
          if (settings.viewDepthCurve == true)
            material.EnableKeyword(Keywords.ViewDepth);
#endif
          material.SetTexture(Textures.CurveTexture, curveTexture);
          material.SetFloat(ShaderIDs.DepthPower, settings.depthPower * 0.1f);
          material.SetInt(ShaderIDs.SampleSky, settings.sampleSky == true ? 1 : 0);
        }

        int lastRenderTarget = -1;

        switch (settings.kuwahara)
        {
          case Algorithms.KuwaharaBasic:
          {
            int[] passRTIDs = new int[settings.passes];
            for (int i = 0; i < settings.passes; ++i)
                passRTIDs[i] = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);

            for (int i = 0; i < settings.passes; ++i)
            {
              cmd.Blit(i == 0 ? colorBuffer : passRTIDs[i - 1], passRTIDs[i], material, (int)Passes.KuwaharaBasic);
              lastRenderTarget = passRTIDs[i];
            }

            for (int i = 0; i < settings.passes; ++i)
            {
              if (passRTIDs[i] != lastRenderTarget)
                ReleaseTemporalRT(cmd, passRTIDs[i]);
            }
          }
          break;
          case Algorithms.KuwaharaGeneralized:
          {
            material.SetFloat(ShaderIDs.Sharpness, settings.sharpness);
            material.SetFloat(ShaderIDs.Hardness, settings.hardness);
            material.SetFloat(ShaderIDs.ZeroCrossing, settings.zeroCrossing);

            int[] passRTIDs = new int[settings.passes];
            for (int i = 0; i < settings.passes; ++i)
                passRTIDs[i] = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);

            for (int i = 0; i < settings.passes; ++i)
            {
              cmd.Blit(i == 0 ? colorBuffer : passRTIDs[i - 1], passRTIDs[i], material, (int)Passes.KuwaharaGeneralized);
              lastRenderTarget = passRTIDs[i];
            }

            for (int i = 0; i < settings.passes; ++i)
            {
              if (passRTIDs[i] != lastRenderTarget)
                ReleaseTemporalRT(cmd, passRTIDs[i]);
            }
          }
          break;
          case Algorithms.KuwaharaDirectional:
          {
            int[] passRTIDs = new int[settings.passes];
            for (int i = 0; i < settings.passes; ++i)
                passRTIDs[i] = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);

            for (int i = 0; i < settings.passes; ++i)
            {
              cmd.Blit(i == 0 ? colorBuffer : passRTIDs[i - 1], passRTIDs[i], material, (int)Passes.KuwaharaDirectional);
              lastRenderTarget = passRTIDs[i];
            }

            for (int i = 0; i < settings.passes; ++i)
            {
              if (passRTIDs[i] != lastRenderTarget)
                ReleaseTemporalRT(cmd, passRTIDs[i]);
            }
          }
          break;          
          case Algorithms.KuwaharaAnisotropic:
          {
            int rt0 = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);
            material.SetInt(ShaderIDs.Blur, settings.blur);
            material.SetFloat(ShaderIDs.Sharpness, settings.sharpness);
            material.SetFloat(ShaderIDs.Hardness, settings.hardness);
            material.SetFloat(ShaderIDs.Alpha, settings.alpha);
            material.SetFloat(ShaderIDs.ZeroCrossing, settings.zeroCrossing);

            int rt1 = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);
            int rt2 = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);
            int rt3 = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);

            cmd.Blit(colorBuffer, rt0, material, (int)Passes.Tensor);
            cmd.Blit(rt0, rt1, material, (int)Passes.BlurHorizontal);
            cmd.Blit(rt1, rt2, material, (int)Passes.BlurVertical);

            cmd.SetGlobalTexture(Textures.TensorTexture, rt2);

            int[] passRTIDs = new int[settings.passes];
            for (int i = 0; i < settings.passes; ++i)
              passRTIDs[i] = i == 0 ? rt3 : GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);

            for (int i = 0; i < settings.passes; ++i)
            {
              cmd.Blit(i == 0 ? colorBuffer : passRTIDs[i - 1], passRTIDs[i], material, (int)Passes.KuwaharaAnisotropic);
              lastRenderTarget = passRTIDs[i];
            }

            for (int i = 0; i < settings.passes - 1; ++i)
            {
              if (passRTIDs[i] != lastRenderTarget)
                ReleaseTemporalRT(cmd, passRTIDs[i]);
            }

            ReleaseTemporalRT(cmd, rt0);
            ReleaseTemporalRT(cmd, rt1);
            ReleaseTemporalRT(cmd, rt2);
          }
          break;
          case Algorithms.TomitaTsuji:
          {
            int[] passRTIDs = new int[settings.passes];
            for (int i = 0; i < settings.passes; ++i)
                passRTIDs[i] = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);

            for (int i = 0; i < settings.passes; ++i)
            {
              cmd.Blit(i == 0 ? colorBuffer : passRTIDs[i - 1], passRTIDs[i], material, (int)Passes.TomitaTsuji);
              lastRenderTarget = passRTIDs[i];
            }

            for (int i = 0; i < settings.passes; ++i)
            {
              if (passRTIDs[i] != lastRenderTarget)
                ReleaseTemporalRT(cmd, passRTIDs[i]);
            }
          }
          break;
          case Algorithms.SymmetricNearestNeighbour:
          {
            int[] passRTIDs = new int[settings.passes];
            for (int i = 0; i < settings.passes; ++i)
                passRTIDs[i] = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);

            for (int i = 0; i < settings.passes; ++i)
            {
              cmd.Blit(i == 0 ? colorBuffer : passRTIDs[i - 1], passRTIDs[i], material, (int)Passes.SymmetricNearestNeighbour);
              lastRenderTarget = passRTIDs[i];
            }

            for (int i = 0; i < settings.passes; ++i)
            {
              if (passRTIDs[i] != lastRenderTarget)
                ReleaseTemporalRT(cmd, passRTIDs[i]);
            }
          }
          break;
        }

        if (settings.detail != Detail.None || settings.waterColor > 0.0f)
        {
          int deatilRTID = GetTemporalRT(cmd, (int)(Screen.width * renderSize), (int)(Screen.height * renderSize), settings.filterMode);

          material.SetFloat(ShaderIDs.DetailStrength, settings.detailStrength);

          switch (settings.detail)
          {
            case Detail.None: break;

            case Detail.Sharpen:
              material.EnableKeyword(Keywords.DetailSharpen);
              break;

            case Detail.Emboss:
              material.EnableKeyword(Keywords.DetailEmboss);
              material.SetFloat(ShaderIDs.EmbossStrength, settings.embossStrength);
              material.SetFloat(ShaderIDs.EmbossAngle, settings.embossAngle * Mathf.Deg2Rad);
              break;
          }

          if (settings.waterColor > 0.0f)
          {
            material.EnableKeyword(Keywords.WaterColor);
            material.SetFloat(ShaderIDs.WaterColor, settings.waterColor);
          }

          cmd.Blit(lastRenderTarget, deatilRTID, material, (int)Passes.Detail);
          cmd.Blit(deatilRTID, colorBuffer);

          ReleaseTemporalRT(cmd, deatilRTID);
        }
        else
          cmd.Blit(lastRenderTarget, colorBuffer);

        ReleaseTemporalRT(cmd, lastRenderTarget);

        temporalRTIDs.Clear();
        countIDs = 0;

        if (settings.enableProfiling == true)
          ProfilerMarker.End();

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
      }
    }
  }
}
