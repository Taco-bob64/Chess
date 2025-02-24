using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    WON,
    LOST,
    DRAW,
    PLAYING
}
public class PlayerLogic : MonoBehaviour
{
    public bool isWhite = true;
    //is this the player that is on the bottom of the board(typically the human)
    public bool isMain = false;
    public double clockTime = 0;
    public bool myTurn = false;
    public bool inCheck = false;
    public bool isBot;
    public PlayerState state = PlayerState.PLAYING;
    public List<GameObject> friendlymaterial = new List<GameObject>();
    public List<GameObject> threatenedSquares = new List<GameObject>();
    public List<Move> possibleMoves = new List<Move>();
    public List<PieceType> captureMaterial = new List<PieceType>();
    private bool movesGenerated = false;
    public GameObject clock;
    // Start is called before the first frame update

  public void startOfTurn()
    {
        if(clock != null && clock.GetComponent<clockController>().inUse)
        {
            clock.GetComponent<clockController>().unPauseClock();
        }


        possibleMoves.Clear();  
        foreach (var piece in friendlymaterial)
        {
            piece.GetComponent<PieceLogic>().generatePossibleMoves();

            foreach (var move in piece.GetComponent<PieceLogic>().possibleMoves)
            {
                possibleMoves.Add(move);
            }
        }
        movesGenerated = true;

        if (possibleMoves.Count == 0) // no possible moves either checkmate or stalemate
        {
            //if we're in check its mate

            if (inCheck)
            {
                state = PlayerState.LOST;

            }
            else
            {
                state = PlayerState.DRAW;
            }
        }
    }


    public void endOfTurn()
    {
    
            threatenedSquares.Clear();
            //we're ging to accept duplicates for noew to save performace, i dont think this will affect the logic
            foreach (var piece in friendlymaterial)
            {
                piece.GetComponent<PieceLogic>().generatePossibleThreats();
                //print(piece.name + "possible moves: ");

                foreach (var sqr in piece.GetComponent<PieceLogic>().possibleThreats)
                {
                    threatenedSquares.Add(sqr);
                }
            }

        if (clock != null && clock.GetComponent<clockController>().inUse)
        {
            clock.GetComponent<clockController>().pauseClock();
        }
    }

    public void generateTestThreats(ref List<GameObject> listRef)
    {
        
        foreach (var piece in friendlymaterial)
        {
            //if we are not hidden, 
            if (!piece.GetComponent<PieceLogic>().hidden)
            {
                piece.GetComponent<PieceLogic>().generatePossibleThreats();
                //print(piece.name + "possible moves: ");

                foreach (var sqr in piece.GetComponent<PieceLogic>().possibleThreats)
                {
                    listRef.Add(sqr);
                }
            }

        }
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //if we are a bot, this is where we decide on moves, max of 1 move decision perframe
        if(isBot && myTurn && state == PlayerState.PLAYING && movesGenerated)
        {
            if (possibleMoves.Count > 0)
            {
                //pick a move from possible moves using ai
                Move myMove = BotManager.Instance.getBotsMove(isMain, ref possibleMoves);
                //playMove
                myMove.piece.GetComponent<PieceLogic>().game.GetComponent<GameLogic>().moveMade(myMove);
            }
            else
            {
                print("no moves to makes");
                if (inCheck)
                    state = PlayerState.LOST;
                else
                    state = PlayerState.DRAW;
            }
            movesGenerated = false;
        }
       
    }

}

