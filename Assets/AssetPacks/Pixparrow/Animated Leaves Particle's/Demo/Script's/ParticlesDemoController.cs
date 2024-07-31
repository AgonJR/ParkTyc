using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ParticlesDemoController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] leavesParticles,leavesVfx,leaves3dVfx,petalsParticles, petalsVfx, petals3dVfx,uiLeaves, uiPetals;
   [SerializeField]
    private float speed=1;

    [SerializeField]
    Text headerText;
   public int leavesActiveGameobject = 0, petalsActiveGameobject = 0;

    bool leaves = true, part = true, vfx3D = false;

    private void Start()
    {
        if (leavesParticles.Length != 0)
            for (int i = 0; i < leavesParticles.Length; i++)
            {
                leavesParticles[i].SetActive(false);
            }

        if (leavesVfx.Length != 0)
            for (int i = 0; i < leavesVfx.Length; i++)
            {
                leavesVfx[i].SetActive(false);
            }
        if (leaves3dVfx.Length != 0)
            for (int i = 0; i < leaves3dVfx.Length; i++)
            {
                leaves3dVfx[i].SetActive(false);
            }

        if (petalsParticles.Length != 0)
            for (int i = 0; i < petalsParticles.Length; i++)
            {
                petalsParticles[i].SetActive(false);
            }

        if (petalsVfx.Length != 0)
            for (int i = 0; i < petalsVfx.Length; i++)
            {
                petalsVfx[i].SetActive(false);
            }

        if (petals3dVfx.Length != 0)
            for (int i = 0; i < petals3dVfx.Length; i++)
            {
                petals3dVfx[i].SetActive(false);
            }

        if (uiLeaves.Length != 0)
            for (int i = 0; i < uiLeaves.Length; i++)
            {
                uiLeaves[i].SetActive(false);
            }

        if (uiPetals.Length != 0)
            for (int i = 0; i < uiPetals.Length; i++)
            {
                uiPetals[i].SetActive(false);
            }

        leavesParticles[leavesActiveGameobject].SetActive(true);
        uiLeaves[leavesActiveGameobject].SetActive(true);
        headerText.text = leavesParticles[leavesActiveGameobject].name;
    }

    public void LateUpdate()
    {
        transform.Rotate(new Vector3(0, speed*50 * Time.deltaTime, 0));
    }
    public void nextObject()
    {
          
        if (leaves)
        {
            uiLeaves[leavesActiveGameobject].SetActive(false);
            if (part)
            {
                leavesParticles[leavesActiveGameobject].SetActive(false);
                if (leavesActiveGameobject == leavesParticles.Length - 1)
                {
                    leavesActiveGameobject =-1;
                }
                leavesActiveGameobject += 1;
                leavesParticles[leavesActiveGameobject].SetActive(true);
                headerText.text = leavesParticles[leavesActiveGameobject].name;
            }
            else if(vfx3D)
            {
                leaves3dVfx[leavesActiveGameobject].SetActive(false);
                if (leavesActiveGameobject == leavesParticles.Length - 1)
                {
                    leavesActiveGameobject = -1;
                }
                leavesActiveGameobject += 1;
                leaves3dVfx[leavesActiveGameobject].SetActive(true);
                headerText.text = leaves3dVfx[leavesActiveGameobject].name;
            }
            else
            {
                leavesVfx[leavesActiveGameobject].SetActive(false);
                if (leavesActiveGameobject == leavesParticles.Length - 1)
                {
                    leavesActiveGameobject = -1;
                }
                leavesActiveGameobject += 1;
                leavesVfx[leavesActiveGameobject].SetActive(true);
                headerText.text = leavesVfx[leavesActiveGameobject].name;
            }
            uiLeaves[leavesActiveGameobject].SetActive(true);
        }
        else
        {
            uiPetals[petalsActiveGameobject].SetActive(false);
            if (part)
            {
                petalsParticles[petalsActiveGameobject].SetActive(false);
                if (petalsActiveGameobject == petalsParticles.Length - 1)
                {
                    petalsActiveGameobject = -1;
                }
                petalsActiveGameobject += 1;
                petalsParticles[petalsActiveGameobject].SetActive(true);
                headerText.text = petalsParticles[petalsActiveGameobject].name;

            }
            else if(vfx3D)
            {
                petals3dVfx[petalsActiveGameobject].SetActive(false);
                if (petalsActiveGameobject == petalsParticles.Length - 1)
                {
                    petalsActiveGameobject = -1;
                }
                petalsActiveGameobject += 1;
                petals3dVfx[petalsActiveGameobject].SetActive(true);
                headerText.text = petals3dVfx[petalsActiveGameobject].name;

            }
            else
            {
                petalsVfx[petalsActiveGameobject].SetActive(false);
                if (petalsActiveGameobject == petalsParticles.Length - 1)
                {
                    petalsActiveGameobject = -1;
                }
                petalsActiveGameobject += 1;
                petalsVfx[petalsActiveGameobject].SetActive(true);
                headerText.text = petalsVfx[petalsActiveGameobject].name;
            }
            uiPetals[petalsActiveGameobject].SetActive(true);
        }
    }

    public void PrevObject()
    {
      
        if (leaves)
        {
            uiLeaves[leavesActiveGameobject].SetActive(false);
            if (part)
            {
                leavesParticles[leavesActiveGameobject].SetActive(false);
                if (leavesActiveGameobject == 0)
                {
                    leavesActiveGameobject = leavesParticles.Length ;
                }
                leavesActiveGameobject -= 1;
                leavesParticles[leavesActiveGameobject].SetActive(true);
                headerText.text = leavesParticles[leavesActiveGameobject].name;
            }
            else if(vfx3D)
            {
                leaves3dVfx[leavesActiveGameobject].SetActive(false);
                if (leavesActiveGameobject == 0)
                {
                    leavesActiveGameobject = leavesParticles.Length ;
                }
                leavesActiveGameobject -= 1;
                leaves3dVfx[leavesActiveGameobject].SetActive(true);
                headerText.text = leaves3dVfx[leavesActiveGameobject].name;
            }
            else
            {
                leavesVfx[leavesActiveGameobject].SetActive(false);
                if (leavesActiveGameobject == 0)
                {
                    leavesActiveGameobject = leavesParticles.Length;
                }
                leavesActiveGameobject -= 1;
                leavesVfx[leavesActiveGameobject].SetActive(true);
                headerText.text = leavesVfx[leavesActiveGameobject].name;
                uiLeaves[leavesActiveGameobject].SetActive(true);
            }
            uiLeaves[leavesActiveGameobject].SetActive(true);
        }
        else
        {
            uiPetals[petalsActiveGameobject].SetActive(false);
            if (part)
            {
                petalsParticles[petalsActiveGameobject].SetActive(false);
                if (petalsActiveGameobject ==0)
                {
                    petalsActiveGameobject = petalsParticles.Length;
                }
                petalsActiveGameobject -= 1;
                petalsParticles[petalsActiveGameobject].SetActive(true);
                headerText.text = petalsParticles[petalsActiveGameobject].name;
            }
            else if (vfx3D)
            {
                petals3dVfx[petalsActiveGameobject].SetActive(false);
                if (petalsActiveGameobject == 0)
                {
                    petalsActiveGameobject = petalsParticles.Length ;
                }
                petalsActiveGameobject -= 1;
                petals3dVfx[petalsActiveGameobject].SetActive(true);
                headerText.text = petals3dVfx[petalsActiveGameobject].name;
            }
            else
            {
                petalsVfx[petalsActiveGameobject].SetActive(false);
                if (petalsActiveGameobject == 0)
                {
                    petalsActiveGameobject = petalsParticles.Length;
                }
                petalsActiveGameobject -= 1;
                petalsVfx[petalsActiveGameobject].SetActive(true);
                headerText.text = petalsVfx[petalsActiveGameobject].name;
            }          
            uiPetals[petalsActiveGameobject].SetActive(true);
        }
    }

    public void ShowParticleSystem()
    {
        part = true;
        vfx3D = false;
        if (leaves)
        {

            leavesParticles[leavesActiveGameobject].SetActive(true);
            leavesVfx[leavesActiveGameobject].SetActive(false);
            leaves3dVfx[leavesActiveGameobject].SetActive(false);
            headerText.text = leavesParticles[leavesActiveGameobject].name;
        }
        else
        {
            petalsParticles[petalsActiveGameobject].SetActive(true);
            petalsVfx[petalsActiveGameobject].SetActive(false);
            petals3dVfx[petalsActiveGameobject].SetActive(false);
            headerText.text = petalsParticles[petalsActiveGameobject].name;

        }
    }

    public void ShowVfx()
    {
        part = false;
        vfx3D = false;
        if (leaves)
        {
            leavesParticles[leavesActiveGameobject].SetActive(false);
            leavesVfx[leavesActiveGameobject].SetActive(true);
            leaves3dVfx[leavesActiveGameobject].SetActive(false);
            headerText.text = leavesVfx[leavesActiveGameobject].name;
        }
        else
        {
            petalsParticles[petalsActiveGameobject].SetActive(false);
            petalsVfx[petalsActiveGameobject].SetActive(true);
            petals3dVfx[petalsActiveGameobject].SetActive(false);
            headerText.text = petalsVfx[petalsActiveGameobject].name;
        }

    }

    public void Show3dVfx()
    {
        part = false;
        vfx3D = true;
        if (leaves)
        {
            leavesParticles[leavesActiveGameobject].SetActive(false);
            leavesVfx[leavesActiveGameobject].SetActive(false);
            leaves3dVfx[leavesActiveGameobject].SetActive(true);
            headerText.text = leaves3dVfx[leavesActiveGameobject].name;
        }
        else
        {

            petalsParticles[petalsActiveGameobject].SetActive(false);
            petalsVfx[petalsActiveGameobject].SetActive(false);
            petals3dVfx[petalsActiveGameobject].SetActive(true);
            headerText.text = petals3dVfx[petalsActiveGameobject].name;

        }
    }

    public void Leaves()
    {
        petalsParticles[petalsActiveGameobject].SetActive(false);
        petalsVfx[petalsActiveGameobject].SetActive(false);
        petals3dVfx[petalsActiveGameobject].SetActive(false);
        uiLeaves[leavesActiveGameobject].SetActive(true);
        uiPetals[petalsActiveGameobject].SetActive(false);

        leaves = true;
        if (part)
        {
            leavesParticles[leavesActiveGameobject].SetActive(true);
            leavesVfx[leavesActiveGameobject].SetActive(false);
            leaves3dVfx[leavesActiveGameobject].SetActive(false);
            headerText.text = leavesParticles[leavesActiveGameobject].name;
        }
        else if(vfx3D)
        {
            leavesParticles[leavesActiveGameobject].SetActive(false);
            leavesVfx[leavesActiveGameobject].SetActive(false);
            leaves3dVfx[leavesActiveGameobject].SetActive(true);
            headerText.text = leaves3dVfx[leavesActiveGameobject].name;
        }
        else
        {
            leavesParticles[leavesActiveGameobject].SetActive(false);
            leavesVfx[leavesActiveGameobject].SetActive(true);
            leaves3dVfx[leavesActiveGameobject].SetActive(false);
            headerText.text = leavesVfx[leavesActiveGameobject].name;
        }
    }
    public void Petals()
    {
        leavesParticles[leavesActiveGameobject].SetActive(false);
        leavesVfx[leavesActiveGameobject].SetActive(false);
        leaves3dVfx[leavesActiveGameobject].SetActive(false);
        uiLeaves[leavesActiveGameobject].SetActive(false);
        uiPetals[petalsActiveGameobject].SetActive(true);

        leaves = false;

        if (part)
        {
            petalsParticles[petalsActiveGameobject].SetActive(true);
            petalsVfx[petalsActiveGameobject].SetActive(false);
            petals3dVfx[petalsActiveGameobject].SetActive(false);
            headerText.text = petalsParticles[petalsActiveGameobject].name;
        }
        else if(vfx3D)
        {
            petalsParticles[petalsActiveGameobject].SetActive(false);
            petalsVfx[petalsActiveGameobject].SetActive(false);
            petals3dVfx[petalsActiveGameobject].SetActive(true);
            headerText.text = petalsVfx[petalsActiveGameobject].name;
        }
        else
        {
            petalsParticles[petalsActiveGameobject].SetActive(false);
            petalsVfx[petalsActiveGameobject].SetActive(true);
            petals3dVfx[petalsActiveGameobject].SetActive(false);
            headerText.text = petalsVfx[petalsActiveGameobject].name;
        }
    }
}
