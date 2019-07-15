using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPackage : MonoBehaviour
{
    [SerializeField]
    Item keyPackage;
    bool isBehaviour;
    SceneEventSystem sceneEventSystem;
    SecondFloor secondFloor;

    void Awake()
    {
        isBehaviour = false;
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
        secondFloor = GameObject.Find("2F").GetComponent<SecondFloor>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (!isBehaviour)
        {
            player.SendMessage("AddItem", keyPackage);
            MessageLoad();
        }
        isBehaviour = true;

        Destroy(gameObject);
    }

    void MessageLoad()
    {
        sceneEventSystem.PushSystemMessage("교무실 열쇠를 얻었습니다.", 1);
        secondFloor.SchoolOfficeEvent();
    }
}
