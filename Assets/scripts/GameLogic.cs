using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum SpecialMoveType
{
    EN_PASSANT,
    LONG_CASTLE,
    SHORT_CASTLE,
    PROMOTION,
    NONE
}

public struct Move
{
    public Move(GameObject movedPiece, GameObject arrivalSquare, PieceType encumbentPiece = PieceType.NONE, 
        SpecialMoveType special = SpecialMoveType.NONE, PieceType promotedTo = PieceType.NONE)
    {
        piece = movedPiece;
        isWhite = movedPiece.GetComponent<PieceLogic>().isWhite;
        startingSquare = movedPiece.GetComponent<PieceLogic>().square;
        finishSquare = arrivalSquare;
        specialMove = special;
        typePromotedTo = promotedTo;


        if (encumbentPiece != PieceType.NONE)
        {
            wasMaterialCaptured = true;
            capturedMaterialType = encumbentPiece;
        }
        else
        {
            capturedMaterialType = PieceType.NONE;
            wasMaterialCaptured= false;
        }
        name = "";

        if (specialMove == SpecialMoveType.SHORT_CASTLE)
            name = "O-O";
        else if (specialMove == SpecialMoveType.SHORT_CASTLE)
            name = "O-O-O";
        else
        {
            char movement = '-';
            if (wasMaterialCaptured)
                movement = 'x';

            string normalName = getStringFromPieceType(piece.GetComponent<PieceLogic>().type) + startingSquare.name + movement + arrivalSquare.name;

            if (specialMove == SpecialMoveType.PROMOTION)
            {
                normalName += "=" + getStringFromPieceType(promotedTo);
            }
            name = normalName;
        }
      


    }

    private string getStringFromPieceType(PieceType type)
    {
        switch (type)
        {
            case PieceType.ROOK:
                {
                    return "R";
                }
            case PieceType.KNIGHT:
                {
                   return "N";
                }
            case PieceType.BISHOP:
                {
                    return "B";
                }
            case PieceType.QUEEN:
                {
                    return "Q";
                }
            case PieceType.KING:
                {
                    return "K";
                }
            default:
                {
                    return ""; 
                }
        }
    }

    public int CalculateDistance()
    {
        //returns the longest value among teh differences between xs and ys,
        //this is basically the shortest path when diagonal movements are allowed
        SqrPos sqrPos1 = startingSquare.GetComponent<GridElementLogic>().sqrPos;
        SqrPos sqrPos2 = finishSquare.GetComponent<GridElementLogic>().sqrPos;

        int deltaX = sqrPos2.x - sqrPos1.x;
        int detlaY = sqrPos2.y - sqrPos1.y;

        if(Mathf.Abs(deltaX) > Mathf.Abs(detlaY)) 
        { return deltaX; }
        else { return detlaY; }
    }

    public bool isWhite;
    public GameObject piece;
    public GameObject startingSquare;
    public GameObject finishSquare;
    public bool wasMaterialCaptured;
    public PieceType capturedMaterialType;
    public SpecialMoveType specialMove;
    public PieceType typePromotedTo;
    public string name;
}


public class GameLogic : MonoBehaviour
{
    public GameObject chessBoard;
    public GameObject pieceHandler;
    public GameObject whitePlayer;
    public GameObject blackPlayer;
    public GameObject turnIndicator;

    public GameObject moveDisplayer;

    public GameObject mainMaterialCapturedTracker;
    public GameObject farMaterialCapturedTracker;


    public GameObject mainClock; //close one(the one on the bottom)
    public GameObject farClock;

    private bool clocksSetYet = false;
    public bool usingClocks;
    private double totalClockTime = 600.0f;
    private double bonusClockTime = 5.0f;

    public GameObject gameEndPopup;
    public GameObject promotionPopup;
    bool waitingForPromotion = false;
    Move stalledMoveFromPromotion = new Move();

    public GameObject buttonResign;


    //the piece the opponent just moved
    public bool firstMovePlayed = false;
    private Move previousMovePlayed;
    public Move getPreviousMovePlayed()
    {
        return previousMovePlayed;
    }


