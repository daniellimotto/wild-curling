using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    public Vector3 targetCenter;
    public float targetRadius = 5f;
    public float minDistanceRed = float.MaxValue;
    public float minDistanceBlue = float.MaxValue;

    public KeyValuePair<Team, int> CalculateScore(){
        Stone[] stones = FindObjectsOfType<Stone>();
        Dictionary<Stone, float> stoneDistances = new Dictionary<Stone, float>();

        foreach (Stone stone in stones){
            float distance = Vector3.Distance(stone.transform.position, targetCenter);
            stoneDistances.Add(stone, distance);

            if (stone.team == Team.red && distance < minDistanceRed){
                minDistanceRed = distance;
            } else if (stone.team == Team.blue && distance < minDistanceBlue){
                minDistanceBlue = distance;
            }
        }
        if(minDistanceRed < minDistanceBlue){
            int points = 0;
            foreach (KeyValuePair<Stone, float> entry in stoneDistances){
                if (entry.Key.team == Team.red && entry.Value < minDistanceBlue){
                    points++;
                }
            }
            return new KeyValuePair<Team, int>(Team.red, points);
        }
        else if (minDistanceBlue < minDistanceRed){
            int points = 0;
            foreach (KeyValuePair<Stone, float> entry in stoneDistances){
                if (entry.Key.team == Team.blue && entry.Value < minDistanceRed){
                    points++;
                }
            }
            return new KeyValuePair<Team, int>(Team.blue, points);
        }
        else{
            return new KeyValuePair<Team, int>(Team.red, 0);
        }
    }
    void EndOfEndgame(){
        KeyValuePair<Team, int> score = CalculateScore();
        Debug.Log("Team " + score.Key + " scored " + score.Value + " points!");
    }
    

}
