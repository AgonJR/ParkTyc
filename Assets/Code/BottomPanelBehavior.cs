using UnityEngine;
using TMPro;

public class BottomPanelBehavior : MonoBehaviour
{
    public GameObject BottomPanel;
    public TMP_Text Karat;
    public GameObject KaratBox;
    public float speed = 200.0f;
    private Vector2 target;
    private Vector2 position;
    private float timeout = 2.0f;
    private bool active = false;

    void Start()
    {
        position = gameObject.transform.position;
        target = position;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, step);
        if (active) {
            timeout -= Time.deltaTime;
        }
        if (timeout <= 0.0f) {
            active = false;
            onEnd();
        }
    }

    public void onHover() 
    {
        target = new Vector2((Screen.width / 2), 50);
        Karat.text = "";
        timeout = 2.0f;
        active = true;
    }
    
    public void onEnd()
    {
        if (active == true) {
            return;
        }
        Karat.text = "^";
        target = new Vector2((Screen.width / 2), -200);
    }

}
