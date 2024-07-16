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
#ifndef OILPAINT_WATERCOLOR
#define OILPAINT_WATERCOLOR

float _WaterColorStrength;
int _WaterColorBlend;

inline half3 WaterColor(half3 color, half3 pixel, float2 uv)
{
  const half3 hc = 1.0 * SAMPLE_MAIN_LOD(uv + float2(-1.0, -1.0) * _MainTex_TexelSize.xy).rgb +  2.0 * SAMPLE_MAIN_LOD(uv + float2( 0.0, -1.0) * _MainTex_TexelSize.xy).rgb +
                   1.0 * SAMPLE_MAIN_LOD(uv + float2( 1.0, -1.0) * _MainTex_TexelSize.xy).rgb + -1.0 * SAMPLE_MAIN_LOD(uv + float2(-1.0,  1.0) * _MainTex_TexelSize.xy).rgb +
                  -2.0 * SAMPLE_MAIN_LOD(uv + float2( 0.0,  1.0) * _MainTex_TexelSize.xy).rgb + -1.0 * SAMPLE_MAIN_LOD(uv + float2( 1.0,  1.0) * _MainTex_TexelSize.xy).rgb;
             
  const half3 vc = 1.0 * SAMPLE_MAIN_LOD(uv + float2(-1.0, -1.0) * _MainTex_TexelSize.xy).rgb +  2.0 * SAMPLE_MAIN_LOD(uv + float2(-1.0,  0.0) * _MainTex_TexelSize.xy).rgb +
                   1.0 * SAMPLE_MAIN_LOD(uv + float2(-1.0,  1.0) * _MainTex_TexelSize.xy).rgb + -1.0 * SAMPLE_MAIN_LOD(uv + float2( 1.0, -1.0) * _MainTex_TexelSize.xy).rgb +
                  -2.0 * SAMPLE_MAIN_LOD(uv + float2( 1.0,  0.0) * _MainTex_TexelSize.xy).rgb + -1.0 * SAMPLE_MAIN_LOD(uv + float2( 1.0,  1.0) * _MainTex_TexelSize.xy).rgb;

  return lerp(pixel, color.rgb - dot(half3(0.2126, 0.7152, 0.0722), vc * vc + hc * hc), _WaterColorStrength);
}

#endif