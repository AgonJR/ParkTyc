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
namespace FronkonGames.Artistic.OilPaint
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class OilPaint
  {
    /// <summary> Oil paint algorithms. </summary>
    public enum Algorithms
    {
      /// <summary> Basic Kuwahara (default). </summary>
      KuwaharaBasic,

      /// <summary> Generalized Kuwahara. </summary>
      KuwaharaGeneralized,

      /// <summary> Directional Kuwahara. </summary>
      KuwaharaDirectional,

      /// <summary> Anisotropic Kuwahara. </summary>
      KuwaharaAnisotropic,

      /// <summary> Tomita-Tsuji algorithm. </summary>
      TomitaTsuji,

      /// <summary> Symmetric Nearest Neighbour algorithm. </summary>
      SymmetricNearestNeighbour
    }

    /// <summary> Detail algorithms. </summary>
    public enum Detail
    {
      /// <summary> Node (default). </summary>
      None,

      /// <summary> Sharpen algorithm. </summary>
      Sharpen,

      /// <summary> Emboss algorithm. </summary>
      Emboss,
    }
  }
}
