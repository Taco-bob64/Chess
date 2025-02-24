using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UnityEngine;

using System.IO;



public class BotManager : MonoBehaviour
{
    private Bot mainBot = new Bot();
    private Bot farBot = new Bot();

    int numberOfGamesPlayed = 0;

    public static BotManager Instance;
    //yes im actually doing this
    private StringBuilder currentBoardState = new StringBuilder("RNBQKBNR" +
        "PPPPPPPP"+"00000000"+ "00000000" + "00000000" + "00000000"+ "pppppppp" + "rnbqkbnr");
      
    //this will be teh aski value of a which is basically our 1 value
    private int askiSubtractor = 'a';
    private int rowSize = 8;
    private const int CAPITAL_DISTANCE = -32;
    private const int H1_INDEX = 7;
    private const int A1_INDEX = 0;
    private const int H8_INDEX = 63;
    private const int A8_INDEX = 56;

    private Dictionary<string, Move> currentPairs = new Dictionary<string, Move>();
    public void endOfGameReached(PlayerState stateOfMain)
    {
        numberOfGamesPlayed++;
        print(numberOfGamesPlayed);
        if(stateOfMain == PlayerState.WON)
        {
            mainBot.provideFeedBackForCompletedActionsInStep(1.0f);
            farBot.provideFeedBackForCompletedActionsInStep(-1.0f);
        }
        else if(stateOfMain == PlayerState.LOST)
        {
            mainBot.provideFeedBackForCompletedActionsInStep(-1.0f);
            farBot.provideFeedBackForCompletedActionsInStep(1.0f);
        }
        else
        {
            mainBot.provideFeedBackForCompletedActionsInStep(-0.5f);
            farBot.provideFeedBackForCompletedActionsInStep(-0.5f);
        }

       
    }

    public Move getBotsMove(bool mainBotsMove, ref List<Move> moves)
    {
        Bot bot = farBot;
        if(mainBotsMove) 
        {
            bot = mainBot;
        }
        List<string> strings = convertMoves(ref moves);
        string action = bot.deterimineAction(ref strings);
        currentBoardState = new StringBuilder(action.Substring(0, action.Length - 1));
        return currentPairs[action];
    }

    

    int getStringBoardIndex(string squareName)
    {
        //a1 is 0 and h8 is 63
        //we need an x value to start as 1 so when we multiply be the row things go okay
        int xValue = squareName[0] - askiSubtractor;
     
        //for example 'a' here would be 1
        int rowAdd = rowSize * (Convert.ToInt32(squareName[1].ToString())-1);

        return xValue + rowAdd;  
    }

    char getCharRepresentationOfPiece(PieceType pieceType, bool isWhite)
    {
        char c = 'p';
        switch(pieceType) 
        {
            case PieceType.BISHOP:
                c = 'b'; break;
            case PieceType.KNIGHT:
                c = 'n'; break;
            case PieceType.KING:
                c = 'k'; break;
            case PieceType.QUEEN:
                c = 'q'; break;
            case PieceType.ROOK:
                c = 'r'; break;
            default:
                break;
        }

        if (isWhite)
            c = (char)(c + CAPITAL_DISTANCE);
        return c;
    }
    

    string convertChessGameMoveToStringData(Move m)
    {
        //65 char string, always starting from a1 to h8
        //assume move has all the correct data

        StringBuilder newBoardState = new StringBuilder(currentBoardState.ToString());
        newBoardState[getStringBoardIndex(m.startingSquare.name)] = '0';
        //will autamatically capture apiece just be the nature of it
        newBoardState[getStringBoardIndex(m.finishSquare.name)] = getCharRepresentationOfPiece(m.piece.GetComponent<PieceLogic>().type, m.isWhite);

        if(m.specialMove != SpecialMoveType.NONE)
        {
            char rookChar = 'r';
            if (m.piece.GetComponent<PieceLogic>().isWhite)
                rookChar = 'R';

            //for castling, rook has to be on edge of board
            //short castle always right cause we from whites perspective

            if (m.specialMove == SpecialMoveType.SHORT_CASTLE)
            {
                if (m.startingSquare.name[1] == '1')
                {
                    newBoardState[H1_INDEX] = '0';
                    newBoardState[H1_INDEX - 2] = rookChar;
                }
                else
                {
                    newBoardState[H8_INDEX] = '0';
                    newBoardState[H8_INDEX - 2] = rookChar;
                }

            }
            else if(m.specialMove == SpecialMoveType.LONG_CASTLE)
            {
                if (m.startingSquare.name[1] == '1')
                {
                    newBoardState[A1_INDEX] = '0';
                    newBoardState[A1_INDEX + 3] = rookChar;
                }
                else
                {
                    newBoardState[A8_INDEX] = '0';
                    newBoardState[A8_INDEX + 3] = rookChar;
                }
            }
            else if(m.specialMove == SpecialMoveType.EN_PASSANT)
            {
                //remove pawn from en pessant
                char c = m.finishSquare.name[0];
                char b = m.startingSquare.name[1];
                string temp ="" + c + b;

                newBoardState[getStringBoardIndex(temp)] = '0';
            }
            else if(m.specialMove == SpecialMoveType.PROMOTION)
            {
                newBoardState[getStringBoardIndex(m.finishSquare.name)] = getCharRepresentationOfPiece(m.typePromotedTo, m.isWhite);
            }
        }
        char color = 'b';
        if (m.isWhite)
            color = 'w';
        newBoardState.Append(color);
        return newBoardState.ToString();
    }

     List<string> convertMoves(ref List<Move> moves)
    { 
        currentPairs.Clear();
        List<string> strings = new List<string>();
        for (int i =0; i < moves.Count; i++)
        {
            string s = convertChessGameMoveToStringData(moves[i]);
            if (currentPairs.ContainsKey(s))
            {
            }
            else
            {
                currentPairs.Add(s, moves[i]);
                strings.Add(s);
            }

        }
        return strings;
        
    }



    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        mainBot.readNewOldDicDataToDictionary();
    }

    private void OnDestroy()
    {
        mainBot.writeNewDicDataToFile();
    }
}
