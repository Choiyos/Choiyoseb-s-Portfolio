using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TV : MonoBehaviour
{
    bool isOn;

    public bool IsOn
    {
        get
        {
            return isOn;
        }

        set
        {
            isOn = value;
        }
    }

    private void Awake()
    {
        isOn = false;
    }

    public void TurnTv()
    {
        if (!isOn)
            isOn = true;
        else
            isOn = false;
    }
}
