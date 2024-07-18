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
#ifndef OILPAINT_KUWAHARA_ANISOTROPIC
#define OILPAINT_KUWAHARA_ANISOTROPIC

TEXTURE2D_X(_TensorTex);

int _Blur;
uint _Radius;
float _Sharpness;
float _Hardness;
float _Alpha;
float _ZeroCrossing;

inline float Gaussian(float sigma, float pos)
{
  return (1.0 / sqrt(2.0 * PI * sigma * sigma)) * exp(-(pos * pos) / (2.0 * sigma * sigma));
}

half4 CalculateTensors(float2 uv)
{
  half3 x = (1.0 * SAMPLE_MAIN(uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y)).rgb +
             2.0 * SAMPLE_MAIN(uv + float2(-_MainTex_TexelSize.x,  0.0)).rgb +
             1.0 * SAMPLE_MAIN(uv + float2(-_MainTex_TexelSize.x,  _MainTex_TexelSize.y)).rgb +
            -1.0 * SAMPLE_MAIN(uv + float2( _MainTex_TexelSize.x, -_MainTex_TexelSize.y)).rgb +
            -2.0 * SAMPLE_MAIN(uv + float2( _MainTex_TexelSize.x,  0.0)).rgb +
            -1.0 * SAMPLE_MAIN(uv + float2( _MainTex_TexelSize.x,  _MainTex_TexelSize.y)).rgb) / 4.0;

  half3 y = (1.0 * SAMPLE_MAIN(uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y)).rgb +
             2.0 * SAMPLE_MAIN(uv + float2( 0.0,                  -_MainTex_TexelSize.y)).rgb +
             1.0 * SAMPLE_MAIN(uv + float2( _MainTex_TexelSize.x, -_MainTex_TexelSize.y)).rgb +
            -1.0 * SAMPLE_MAIN(uv + float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y)).rgb +
            -2.0 * SAMPLE_MAIN(uv + float2( 0.0,                  _MainTex_TexelSize.y)).rgb +
            -1.0 * SAMPLE_MAIN(uv + float2( _MainTex_TexelSize.x, _MainTex_TexelSize.y)).rgb) / 4.0;

  return half4(dot(x, x), dot(y, y), dot(x, y), 1.0);
}

half4 BlurHorizontal(float2 uv)
{
  half3 pixel = (half3)0.0;
  float kernelSum = 0.0;

  [loop]
  for (int x = -_Blur; x <= _Blur; ++x)
  {
    half3 color = SAMPLE_MAIN(uv + float2(x * _MainTex_TexelSize.x, 0.0)).rgb;
    const float gauss = Gaussian(2.0, x);

    pixel += color * gauss;
    kernelSum += gauss;
  }

  return half4(pixel / kernelSum, 1.0);
}

half3 BlurVertical(float2 uv)
{
  half3 pixel = (half3)0.0;
  float kernelSum = 0.0;

  [loop]
  for (int y = -_Blur; y <= _Blur; ++y)
  {
    half3 color = SAMPLE_MAIN(uv + float2(0.0, y * _MainTex_TexelSize.y)).rgb;
    const float gauss = Gaussian(2.0, y);

    pixel += color * gauss;
    kernelSum += gauss;
  }

  return pixel / kernelSum;
}

half4 CalculateAnisotropy(half3 pixel)
{
  float lambda1 = 0.5 * (pixel.y + pixel.x + sqrt(pixel.y * pixel.y - 2.0 * pixel.x * pixel.y + pixel.x * pixel.x + 4.0 * pixel.z * pixel.z));
  float lambda2 = 0.5 * (pixel.y + pixel.x - sqrt(pixel.y * pixel.y - 2.0 * pixel.x * pixel.y + pixel.x * pixel.x + 4.0 * pixel.z * pixel.z));

  float2 v = float2(lambda1 - pixel.x, -pixel.z);
  float2 t = length(v) > 0.0 ? normalize(v) : float2(0.0, 1.0);
  float phi = -atan2(t.y, t.x);
  float A = (lambda1 + lambda2 > 0.0) ? (lambda1 - lambda2) / (lambda1 + lambda2) : 0.0;

  return half4(t, phi, A);
}