    void handleCapturedMaterial(ref Move m, GameObject encumbent)
    {

        if (m.isWhite)
            whitePlayer.GetComponent<PlayerLogic>().captureMaterial.Add(m.capturedMaterialType);
        else
            blackPlayer.GetComponent<PlayerLogic>().captureMaterial.Add(m.capturedMaterialType);

            pieceHandler.GetComponent<InstantiateGamePieces>().deletePiece(encumbent);

        GameObject trackerToAddTo = mainMaterialCapturedTracker;
        GameObject otherTracker = farMaterialCapturedTracker;

        if ((whitePlayer.GetComponent<PlayerLogic>().isMain && !m.isWhite) ||
            !whitePlayer.GetComponent<PlayerLogic>().isMain && m.isWhite)
        {
            trackerToAddTo = farMaterialCapturedTracker;
            otherTracker = mainMaterialCapturedTracker;
        }

        trackerToAddTo.GetComponent<CapturedMaterialDisplayer>().addCapturdMaterial(m.capturedMaterialType, !m.isWhite);

        int advantage = trackerToAddTo.GetComponent<CapturedMaterialDisplayer>().getTotalMaterial()
            - otherTracker.GetComponent<CapturedMaterialDisplayer>().getTotalMaterial();

        trackerToAddTo.GetComponent<CapturedMaterialDisplayer>().displayMaterialAdvantage(advantage);
        //other trackers advantage is the opposite
       otherTracker.GetComponent<CapturedMaterialDisplayer>().displayMaterialAdvantage(-advantage);



    }

    public void postMove(Move m)
    {



        moveDisplayer.GetComponent<MoveDisplayerLogic>().addMove(ref m);
        previousMovePlayed = m;

        if (firstMovePlayed == false)
        {
            firstMovePlayed = true;
            mainClock.GetComponent<clockController>().gameStarted = true;
            farClock.GetComponent<clockController>().gameStarted = true;
        }



        handleChangeTurns();
    }

    //only called when we actually move a piece
    //we've already confirmed its either an empty square or a square with empty material and its a legal move
    public void moveMade(Move move)
    {

       GameObject encumbentPiece = null;
        if (move.wasMaterialCaptured)
            encumbentPiece = getFindPieceOnSquare(move.finishSquare);


        //special move checks

        if (move.specialMove == SpecialMoveType.EN_PASSANT)
        {
            handleEnPassant(ref move, ref encumbentPiece);
        }
        else if (move.specialMove == SpecialMoveType.SHORT_CASTLE || move.specialMove == SpecialMoveType.LONG_CASTLE)
        {
            GameObject rook = getRookInCastle(ref move);
            handleCastle(ref move, rook);
        }

        //handle destruction of encumbent peice
        if (move.wasMaterialCaptured)
        {
            handleCapturedMaterial(ref move, encumbentPiece);
        }

        //move our piece to the square successfuly
        move.piece.GetComponent<PieceLogic>().successfulMove(move.finishSquare);

        if (move.specialMove == SpecialMoveType.PROMOTION)
        {
            //check if player is a bot or not, if they are a
            //bot they would have chosen already but if they are a player they need a selection prompt

            if (!getPlayerWithColor(move.isWhite).GetComponent<PlayerLogic>().isBot)
            {


                //stall until a piece is picked, we can't change the turns yet
                promotionPopup.GetComponent<PromotionPopupHandler>().activate();
                stalledMoveFromPromotion = move;
                waitingForPromotion = true;
                move.piece.GetComponent<PieceLogic>().myCursor.GetComponent<ClickToMove>().disablePiecePickup();
                return;
            }
            else
            {
                promotionPieceReplace(ref move);
            }
        }


        postMove(move);
    }


    void handleCastle(ref Move m, GameObject rook)
    {
        //find distance between rook and king to see if short or long
        SqrPos rPos = rook.GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        SqrPos kPos = m.piece.GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;

        int deltaX = rPos.x - kPos.x;
        //going in opposite direction to place the rook
        int dir = -1;
        if (deltaX < 0)
            dir = 1;
        SqrPos arrivalPos = m.finishSquare.GetComponent<GridElementLogic>().sqrPos;
        arrivalPos.Add(dir, 0);
        GameObject rooksNewSquare = fetchSquare(arrivalPos.name);

        //move rook

        rook.GetComponent<PieceLogic>().forcedMove(rooksNewSquare);
    }

