using UnityEngine;
using TMPro;

public class BottomPanelBehavior : MonoBehaviour
{
    public GameObject BottomPanel;
    public TMP_Text Karat;
    public float speed = 200.0f;
    private Vector2 target;
    private Vector2 position;

    // TO DO - Please Clean Up Comments
    
    void Start()
    {
        // faceButton = GetComponent<RectTransform>();
        // var karatPosition = Karat.transform.position;
        // target = new Vector2(0.0f, 0.0f);
        position = gameObject.transform.position;
        target = position;
        // Debug.Log(position);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, step);

        // BottomPanel.position = Vector3.MoveTowards(BottomPanel.position, Karat.position, step);
    }

    public void onHover() 
    {
        target = new Vector2(960, 0);
        // Debug.Log(target);
        // Karat.SetActive(false);
        Karat.text = "";
        // var karatPosition = Karat.transform.position;
        // Debug.Log(karatPosition);
        // StartCoroutine("UITransition");
        //self.
        // Debug.Log("test");
        // BottomPanel.SetActive(false);
        // BottomPanel.
        // faceButton.localPosition = Vector3.SmoothDamp(faceButton.localPosition, newPos, ref buttonVelocity, smoothTime);
    }
    
    public void onEnd()
    {
        Karat.text = "^";
        target = new Vector2(960, -200);
        // StopCoroutine("UITransition");
    }

}
