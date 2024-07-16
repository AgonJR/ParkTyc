using UnityEngine;

namespace FronkonGames.Artistic.OilPaint
{
  /// <summary> Simple object rotation. </summary>
  /// <remarks> This code is designed for a simple demo, not for production environments. </remarks>
  public sealed class Rotate : MonoBehaviour
  {
    [SerializeField]
    private Vector3 angularSpeed;

    private void Update() => transform.Rotate(angularSpeed * Time.deltaTime);
  }
}