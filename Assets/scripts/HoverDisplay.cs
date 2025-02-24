using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HoverDisplay : MonoBehaviour
{
    public GameObject textToDisplayOnHover;
    public GameObject myCursor;
    private Color trueColor;
    public Color hoverColor;
    private bool clickedOn = false;

    // Start is called before the first frame update
    void Start()
    {
        trueColor = GetComponent<SpriteRenderer>().color;
    }
    public bool acknowledgeClickedOn()
    {
        if (clickedOn)
        {
            clickedOn = false;
            return true;
        }
        else
            return false;     
    }

    // Update is called once per frame
    void Update()
    {
        if(myCursor.GetComponent<ClickToMove>().containsCursor(GetComponent<Collider2D>()))
        {
            GetComponent<SpriteRenderer>().color = hoverColor;
            Color visible = textToDisplayOnHover.GetComponent<TextMeshProUGUI>().color;
            textToDisplayOnHover.GetComponent<TextMeshProUGUI>().color = new Color(visible.r, visible.g, visible.b, 1.0f);

            //if clicked on, save that we were clicked on

            if(Input.GetMouseButton(0))
            {
                clickedOn = true;
            }
        }
        else
        {
            GetComponent<SpriteRenderer>().color = trueColor;
            Color visible = textToDisplayOnHover.GetComponent<TextMeshProUGUI>().color;
            textToDisplayOnHover.GetComponent<TextMeshProUGUI>().color = new Color(visible.r, visible.g, visible.b, 0.0f);
        }
    }
}
