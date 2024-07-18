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
#ifndef OILPAINT_PROCESS_DEPTH
#define OILPAINT_PROCESS_DEPTH

#pragma multi_compile ___ VIEW_DEPTH

TEXTURE2D_X(_CurveTex);

int _SampleSky;
float _DepthPower;

inline float SampleDepthCurve(float depth)
{
  return SAMPLE_TEXTURE2D_X(_CurveTex, sampler_LinearClamp, float2(pow(abs(depth), _DepthPower), 0.5)).x;
}

inline uint CalculateRadius(uint radius, float depth)
{
  return round(lerp(1, radius, SampleDepthCurve(depth)));
}

inline float CalculateDetail(float detail, float depth)
{
  return lerp(0.0, detail, SampleDepthCurve(depth));
}

inline half3 Palette(in float t, in half3 a, in half3 b, in half3 c, in half3 d)
{
  return a + b * cos(6.28318 * (c * t + d));
}

inline half4 ViewRadius(uint radius, float depth)
{
  float palette = (float)CalculateRadius(radius, depth) / (float)radius;

  return half4(Palette(palette, half3(0.5, 0.5,  0.5),
                                half3(0.5, 0.5,  0.5),
                                half3(0.8, 0.8,  0.8),
                                half3(0.0, 0.33, 0.67) + 0.21), 1.0);
}

#endif