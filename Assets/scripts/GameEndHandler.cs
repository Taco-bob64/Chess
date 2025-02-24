using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameEndHandler : MonoBehaviour
{
    public GameObject gameEndText;
    public GameObject gameEndBackground;

    public GameObject restartButton;
    public GameObject exitButton;

    public Color defeatBackgroundColor;
    public Color defeatTextColor;

    public Color victoryBackgroundColor;
    public Color victoryTextColor;

    public Color drawBackgroundColor;
    public Color drawTextColor;

    public bool active = false;


    public void activate(PlayerState state)
    {
        active = true;
        gameObject.SetActive(true);
        var text = gameEndText.GetComponent<TextMeshProUGUI>();
        if (state == PlayerState.LOST)
        {
            gameEndBackground.GetComponent<SpriteRenderer>().color = defeatBackgroundColor;
            text.color = defeatTextColor;
            text.text = "Defeat";
        }
        else if (state == PlayerState.DRAW) 
        {
            gameEndBackground.GetComponent<SpriteRenderer>().color = drawBackgroundColor;
            text.color = drawTextColor;
            text.text = "Draw";
        }
        else
        {
            gameEndBackground.GetComponent<SpriteRenderer>().color = victoryBackgroundColor;
            text.color = victoryTextColor;
            text.text = "Victory!";
        }

    }

    public void deactivate()
    {
        gameObject.SetActive(false);
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
