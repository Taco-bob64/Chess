using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class clockController : MonoBehaviour
{
    private bool paused = true;
    private double bonusTime = 0.0f;
    public  bool inUse = false;
    private double totalTime = 600.0f;
    private double currentTime = 600.0f;
    public bool gameStarted = false;

    public GameObject displayBox;
    public void setClock(double maxTime, double bonus)
    {
        totalTime = maxTime;
        bonusTime = bonus;
        currentTime = totalTime;
        inUse = true;
        paused = true;
    }

    public void unPauseClock()
    {
        if (gameStarted)
        {
            paused = false;
        }
    }

    public void pauseClock()
    {
        if (gameStarted)
        {
            paused = true;
            currentTime += bonusTime;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        displayBox.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(inUse) 
        {
            if (!paused)
            {
                currentTime -= Time.deltaTime;

                //get the minute value from time

            }
            int minutes = (int)(currentTime / 60.0);
            int seconds = (int)currentTime % 60;
            string strSeconds = seconds.ToString();
            if(seconds < 10)
                strSeconds = "0" + strSeconds;

            GetComponent<TextMeshProUGUI>().text = minutes + ":" + strSeconds;


        }
        displayBox.SetActive(false);
    }
}
