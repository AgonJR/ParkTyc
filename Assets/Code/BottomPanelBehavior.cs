using UnityEngine;
using TMPro;

public class BottomPanelBehavior : MonoBehaviour
{
    public GameObject BottomPanel;
    public TMP_Text Karat;
    public float speed = 200.0f;
    private Vector2 target;
    private Vector2 position;

    void Start()
    {
        position = gameObject.transform.position;
        target = position;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, step);
    }

    public void onHover() 
    {
        target = new Vector2((Screen.width / 2), 50);
        Karat.text = "";
    }
    
    public void onEnd()
    {
        Karat.text = "^";
        target = new Vector2((Screen.width / 2), -200);
    }

}
