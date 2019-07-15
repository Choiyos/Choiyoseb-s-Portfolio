using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kid2 : MonoBehaviour
{
    SceneEventSystem sceneEventSystem; //우선은 직접참조.

    void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (!sceneEventSystem.Player.GetComponent<PlayerProperty>().IsWithchild)
        {
            sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "무서웠지? 금방 구해줄게", 2);
            sceneEventSystem.Player.GetComponent<PlayerProperty>().IsWithchild = true;
            gameObject.SetActive(false);
            sceneEventSystem.SaveStudent(player.GetComponent<PlayerInput>());
        }
        else
        {
            sceneEventSystem.SpeechBubbles(sceneEventSystem.Player.gameObject, "데리고 있는 아이를 구출한 후에 다시 구하러 올게!", 2);
        }
    }
}
