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
#ifndef OILPAINT_DETAIL
#define OILPAINT_DETAIL

float _DetailStrength;

half3 Sharpen(float2 uv, half3 pixel, float depth)
{
  half3 detail = (half3)0.0;
  const float3x3 Kernel = { 0.0, -1.0,  0.0, 
                           -1.0,  5.0, -1.0, 
                            0.0, -1.0,  0.0 };

  [loop]
  for (int x = 0; x < 3; ++x)
  {
    [loop]
    for (int y = 0; y < 3; ++y)
    {
      float xMargin = min(max(0.0, uv.x + (x - 1.0) * _MainTex_TexelSize.x), 1.0);
      float yMargin = min(max(0.0, uv.y + (y - 1.0) * _MainTex_TexelSize.y), 1.0);

      detail += Kernel[x][y] * SAMPLE_MAIN(float2(xMargin, yMargin)).rgb;
    }
  }

#if PROCESS_DEPTH
  _DetailStrength = CalculateDetail(_DetailStrength, depth);
#endif

  return lerp(pixel, detail, _DetailStrength);
}

float _EmbossStrength;
float _EmbossAngle;

half3 Emboss(float2 uv, half3 pixel, float depth)
{
  float2 d = _MainTex_TexelSize.xy;
  d = mul(d, float2x2(cos(_EmbossAngle), -sin(_EmbossAngle), sin(_EmbossAngle), cos(_EmbossAngle)));

  const half3 c1 = SAMPLE_MAIN(uv + float2(-d.x, -d.y)).rgb;
  const half3 c2 = SAMPLE_MAIN(uv + float2( 0.0, -d.y)).rgb;
  const half3 c4 = SAMPLE_MAIN(uv + float2(-d.x,  0.0)).rgb;
  const half3 c6 = SAMPLE_MAIN(uv + float2( d.x,  0.0)).rgb;
  const half3 c8 = SAMPLE_MAIN(uv + float2( 0.0,  d.y)).rgb;
  const half3 c9 = SAMPLE_MAIN(uv + float2( d.x,  d.y)).rgb;

  const float3 c0 = Luminance(-c1 - c2 - c4 + c6 + c8 + c9);

#if PROCESS_DEPTH
  _DetailStrength = CalculateDetail(_DetailStrength, depth);
#endif

  return lerp(pixel, pixel * _EmbossStrength * _DetailStrength, c0.r);
}

#endif