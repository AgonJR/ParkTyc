using UnityEngine;

public class GridTile : MonoBehaviour
{
    public GameObject highlightTile;

    private Renderer _rendererRef;
    private Vector3 _highlightPos;

    private static GameObject _highlightTileObject;

    void Awake()
    {
        if (GridTile._highlightTileObject == null)
        {
            _highlightTileObject = Instantiate(highlightTile, Vector3.zero, highlightTile.transform.rotation);
            _highlightTileObject.SetActive(false);
        }
    }

    void Start()
    {
        _rendererRef = gameObject.GetComponent<MeshRenderer>();
        _highlightPos = new Vector3(gameObject.transform.position.x, 0.3f, gameObject.transform.position.z);
    }

    void OnMouseEnter()
    {
        _highlightTileObject.SetActive(true);
        _highlightTileObject.transform.position = _highlightPos;
    }

    void OnMouseExit()
    {
        _highlightTileObject.SetActive(false);
    }
}
