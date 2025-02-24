using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ClickToMove : MonoBehaviour
{
    public Camera cam;
    public GameObject heldPiece = null;
    public GameObject lastSquareHovered = null;
    private bool holdingPiece = false;
    private bool disabledFromPickingUp = false;



    //to stop piece swapping when putting down or picking pieces
    private int justHeldTimer = 0;
    private bool justHeld = false;

    public void disablePiecePickup()
    {
        disabledFromPickingUp = true;
    }

    public void enablePiecePickup()
    {
        disabledFromPickingUp = false;
    }

    public bool containsCursor(Collider2D coll)
    {
        if (coll.bounds.Contains(transform.position))
            return true;
        return false;
    }

    public bool abletoToHoldPiece()
    {
        if (!holdingPiece && !justHeld && !disabledFromPickingUp)
            return true;
        else
            return false;
    }

    public bool isHoldingPiece()
    {
        return holdingPiece;
    }

    public void putPieceDown()
    {
        holdingPiece = false;
        justHeld = true;
        justHeldTimer = 1;
    }

    public void pickUpPiece()
    {
        holdingPiece = true;
    }
    void setMouseInCamera()
    {
       float mousePosX = Input.mousePosition.x;
       float mousePosY = Input.mousePosition.y;
       Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePosX, mousePosY, cam.nearClipPlane));
        point.z = 0.0f;
        transform.position = point;

    }
    // Start is called before the first frame update
    void Start()
    {
        setMouseInCamera();

    }

    // Update is called once per frame
    void Update()
    {
        setMouseInCamera();

        if (justHeld && justHeldTimer <= 0)
        {
            justHeldTimer = 0;
            justHeld = false;
        }
        else if (justHeld)
        {
            justHeldTimer -= 1;
        }


    }
}
