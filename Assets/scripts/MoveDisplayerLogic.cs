using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class MoveDisplayerLogic : MonoBehaviour
{
    private List<Move> moves = new List<Move>();
    private bool updated = false;
    public GameObject textDisplay;
    public void addMove(ref Move move)
    {
        moves.Add(move);
        updated = false;
    }

    private void displayMoves()
    {
        var myTextDisplay = textDisplay.GetComponent<TextMeshProUGUI>();
        string text = "";
        int turnNumber = 1;
        for(int i =0; i < moves.Count; i++)
        {
            
            
            string entry = "";
            //string spacer = "      ";
            if(i %2 == 0 && i !=0)
            {
                entry += '\n';
                turnNumber++;
                entry += turnNumber + ". ";
            }
            else if(i != 0)
            {
              //  text += spacer;
            }
            else
            {
                entry += turnNumber + ". ";
            }
            entry += moves[i].name;
            entry = string.Format("{0, -20}", entry);
            text += entry;
            

        }
        
        myTextDisplay.text = text;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!updated)
        {
            displayMoves();
            updated = true;
        }
            

    }
}
