using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsedRoomDoor : MonoBehaviour, BehavioralObject
{
    
    int interNum = 0;//상호작용한 횟수
    Inventory inventory;
    SceneEventSystem sceneEventSystem;
    [SerializeField]
    GameObject kid;
    WaitForSeconds wait = new WaitForSeconds(1.5f);
    private void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
        inventory = sceneEventSystem.Player.GetComponent<Inventory>();
    }
    

    public void BehaviorByInteraction(GameObject player)
    {
        StartCoroutine(TalkWithKid());
    }

    private IEnumerator TalkWithKid()
    {
        gameObject.layer = 0;
        sceneEventSystem.DirectMode();
        yield return new WaitForSeconds(0.5f);
        switch (interNum)
        {

            case 0:

                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "천장이 내려앉았다. 찌그러진 문은 온 힘을 다해 당겨도 미동조차 않는다.", 1.5f);
                yield return wait;
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "문 아래에는 빗물이 잔뜩 고여 있다.", 1.5f);
                yield return wait;

                sceneEventSystem.SpeechBubbles(kid, "밖에 누구 있어요?", 1.5f);
                yield return wait;
                sceneEventSystem.SpeechBubbles(kid, "살려주세요", 1.5f);
                yield return wait;
                sceneEventSystem.SpeechBubbles(kid, "저 교실에 갇혔어요!!", 1.5f);
                yield return wait;

                sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "안에 누구 있니?", 1.5f);
                yield return wait;
                sceneEventSystem.SpeechBubbles(kid, "선생님??! 살려주세요!! 교실에 숨어있는데 갑자기 천장이 무너졌어요!!", 1.5f);
                yield return wait;
                sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "안에서 열 방법이 없니?", 1.5f);
                yield return wait;
                sceneEventSystem.SpeechBubbles(kid, "꿈쩍도 안 해요.. 제발 구해주세요!!", 1.5f);
                yield return wait;
                sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "알았다. 선생님이 구할 방법을 꼭 찾을게. 조금만 기다리렴.", 1.5f);
                yield return wait;
                sceneEventSystem.SpeechBubbles(kid, "네… 꼭 구해주셔야 해요. 너무 무서워요..", 1.5f);
                yield return wait;
                sceneEventSystem.IsInteractedThirdFloorCollabsedClass = true;
                interNum++;
                break;
            case 1:
                if (inventory.IsItemExist("Sodium"))
                {
                    sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "선생님 목소리 들리니? 선생님이 곧 이 문을 터트릴 거야. 위험할 수 있으니 교탁 밑으로 숨어있거라.", 1.5f);
                    yield return wait;
                    sceneEventSystem.SpeechBubbles(kid, "네.. 알겠어요!!", 1.5f);
                    yield return wait;
                    interNum++;
                    break;
                }
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "힘으로는 절대로 열 수 없다. 다른 방법이 필요하다.", 1.5f);
                yield return wait;
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "문을 날려버릴… 폭탄.. 같은 게 어디 없을까?", 1.5f);
                yield return wait;
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "문 아래에는 빗물이 잔뜩 고여 있다.", 1.5f);
                yield return wait;
                break;
            case 2:
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "소듐이 들어있는 봉투를 찢은 뒤, 묵직한 금속 덩어리를 웅덩이에 던져 넣었다.", 1.5f);
                yield return wait;
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player,"잠깐.. 나도 도망쳐야", 1.5f);
                yield return wait;
                yield return StartCoroutine(Boom());
                interNum++;
                break;
            default:
                break;
        }
        sceneEventSystem.DirectMode();
        yield return new WaitForSeconds(0.5f);
        gameObject.layer = 8;
    }

    IEnumerator Boom()
    {
        sceneEventSystem.TouchCanvas.SetActive(false);
        yield return StartCoroutine(GameObject.Find("3F").GetComponent<ThirdFloor>().Fade(true));

        sceneEventSystem.Player.GetComponent<PlayerInput>().enabled = false;
        sceneEventSystem.Player.transform.GetChild(0).GetComponent<Animator>().SetInteger("State", 3);
        AudioManager.Instance.PlaySoundOneShot("Boom01", 2);

        GetComponent<Door>().IsUnlocked = true;
        GetComponent<Door>().BehaviorByInteraction(null);

        //쓰러진 주인공
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(GameObject.Find("3F").GetComponent<ThirdFloor>().Fade(false));

        yield return new WaitForSeconds(1);
        sceneEventSystem.Player.transform.GetChild(0).GetComponent<Animator>().SetInteger("State", 0);
        sceneEventSystem.Player.GetComponent<PlayerInput>().enabled = true;
        sceneEventSystem.TouchCanvas.SetActive(true);
        gameObject.layer = 0;
        gameObject.SetActive(false);
        

    }

}
    
