using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 scaleChange, positionChange;

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //Output to console the GameObject's name and the following message
        // Debug.Log("Cursor Entering " + name + " GameObject" + pointerEventData.hovered[0]);

    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        //Output the following message with the GameObject's name
        // Debug.Log("Cursor Exiting " + name + " GameObject");
    }

    public void Enlarge(GameObject obj)
    {
        // gameobject.transform
        // Debug.Log("Boing" + test);
        obj.transform.localScale += new Vector3(1f, 1f, 0f);
    }

    // public void ShowTooltip(Object object)
    // {
    //     Debug.Log(object.name);
    // }
    public void Shrink(GameObject obj)
    {
        obj.transform.localScale += new Vector3(-1f, -1f, 0f);
        // Debug.Log("Boing");
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

}
