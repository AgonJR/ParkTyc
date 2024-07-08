using UnityEngine;

public class GridTile : MonoBehaviour
{

    public Material hoverMaterial;

    private Material _originalMat; 
    private Renderer _rendererRef;  

    void Start()
    {
        _rendererRef = gameObject.GetComponent<MeshRenderer>();
        _originalMat = _rendererRef.material;
    }

    void OnMouseEnter()
    {
        if (hoverMaterial != null)
        {
            _rendererRef.material = hoverMaterial;
        }
    }

    void OnMouseExit()
    {
        _rendererRef.material = _originalMat; 
    }
}
