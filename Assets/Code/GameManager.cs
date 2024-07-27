using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("All Managers")]
    public NPCManager  npcManagerRef ;
    public GridManager gridManagerRef;
    public HUDManager  hudManagerRef ;

    [Header("Dev. Tools")]
    public int startingScore = 0;

    public static int Score;


    public void Start()
    {
        instance = this;
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
        Score = startingScore;
        
        hudManagerRef.DisplayScoreChange(0);
    }
}
