using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroadcastRoomDesk : MonoBehaviour {

    [SerializeField]
    Item  wetNote;
    SceneEventSystem sceneEventSystem;
    private void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }
    public void BehaviorByInteraction(GameObject player)
    {
        player.SendMessage("AddItem", wetNote);
        sceneEventSystem.PushSystemMessage("축축한 쪽지를 얻었다.", 1);
        gameObject.layer = 0;
    }
}
