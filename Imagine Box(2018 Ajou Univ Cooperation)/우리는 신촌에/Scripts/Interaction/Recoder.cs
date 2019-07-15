using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoder : MonoBehaviour
{
    [SerializeField]
    Item recoder;
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
            player.SendMessage("AddItem", recoder);
            MessageLoad();
        }
        isBehaviour = true;

        Destroy(gameObject);
    }

    void MessageLoad()
    {
        sceneEventSystem.PushSystemMessage("리코더를 얻었다.", 1);
    }
}
