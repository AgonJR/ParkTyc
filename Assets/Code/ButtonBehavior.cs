using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    public GameObject ToolTip;

    private bool isActive = false;
    public float timeToTip = 3.0f;
    private float currentTimer;

    public void onHover() {
        // ToolTip.SetActive(true);
        isActive = true;
    }

    public void exitHover() {
        isActive = false;
        ToolTip.SetActive(false);
    }

    private void setToolTip() {
        ToolTip.SetActive(true);
    }

    void Start()
    {
        ToolTip.SetActive(false);
        currentTimer = timeToTip;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive) {
            // Debug.Log("Start Timer!");
            currentTimer -= Time.deltaTime;
        }

        if (currentTimer <= 0.0f) {
            setToolTip();
            currentTimer = timeToTip;
            isActive = false;
        }
    }
}
