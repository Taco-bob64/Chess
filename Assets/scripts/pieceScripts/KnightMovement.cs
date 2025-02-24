using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightMovement : PieceMovement
{
  
    override public List<Move> generateMoves()
    {
        //refresh moves
        moves.Clear();

        //we fiest need the square we are on
        currentSquarePos = GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        //print("square we are on: " + currentSquarePos.name);
       singleMove(1, 2, tryAddMove);
        singleMove(-1, 2, tryAddMove);
        singleMove(1, -2, tryAddMove);
        singleMove(-1, -2, tryAddMove);
        singleMove(2, 1, tryAddMove);
        singleMove(-2, -1, tryAddMove);
        singleMove(-2, 1, tryAddMove);
        singleMove(2, -1, tryAddMove);

        return moves;
    }

    override public List<GameObject> generateThreats()
    {
        //refresh moves
        threats.Clear();

        //we fiest need the square we are on
        currentSquarePos = GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        //print("square we are on: " + currentSquarePos.name);
        singleMove(1, 2, tryAddThreat);
        singleMove(-1, 2, tryAddThreat);
        singleMove(1, -2, tryAddThreat);
        singleMove(-1, -2, tryAddThreat);
        singleMove(2, 1, tryAddThreat);
        singleMove(-2, -1, tryAddThreat);
        singleMove(-2, 1, tryAddThreat);
        singleMove(2, -1, tryAddThreat);
        return threats;
    }
}

