using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileOrnamentRandomizer : MonoBehaviour
{
    public List<GameObject> tileOrnaments;

    public int Minimum = 0;
    public int Maximum = 0;

    void OnEnable()
    {
        RandomActivations();
    }

    private void RandomActivations()
    {
        int rng = (int)Random.Range(Minimum, Mathf.Min(Maximum, tileOrnaments.Count));

        List<int> unusedIndices = new List<int>();

        for (int i = 0; i < tileOrnaments.Count; i++)
        {
            unusedIndices.Add(i);
            tileOrnaments[i].SetActive(false);
        }


        for (int i = 0; i < rng; i++)
        {
            int randomIndex = unusedIndices[(int)Random.Range(0, unusedIndices.Count)];

            tileOrnaments[randomIndex].SetActive(true);

            unusedIndices.Remove(randomIndex);
        }
    }
}
