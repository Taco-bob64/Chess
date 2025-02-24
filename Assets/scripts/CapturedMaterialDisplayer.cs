using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CapturedMaterialDisplayer : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject rook;
    public GameObject pawn;
    public GameObject knight;
    public GameObject bishop;
    public GameObject queen;
    public GameObject matAdvantageDisplay;
    private Vector3 startingPosition;
    

    //current position where each shoudl be placed
    //left edge
    private Vector3 pawnStartPos;
    //1/3 in
    private Vector3 knightStartPos;
    private Vector3 bishopStartPos;
    private Vector3 rookStartPos;
    private Vector3 queenStartPos;


    private float pawnOffset;
    private float otherOffset;

    private int totalMaterial = 0;

    public Color darkColor;
    public Color lightColor;


    public int getTotalMaterial()
    {
        return totalMaterial;
    }

    public void displayMaterialAdvantage(int advantage)
    {
        if(advantage > 0)
            matAdvantageDisplay.GetComponent<TextMeshProUGUI>().text = "+ " + advantage.ToString();
        else
        {
            matAdvantageDisplay.GetComponent<TextMeshProUGUI>().text = "";
        }
    }


    public void addCapturdMaterial(PieceType pieceType, bool isWhite)
    {
        Color color = darkColor;
        if (isWhite)
            color = lightColor;

        GameObject newMini;
        switch(pieceType)
        {
            case PieceType.BISHOP:
            {
                     newMini = Instantiate(bishop, bishopStartPos, Quaternion.identity);

                    bishopStartPos.x += otherOffset;
                    totalMaterial += 3;
                   
                break;
            }
            case PieceType.KNIGHT:
            {
                    newMini = Instantiate(knight, knightStartPos, Quaternion.identity);
                    knightStartPos.x += otherOffset;
                    totalMaterial += 3;
                    break;
            }
                case PieceType.QUEEN:
            {
                    newMini = Instantiate(queen, queenStartPos, Quaternion.identity);
                    queenStartPos.x += otherOffset;
                    totalMaterial += 9;
                    break;
            }
                case PieceType.ROOK:
            {
                    newMini = Instantiate(rook, rookStartPos, Quaternion.identity);
                    rookStartPos.x += otherOffset;
                    totalMaterial += 5;
                    break;
            }
            default:
                
            {
                    newMini = Instantiate(pawn, pawnStartPos, Quaternion.identity);
                    pawnStartPos.x += pawnOffset;
                    totalMaterial += 1;
                    break;
            }
            
        }
        newMini.GetComponent<SpriteRenderer>().color = color;
    }

    void Start()
    {
        startingPosition = transform.position;
        startingPosition.x -= transform.lossyScale.x * 0.5f - pawn.transform.lossyScale.x * 0.85f;
        startingPosition.y += transform.lossyScale.y * 0.4f;
        otherOffset = pawn.transform.lossyScale.x * 0.78f;
        pawnOffset = pawn.transform.lossyScale.x * 0.45f;

        //left edge
        pawnStartPos = startingPosition;
        //1/3 in
        knightStartPos = startingPosition;
        knightStartPos.x += transform.lossyScale.x * 0.333f;
        //all next 3 are 1/6 in
        bishopStartPos = knightStartPos;
        bishopStartPos.x += transform.lossyScale.x * 0.165f;

        rookStartPos = bishopStartPos;
        rookStartPos.x += transform.lossyScale.x * 0.165f;

       queenStartPos = rookStartPos;
        queenStartPos.x += transform.lossyScale.x * 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
