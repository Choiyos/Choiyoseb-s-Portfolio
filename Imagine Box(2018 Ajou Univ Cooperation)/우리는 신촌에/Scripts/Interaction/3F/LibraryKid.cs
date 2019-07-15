using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryKid : MonoBehaviour, BehavioralObject
{

    SceneEventSystem sceneEventSystem; //우선은 직접참조.
    int interactionNum = 0;
    bool isZombieDead = false;
    GameObject libraryZombie;
    public bool IsZombieDead
    {
        get
        {
            return isZombieDead;
        }

        set
        {
            isZombieDead = value;
        }
    }

    void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
        libraryZombie = GameObject.Find("LibraryZombie");
    }

    public void BehaviorByInteraction(GameObject player)
    {
        StartCoroutine(Talk());

    }

    private IEnumerator Talk()
    {
        switch (interactionNum)
        {
            case 0:
                gameObject.layer = 0;
                sceneEventSystem.DirectMode();
                Camera.main.GetComponent<CameraMove>().SetTarget(gameObject);
                yield return new WaitForSeconds(0.5f);

                sceneEventSystem.SpeechBubbles(gameObject, "…?!", 2);
                yield return new WaitForSeconds(2.0f);
                sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "쉿. 괜찮아. 선생님이 구하러 왔어.", 2);
                yield return new WaitForSeconds(2.0f);
                sceneEventSystem.SpeechBubbles(gameObject, "선생님… 저기에…", 2);
                yield return new WaitForSeconds(2.0f);
                sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "응, 선생님도 봤어. 조금만 기다려. 금방 구해줄게.", 2);
                GameObject.Find("UnstableBookshelf").gameObject.layer = 8;

                Camera.main.GetComponent<CameraMove>().SetTarget(sceneEventSystem.Player);
                sceneEventSystem.DirectMode();

                interactionNum++;
                gameObject.layer = 8;
                break;

            case 1:
                if (GameObject.Find("UnstableBookshelf").GetComponent<UnstableBook>().IsInteractWithBookshelf)
                {

                    gameObject.layer = 0;
                    sceneEventSystem.DirectMode();
                    Camera.main.GetComponent<CameraMove>().SetTarget(gameObject);
                    yield return new WaitForSeconds(0.5f);


                    sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "☆☆아, 선생님이 좋은 생각이 떠올랐어.\n내가 신호를 주면 소리를 질러 저 좀비를 유인할 수 있겠니 ?", 3);
                    yield return new WaitForSeconds(3.0f);
                    sceneEventSystem.SpeechBubbles(gameObject, "…네?", 1.5f);
                    yield return new WaitForSeconds(1.5f);
                    sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "무서울 거라는 거 알아. 그래도 선생님을 한번 믿어봐.\n반드시 구해줄게.", 2);
                    yield return new WaitForSeconds(2);
                    sceneEventSystem.SpeechBubbles(gameObject, "…네. 알겠어요.", 1.5f);
                    yield return new WaitForSeconds(1.5f);
                    GameObject.Find("3F").GetComponent<ThirdFloor>().ActiveSignalButton();
                    Camera.main.GetComponent<CameraMove>().SetTarget(sceneEventSystem.Player);
                    sceneEventSystem.DirectMode();

                    interactionNum++;

                    gameObject.layer = 8;
                }
                else
                {
                    sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "방법을 찾아보자.", 1.5f);
                }
                break;
            case 2:
                if (IsZombieDead)
                {
                    sceneEventSystem.SpeechBubbles(sceneEventSystem.Player, "잘 했어. 이제 나가자.", 2);
                    sceneEventSystem.SaveStudent(sceneEventSystem.Player.GetComponent<PlayerInput>());
                    GameObject.Find("3F").GetComponent<ThirdFloor>().StopMonitoring();
                    sceneEventSystem.ToBeSavedKid = name;
                    sceneEventSystem.Player.GetComponent<PlayerProperty>().IsWithchild = true;
                    sceneEventSystem.SaveStudent(sceneEventSystem.Player.GetComponent<PlayerInput>());
                    Destroy(gameObject);
                    
                }
                break;
            default:
                break;
        }
    }

    public void Screaming()
    {
        SoundGenerator.SpreadSound(new Sound(9999, 50f), transform.position);
        libraryZombie.GetComponent<EnemyProperty>().ToRunRadius = 0;
    }
    
}
