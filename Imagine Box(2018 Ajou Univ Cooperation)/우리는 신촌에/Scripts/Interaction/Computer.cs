using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Computer : MonoBehaviour, BehavioralObject
{
    bool isBehaviour;
    SceneEventSystem sceneEventSystem; //우선은 직접참조.
    GameObject schoolMessenger;

    // Use this for initialization
    void Awake()
    {
        isBehaviour = false;
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
        schoolMessenger = sceneEventSystem.TouchCanvas.transform.Find("SchoolMessengerWindow").gameObject;
    }

    void SchoolMessengerWindowOn()
    {
        schoolMessenger.SetActive(true);
    }

    public void SchoolMessengerWindowOff()
    {
        schoolMessenger.SetActive(false);
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (!isBehaviour)
        {
            StartCoroutine(MessageLoad());
        }
        isBehaviour = true;
    }

    private IEnumerator MessageLoad()
    {
        SchoolMessengerWindowOn();
        yield return new WaitUntil(()=> !sceneEventSystem.TouchCanvas.transform.Find("SchoolMessengerWindow").gameObject.activeSelf);
        sceneEventSystem.PushSystemMessage("수위의 전화번호를 등록했다.", 1);
        gameObject.layer = 0;
        sceneEventSystem.FirstFloor.EventTrigger("Computer");
    }
}
