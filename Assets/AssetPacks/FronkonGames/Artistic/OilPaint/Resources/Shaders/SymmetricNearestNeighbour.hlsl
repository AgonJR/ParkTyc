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
#ifndef OILPAINT_SNN
#define OILPAINT_SNN

int _Radius;

inline float CalcDistance(half3 c1, half3 c2)
{
/* Fast version.

  float dr = c1.r - c2.r;
  float dg = c1.g - c2.g;
  float db = c1.b - c2.b;

  return dr * dr + dg * dg + db * db;
*/
  float3 c = c1 - c2;
  float y = c.r * 0.2124681075446384 + c.g * 0.4169973963260294 + c.b * 0.08137907133969426;
  float i = c.r * 0.3258860837850668 - c.g * 0.14992193838645426 - c.b * 0.17596414539861255;
  float q = c.r * 0.0935501584120867 - c.g * 0.23119531908149002 + c.b * 0.13764516066940333;

  return y * y + i * i + q * q;
}

inline half4 SymmetricNearestNeighbour(half3 pixel, float depth, float2 uv)
{
  float4 sum = (float4)0.0;
  float2 invSize = 1.0 / _ScreenParams.xy;

  int radius = _Radius;
#if PROCESS_DEPTH
#if VIEW_DEPTH
  return pow(ViewRadius(radius, depth), 2.0) * SAMPLE_MAIN(uv);
#endif
  radius = CalculateRadius(radius, depth);
#endif

  [loop]
  for (int i = 0; i <= radius; ++i)
  {
    half3 c1 = SAMPLE_MAIN_LOD(uv + float2( i, 0.0) * invSize).rgb;
    half3 c2 = SAMPLE_MAIN_LOD(uv + float2(-i, 0.0) * invSize).rgb;

    float d1 = CalcDistance(c1, pixel);
    float d2 = CalcDistance(c2, pixel);

    sum.rgb += d1 < d2 ? c1 : c2;
    
    sum.a += 1.0f;
  }

  [loop]
  for (int j = 1; j <= radius; ++j)
  {
    [loop]
    for (int i = -radius; i <= radius; ++i)
    {
      half3 c1 = SAMPLE_MAIN_LOD(uv + float2( i,  j) * invSize).rgb;
      half3 c2 = SAMPLE_MAIN_LOD(uv + float2(-i, -j) * invSize).rgb;

      float d1 = CalcDistance(c1, pixel);
      float d2 = CalcDistance(c2, pixel);

      sum.rgb += d1 < d2 ? c1 : c2;

      sum.a += 1.0f;
    }
  }

  return half4(sum.rgb / sum.a, 1.0);
}

#endif