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
#ifndef OILPAINT_KUWAHARA_DIRECTIONAL
#define OILPAINT_KUWAHARA_DIRECTIONAL

int _Radius;

half4 KuwaharaDirectional(float2 uv, float depth)
{
#if PROCESS_DEPTH
#if VIEW_DEPTH
  return pow(ViewRadius(_Radius, depth), 2.0) * SAMPLE_MAIN(uv);
#endif
  _Radius = CalculateRadius(_Radius, depth);
#endif

  const float KernelX[9] = { -1.0, -2.0, -1.0,  0.0, 0.0, 0.0,  1.0, 2.0, 1.0 };
  const float KernelY[9] = { -1.0,  0.0,  1.0, -2.0, 0.0, 2.0, -1.0, 0.0, 1.0 };

  int i = 0;
  float Gx = 0.0;
  float Gy = 0.0;

  [loop]
  for (int x = -1; x <= 1; ++x)
  {
    [loop]
    for (int y = -1; y <= 1; ++y)
    {
      if (i == 4)
      {
        i++;
        continue;
      }

      float2 offset = float2(x, y) * _MainTex_TexelSize.xy;
      half3 c = SAMPLE_MAIN_LOD(uv + offset).rgb;

      float l = dot(c, float3(0.2125, 0.7152, 0.0722));

      Gx += l * KernelX[i];
      Gy += l * KernelY[i];

      i++;
    }
  }

  float angle = 0.0;
  if (abs(Gx) > 0.001)
    angle = atan(Gy / Gx);

  float s = sin(angle);
  float c = cos(angle);

  float3 mean[4] = { (float3)0.0, (float3)0.0, (float3)0.0, (float3)0.0 };
  float3 sigma[4] = { (float3)0.0, (float3)0.0, (float3)0.0, (float3)0.0 };

  float2 offsets[4] =
  {
    float2(-_Radius, -_Radius),
    float2(-_Radius,  0.0),
    float2(0.0,      -_Radius),
    float2(0.0,       0.0)
  };

  [loop]
  for (i = 0; i < 4; ++i)
  {
    [loop]
    for (int j = 0; j <= _Radius; ++j)
    {
      for (int k = 0; k <= _Radius; ++k)
      {
        float2 pos = float2(j, k) + offsets[i];
        float2 offs = pos * _MainTex_TexelSize.xy;
        offs = float2(offs.x * c - offs.y * s, offs.x * s + offs.y * c);
        float2 uvpos = uv + offs;

        half3 c = saturate(SAMPLE_MAIN_LOD(uvpos).rgb);

        mean[i] += c;
        sigma[i] += c * c;
      }
    }
  }

  float n = pow(float(_Radius + 1), 2.0);

  float sigma_f;
  float min = 1.0;

  half4 pixel = 0.0;
  for (i = 0; i < 4; ++i)
  {
    mean[i] /= n;
    sigma[i] = abs(sigma[i] / n - mean[i] * mean[i]);
    sigma_f = sigma[i].b + sigma[i].y + sigma[i].z;
    
    if (sigma_f < min)
    {
      min = sigma_f;
      pixel = half4(mean[i], 1.0);
    }
  }

  return pixel;
}

#endif