using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromotionPopupHandler : MonoBehaviour
{

    public GameObject rookHover;
    public GameObject bishopHover;
    public GameObject knightHover;
    public GameObject queenHover;



    public void activate()
    {
        gameObject.SetActive(true);      
    }

    public void deactivate()
    {
        gameObject.SetActive(false);
    }

    public PieceType promotionType()
    {
        if (rookHover.GetComponent<HoverDisplay>().acknowledgeClickedOn())
        {
            deactivate();
            return PieceType.ROOK;
            
        }
        else if (knightHover.GetComponent<HoverDisplay>().acknowledgeClickedOn())
        {
            deactivate();
            return PieceType.KNIGHT;
        }
        else if (queenHover.GetComponent<HoverDisplay>().acknowledgeClickedOn())
        {
            deactivate();
            return PieceType.QUEEN;
        }
        else if (bishopHover.GetComponent<HoverDisplay>().acknowledgeClickedOn())
        {
            deactivate();
            return PieceType.BISHOP;
        }
        else
        {
            return PieceType.NONE;
        }
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


