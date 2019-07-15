using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
    // 인게임 상에서 플레이어에게 메세지를 띄우는 기능의 스크립트.
    float colorAlpha;
    bool _switch;
    float durationTime = 3;

    public void PunchMessage(string txt)
    {
        colorAlpha = 0;
        _switch = false;
        GetComponent<Text>().text = txt;
        StartCoroutine("_PunchMessage");
    }

    IEnumerator _PunchMessage()
    {
        while (true)
        {
            if (GetComponent<Text>().color.a <= 0.99f && _switch == false)
            {
                colorAlpha += Time.deltaTime / durationTime;
                GetComponent<Text>().color = new Color(0, 255, 255, colorAlpha);
                yield return null;
            }
            else
            {
                _switch = true;
                colorAlpha -= Time.deltaTime / durationTime;
                GetComponent<Text>().color = new Color(0, 255, 255, colorAlpha);
                yield return null;
                if (GetComponent<Text>().color.a <= 0.01f) yield break;
            }
        }
    }
}