    void handleEnPassant(ref Move m, ref GameObject encumbentPiece)
    {
        SqrPos startingSquarePos = m.piece.GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        SqrPos finalSquarePos = m.finishSquare.GetComponent<GridElementLogic>().sqrPos;

        int xDelta = finalSquarePos.x - startingSquarePos.x; //direction of where encumbent is

        startingSquarePos.Add(xDelta, 0);
        encumbentPiece = getFindPieceOnSquare(fetchSquare(startingSquarePos.name));
    }


    void handleChangeTurns()
    {
            
        turnIndicator.GetComponent<TurnIndicatorLogic>().changeTurn();

        //check for draw due to insufficient material

        checkForInsufficentMaterial();

        if(turnIndicator.GetComponent<TurnIndicatorLogic>().getIsWhiteTurn())
        {
            whitePlayer.GetComponent<PlayerLogic>().inCheck = checkIfInCheck(whitePlayer);
            whitePlayer.GetComponent<PlayerLogic>().startOfTurn();
        }
        else
        {
            blackPlayer.GetComponent<PlayerLogic>().inCheck = checkIfInCheck(blackPlayer);
            blackPlayer.GetComponent<PlayerLogic>().startOfTurn();
        }



    }

    GameObject getRookInCastle(ref Move m)
    {
        SqrPos sqrPos1 = m.piece.GetComponent<PieceLogic>().square.GetComponent<GridElementLogic>().sqrPos;
        SqrPos sqrPos2 = m.finishSquare.GetComponent<GridElementLogic>().sqrPos;

        int deltaX = sqrPos2.x - sqrPos1.x;

            int dir = 1;
            if (deltaX < 0)
                dir = -1;
            GameObject potentialRook = null;
            for (int i =0; i < 2; i++)
            {
                 sqrPos2.Add(dir, 0);
                 potentialRook = getFindPieceOnSquare(fetchSquare(sqrPos2.name));
                 if (potentialRook != null)
                    break;
            }
            return potentialRook;
    }





    public bool isOpponentsColor(GameObject myPiece, GameObject unknownPiece) 
    {
        if (myPiece.GetComponent<PieceLogic>().isWhite != unknownPiece.GetComponent<PieceLogic>().isWhite)
            return true;
        return false;
    }

    public bool isThisPlayerAllowed(GameObject piece)
    {
        GameObject player = getPlayerWithColor(piece.GetComponent<PieceLogic>().isWhite);
        if (player.GetComponent<PlayerLogic>().myTurn && !player.GetComponent<PlayerLogic>().isBot)
            return true;
        else
            return false;
    }

    public bool checkIfInCheck(GameObject player)
    {
        GameObject king = getKing(player);
        if (squareUnderThreat(king.GetComponent<PieceLogic>().square, king.GetComponent<PieceLogic>().isWhite))
            {
            //yes king is under threat
            return true;
           }
        return false;

    }

    bool findIfPieceIsInMaterial(ref List<GameObject> mats, PieceType type)
    {
        for (int i = 0; i < mats.Count; i++)
        {
            if (mats[i].GetComponent<PieceLogic>().type == type)
                return true;
        }
        return false;
    }

    bool insufficentFriendlyMaterial(ref List<GameObject> mats)
    {
        //all combos require less than three, we dont want to do logic unless its small
        if(mats.Count <= 2)
        {
            if(!findIfPieceIsInMaterial(ref mats, PieceType.PAWN) && !findIfPieceIsInMaterial(ref mats, PieceType.QUEEN)
                && !findIfPieceIsInMaterial(ref mats, PieceType.ROOK))
            {
                //no pawns, rooks, or queens, and king and something, so insufficient
                return true;
            }
        }
        return false;
    }

    void checkForInsufficentMaterial()
    {
        var whiteMats = whitePlayer.GetComponent<PlayerLogic>().friendlymaterial;
        var blackMats = blackPlayer.GetComponent<PlayerLogic>().friendlymaterial;

        //insufficent combinations
        //king vs king
        //king bishop vs. king bishop
        //king knight vs. king bishop
        //king bishop vs. king knight
        if(insufficentFriendlyMaterial(ref whiteMats) && insufficentFriendlyMaterial(ref blackMats))
        {
            whitePlayer.GetComponent<PlayerLogic>().state = PlayerState.DRAW;
            blackPlayer.GetComponent<PlayerLogic>().state = PlayerState.DRAW;
        }

    }
    public GameObject getKing(GameObject player)
    {
        foreach (var piece in player.GetComponent<PlayerLogic>().friendlymaterial)
        {
            if (piece.GetComponent<PieceLogic>().type == PieceType.KING)
            {
                return piece;
            }
        }
        return null;
    }

