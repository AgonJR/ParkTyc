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
#ifndef OILPAINT_KUWAHARA_GENERALIZED
#define OILPAINT_KUWAHARA_GENERALIZED

int _Radius;
float _Sharpness;
float _Hardness;

half4 KuwaharaGeneralized(float2 uv, float depth)
{
#if PROCESS_DEPTH
#if VIEW_DEPTH
  return pow(ViewRadius(_Radius, depth), 2.0) * SAMPLE_MAIN(uv);
#endif
  _Radius = CalculateRadius(_Radius, depth);
#endif

  const float zeta = 2.0 / float(_Radius);

  float4 m[8];
  float3 s[8];
  for (int k = 0; k < 8; ++k)
  {
    m[k] = 0.0;
    s[k] = 0.0;
  }

  [loop]
  for (int y = -_Radius; y <= _Radius; ++y)
  {
    [loop]
    for (int x = -_Radius; x <= _Radius; ++x)
    {
      float2 v = float2(x, y) / float(_Radius);
      half3 c = SAMPLE_MAIN_LOD(uv + float2(x, y) * _MainTex_TexelSize.xy).rgb;
      c = clamp(c, 0.0, 1.0);
      float sum = 0.0;
      float w[8];
      float z, vxx, vyy;

      vxx = vyy = zeta;
      z = max(0.0, v.y + vxx); 
      w[0] = z * z;
      sum += w[0];
      z = max(0.0, -v.x + vyy); 

      w[2] = z * z;
      sum += w[2];
      z = max(0.0, -v.y + vxx); 
      w[4] = z * z;
      sum += w[4];
      z = max(0.0, v.x + vyy); 
      w[6] = z * z;
      sum += w[6];
      v = 1.4142 / 2.0 * float2(v.x - v.y, v.x + v.y);
      vxx = vyy = zeta;
      z = max(0.0, v.y + vxx); 
      w[1] = z * z;
      sum += w[1];
      z = max(0.0, -v.x + vyy); 
      w[3] = z * z;
      sum += w[3];
      z = max(0.0, -v.y + vxx); 
      w[5] = z * z;
      sum += w[5];
      z = max(0.0, v.x + vyy); 
      w[7] = z * z;
      sum += w[7];

      const float g = exp(-3.125 * dot(v, v)) / sum;
      for (int k = 0; k < 8; ++k)
      {
        float wk = w[k] * g;
        m[k] += float4(c * wk, wk);
        s[k] += c * c * wk;
      }
    }
  }

  half4 pixel = 0.0;
  for (k = 0; k < 8; ++k)
  {
    m[k].rgb /= m[k].w;
    s[k] = abs(s[k] / m[k].w - m[k].rgb * m[k].rgb);

    float sigma2 = s[k].r + s[k].g + s[k].b;
    float w = 1.0 / (1.0 + pow(abs(_Hardness * 1000.0 * sigma2), 0.5 * _Sharpness));

    pixel += float4(m[k].rgb * w, w);
  }

  return clamp((pixel / pixel.w), 0.0, 1.0);
}

#endif