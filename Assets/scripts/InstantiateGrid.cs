using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateGrid : MonoBehaviour
{
    public GameObject myCursor;
    public GameObject squre;
    public Color darkSquareColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    public Color lightSquareColor = new Color(0.9f, 0.7f, 0.6f, 1.0f);
    public Dictionary<string, GameObject> grid = new Dictionary<string, GameObject>();
    string setSquareName(int i)
    {
        char rank = 'a';
        int row = 1;
        switch(i%8)
        {
            case 1: rank = 'b'; break;
            case 2: rank = 'c'; break;
            case 3: rank = 'd'; break;
            case 4: rank = 'e'; break;
            case 5: rank = 'f'; break;
            case 6: rank = 'g'; break;
            case 7: rank = 'h'; break;
            default: rank = 'a'; break;
        }

        row = (i/8 + 1);
        return rank + row.ToString(); 
    }


    Vector3 setSquareLoc(int i)
    {
        Vector3 vec = new Vector3(0.0f, 0.0f, 0.0f);
        int rankMultiplier = (i % 8);
        int rowMultiplier = (i/8 + 1);
        float offsetX = squre.transform.lossyScale.x * 4;
        float offsetY = squre.transform.lossyScale.y * 4;


        vec.x = rankMultiplier * squre.transform.lossyScale.x - offsetX;
        vec.y = rowMultiplier * squre.transform.lossyScale.y - offsetY;

        return vec;
    }
    // Start is called before the first frame update
    void Awake()
    {
        //lazy flipping variable
        bool dark = true;
        for(int i = 0; i < 64; i++)
        {
            string name = setSquareName(i);
            GameObject mySquare = Instantiate(squre, setSquareLoc(i), Quaternion.identity, transform);
            GridElementLogic script = mySquare.GetComponent<GridElementLogic>();
            script.myCursor = myCursor;
            SpriteRenderer sp = mySquare.GetComponent<SpriteRenderer>();
            if (i % 8 == 0 && i != 0) // ensure we start on dark
            {
                if (dark)
                    dark = false;
                else
                    dark = true;
            }
                //flip te true fals ething cause we on new row}

            if(dark) 
                { sp.color *= darkSquareColor;
                dark = false;
            }
            else 
                { sp.color *= lightSquareColor;
                dark = true;
            }

            mySquare.GetComponent<GridElementLogic>().trueColor = mySquare.GetComponent<SpriteRenderer>().color;
            mySquare.name = name;
            grid.Add(name, mySquare);
        }
        
        transform.position = Vector3.zero;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
