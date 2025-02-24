using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class KingMovement :  PieceMovement
{


    protected override bool tryAddMove(SqrPos sqrPos)
    {
        if (sqrPos.inBounds() && !GetComponent<PieceLogic>().game.GetComponent<GameLogic>().squareUnderThreat(
            GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(sqrPos.name), 
            GetComponent<PieceLogic>().isWhite))
            return base.tryAddMove(sqrPos);

        return false;
    }
    //takes in an amount of squares and the direction
    void castleLooper(bool isLong, bool checkingRight)
    {
        int dir = 1;
        int amount = 3;
        if (!isLong)
            amount = 2;
        if(!checkingRight) 
            dir = -1;
        

        SqrPos sqrChecker = currentSquarePos;
        for (int i = 0; i< amount; i++)
        {
            sqrChecker.Add(dir, 0);

            //king hasn't moved so these squares are assumed to exist
            GameObject sqr = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(sqrChecker.name);
            var potentialEncumbent = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getFindPieceOnSquare(sqr);
            //can't castle through check if its the 1st or second square(only called on 0 and 1),
            //third can be castled through though
            if(i < 2 &&potentialEncumbent != null || GetComponent<PieceLogic>().game.GetComponent<GameLogic>().squareUnderThreat(sqr, GetComponent<PieceLogic>().isWhite)) 
            {
                return;
            }
            
        }

        //we through loop and on rook square

        sqrChecker.Add(dir, 0);
        var potentialRook = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getFindPieceOnSquare(GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(sqrChecker.name));

        if (potentialRook != null)
        {
            if (potentialRook.GetComponent<PieceLogic>().isWhite == GetComponent<PieceLogic>().isWhite 
                && !potentialRook.GetComponent<PieceLogic>().hasMovedYet)
            {
                //okay so add the square thats one in teh opposite direction of dir
                //1 away if short and 2 away if long
                SpecialMoveType castleType = SpecialMoveType.SHORT_CASTLE;
                if(isLong)
                {
                    castleType = SpecialMoveType.LONG_CASTLE;   
                    sqrChecker.Add(-2 * dir, 0);
                }
                    
                else
                    sqrChecker.Add(-dir, 0);

                moves.Add(new Move(gameObject, GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(sqrChecker.name), PieceType.NONE, castleType));
            }
                
        }

    }


    void castleVerify()
    {
        if (GetComponent<PieceLogic>().game.GetComponent<GameLogic>().whitePlayer.GetComponent<PlayerLogic>().isMain)
        {
            //castle directions based on whether black or white is main
            //if white is main, short castle right and long catle left
            //if black is main, short is left and long is right

            castleLooper(false, true);

            //check two square to the right and the third is a friendly rook who hasnt moved yet

            castleLooper(true, false);
        }
        else
        {
            castleLooper(true, true); 

            castleLooper(false, false);
        }

    }


    override public List<Move> generateMoves()
    {
        moves.Clear();

        //we fiest need the square we are on
        currentSquarePos = GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;

        singleMove(1, 1, tryAddMove);
        singleMove(-1, 1, tryAddMove);
        singleMove(1, -1, tryAddMove);
        singleMove(-1, -1, tryAddMove);
        singleMove(0, 1, tryAddMove);
        singleMove(0, -1, tryAddMove);
        singleMove(1, 0, tryAddMove);
        singleMove(-1, 0, tryAddMove);

        //print("square we are on: " + currentSquarePos.name);


        //check for castling, cant have moved yet and cant be under threat(in check)

        if (!GetComponent<PieceLogic>().hasMovedYet  
            && !GetComponent<PieceLogic>().game.GetComponent<GameLogic>().squareUnderThreat(GetComponent<PieceLogic>().square, GetComponent<PieceLogic>().isWhite)) 
        {
            castleVerify();
        }


        return moves;
    }

    override public List<GameObject> generateThreats()
    {
        threats.Clear();

        //we fiest need the square we are on
        currentSquarePos = GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;

        singleMove(1, 1, tryAddThreat);
        singleMove(-1, 1, tryAddThreat);
        singleMove(1, -1, tryAddThreat);
        singleMove(-1, -1, tryAddThreat);
        singleMove(0, 1, tryAddThreat);
        singleMove(0, -1, tryAddThreat);
        singleMove(1, 0, tryAddThreat);
        singleMove(-1, 0, tryAddThreat);
        return threats;
    }

  
}
