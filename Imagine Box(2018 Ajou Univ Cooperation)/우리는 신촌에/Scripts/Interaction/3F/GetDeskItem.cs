using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetDeskItem : MonoBehaviour,BehavioralObject {

    [SerializeField]
    Item lighter, note;

    [SerializeField]
    GameObject lighterObject, noteObject;
    SceneEventSystem sceneEventSystem;
    private void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }
    public void BehaviorByInteraction(GameObject player)
    {
        player.SendMessage("AddItem", lighter);
        player.SendMessage("AddItem", note);
        sceneEventSystem.PushSystemMessage("라이터와 피묻은 쪽지를 얻었다.", 1);
        gameObject.layer = 0;
        lighterObject.SetActive(false);
        noteObject.SetActive(false);
    }
    
}
