using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Broadcast : MonoBehaviour, BehavioralObject {

    SceneEventSystem sceneEventSystem;

    void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (!sceneEventSystem.IsBlackouted)
        {
            GameObject.Find("3F").GetComponent<ThirdFloor>().ShowBroadcastUI(true);
            GameObject.Find("IngameMap").transform.GetChild(1).GetComponent<SecondFloor>().IsBroadcasted = true;
        }
        else
        {
            sceneEventSystem.PushSystemMessage("정전으로인해 작동하지 않는다.", 1f);
        }
    }
    
}
