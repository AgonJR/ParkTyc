// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Shader "Hidden/Fronkon Games/Artistic/Oil Paint URP"
{
  Properties
  {
    _MainTex("Main Texture", 2D) = "white" {}
  }

  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
    }
    LOD 100
    ZTest Always ZWrite Off Cull Off

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Kuwahara Basic)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile ___ PROCESS_DEPTH

      #include "Artistic.hlsl"
#if PROCESS_DEPTH
      #include "ProcessDepth.hlsl"
#endif
      #include "KuwaharaBasic.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;
        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = (half4)0.0;

#if PROCESS_DEPTH
        const float depth = SampleLinear01Depth(uv);

        if (_SampleSky == 1)
          pixel = KuwaharaBasic(uv, depth);
        else
          pixel = depth < 0.98 ? KuwaharaBasic(uv, depth) : color;
#else
        pixel = KuwaharaBasic(uv, 0.0);
#endif

#if ARTISTIC_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Kuwahara Generalized)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile ___ PROCESS_DEPTH

      #include "Artistic.hlsl"
#if PROCESS_DEPTH
      #include "ProcessDepth.hlsl"
#endif
      #include "KuwaharaGeneralized.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;
        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = (half4)0.0;

#if PROCESS_DEPTH
        const float depth = SampleLinear01Depth(uv);

        if (_SampleSky == 1)
          pixel = KuwaharaGeneralized(uv, depth);
        else
          pixel = depth < 0.98 ? KuwaharaGeneralized(uv, depth) : color;
#else
        pixel = KuwaharaGeneralized(uv, 0.0);
#endif

#if ARTISTIC_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Kuwahara Directional)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile ___ PROCESS_DEPTH

      #include "Artistic.hlsl"
#if PROCESS_DEPTH
      #include "ProcessDepth.hlsl"
#endif
      #include "KuwaharaDirectional.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;
        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = (half4)0.0;

#if PROCESS_DEPTH
        const float depth = SampleLinear01Depth(uv);

        if (_SampleSky == 1)
          pixel = KuwaharaDirectional(uv, depth);
        else
          pixel = depth < 0.98 ? KuwaharaDirectional(uv, depth) : color;
#else
        pixel = KuwaharaDirectional(uv, 0.0);
#endif

#if ARTISTIC_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Tensor)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash

      #include "Artistic.hlsl"
      #include "KuwaharaAnisotropic.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;

        return CalculateTensors(uv);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Blur Horizontal)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash

      #include "Artistic.hlsl"
      #include "KuwaharaAnisotropic.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;

        return BlurHorizontal(uv);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Blur Vertical & Anisotropy)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash

      #include "Artistic.hlsl"
      #include "KuwaharaAnisotropic.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;

        half4 pixel = (half4)0.0;
        float kernelSum = 0.0;

        pixel.rgb = BlurVertical(uv);

        return CalculateAnisotropy(pixel.rgb);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Kuwahara Anisotropic)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile ___ PROCESS_DEPTH

      #include "Artistic.hlsl"
#if PROCESS_DEPTH
      #include "ProcessDepth.hlsl"
#endif
      #include "KuwaharaAnisotropic.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;
        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = (half4)0.0;

#if PROCESS_DEPTH
        const float depth = SampleLinear01Depth(uv);
        if (_SampleSky == 1)
          pixel = Anisotropic(uv, depth);
        else
          pixel = depth < 0.98 ? Anisotropic(uv, depth) : color;
#else
        pixel = Anisotropic(uv, 0.0);
#endif

#if ARTISTIC_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Tomita Tsuji)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile ___ PROCESS_DEPTH

      #include "Artistic.hlsl"
#if PROCESS_DEPTH
      #include "ProcessDepth.hlsl"
#endif
      #include "TomitaTsuji.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;
        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = (half4)0.0;

#if PROCESS_DEPTH
        const float depth = SampleLinear01Depth(uv);

        if (_SampleSky == 1)
          pixel = TomitaTsuji(uv, depth);
        else
          pixel = depth < 0.98 ? TomitaTsuji(uv, depth) : color;
#else
        pixel = TomitaTsuji(uv, 0.0);
#endif

#if ARTISTIC_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Symmetric Nearest Neighbour)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile ___ PROCESS_DEPTH

      #include "Artistic.hlsl"
#if PROCESS_DEPTH
      #include "ProcessDepth.hlsl"
#endif
      #include "SymmetricNearestNeighbour.hlsl"

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;
        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = (half4)0.0;

#if PROCESS_DEPTH
        const float depth = SampleLinear01Depth(uv);

        if (_SampleSky == 1)
          pixel = SymmetricNearestNeighbour(color.rgb, depth, uv);
        else
          pixel = depth < 0.98 ? SymmetricNearestNeighbour(color.rgb, depth, uv) : color;
#else
        pixel = SymmetricNearestNeighbour(color.rgb, 0.0, uv);
#endif

#if ARTISTIC_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }
      ENDHLSL
    }

    Pass
    {
      Name "Fronkon Games Artistic Oil Paint (Detail)"

      HLSLPROGRAM
      #pragma vertex ArtisticVert
      #pragma fragment ArtisticFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile ___ DETAIL_SHARPEN DETAIL_EMBOSS
      #pragma multi_compile ___ WATER_COLOR

      #include "Artistic.hlsl"
#if DETAIL_SHARPEN || DETAIL_EMBOSS
      #include "Detail.hlsl"
#endif
#if WATER_COLOR
      #include "WaterColor.hlsl"
#endif
#if PROCESS_DEPTH
      #include "ProcessDepth.hlsl"
#endif

      half4 ArtisticFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;

        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = color;

        float depth = 0.0;
#if PROCESS_DEPTH
        depth = SampleLinear01Depth(uv);

        if (_SampleSky == 0)
          _DetailStrength = 0.0;
#endif

#ifdef DETAIL_SHARPEN
        pixel.rgb = Sharpen(uv, pixel.rgb, depth);
#elif DETAIL_EMBOSS
        pixel.rgb = Emboss(uv, pixel.rgb, depth);
#endif

#if WATER_COLOR
        pixel.rgb = WaterColor(color.rgb, pixel.rgb, uv);
#endif
        // Color adjust.
        pixel.rgb = ColorAdjust(pixel.rgb);

#if ARTISTIC_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }
      ENDHLSL
    }    
  }
  
  FallBack "Diffuse"
}
