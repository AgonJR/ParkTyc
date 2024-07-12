using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    public GameObject targetExit;
    public float minExitDistance;

    private void FixedUpdate()
    {
        ExitCheck();
    }

    private void ExitCheck()
    {
        if (targetExit != null)
        {
            float distance = Vector3.Distance(transform.position, targetExit.transform.position);

            if (distance < minExitDistance)
            {
                Destroy(gameObject);
            }
        }
    }


    public GameObject GetTargetExitTile(List<GameObject> exitTiles)
    {
        if ( targetExit == null )
        {
            targetExit = exitTiles[(int)Random.Range(0, exitTiles.Count)];
        }

        return targetExit;
    }
}
