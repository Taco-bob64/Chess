using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//needs to know opponents previous move to see for en passant
public class PawnMovement : PieceMovement 
{
    private int moveDir = 1;
    
    // possible mutates moves
    void checkForPromotion(ref Move m)
    {
        int destinationVal = 8;
        if (moveDir < 0)
            destinationVal = 1;

        if(m.finishSquare.GetComponent<GridElementLogic>().sqrPos.y == destinationVal)
        {
            //promotion
            m.specialMove = SpecialMoveType.PROMOTION;
            //now add a potential move that incles each type
            m.typePromotedTo = PieceType.BISHOP;
            moves.Add(m);
            m.typePromotedTo = PieceType.KNIGHT;
            moves.Add(m);
            m.typePromotedTo = PieceType.ROOK;
            moves.Add(m);
            m.typePromotedTo = PieceType.QUEEN;
            moves.Add(m);
        }
        else
        {
            moves.Add(m);
        }
    }


    override protected bool tryAddMove(SqrPos sqrPos)
    {
        if (sqrPos.inBounds())
        {
            //pawn can't take so check if squres are empty
            GameObject sqr = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(sqrPos.name);

            if(GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getFindPieceOnSquare(sqr) == null)
            {
                Move m = new Move(gameObject, sqr);
                //note: check for prmation hadles move adding to moves
                checkForPromotion(ref m);
                return true;
            }
                 
        }
        return false;
    }
    private void tryDiagonalMoves(int horizontalCheckDirection)
    {
        var possibleSquarePos = currentSquarePos;
        possibleSquarePos.Add(horizontalCheckDirection, moveDir);
        if (possibleSquarePos.inBounds())
        {
            GameObject potentialSquare = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(possibleSquarePos.name);
            GameObject potentialPiece = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getFindPieceOnSquare(potentialSquare);
            if (potentialPiece != null)
            {
                if(GetComponent<PieceLogic>().game.GetComponent<GameLogic>().isOpponentsColor(gameObject, potentialPiece))
                {
                    //yes we can
                    Move m = new Move(gameObject, potentialSquare, potentialPiece.GetComponent<PieceLogic>().type);
                    //note: check for prmation hadles move adding to moves
                    checkForPromotion(ref m);
                }
            }
        }
    }

    private void tryDiagonalThreats(int horizontalCheckDirection)
    {
        SqrPos possibleSquarePos = currentSquarePos;
        possibleSquarePos.Add(horizontalCheckDirection, moveDir);
        if (possibleSquarePos.inBounds())
        {
          //  print("diagonal threat in bounds");
          GameObject potentialSquare = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(possibleSquarePos.name);
          threats.Add(potentialSquare);
             
        }
    }

    private void enPassantAdd(SqrPos sqrPos)
    {
        //pawn can't take so check if squres are empty
        GameObject sqr = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(sqrPos.name);   
            moves.Add(new Move(gameObject, sqr, PieceType.PAWN, SpecialMoveType.EN_PASSANT));
    }

    private void enPassantCheck()
    {
        if (GetComponent<PieceLogic>().game.GetComponent<GameLogic>().firstMovePlayed)
        {
            Move lastMovePlayed = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getPreviousMovePlayed();

            if (lastMovePlayed.piece.GetComponent<PieceLogic>().type == PieceType.PAWN && Mathf.Abs(lastMovePlayed.CalculateDistance()) == 2)
            {
                var possibleSquarePos = currentSquarePos;
                SqrPos rPossible = currentSquarePos;
                rPossible.Add(1, 0);

                possibleSquarePos.Add(-1, 0);



                if (rPossible.name == lastMovePlayed.finishSquare.name)
                {
                    rPossible.Add(0, moveDir);
                   enPassantAdd(rPossible);
                }
                else if (possibleSquarePos.name == lastMovePlayed.finishSquare.name)
                {
                    possibleSquarePos.Add(0, moveDir);
                    enPassantAdd(possibleSquarePos);
                }

            }
        }
    }


    override public List<Move> generateMoves()
    {
        //refresh moves
        moves.Clear();
        
        //we fiest need the square we are on
        currentSquarePos = GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        //print("square we are on: " + currentSquarePos.name);
        SqrPos possibleSquarePos = currentSquarePos;

        //first generate 1 move in the direction toeards the other side

        if (!GetComponent<PieceLogic>().isWhite)
        {
            moveDir = -1;
        }

        possibleSquarePos.Add(0, moveDir);
        tryAddMove(possibleSquarePos);


        //if we havnt moved yet
        //we also have square 2 in front of us,
        if (!movedYet && possibleSquarePos.inBounds())
        {
            //make sure nothing directly in front of pawn
            GameObject sqr = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(possibleSquarePos.name);

            if (GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getFindPieceOnSquare(sqr) == null)
            {
                possibleSquarePos.Add(0, moveDir);
                tryAddMove(possibleSquarePos);
            }
           
        }



        //also check for en pessant
       enPassantCheck();

        //check for attacking diagonal positions

        tryDiagonalMoves(1);
        tryDiagonalMoves(-1);

        return moves;
    }

    override public List<GameObject> generateThreats()
    {
        threats.Clear();
        //we fiest need the square we are on
        currentSquarePos = GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        //first generate 1 move in the direction toeards the other side, if the piece's color is main(on the bottom), firection is +
        if (!GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getPlayerWithColor(GetComponent<PieceLogic>().isWhite).GetComponent<PlayerLogic>().isMain)
        {
            moveDir = -1;
        }

        tryDiagonalThreats(1);
        tryDiagonalThreats(-1);
        return threats;
    }


}