    //we want to know if teh piece being moved has made the player not in check or in check
    public bool testIfWeInCheckFuture(GameObject testPiece)
    {
        //generate a new threat list assuming piece is in new location
        List<GameObject> newThreats = new List<GameObject>();
        GameObject friendlyPlayer = blackPlayer;
        GameObject opponentPlayer = whitePlayer;

        if (testPiece.GetComponent<PieceLogic>().isWhite)
        {
            friendlyPlayer = whitePlayer;
            opponentPlayer = blackPlayer;
        }

        opponentPlayer.GetComponent<PlayerLogic>().generateTestThreats(ref newThreats);

        //find king square

        GameObject king = getKing(friendlyPlayer);

        //now check if king square is still under threat
        return squareUnderThreat(king.GetComponent<PieceLogic>().square, ref newThreats);
    }

    public bool squareUnderThreat(GameObject sqr, bool friendlyColorIsWhite)
    {
        //we want opposite sides threat
        ref List<GameObject> sqrs = ref whitePlayer.GetComponent<PlayerLogic>().threatenedSquares;

        if(friendlyColorIsWhite == true)
            sqrs = ref blackPlayer.GetComponent<PlayerLogic>().threatenedSquares;

        foreach (var s in sqrs)
        {
            if (s.name == sqr.name)
                return true;
        }

        return false;
    }

    //overload if we want to pass in our own list
    public bool squareUnderThreat(GameObject sqr, ref List<GameObject> sqrs)
    {
        foreach (var s in sqrs)
        {
            if (s.name == sqr.name)
                return true;
        }

        return false;
    }




    public GameObject fetchSquare(string name)
    {
        if(chessBoard.GetComponent<InstantiateGrid>().grid.ContainsKey(name))
        {
            return chessBoard.GetComponent<InstantiateGrid>().grid[name];
        }
        else {
            print("we returned null on key " + name);
            return null; }
    }