half4 Anisotropic(float2 uv, float depth)
{
  const half4 tensor = SAMPLE_TEXTURE2D_X(_TensorTex, sampler_LinearClamp, uv);

  uint radius = _Radius / 2;
#if PROCESS_DEPTH
#if VIEW_DEPTH
  return pow(ViewRadius(radius, depth), 2.0) * SAMPLE_MAIN(uv);
#endif
  radius = CalculateRadius(radius, depth);
#endif

  float a = float(radius) * clamp((_Alpha + tensor.w) / _Alpha, 0.1, 2.0);
  float b = float(radius) * clamp(_Alpha / (_Alpha + tensor.w), 0.1, 2.0);

  float cos_phi = cos(tensor.z);
  float sin_phi = sin(tensor.z);

  float2x2 R = { cos_phi, -sin_phi, sin_phi, cos_phi};
  float2x2 S = { 0.5 / a, 0.0, 0.0, 0.5 / b};

  float2x2 SR = mul(S, R);

  int max_x = int(sqrt(a * a * cos_phi * cos_phi + b * b * sin_phi * sin_phi));
  int max_y = int(sqrt(a * a * sin_phi * sin_phi + b * b * cos_phi * cos_phi));

  float sinZeroCross = sin(_ZeroCrossing);
  float eta = (0.01 + cos(_ZeroCrossing)) / (sinZeroCross * sinZeroCross);
  int k;
  float4 m[8];
  float3 s[8];

  for (k = 0; k < 8; ++k)
  {
    m[k] = 0.0;
    s[k] = 0.0;
  }

  [loop]
  for (int y = -max_y; y <= max_y; ++y)
  {
    [loop]
    for (int x = -max_x; x <= max_x; ++x)
    {
      float2 v = mul(SR, float2(x, y));
      if (dot(v, v) <= 0.25f)
      {
        half3 c = SAMPLE_MAIN_LOD(uv + float2(x, y) * _MainTex_TexelSize.xy).rgb;
        c = saturate(c);
        float sum = 0;
        float w[8];
        float z, vxx, vyy;

        vxx = 0.01 - eta * v.x * v.x;
        vyy = 0.01 - eta * v.y * v.y;

        z = max(0, v.y + vxx); 
        w[0] = z * z;
        sum += w[0];

        z = max(0, -v.x + vyy); 
        w[2] = z * z;
        sum += w[2];

        z = max(0, -v.y + vxx); 
        w[4] = z * z;
        sum += w[4];

        z = max(0, v.x + vyy); 
        w[6] = z * z;
        sum += w[6];
        v = sqrt(2.0) / 2.0 * float2(v.x - v.y, v.x + v.y);
        vxx = 0.01 - eta * v.x * v.x;
        vyy = 0.01 - eta * v.y * v.y;

        z = max(0, v.y + vxx); 
        w[1] = z * z;
        sum += w[1];

        z = max(0, -v.x + vyy); 
        w[3] = z * z;
        sum += w[3];

        z = max(0, -v.y + vxx); 
        w[5] = z * z;
        sum += w[5];
        
        z = max(0, v.x + vyy); 
        w[7] = z * z;
        sum += w[7];

        float g = exp(-3.125 * dot(v,v)) / sum;

        for (int k = 0; k < 8; ++k)
        {
          float wk = w[k] * g;
          m[k] += float4(c * wk, wk);
          s[k] += c * c * wk;
        }
      }
    }
  }

  half4 pixel = (float4)0.0;
  [loop]
  for (k = 0; k < 8; ++k)
  {
    m[k].rgb /= m[k].w;
    s[k] = abs(s[k] / m[k].w - m[k].rgb * m[k].rgb);

    float sigma2 = s[k].r + s[k].g + s[k].b;
    float w = 1.0 / (1.0 + pow(abs(_Hardness * 1000.0 * sigma2), 0.5 * _Sharpness));

    pixel += half4(m[k].rgb * w, w);
  }

  pixel.rgb = saturate(pixel.rgb / pixel.w);

  return pixel;
}

#endif