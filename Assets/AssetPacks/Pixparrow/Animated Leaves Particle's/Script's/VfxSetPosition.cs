using UnityEngine;
using UnityEngine.VFX;

public class VfxSetPosition : MonoBehaviour
{
    [SerializeField]
    private VisualEffect[] vfxObjects;

    void Start()
    {
        if (vfxObjects != null)
        {
            for (int i = 0; i < vfxObjects.Length; i++)
            {            
                vfxObjects[i].SetVector3("Position", vfxObjects[i].transform.position);
            }
         
        }
    }

    
}
