using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBox : MonoBehaviour, BehavioralObject
{
    SceneEventSystem sceneEventSystem;
    bool isFirst = true;

    void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (isFirst)
        {
            isFirst = false;
        }
        sceneEventSystem.FirstFloor.LockWindowOpen();
    }
}
