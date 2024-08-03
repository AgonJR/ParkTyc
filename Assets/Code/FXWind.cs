using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXWind : MonoBehaviour
{
    public ParticleSystem windParticleSystem;
    public Transform followTarget;

    void FixedUpdate()
    {
        Vector2 position = new Vector2(followTarget.transform.position.x, followTarget.transform.position.z);
        SetDimensions(position);
    }

    // Set all relevant dimensions for the particle system.
    public void SetDimensions(Vector3 position, Vector3 scale)
    {
        ParticleSystem.ShapeModule windShape = windParticleSystem.shape;
        windShape.position = position;
        windShape.scale = scale;
    }

    // Set only particle system position
    public void SetDimensions(Vector2 position)
    {
        ParticleSystem.ShapeModule windShape = windParticleSystem.shape;
        windShape.position = new Vector3(position.x, windShape.position.y, position.y);
    }
}
