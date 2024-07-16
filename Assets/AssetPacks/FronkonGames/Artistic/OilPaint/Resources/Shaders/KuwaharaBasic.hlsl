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
#ifndef OILPAINT_KUWAHARA_BASIC
#define OILPAINT_KUWAHARA_BASIC

int _Radius;

float4 SampleQuadrant(float2 uv, int x1, int x2, int y1, int y2, float n)
{
  float luminanceSum = 0.0;
  float luminanceSum2 = 0.0;
  float3 colSum = 0.0;

  [loop]
  for (int x = x1; x <= x2; ++x)
  {
    [loop]
    for (int y = y1; y <= y2; ++y)
    {
      half3 c = SAMPLE_MAIN_LOD(uv + float2(x, y) * _MainTex_TexelSize.xy).rgb;
      float l = Luminance(c);
      luminanceSum += l;
      luminanceSum2 += l * l;
      colSum += saturate(c);
    }
  }

  float mean = luminanceSum / n;
  float stdev = abs(luminanceSum2 / n - mean * mean);

  return float4(colSum / n, stdev);
}

half4 KuwaharaBasic(float2 uv, float depth)
{
  int radius = _Radius / 2;
#if PROCESS_DEPTH
#if VIEW_DEPTH
  return pow(ViewRadius(radius, depth), 2.0) * SAMPLE_MAIN(uv);
#endif
  radius = CalculateRadius(radius, depth);
#endif

  float windowSize = 2.0 * radius + 1;
  int quadrantSize = int(ceil(windowSize / 2.0));
  int numSamples = quadrantSize * quadrantSize;

  float4 q1 = SampleQuadrant(uv, -radius, 0, -radius, 0, numSamples);
  float4 q2 = SampleQuadrant(uv, 0, radius, -radius, 0, numSamples);
  float4 q3 = SampleQuadrant(uv, 0, radius, 0, radius, numSamples);
  float4 q4 = SampleQuadrant(uv, -radius, 0, 0, radius, numSamples);

  float minstd = min(q1.a, min(q2.a, min(q3.a, q4.a)));
  int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minstd;

  if (dot(q, 1) > 1)
    return saturate(half4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0, 1.0));

  return saturate(half4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0));
}

#endif