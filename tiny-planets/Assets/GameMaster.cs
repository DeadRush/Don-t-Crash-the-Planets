using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public GameObject restartPanel;
    public Text score;
    private bool asLost;
    public float timer;

    private void Update()
    {
        
        if (asLost == false)
        {
            timer += Time.deltaTime;
            score.text = Mathf.RoundToInt(timer).ToString();
        }

    }

    public void GameOver()
    {
        int highScore = PlayerPrefs.GetInt("Highscore",0);
        if (Mathf.RoundToInt(timer) > highScore)
        {
            PlayerPrefs.SetInt("Highscore", (int)timer);
        }
        timer = 0;
        asLost = true;
        Invoke("Delay", 1.5f);
        
    }

    void Delay()
    {
        restartPanel.SetActive(true);
    }
  
}