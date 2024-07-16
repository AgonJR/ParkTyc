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
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FronkonGames.Artistic.OilPaint
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Settings. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class OilPaint
  {
    /// <summary> Settings. </summary>
    [Serializable]
    public sealed class Settings
    {
      public Settings() => ResetDefaultValues();

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Common settings.

      /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
      /// <remarks> An effect with Intensity equal to 0 will not be executed. </remarks>
      public float intensity = 1.0f;
      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Oil Paint settings.

      /// <summary> Kuwahara algorithm. </summary>
      public Algorithms kuwahara = Algorithms.KuwaharaBasic;

      /// <summary> [1, 4]. Default 1. </summary>
      public int passes = 1;

      /// <summary> Size of the kuwahara filter kernel [1, 20]. Default 10. </summary>
      public int radius = 10;

      /// <summary> Size of the gaussian blur kernel for eigenvectors [1, 6]. Default 2. </summary>
      public int blur = 2;

      /// <summary> Adjusts sharpness of the color segments [0, 18]. Default 8. </summary>
      public float sharpness = 8.0f;

      /// <summary> Adjusts hardness of the color segments [1, 100]. Default 8. </summary>
      public float hardness = 8.0f;

      /// <summary> How extreme the angle of the kernel is [0.01, 2]. Default 1. </summary>
      public float alpha = 1.0f;

      /// <summary> How much sectors overlap with each other [0.01, 2]. Default 0.58. </summary>
      public float zeroCrossing = 0.58f;

      /// <summary> Detail algorithm used: None (default), Sharpen or Emboss. </summary>
      public Detail detail = Detail.None;

      /// <summary> Strength of the detail [0, 1]. Default 1. Only valid if Detail it not None. </summary>
      public float detailStrength = 1.0f;

      /// <summary> Strength of the emboss effect [0, 20]. Default 5. Only valid if Detail it Emboss. </summary>
      public float embossStrength = 5.0f;

      /// <summary> Angle of the emboss effect [0, 360]. Default 0. Only valid if Detail it Emboss. </summary>
      public float embossAngle = 0.0f;

      /// <summary> Strength of the Water Color effect [0, 1]. Default 0. </summary>
      public float waterColor = 0.0f;

      /// <summary> Activate the use of the depth buffer to improve the effect. Default False. </summary>
      /// <remarks> The camera must have the 'Depth Texture' field set to 'On'. </remarks>
      public bool processDepth = false;

      /// <summary> Change rate at which kernel sizes change between depths. Only valid if process depth is on. </summary>
      public AnimationCurve depthCurve = DefaultDepthCurve;

      /// <summary> Factor to concentrate the depth curve [0, 1]. Default 1. </summary>
      public float depthPower = 1.0f;

      /// <summary> Affect the sky? Default True. </summary>
      /// <remarks> In night skies (with stars), it is recommended to deactivate this option. </remarks>
      public bool sampleSky = true;

      /// <summary> To view the depth curve in the Editor. </summary>
      /// <remarks> Only works in the Editor. </remarks>
      public bool viewDepthCurve = false;

      /// <summary> Final render size, if it is less than 1 it will be faster but more blurred [0, 1]. Default 1. </summary>
      public float renderSize = 1.0f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Color settings.

      /// <summary> Brightness [-1.0, 1.0]. Default 0. </summary>
      public float brightness = 0.0f;

      /// <summary> Contrast [0.0, 10.0]. Default 1. </summary>
      public float contrast = 1.0f;

      /// <summary> Gamma [0.1, 10.0]. Default 1. </summary>
      public float gamma = 1.0f;

      /// <summary> The color wheel [0.0, 1.0]. Default 0. </summary>
      public float hue = 0.0f;

      /// <summary> Intensity of a colors [0.0, 2.0]. Default 1. </summary>
      public float saturation = 1.0f;
      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Advanced settings.

      /// <summary> Does it affect the Scene View? </summary>
      public bool affectSceneView = false;

      /// <summary> Filter mode. Default Bilinear. </summary>
      public FilterMode filterMode = FilterMode.Bilinear;

      /// <summary> Render pass injection. Default BeforeRenderingPostProcessing. </summary>
      public RenderPassEvent whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;

      /// <summary> Set to true to ignore delta time scaling. </summary>
      public bool ignoreDeltaTimeScale = false;

      /// <summary> Enable render pass profiling. </summary>
      public bool enableProfiling = false;
      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      public static readonly AnimationCurve DefaultDepthCurve = new()
      {
        keys = new Keyframe[]
        {
          new(0.0f, 1.0f),
          new(0.75f, 1.0f),
          new(1.0f, 0.25f),
        }
      };

      // Internal use.
      public bool forceCurveTextureUpdate;

      /// <summary> Reset to default values. </summary>
      public void ResetDefaultValues()
      {
        intensity = 1.0f;

        kuwahara = Algorithms.KuwaharaBasic;
        passes = 1;
        radius = 10;
        blur = 2;
        sharpness = 8.0f;
        hardness = 8.0f;
        alpha = 1.0f;
        zeroCrossing = 0.58f;
        detail = Detail.None;
        detailStrength = 1.0f;
        embossStrength = 5.0f;
        embossAngle = 0.0f;
        waterColor = 0.0f;
        renderSize = 1.0f;
        processDepth = false;
        sampleSky = true;
        depthCurve = new AnimationCurve() { keys = DefaultDepthCurve.keys };
        depthPower = 1.0f;
        viewDepthCurve = false;

        brightness = 0.0f;
        contrast = 1.0f;
        gamma = 1.0f;
        hue = 0.0f;
        saturation = 1.0f;

        affectSceneView = false;
        filterMode = FilterMode.Bilinear;
        whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
        ignoreDeltaTimeScale = false;
        enableProfiling = false;
      }
    }
  }
}
