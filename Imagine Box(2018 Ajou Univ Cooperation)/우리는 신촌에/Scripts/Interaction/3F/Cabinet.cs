using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cabinet : MonoBehaviour, BehavioralObject
{

    [SerializeField]
    Item sodiumPostIt, sodium;
    SceneEventSystem sceneEventSystem;
    int behaviorCount = 0;
    WaitForSeconds wait = new WaitForSeconds(1);
    Inventory inven;
    void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
        inven = sceneEventSystem.Player.GetComponent<Inventory>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        StartCoroutine( InteractCabinet(player));

    }

    private IEnumerator InteractCabinet(GameObject player)
    {
        gameObject.layer = 0;
        switch (behaviorCount)
        {
            case 0:
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "나무 상자가 보인다. 영어로 뭐라 쓰여 있다. Sodium..?", 1.5f);
                yield return wait;
                behaviorCount++;
                break;
            case 1:
                if (inven.IsItemExist("Crowbar"))
                {
                    sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "빠루를 고쳐 잡고, 힘을 주어 유리를 내려쳤다.", 1.5f);
                    yield return wait;
                    sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "캐비닛 유리는 큰 소리와 함께 깨졌다.", 1.5f);
                    yield return wait;
                    behaviorCount++;
                }
                else
                {
                    sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "유리를 깨트리기 위해선 도구가 있어야할 것 같다.", 1.5f);
                    yield return wait;
                }
                break;
            case 2:
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "소듐이라 쓰인 상자에 포스트잇이 붙어 있다.", 1.5f);
                yield return wait;
                player.SendMessage("AddItem", sodiumPostIt);
                sceneEventSystem.PushSystemMessage("포스트잇을 얻었다.", 1);
                yield return wait;
                behaviorCount++;
                break;
            case 3:
                if (sceneEventSystem.IsInteractedThirdFloorCollabsedClass)
                {
                    sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "어쩌면 이걸로 3층 교실의 문을 폭파할 수 있을지도…", 1.5f);
                    yield return wait;
                    player.SendMessage("AddItem", sodium);
                    sceneEventSystem.PushSystemMessage("실험용 소듐 덩어리를 얻었다.", 1);
                    yield return wait;

                    gameObject.layer = 0;
                    behaviorCount++;
                    sceneEventSystem.IsInteractedThirdFloorCollabsedClass = false;
                    yield return null;
                }
                else
                {
                    sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "물에 닿으면 폭발한다니.. 건드리지 않는게 좋겠다.", 1.5f);
                    yield return wait;
                }
                break;
            default:
                break;
        }
        gameObject.layer = 8;
    }
}
