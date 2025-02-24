using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RookMovement : PieceMovement
{
    override public List<Move> generateMoves()
    {
        //refresh moves
        moves.Clear();
        //we fiest need the square we are on
        currentSquarePos = GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        //print("square we are on: " + currentSquarePos.name);
        straightDirectionMoves(1, 0, tryAddMove);
        straightDirectionMoves(0, -1, tryAddMove);
        straightDirectionMoves(-1, 0, tryAddMove);
        straightDirectionMoves(0 , 1, tryAddMove);

        return moves;
    }

    override public List<GameObject> generateThreats()
    {
        threats.Clear();
        //we fiest need the square we are on
        currentSquarePos = GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        //print("square we are on: " + currentSquarePos.name);
        straightDirectionMoves(1, 0, tryAddThreat);
        straightDirectionMoves(0, -1, tryAddThreat);
        straightDirectionMoves(-1, 0, tryAddThreat);
        straightDirectionMoves(0, 1, tryAddThreat);
        return threats;
    }

 
}
