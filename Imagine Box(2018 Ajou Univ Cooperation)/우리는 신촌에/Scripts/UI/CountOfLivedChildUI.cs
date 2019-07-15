using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountOfLivedChildUI : MonoBehaviour
{
    Text uiText;
    [SerializeField]
    Teacher fellowTeacher;
    GameObject gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        uiText = GetComponent<Text>();
        Invoke("UIupdate", 0.1f);
    }

    public void UIupdate()
    {
        uiText.text = string.Format((7 - fellowTeacher.CountOfSavedChild) + "명");
        if (7 - fellowTeacher.CountOfSavedChild == 0)
        {
            gameManager.SendMessage("GameClear");
        }
    }
}
