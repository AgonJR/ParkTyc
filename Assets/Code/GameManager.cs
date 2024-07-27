using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("All Managers")]
    public NPCManager  npcManagerRef ;
    public GridManager gridManagerRef;
    public HUDManager  hudManagerRef ;

    public static int Score;


    public void Start()
    {
        instance = this;
        Score = 0;
    }

    // For Debug / Pause Menu
    public void ExitGame()
    {
        Application.Quit();
    }

    public void AddToScore(int amount)
    {
        Score += amount;

        if (Score < 0) Score = 0;

        hudManagerRef.DisplayScoreChange(amount);
    }

    public void ResetScore()
    {
        Score = 0;
        
        hudManagerRef.DisplayScoreChange(0);
    }
}
