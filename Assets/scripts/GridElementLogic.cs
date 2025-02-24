using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public struct SqrPos
{
    public SqrPos(char x, int y)
    {
        this.x = x;
        this.y = y;
        name = x + y.ToString();
    }

    public SqrPos(string name)
    {
        this.name = name;
        x = name[0];
        y = Convert.ToInt32(name[1].ToString());
    }

    public void Add(int x, int y)
    {
    
        int aski = this.x;
        aski += x;
        this.x =(char)aski;
        this.y += y;
        this.name = this.x + this.y.ToString();
    }

    //checks if valid squrpos in chessboard name like h8 and checks if letter is between a nnd h and num between 1 and 8
    public bool inBounds()
    {
        int aski = x;
      
        if (aski >= 97 && aski <= 104 && y >= 1 && y <= 8)
            return true;
        return false;
    }

    public char x;
    public int y;
    public string name;
}

public enum PreviewType
{
    MOVE,
    CAPTURE,
    NONE
}



public class GridElementLogic : MonoBehaviour
{
   public GameObject myCursor;
    public GameObject outline;
    public GameObject movePreviewSquare;
    public GameObject capturePreviewSquare;
    public float alphaOfOutline = 0.5f;
    public float alphaOfMovePreview = 0.5f;
    public float alphaOfCapturePreview = 0.5f;
    public Color selectedSquareColor;

    private Collider2D coll;
    public SqrPos sqrPos;
    public Color trueColor;
    public PreviewType previewType = PreviewType.NONE;


    public void setHighlight(bool b)
    {
       if(b)
            GetComponent<SpriteRenderer>().color = selectedSquareColor;
       else
            GetComponent<SpriteRenderer>().color = trueColor;
    }

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
        sqrPos = new SqrPos(name);
    }

    // Update is called once per frame
    void Update()
    {
        SpriteRenderer sp = outline.GetComponent<SpriteRenderer>();
        ClickToMove cursorLogic = myCursor.GetComponent<ClickToMove>();

        if (cursorLogic.containsCursor(GetComponent<Collider2D>()))
        { 
            cursorLogic.lastSquareHovered = gameObject;

            if(cursorLogic.isHoldingPiece())
            {
                sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, alphaOfOutline);
            }
            else
                sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, 0.0f);

        }
        else
            sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, 0.0f);


        if(previewType == PreviewType.MOVE)
        {
            SpriteRenderer mps = movePreviewSquare.GetComponent<SpriteRenderer>();
            mps.color = new Color(mps.color.r, mps.color.g, mps.color.b, alphaOfMovePreview);
        }
        else if(previewType == PreviewType.CAPTURE) 
        {
            SpriteRenderer mcs = capturePreviewSquare.GetComponent<SpriteRenderer>();
            mcs.color = new Color(mcs.color.r, mcs.color.g, mcs.color.b, alphaOfCapturePreview);
        }
        else
        {
            SpriteRenderer mps = movePreviewSquare.GetComponent<SpriteRenderer>();
            mps.color = new Color(mps.color.r, mps.color.g, mps.color.b, 0.0f);

            SpriteRenderer mcs = capturePreviewSquare.GetComponent<SpriteRenderer>();
            mcs.color = new Color(mcs.color.r, mcs.color.g, mcs.color.b, 0.0f);
        }
    }
}
