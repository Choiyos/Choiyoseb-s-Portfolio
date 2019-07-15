using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteController : MonoBehaviour
{
    [SerializeField]
    Item remoteController;
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
            player.SendMessage("AddItem", remoteController);
            MessageLoad();
        }
        isBehaviour = true;

        Destroy(gameObject);
    }

    void MessageLoad()
    {
        sceneEventSystem.PushSystemMessage("리모콘을 얻었다.", 1);
    }
}
