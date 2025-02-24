using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum PieceType
{
    PAWN,
    BISHOP,
    KNIGHT,
    ROOK,
    QUEEN,
    KING,
    NONE
}

public abstract class PieceMovement : MonoBehaviour
{
    protected SqrPos currentSquarePos = new SqrPos();
    protected List<Move> moves = new List<Move>();
    protected List<GameObject> threats = new List<GameObject>();
    protected bool movedYet = false;
    
    //returns a list of moves the piece can do
    abstract public List<Move> generateMoves();
    //returns squares a piece can attack
    abstract public List<GameObject> generateThreats();
    //square we moved to
    public void postMoveUpdate()
    {
        movedYet = true;
    }

    //no special move handle
    //only works for rooks, bishops, queens, knights
     virtual protected bool tryAddMove(SqrPos sqrPos)
    {
        if (sqrPos.inBounds())
        {
            //if (gameLog == null)
            //    gameLog = GetComponent<PieceLogic>().game.GetComponent<GameLogic>();

            GameObject sqr = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(sqrPos.name);
            var encumbent = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getFindPieceOnSquare(sqr);
            PieceType pieceType = PieceType.NONE;
            if (encumbent != null)
                pieceType = encumbent.GetComponent<PieceLogic>().type;
            moves.Add(new Move(gameObject, sqr, pieceType));

            //stop going if we hita piece
            if (encumbent != null)
                return false;
            else
                return true;
        }
        return false;
    }


    virtual protected bool tryAddThreat(SqrPos sqrPos)
    {
        if (sqrPos.inBounds())
        {
            GameObject sqr = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().fetchSquare(sqrPos.name);
            var encumbent = GetComponent<PieceLogic>().game.GetComponent<GameLogic>().getFindPieceOnSquare(sqr);

            threats.Add(sqr);

            //stop going if we hita piece
            if (encumbent != null)
                return false;
            else
                return true;
        }
        return false;
    }

    protected void straightDirectionMoves(int xDir, int yDir, Func<SqrPos, bool> func)
    {
        SqrPos possibleSquarePos = currentSquarePos;
        //max distance of 8
        for (int i = 0; i < 8; i++)
        {
            possibleSquarePos.Add(xDir, yDir);
            if (!func(possibleSquarePos))
                break;
            else
                continue;
        }
    }