    void removeFriendlyObstructions(ref List<Move> moves, GameObject piece)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            GameObject potentialPiece = getFindPieceOnSquare(moves[i].finishSquare);
            if (potentialPiece != null)
            {
                if (!isOpponentsColor(piece, potentialPiece))
                {
                    //friendly so remove it
                    moves.RemoveAt(i);
                    i -= 1;

                }
            }
        }
    }

    void removeIllegalCheckMoves(ref List<Move> moves, GameObject piece)
    {
        //eliminate all moves that would result in king still being in threat 
        for (int i = 0; i < moves.Count; i++)
        {
            if (!piece.GetComponent<PieceLogic>().generatedTestMoveStopCheck(moves[i].finishSquare))
            {
                //this move does not prevent check, so remove it
                // print("In second  loop: remove at: " + i);
                moves.RemoveAt(i);

                i -= 1;

            }
        }
    }


    //modifies list of squares removing all that are illegal
    public void weedOutIllegalMoves(ref List<Move> moves, GameObject piece)
    {
        //weed out all squares with freindly material
        removeFriendlyObstructions (ref moves, piece);
       
        ///////////////////////////////////////check
       GameObject player = whitePlayer;
       if (!piece.GetComponent<PieceLogic>().isWhite)
            player = blackPlayer;

        if (player.GetComponent<PlayerLogic>().inCheck)
        {
            removeIllegalCheckMoves(ref moves, piece);
        }
        else if(squareUnderThreat(piece.GetComponent<PieceLogic>().square, piece.GetComponent<PieceLogic>().isWhite))
        {
            //we are not in check, so only check potentially pinned pieces
            //are we in a threat level square(potentially pinned)
            removeIllegalCheckMoves(ref moves, piece);
        }
    }



    public GameObject getFindPieceOnSquare(GameObject square)
    {
        return pieceHandler.GetComponent<InstantiateGamePieces>().findPieceOnSquare(square);
    }

    public GameObject getPlayerWithColor(bool isWhite)
    {
        if (isWhite)
            return whitePlayer;
        else
            return blackPlayer;
    }



 

    void setClocks()
    {
        //assign clocks to players
        if (whitePlayer.GetComponent<PlayerLogic>().isMain)
        {
            whitePlayer.GetComponent<PlayerLogic>().clock = mainClock;
            blackPlayer.GetComponent<PlayerLogic>().clock = farClock;
        }
        else
        {
            whitePlayer.GetComponent<PlayerLogic>().clock = farClock;
            blackPlayer.GetComponent<PlayerLogic>().clock = mainClock;
        }

        whitePlayer.GetComponent<PlayerLogic>().clock.GetComponent<clockController>().setClock(totalClockTime, bonusClockTime);
        blackPlayer.GetComponent<PlayerLogic>().clock.GetComponent<clockController>().setClock(totalClockTime, bonusClockTime);
        clocksSetYet = true;
    }

    //returns new piece
    GameObject promotionPieceReplace(ref Move m)
    {
        //replace with new piece at arrival square
        var newPiece = pieceHandler.GetComponent<InstantiateGamePieces>().addPiece(m.typePromotedTo, m.finishSquare, m.isWhite);

        newPiece.GetComponent<PieceLogic>().previousSquare = m.piece.GetComponent<PieceLogic>().previousSquare;
        newPiece.GetComponent<PieceLogic>().game = gameObject;

        //destroy pawn

        pieceHandler.GetComponent<InstantiateGamePieces>().deletePiece(m.piece);
       return newPiece;
    }

    void checkPromotionFinished()
    {
        PieceType type = promotionPopup.GetComponent<PromotionPopupHandler>().promotionType();
        if (type != PieceType.NONE)
        {
            Move m = stalledMoveFromPromotion;
            m = new Move(m.piece, m.finishSquare, m.capturedMaterialType, SpecialMoveType.PROMOTION, type);
            waitingForPromotion = false;

            //handle deletiona and replace
            GameObject newPiece = promotionPieceReplace(ref m);


            //endTurn now that things are correct
            postMove(m);

            newPiece.GetComponent<PieceLogic>().myCursor.GetComponent<ClickToMove>().enablePiecePickup();

        }
    }

    void checkGameOver()
    {
        GameObject mainPlayer = blackPlayer;
        GameObject otherPlayer = whitePlayer;
        if (whitePlayer.GetComponent<PlayerLogic>().isMain)
        {
            mainPlayer = whitePlayer;
            otherPlayer = blackPlayer;
        }

        PlayerState mainState = mainPlayer.GetComponent<PlayerLogic>().state;

        if (otherPlayer.GetComponent<PlayerLogic>().state == PlayerState.LOST)
            mainState = PlayerState.WON;
        else if(otherPlayer.GetComponent<PlayerLogic>().state == PlayerState.DRAW)
            mainState = PlayerState.DRAW;
        else if(buttonResign.GetComponent<HoverDisplay>().acknowledgeClickedOn())
            mainState = PlayerState.LOST;

        if(mainState != PlayerState.PLAYING)
        {
            gameEndPopup.GetComponent<GameEndHandler>().activate(mainState);
            BotManager.Instance.GetComponent<BotManager>().endOfGameReached(mainState);

            if(whitePlayer.GetComponent<PlayerLogic>().isBot && blackPlayer.GetComponent<PlayerLogic>().isBot)
            {
                //restart
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }     
    
    }
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(!clocksSetYet && usingClocks)
        {
            setClocks();
        }

        if (waitingForPromotion)
        {
            checkPromotionFinished();
        }

    

        if(gameEndPopup.GetComponent<GameEndHandler>().active)
        {
            if (gameEndPopup.GetComponent<GameEndHandler>().restartButton.GetComponent<HoverDisplay>().acknowledgeClickedOn())
            {
                //restart scene in really stupid way
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            }
            else if (gameEndPopup.GetComponent<GameEndHandler>().exitButton.GetComponent<HoverDisplay>().acknowledgeClickedOn())
            {

            }

        }
        else
            checkGameOver();

    }
}
