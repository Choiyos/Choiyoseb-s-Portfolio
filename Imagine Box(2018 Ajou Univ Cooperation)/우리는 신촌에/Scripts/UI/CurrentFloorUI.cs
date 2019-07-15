using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentFloorUI : MonoBehaviour
{
    Text uiText;

    void Awake()
    {
        uiText = GetComponent<Text>();
    }

    public void UIupdate(string floor)
    {
        uiText.text = floor;
    }
}
