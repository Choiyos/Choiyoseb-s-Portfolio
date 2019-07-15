using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsedRoomKid : MonoBehaviour, BehavioralObject
{
    SceneEventSystem sceneEventSystem; //우선은 직접참조.
    bool isFirst = true;
    void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        gameObject.layer = 0;
        StartCoroutine(Talk());

    }

    private IEnumerator Talk()
    {
        yield return null;

        sceneEventSystem.DirectMode();

        sceneEventSystem.SpeechBubbles(gameObject, "선생님?!!", 2);
        yield return new WaitForSeconds(2.0f);
        sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "그래, 선생님이야..", 2);
        yield return new WaitForSeconds(2.0f);
        sceneEventSystem.SpeechBubbles(gameObject, "선생님!!!!", 2);
        yield return new WaitForSeconds(2.0f);
        sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "아이구, 너무 달라붙지 말렴.. 선생님이 지금 몸 상태가…", 2);
        yield return new WaitForSeconds(2.0f);
        sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "아무튼 금방 구해줄게. 조금만 참으렴.", 2);
        yield return new WaitForSeconds(1.0f);

        sceneEventSystem.Player.GetComponent<PlayerProperty>().IsWithchild = true;

        sceneEventSystem.SaveStudent(sceneEventSystem.Player.GetComponent<PlayerInput>());
        Destroy(gameObject);

        sceneEventSystem.DirectMode();
        gameObject.layer = 8;
    }
}
