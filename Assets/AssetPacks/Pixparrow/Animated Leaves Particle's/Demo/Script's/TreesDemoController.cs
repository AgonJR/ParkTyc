using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreesDemoController : MonoBehaviour
{
    [SerializeField]
    private Vector3 cameraOffset, farCameraOffset;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private GameObject[] trees,vfxTrees,vfx3dTrees;
    [SerializeField]
    private float speed = 1;

    [SerializeField]
    Text headerText;
    int ActiveGameobject = 0;

    bool  part = true,vfx3d=false;

    private void Start()
    {
     
        if (trees.Length != 0)
            for (int i = 0; i < trees.Length; i++)
            {
                trees[i].SetActive(false);
            }

        if (vfxTrees.Length != 0)
            for (int i = 0; i < trees.Length; i++)
            {
                vfxTrees[i].SetActive(false);
            }

        if (vfx3dTrees.Length != 0)
            for (int i = 0; i < trees.Length; i++)
            {
                vfx3dTrees[i].SetActive(false);
            }
        trees[ActiveGameobject].SetActive(true);

        headerText.text = trees[ActiveGameobject].name;
    }

    public void LateUpdate()
    {
        transform.Rotate(new Vector3(0, speed * 50 * Time.deltaTime, 0));
        if (ActiveGameobject == 4)
        {
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition , farCameraOffset, 10*Time.deltaTime);
        }
        else
        {
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition , cameraOffset, 10 * Time.deltaTime);
        }
    }
    public void nextObject()
    {

        if (part)
        {
            trees[ActiveGameobject].SetActive(false);
            if (ActiveGameobject == trees.Length - 1)
            {
                ActiveGameobject = -1;
            }
            ActiveGameobject += 1;
            trees[ActiveGameobject].SetActive(true);
            headerText.text = trees[ActiveGameobject].name;
        }
        else if(vfx3d)
        {

            vfx3dTrees[ActiveGameobject].SetActive(false);
            if (ActiveGameobject == trees.Length - 1)
            {
                ActiveGameobject = -1;
            }
            ActiveGameobject += 1;
            vfx3dTrees[ActiveGameobject].SetActive(true);
            headerText.text = vfx3dTrees[ActiveGameobject].name;
        }
        else
        {

            vfxTrees[ActiveGameobject].SetActive(false);
            if (ActiveGameobject == trees.Length - 1)
            {
                ActiveGameobject = -1;
            }
            ActiveGameobject += 1;
            vfxTrees[ActiveGameobject].SetActive(true);
            headerText.text = vfxTrees[ActiveGameobject].name;
        }

    }

    public void PrevObject()
    {
        if (part)
        {
            trees[ActiveGameobject].SetActive(false);
            if (ActiveGameobject ==  0)
            {
                ActiveGameobject = trees.Length;
            }
            ActiveGameobject -= 1;
            trees[ActiveGameobject].SetActive(true);
            headerText.text = trees[ActiveGameobject].name;
        }
        else if (vfx3d)
        {

            vfx3dTrees[ActiveGameobject].SetActive(false);
            if (ActiveGameobject == 0)
            {
                ActiveGameobject = trees.Length;
            }
            ActiveGameobject -= 1;
            vfx3dTrees[ActiveGameobject].SetActive(true);
            headerText.text = vfx3dTrees[ActiveGameobject].name;
        }
        else
        {

            vfxTrees[ActiveGameobject].SetActive(false);
            if (ActiveGameobject == 0)
            {
                ActiveGameobject = trees.Length;
            }
            ActiveGameobject -= 1;
            vfxTrees[ActiveGameobject].SetActive(true);
            headerText.text = vfxTrees[ActiveGameobject].name;
        }
    }

    public void ShowParticleSystem()
    {
        part = true;
        vfx3d = false;
        trees[ActiveGameobject].SetActive(true);
        vfxTrees[ActiveGameobject].SetActive(false);
        vfx3dTrees[ActiveGameobject].SetActive(false);
        headerText.text = trees[ActiveGameobject].name;
    }


    public void ShowVfx()
    {
        part = false;
        vfx3d = false;
        trees[ActiveGameobject].SetActive(false);
        vfxTrees[ActiveGameobject].SetActive(true);
        vfx3dTrees[ActiveGameobject].SetActive(false);
        headerText.text = vfxTrees[ActiveGameobject].name;
    }

    public void Show3dVfx()
    {
        part = false;
        vfx3d = true;
        trees[ActiveGameobject].SetActive(false);
        vfxTrees[ActiveGameobject].SetActive(false);
        vfx3dTrees[ActiveGameobject].SetActive(true);
        headerText.text = vfxTrees[ActiveGameobject].name;
    }
}
