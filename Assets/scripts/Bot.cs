using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using static UnityEditorInternal.ReorderableList;

public struct Value
{
    public Value(float v)
    {
        this.v = v;
        n = 1;

    }
    public Value(float v, float n)
    {
        this.v = v;
        this.n = n;
    }
    public float v;
    public float n;

}


public class Bot 
{

    private float defaultExpectation = 0.5f;
    private float positiveReward = 1.0f;
    private float negativeReward = -1.0f;
    private float neutralReward = 0.0f;
    private bool constantTimeStep = false;
    private float greedy = 0.1f;
    private bool usingSigmaGreed = true;

    private static Dictionary<string, Value> memoryBank = new Dictionary<string, Value>();
    private List<string> currentStepsActions = new List<string>();
    private float maxValueFoundInStep = -10000000;
    Value getActionEstimatedValue(string action)
    {
        if (memoryBank.ContainsKey(action))
        {
            var possibleMax = memoryBank[action];
            if (possibleMax.v > maxValueFoundInStep)
            {
                maxValueFoundInStep = possibleMax.v;
            }
            return memoryBank[action];
        }
        else
        {
            Value v = new Value(defaultExpectation);
            if (v.v > maxValueFoundInStep)
            {
                maxValueFoundInStep = v.v;
            }
            memoryBank.Add(action, v);
            return v;
        }
    }



    void updateUsedActionsValue(float reward)
    {
        if (!constantTimeStep)
        {
            for (int i = 0; i < currentStepsActions.Count; i++)
            {
                Value v = memoryBank[currentStepsActions[i]];
                v.v = v.v + (1 / v.n) * (reward - v.v);
                v.n += 1;
                memoryBank[currentStepsActions[i]] = v;
            }
        }
    }

     string sigmaGreed(ref List<string> possibleActions)
    {
        List<string> maxActions = new List<string>();
        List<string> otherActions = new List<string>();
        List<Value> vals = new List<Value>();    

        for (int i = 0; i < possibleActions.Count; i++)
        {
            vals.Add(getActionEstimatedValue(possibleActions[i]));
        }

        for (int i = 0; i < possibleActions.Count; i++)
        {
            if (vals[i].v == maxValueFoundInStep)
                maxActions.Add(possibleActions[i]);
            else
                otherActions.Add(possibleActions[i]);
        }
        maxValueFoundInStep = -10000000000;
        float rand = UnityEngine.Random.value;
        float rand2 = UnityEngine.Random.value;
        //print("rand 1: " + rand);
        //print("rand 2: " + rand2);
        if (otherActions.Count == 0 || rand > greedy)
        {
            
            //print("greedy");
           // print("max actions size: " + maxActions.Count);
            rand2 *= maxActions.Count; //this is to ensure we dont get an index that equals teh count
            int r = (int)rand2;
            //print("pre adjust r: " + r);
            if (r < 0)
                r = 0;
            else if (r > maxActions.Count-1)
                r= maxActions.Count-1;

           // print("post adjust r: " + r);
            return maxActions[r];
        }
        else
        {
            //print("other actions size: " + otherActions.Count);
            rand2 *= otherActions.Count; //this is to ensure we dont get an index that equals teh count
            int r = (int)rand2;
            //print("pre adjust r: " + r);
            if (r < 0)
                r = 0;
            else if (r > otherActions.Count - 1)
                r = otherActions.Count - 1;
           // print("post adjust r: " + r);
            return otherActions[r];
        }

    }



    //returns one of the inputed strings
    //evalues each one according to memory and then uses algorithm to either greed on expected value or explore
    public string deterimineAction(ref List<string> possibleActions)
    {

        string action = "";

        if (usingSigmaGreed)
        {
           action = sigmaGreed(ref possibleActions);
        }

        currentStepsActions.Add(action);
        return action;
    }



    public void provideFeedBackForCompletedActionsInStep(float reward)
    {

        updateUsedActionsValue(reward);
        currentStepsActions.Clear();
    }


    public void readNewOldDicDataToDictionary()
    {
        StreamReader sr = new StreamReader("Assets\\saved_data\\testFile.txt");

        string str = sr.ReadLine();

        // To read the whole file line by line 
        int i = 0;
        string key = "";
        Value v = new Value();
        while (str != null)
        {   
            //placed in format
            //key
            //value.v
            //value.n

            if (i == 0)
            {
                key = str;
                i++;
            }           
            else if (i == 1)
            {
                v.v = (float)Convert.ToDouble(str);
                i++;
            }           
            else
            {
                v.n = (float)Convert.ToDouble(str);
                i = 0;
                memoryBank.Add(key, v);
            }
            str = sr.ReadLine();     

        }

        sr.Close();
    }


    public void writeNewDicDataToFile()
    {
        StreamWriter sw = new StreamWriter("Assets\\saved_data\\testFile.txt");

        foreach (var mem in memoryBank)
        {
            //placed in format
            //key
            //value.v
            //value.n

            sw.WriteLine(mem.Key);
            // To write in output stream 
            sw.WriteLine(mem.Value.v);
            // To write in output stream 
            sw.WriteLine(mem.Value.n);
            // To write in output stream 

        }

        sw.Flush();
        sw.Close();
    }
}
