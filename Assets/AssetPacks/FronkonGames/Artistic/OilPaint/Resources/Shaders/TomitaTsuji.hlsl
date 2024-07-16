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
#ifndef OILPAINT_TOMITA_TSUJI
#define OILPAINT_TOMITA_TSUJI

int _Radius;

half4 TomitaTsuji(float2 uv, float depth)
{
  float3 m[5] =
  {
    { 0.0, 0.0, 0.0 },
    { 0.0, 0.0, 0.0 },
    { 0.0, 0.0, 0.0 },
    { 0.0, 0.0, 0.0 },
    { 0.0, 0.0, 0.0 }
  };
  float3 s[5] = m;

  half3 c;
  int u, v, i, j;

  int radius = _Radius;
#if PROCESS_DEPTH
#if VIEW_DEPTH
  return pow(ViewRadius(radius, depth), 2.0) * SAMPLE_MAIN(uv);
#endif
  radius = CalculateRadius(radius, depth);
#endif

  for (j = -radius; j <= 0; ++j)
    for (i = -radius; i <= 0; ++i)
    {
      c = SAMPLE_MAIN_LOD(uv + float2(i, j) * _MainTex_TexelSize.xy).rgb;
      m[0] += c;
      s[0] += c * c;
    }

  for (j = -radius; j <= 0; ++j)
    for (i = 0; i <= radius; ++i)
    {
      c = SAMPLE_MAIN_LOD(uv + float2(i, j) * _MainTex_TexelSize.xy).rgb;
      m[1] += c;
      s[1] += c * c;
    }

  for (j = 0; j <= radius; ++j)
    for (i = 0; i <= radius; ++i)
    {
      c = SAMPLE_MAIN_LOD(uv + half2(i, j) * _MainTex_TexelSize.xy).rgb;
      m[2] += c;
      s[2] += c * c;
    }

  for (j = 0; j <= radius; ++j)
    for (i = -radius; i <= 0; ++i)
    {
      c = SAMPLE_MAIN_LOD(uv + float2(i, j) * _MainTex_TexelSize.xy).rgb;
      m[3] += c;
      s[3] += c * c;
    }

  half radiusTT = radius / 2.0;
  for (j = -radiusTT; j <= radiusTT; ++j)
    for (i = -radiusTT; i <= radiusTT; ++i)
    {
      c = saturate(SAMPLE_MAIN_LOD(uv + float2(i, j) * _MainTex_TexelSize.xy).rgb);
      m[4] += c;
      s[4] += c * c;
    }

  half3 pixel = half3(0.0, 0.0, 0.0);
  half minSigma2 = 1e+2;

  half n = (half)radius + 1;
  n *= n;

  m[0] /= n;
  s[0] = abs(s[0] / n - m[0] * m[0]);

  half sigma2 = s[0].r + s[0].g + s[0].b;
  if (sigma2 < minSigma2)
  {
    minSigma2 = sigma2;
    pixel = m[0];
  }

  m[1] /= n;
  s[1] = abs(s[1] / n - m[1] * m[1]);

  sigma2 = s[1].r + s[1].g + s[1].b;
  if (sigma2 < minSigma2)
  {
    minSigma2 = sigma2;
    pixel = m[1];
  }

  m[2] /= n;
  s[2] = abs(s[2] / n - m[2] * m[2]);

  sigma2 = s[2].r + s[2].g + s[2].b;
  if (sigma2 < minSigma2)
  {
    minSigma2 = sigma2;
    pixel = m[2];
  }

  m[3] /= n;
  s[3] = abs(s[3] / n - m[3] * m[3]);

  sigma2 = s[3].r + s[3].g + s[3].b;
  if (sigma2 < minSigma2)
  {
    minSigma2 = sigma2;
    pixel = m[3];
  }
  
  m[4] /= n;
  s[4] = abs(s[4] / n - m[4] * m[4]);

  sigma2 = s[4].r + s[4].g + s[4].b;
  if (sigma2 < minSigma2)
  {
    minSigma2 = sigma2;
    pixel = m[4];
  }

  return half4(pixel, 1.0);
}

#endif