    //generate potential squares in each direction until we hit a piece
    protected void singleMove(int x, int y, Func<SqrPos, bool> func)
    {
        SqrPos possibleSquarePos = currentSquarePos;
        possibleSquarePos.Add(x, y);
        func(possibleSquarePos);
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

public class PieceLogic : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject game;
    public GameObject myCursor;
    private ClickToMove myCursorLogic;
    private bool beingHeld = false;


    public GameObject square;
    public GameObject previousSquare = null;
    public PieceType type;
    public bool isWhite = true;

    //if we want to generate threats but exclude specific peices, we can hide them
    public bool hidden = false;

    public bool hasMovedYet = false;
    public List<Move> possibleMoves = new List<Move>();
    public List<GameObject> possibleThreats;
    private Move selectedMove = new Move();


    public Color whiteColorMultiplier;
    public Color blackColorMultiplier;

   bool clickedSquareInPossibleMoves(GameObject sqr)
   {
        foreach (var m in possibleMoves)
        {
            if (m.finishSquare.name == sqr.name)
            {
                //we're allowed to move
                selectedMove = m;
                return true; 
            }

        }
        return false;
    }


    //called on start of new turn

    //generate possible moves based on movement then cull out illegals
    public void generatePossibleMoves()
    {
        possibleMoves.Clear();
        possibleMoves = GetComponent<PieceMovement>().generateMoves();
        //passing by non-const reference expecting mutation
        game.GetComponent<GameLogic>().weedOutIllegalMoves(ref possibleMoves, gameObject);
    }

    public void generatePossibleThreats()
    {
        possibleThreats.Clear();
        possibleThreats = GetComponent<PieceMovement>().generateThreats();
    }


    //assumed valid move square
    public bool generatedTestMoveStopCheck(GameObject testSquare)
    {
        var encumbent = game.GetComponent<GameLogic>().getFindPieceOnSquare(testSquare);
        if (encumbent != null)
        {
            //there is an enemy pei
            //
            //piece here, we know its enemy because by this point friendly pieces have been removed
            encumbent.GetComponent<PieceLogic>().hidden = true;
        }
        //lets first check if this move would be a capture
   
        GameObject realSquare = square;
        square = testSquare;

        //generate a new set of threats from opponent
       bool check =  game.GetComponent<GameLogic>().testIfWeInCheckFuture(gameObject);
        square = realSquare;
        if(encumbent != null)
            encumbent.GetComponent<PieceLogic>().hidden = false;

        return !check;
    }


    void returnPieceLogic(bool successful = false)
    {
        //return piece to where it was picked up from if we right click or just also put pice down logic

        //we need to know if we actually moved to know which square the color needs to be changed back for
        if(successful)
            previousSquare.GetComponent<GridElementLogic>().setHighlight(false); 
        else
            square.GetComponent<GridElementLogic>().setHighlight(false);


        foreach (Move m in possibleMoves)
        {      
            m.finishSquare.GetComponent<GridElementLogic>().previewType = PreviewType.NONE;
        }
        GetComponent<SpriteRenderer>().sortingLayerName = "pieces";
        beingHeld = false;
        myCursorLogic.putPieceDown();
        transform.position = square.transform.position;
    }

    void updateSquaresOnMove(GameObject arrivalSquare)
    {
        previousSquare = square;
        square = arrivalSquare;
        hasMovedYet = true;
    }


    public void successfulMove(GameObject arrivalSquare)
    {
        updateSquaresOnMove(arrivalSquare);
        returnPieceLogic(true);
        GetComponent<PieceMovement>().postMoveUpdate();
    }

    //for atypical moving, like castling 
    public void forcedMove(GameObject arrivalSquare)
    {
        updateSquaresOnMove(arrivalSquare);
        GetComponent<PieceMovement>().postMoveUpdate();
        transform.position = square.transform.position;
    }

    void attemptMove()
    {
        if (clickedSquareInPossibleMoves(myCursorLogic.lastSquareHovered))
        {
            game.GetComponent<GameLogic>().moveMade(selectedMove);
        }
        else
            returnPieceLogic();

    }

    void displayMoveOptions()
    {
        //display legal moves

        //highlight current square

        square.GetComponent<GridElementLogic>().setHighlight(true);

        for (int i = 0; i < possibleMoves.Count; i++)
        {
            
            if (!possibleMoves[i].wasMaterialCaptured)
                possibleMoves[i].finishSquare.GetComponent<GridElementLogic>().previewType = PreviewType.MOVE;
            else
                possibleMoves[i].finishSquare.GetComponent<GridElementLogic>().previewType = PreviewType.CAPTURE;

        }
    }


    void updateWhenHeld()
    {
        transform.position = myCursor.transform.position;
        GetComponent<SpriteRenderer>().sortingLayerName = "pieceHeld";
        //if we're held, left click again on valid place will put us down
        displayMoveOptions();
        
        if (Input.GetMouseButtonDown(0))
        {
            //first test if we are actually over a square or we left the board be checking if we still hovering
            if(myCursorLogic.containsCursor(myCursorLogic.lastSquareHovered.GetComponent<Collider2D>()))
            {
                //okay we are left clicking and we are in square

                attemptMove();
            }
            else
            {
                //we are trying to pace the piece in an invalid location so just do what right click does
                returnPieceLogic();
            }
                
        }
        else if (Input.GetMouseButtonDown(1))
        {
            returnPieceLogic();
        }
    }
    void Start()
    {
        if (isWhite)
        {
            GetComponent<SpriteRenderer>().color *= whiteColorMultiplier;
        }
        else
        {
            GetComponent<SpriteRenderer>().color *= blackColorMultiplier;
        }
           
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D collider = GetComponent<Collider2D>();
        myCursorLogic = myCursor.GetComponent<ClickToMove>();

        if (beingHeld)
        {
            updateWhenHeld();
        }
        else if (myCursorLogic.abletoToHoldPiece() && game.GetComponent<GameLogic>().isThisPlayerAllowed(gameObject))
        {
            //if there is not a piece being held right now
            if (collider.bounds.Contains(myCursor.transform.position) && Input.GetMouseButtonDown(0))
            {
                //piece is now being held
                beingHeld = true;
                myCursorLogic.pickUpPiece();
            }
        }
    }
}
