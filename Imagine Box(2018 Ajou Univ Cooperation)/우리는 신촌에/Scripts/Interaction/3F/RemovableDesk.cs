using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovableDesk : MonoBehaviour, BehavioralObject
{

    SceneEventSystem sceneEventSystem;

    private void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        sceneEventSystem.PushSystemMessage("빠루로 힘껏 내려쳤더니 책상이 부서졌다.", 1);
        Destroy(gameObject);
    }
}
