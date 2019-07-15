using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField]
    Item item;
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
            player.SendMessage("AddItem", item);
            MessageLoad();
        }
        isBehaviour = true;

        Destroy(gameObject);
    }

    void MessageLoad()
    {
        sceneEventSystem.PushSystemMessage(string.Format(item.itemKorName + "을(를) 얻었다."), 1);
    }
}
