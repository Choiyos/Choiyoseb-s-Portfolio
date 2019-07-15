using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeteriaDoor : MonoBehaviour, BehavioralObject
{

    bool isBehaviour;
    SceneEventSystem sceneEventSystem; //우선은 직접참조.
    GameObject schoolMessenger;

    // Use this for initialization
    void Awake()
    {
        isBehaviour = false;
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (GetComponent<Door>().IsUnlocked && !isBehaviour)
        {
            StartCoroutine(MessageLoad());

            isBehaviour = true;
        }
    }

    private IEnumerator MessageLoad()
    {
        yield return null;
        sceneEventSystem.PushSystemMessage("학생식당의 문을 열었다.", 1);
        sceneEventSystem.FirstFloor.EventTrigger("cafeteriaKey");
    }
}
