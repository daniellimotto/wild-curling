using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public enum GameMode { Casual, Wild }
    public enum GameState { MainMenu, Playing, EndOfEnd, GameOver }
    
    public GameMode currentMode = GameMode.Casual;
    public GameState currentState = GameState.MainMenu;
    
    public int totalEnds = 4;
    public int currentEnd = 1;
    public int stonesPerEnd = 8;
    public int currentStone = 0;
    
    public int redTeamScore = 0;
    public int blueTeamScore = 0;
    
    public TextMeshProUGUI redScoreText;
    public TextMeshProUGUI blueScoreText;
    public TextMeshProUGUI currentEndText;
    
    public ScoringSystem scoringSystem;
    public Transform stoneSpawnPoint;
    public GameObject standardStonePrefab;
    public GameObject jumboStonePrefab;
    public GameObject miniStonePrefab;
    public GameObject freezeStonePrefab;
    
    // Reference to the power-up wheel
    public PowerUpWheel powerUpWheel;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateUIText();
    }
    
    // Update is called once per frame
    void Update()
    {
        // For testing purposes, pressing space will end the current turn
        if (Input.GetKeyDown(KeyCode.Space) && currentState == GameState.Playing)
        {
            EndTurn();
        }
        
        if (Input.GetKeyDown(KeyCode.W) && powerUpWheel != null && !powerUpWheel.isSpinning)
        {
            powerUpWheel.SpinWheel();
        }
    }
    
    public void StartGame(GameMode mode)
    {
        currentMode = mode;
        currentState = GameState.Playing;
        currentEnd = 1;
        currentStone = 0;
        redTeamScore = 0;
        blueTeamScore = 0;
        
        UpdateUIText();
        SpawnNextStone();
    }
    
    public void EndTurn()
    {
        currentStone++;
        
        if (currentStone >= stonesPerEnd)
        {
            EndOfEnd();
        }
        else
        {
            SpawnNextStone();
        }
    }
    
    private void EndOfEnd()
    {
        currentState = GameState.EndOfEnd;
        
        // Calculate score for this end
        if(scoringSystem != null)
        {
            KeyValuePair<Team, int> score = scoringSystem.CalculateScore();
            if(score.Key == Team.red)
            {
                redTeamScore += score.Value;
            }
            else if(score.Key == Team.blue)
            {
                blueTeamScore += score.Value;
            }
            
            UpdateUIText();
        }
        
        // Move to next end or end game
        currentEnd++;
        currentStone = 0;
        
        if (currentEnd > totalEnds)
        {
            EndGame();
        }
        else
        {
            // For prototype: auto-start next end after 3 seconds
            Invoke("StartNextEnd", 3f);
        }
    }
    
    private void StartNextEnd()
    {
        // Clear all stones from previous end
        Stone[] stones = FindObjectsOfType<Stone>();
        foreach(Stone stone in stones)
        {
            Destroy(stone.gameObject);
        }
        
        currentState = GameState.Playing;
        UpdateUIText();
        SpawnNextStone();
    }
    
    private void EndGame()
    {
        currentState = GameState.GameOver;
        Debug.Log("Game Over! Final Score - Red: " + redTeamScore + ", Blue: " + blueTeamScore);
        // Show game over UI, etc.
    }
    
    private void SpawnNextStone()
    {
        // Determine which team's turn it is (red for even stones, blue for odd)
        Team currentTeam = (currentStone % 2 == 0) ? Team.red : Team.blue;
        
        GameObject stonePrefab = standardStonePrefab;
        
        // If in Wild mode, use the power-up stone if available
        if (currentMode == GameMode.Wild && powerUpWheel != null)
        {
            // Check if we need to activate the power-up wheel
            if (powerUpWheel.currentPowerUp == PowerUpWheel.PowerUpType.None)
            {
                // Start the power-up wheel sequence
                StartCoroutine(PowerUpWheelSequence(currentTeam));
                return; // Exit method, will resume after wheel sequence
            }
            
            switch (powerUpWheel.currentPowerUp)
            {
                case PowerUpWheel.PowerUpType.Jumbo:
                    stonePrefab = jumboStonePrefab;
                    break;
                case PowerUpWheel.PowerUpType.Mini:
                    stonePrefab = miniStonePrefab;
                    break;
                case PowerUpWheel.PowerUpType.Freeze:
                    stonePrefab = freezeStonePrefab;
                    break;
            }
            
            // Reset power-up for next turn
            powerUpWheel.ResetPowerUp();
        }
        
        // Spawn the stone
        if (stonePrefab != null && stoneSpawnPoint != null)
        {
            GameObject stoneObj = Instantiate(stonePrefab, stoneSpawnPoint.position, stoneSpawnPoint.rotation);
            Stone stone = stoneObj.GetComponent<Stone>();
            if (stone != null)
            {
                stone.team = currentTeam;
            }
        }
    }
    
    // New method to handle the power-up wheel sequence
    private IEnumerator PowerUpWheelSequence(Team currentTeam)
    {
        // Activate camera view for the wheel if needed
        // ShowWheelCamera();
        
        // Wait for a short delay before spinning
        yield return new WaitForSeconds(1.0f);
        
        // Spin the wheel
        powerUpWheel.SpinWheel();
        
        // Wait until the wheel stops spinning
        while (powerUpWheel.isSpinning)
        {
            yield return null;
        }
        
        // Wait for a short delay to show the result
        yield return new WaitForSeconds(1.5f);
        
        // Return to game view if needed
        // ShowGameCamera();
        
        // Resume stone spawning with the power-up
        SpawnNextStone();
    }
    
    private void UpdateUIText()
    {
        if (redScoreText != null)
            redScoreText.text = "Red: " + redTeamScore;
        
        if (blueScoreText != null)
            blueScoreText.text = "Blue: " + blueTeamScore;
        
        if (currentEndText != null)
            currentEndText.text = "End: " + currentEnd + "/" + totalEnds;
    }
}
