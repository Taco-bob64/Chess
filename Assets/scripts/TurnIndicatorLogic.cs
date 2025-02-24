using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnIndicatorLogic : MonoBehaviour
{
    public GameObject whitePlayer;
    public GameObject blackPlayer;

    private bool isWhite = true;
    public Color blackColor = Color.black;
    public Color whiteColor = Color.white;
    private bool firstTurn = true;

    public bool getIsWhiteTurn()
    {
        return isWhite;
    }

    public void changeTurn()
    {
        isWhite = !isWhite;
        if (isWhite)
        {
            GetComponent<SpriteRenderer>().color = whiteColor;
            whitePlayer.GetComponent<PlayerLogic>().myTurn = true;
            blackPlayer.GetComponent<PlayerLogic>().myTurn = false;
            blackPlayer.GetComponent<PlayerLogic>().endOfTurn();
        }
        else
        {
            GetComponent<SpriteRenderer>().color = blackColor;
            whitePlayer.GetComponent<PlayerLogic>().myTurn = false;
            blackPlayer.GetComponent<PlayerLogic>().myTurn = true;
            whitePlayer.GetComponent<PlayerLogic>().endOfTurn();
            
        }



    }
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().color = whiteColor;

    }

    // Update is called once per frame
    void Update()
    {
        if(firstTurn)
        {
            whitePlayer.GetComponent<PlayerLogic>().myTurn = true;
            blackPlayer.GetComponent<PlayerLogic>().myTurn = false;
            whitePlayer.GetComponent<PlayerLogic>().startOfTurn();
            blackPlayer.GetComponent<PlayerLogic>().endOfTurn();
            firstTurn = false;
        }
    }
}
