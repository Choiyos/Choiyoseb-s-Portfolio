using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemMessageQueue : MonoBehaviour
{
    Queue<GameObject> systemMessageQueue;
    string tempMessage; // 출력중인 메세지를 임시로 담을 변수

    void Awake()
    {
        systemMessageQueue = new Queue<GameObject>();
        tempMessage = null;
        StartCoroutine(CheckUpdate());
    }

    bool CheckForDuplicateMessages(GameObject message) // 중복된 메세지를 확인
    {
        string messageText = message.GetComponent<Text>().text;

        if (tempMessage != null && tempMessage == messageText)
        {
            return false;
        }

        foreach (var messageInQueue in systemMessageQueue)
        {
            if (messageInQueue.GetComponent<Text>().text == messageText)
            {
                return false;
            }
        }
        return true;
    }

    public IEnumerator EnqueueMessage(GameObject message) // 메세지를 큐에 넣음
    {
        yield return null;
        if (CheckForDuplicateMessages(message)) // 중복메세지가 없을 경우 메세지 인큐
        {
            systemMessageQueue.Enqueue(message);
            float index = systemMessageQueue.Count;
        }
        else // 중복메세지가 있을 경우 메세지 반환
        {
            yield return StartCoroutine(message.GetComponent<SystemMessageAnimation>().Dispose());
        }
    }

    IEnumerator CheckUpdate() // 업데이트를 돌면서 메세지 큐가 비어 있지 않다면 해당 메세지를 출력 작업
    {
        WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();
        while (true)
        {
            yield return fixedUpdate;
            if (systemMessageQueue.Count != 0)
            {
                GameObject dequeuedMessage = systemMessageQueue.Dequeue();
                dequeuedMessage.SetActive(true);
                dequeuedMessage.SendMessage("PushSystemMessage");
                tempMessage = dequeuedMessage.GetComponent<Text>().text;
                yield return StartCoroutine(dequeuedMessage.GetComponent<SystemMessageAnimation>().PopSystemMessage());
                tempMessage = null;
            }
        }
    }
}
