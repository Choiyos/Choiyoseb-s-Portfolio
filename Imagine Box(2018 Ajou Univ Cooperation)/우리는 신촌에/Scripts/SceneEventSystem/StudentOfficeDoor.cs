using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudentOfficeDoor : MonoBehaviour, BehavioralObject
{
    [SerializeField]
    private SceneEventSystem sceneEventSystem;
    [SerializeField]
    private GameObject student;
    [SerializeField]
    private GameObject schoolMessenger;
    bool isInteracted = false;
    [SerializeField]
    Text[] cipherText;//자물쇠 네개의 숫자 텍스트를 담아놓을 배열.
    [SerializeField]
    GameObject lockWindow;

    string cipher = "8731";

    public void BehaviorByInteraction(GameObject player)
    {
        if (!isInteracted&& !sceneEventSystem.IsBlackouted)
        {
            StartCoroutine(StudentOffice_());
        }
        else if(sceneEventSystem.IsBlackouted)
        {
            LockWindowOpen();
        }
    }

    IEnumerator StudentOffice_()
    {
        sceneEventSystem.TouchCanvas.SetActive(false);
        sceneEventSystem.SpeechBubbles(student, "살려주세요! 안에 저 있어요!", 1.5f);
        yield return new WaitForSeconds(1.5f);
        sceneEventSystem.SpeechBubbles(student, "선생님이에요? 구해주세요!", 1.5f);
        yield return new WaitForSeconds(2f);
        schoolMessenger.SetActive(true);
        yield return new WaitForSeconds(4);
        schoolMessenger.SetActive(false);
        sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "지금 당장 문을 열 수는 없을 것 같다...", 1.5f);
        yield return new WaitForSeconds(1.5f);
        sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "그래, 선생님이야!", 1.5f);
        yield return new WaitForSeconds(1.5f);
        sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "지금 당장은 열리지 않지만, 금방 구하러 올게!", 1.5f);
        yield return new WaitForSeconds(1.5f);
        sceneEventSystem.SpeechBubbles(student, "네... 꼭 다시 구하러 와줘야 해요!", 1.5f);
        yield return new WaitForSeconds(1.5f);
        sceneEventSystem.TouchCanvas.SetActive(true);
        isInteracted = true;

    }

    public void TryUnlock()
    {
        string inputCipher = String.Empty;
        foreach (Text item in cipherText)
        {
            inputCipher = string.Concat(inputCipher, item.text);
        }
        if (inputCipher == cipher)
        {
            sceneEventSystem.PushSystemMessage("도어락이 열렸다.", 1);
            GetComponent<Door>().IsUnlocked = true;

            Destroy(this);
        }
        else
        {
            sceneEventSystem.PushSystemMessage("열리지 않는다.", 1);
        }
        Time.timeScale = 1;
        lockWindow.SetActive(false);
    }


    public void Cipher(string value)
    {
        string[] temp = value.Split('_');
        string order = temp[0];
        string upOrDown = temp[1];
        switch (upOrDown)
        {
            case "UP":
                cipherText[int.Parse(order) - 1].text = int.Parse(cipherText[int.Parse(order) - 1].text) == 9
                    ? (0).ToString() : ((int.Parse(cipherText[int.Parse(order) - 1].text)) + 1).ToString();
                break;
            case "DOWN":
                cipherText[int.Parse(order) - 1].text = int.Parse(cipherText[int.Parse(order) - 1].text) == 0
                    ? (9).ToString() : ((int.Parse(cipherText[int.Parse(order) - 1].text)) - 1).ToString();
                break;
            default:
                break;
        }
    }

    public void LockWindowOpen()
    {
        foreach (Text item in cipherText)
        {
            item.text = "0";
        }
        lockWindow.SetActive(true);
        Time.timeScale = 0;

    }
}
