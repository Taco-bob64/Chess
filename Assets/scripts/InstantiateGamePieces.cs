using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class InstantiateGamePieces : MonoBehaviour
{
    public GameObject pawn;
    public GameObject bishop;
    public GameObject knight;
    public GameObject rook;
    public GameObject queen;
    public GameObject king;

    public GameObject game;
    public GameObject myCursor;
    public GameObject chessBoard;
    public GameObject blackPlayer;
    public GameObject whitePlayer;
    private Dictionary <string, GameObject> grid;
    public List<GameObject> gamePieces = new List<GameObject>();


    public GameObject instantiatePieceVars(GameObject piece, bool isWhite, string squareName)
    {
        GameObject newPiece = Instantiate(piece, grid[squareName].transform.position, Quaternion.identity);
        newPiece.GetComponent<PieceLogic>().isWhite = isWhite;
        newPiece.GetComponent<PieceLogic>().myCursor = myCursor;
        newPiece.GetComponent<PieceLogic>().square = grid[squareName];
        newPiece.GetComponent<PieceLogic>().game = game;
        gamePieces.Add(newPiece);

        if(isWhite)
        {
            whitePlayer.GetComponent<PlayerLogic>().friendlymaterial.Add(newPiece);
        }
        else
            blackPlayer.GetComponent<PlayerLogic>().friendlymaterial.Add(newPiece);

        return newPiece;
    }

    //assumes this is actually a piece
    public void deletePiece(GameObject piece) 
    {
      if (piece.GetComponent<PieceLogic>().isWhite)
           whitePlayer.GetComponent<PlayerLogic>().friendlymaterial.Remove(piece);
      else
           blackPlayer.GetComponent<PlayerLogic>().friendlymaterial.Remove(piece);
      gamePieces.Remove(piece);
      Destroy(piece);        
    }

    GameObject getPrefabFromPieceType(PieceType type)
    {
        switch(type)
        {
            case PieceType.BISHOP: return bishop;
            case PieceType.KNIGHT: return knight;
            case PieceType.ROOK: return rook;
            case PieceType.QUEEN: return queen;
            case PieceType.KING: return king;
            default: return pawn;
        }
    }

    public GameObject addPiece(PieceType type, GameObject square, bool isWhite)
    {
        return instantiatePieceVars(getPrefabFromPieceType(type), isWhite, square.name);
    }

    //assumes valid square
    //returns piece or null if no piece
    public GameObject findPieceOnSquare(GameObject square)
    {
        for(int i =0; i <  gamePieces.Count; i++)
        {
            if (gamePieces[i].GetComponent<PieceLogic>().square.name == square.name)
                return gamePieces[i];
        }
        return null;
    }
    // Start is called before the first frame update
    void Start()
    {
        grid = chessBoard.GetComponent<InstantiateGrid>().grid;
        //hard coded cause there isn't really a useful pattern
        
        //rooks
        instantiatePieceVars(rook, true, "a1");
        instantiatePieceVars(rook, true, "h1");
        instantiatePieceVars(rook, false, "a8");
        instantiatePieceVars(rook, false, "h8");

        //knights
        instantiatePieceVars(knight, true, "b1");
        instantiatePieceVars(knight, true, "g1");
        instantiatePieceVars(knight, false, "b8");
        instantiatePieceVars(knight, false, "g8");

        //bishops
        instantiatePieceVars(bishop, true, "c1");
        instantiatePieceVars(bishop, true, "f1");
        instantiatePieceVars(bishop, false, "c8");
        instantiatePieceVars(bishop, false, "f8");

        //queens
        instantiatePieceVars(queen, true, "d1");
        instantiatePieceVars(queen, false, "d8");

        //kings
        instantiatePieceVars(king, true, "e1");
        instantiatePieceVars(king, false, "e8");

        //pawns

        for(int i = 0; i < 8; i++)
        {
            char rank = 'a';
            switch (i)
            {
                case 0: rank = 'a'; break;
                case 1: rank = 'b'; break;
                case 2: rank = 'c'; break;
                case 3: rank = 'd'; break;
                case 4: rank = 'e'; break;
                case 5: rank = 'f'; break;
                case 6: rank = 'g'; break;
                case 7: rank = 'h'; break;
            }
            instantiatePieceVars(pawn, true, rank + "2");
            instantiatePieceVars(pawn, false, rank + "7");
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
