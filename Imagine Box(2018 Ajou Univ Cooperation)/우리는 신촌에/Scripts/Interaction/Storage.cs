using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour, BehavioralObject
{
    [SerializeField]
    Item cafeKey, officeKey;
    bool isBehaviour;
    SceneEventSystem sceneEventSystem;

    void Awake()
    {
        isBehaviour = false;
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (!isBehaviour)
        {
            player.SendMessage("AddItem", cafeKey);
            player.SendMessage("AddItem", officeKey);
            MessageLoad();
        }
        isBehaviour = true;
    }

    void MessageLoad()
    {
        gameObject.layer = 0;
        sceneEventSystem.PushSystemMessage("교무실 열쇠를 얻었다.", 1);
        sceneEventSystem.PushSystemMessage("학생식당 열쇠를 얻었다.", 1);
    }
}
