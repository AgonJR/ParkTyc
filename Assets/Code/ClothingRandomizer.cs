using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ClothingGroup
{
    [Tooltip("Clothing items, all but one will be visible. All others will become hidden.")]
    public GameObject[] clothingItems;
}


public class ClothingRandomizer : MonoBehaviour
{
    [Tooltip("Groups of items, from each, only a single garment will be visible")]
    public ClothingGroup[] clothingGroups;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < this.clothingGroups.Length; i++) {            
            ClothingGroup clothingGroup = clothingGroups[i];

            int enableIndex = Random.Range(0, clothingGroup.clothingItems.Length);
            System.Array.ForEach(clothingGroup.clothingItems, item => item.SetActive(false));
            clothingGroup.clothingItems[enableIndex].SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
