using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Text HighscoreValue;
    // Start is called before the first frame update
    void Start()
    {
        var highscore = PlayerPrefs.GetInt("Highscore",0);
        Debug.Log("highscore:"+highscore);
        HighscoreValue.text = highscore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
