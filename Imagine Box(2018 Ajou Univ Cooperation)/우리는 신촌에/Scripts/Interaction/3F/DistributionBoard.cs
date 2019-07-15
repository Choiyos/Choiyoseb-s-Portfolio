using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistributionBoard : MonoBehaviour, BehavioralObject
{

    SceneEventSystem sceneEventSystem;
    [SerializeField]
    GameObject zombie;
    GameObject broadcastRoomDoor;

    [SerializeField]
    Door studentOfficeDoor;

    int interactionNum = 0;

    private void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
        broadcastRoomDoor = GameObject.Find("Broadcastroom Back Door");
    }

    void OnEnable()
    {
        StartCoroutine(ElectricShock());

    }

    private IEnumerator ElectricShock()
    {
        while (true && zombie.activeInHierarchy)
        {
            yield return null;
            if (Vector3.Distance(zombie.transform.position, transform.position) < 15)
            {
                Camera.main.GetComponent<CameraMove>().SetTarget(zombie);
                sceneEventSystem.Player.GetComponent<PlayerInput>().enabled = false;
                sceneEventSystem.Player.transform.GetChild(0).GetComponent<Animator>().SetInteger("State", 0);
                sceneEventSystem.DirectMode();
                GameObject.Find("Canvas").transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Joystick>().OnPointerUp(null);
                yield return new WaitForSeconds(0.5f);
                zombie.GetComponent<EnemyProperty>().ToRunRadius = 0;
                zombie.GetComponent<EnemyProperty>().RunToAttackRadius = 0;
                zombie.GetComponent<EnemyProperty>().RunToIdleRadius = 0;
                yield return new WaitForSeconds(1);
                SoundGenerator.SpreadSound(new Sound(9999, 30f), transform.position + new Vector3(0, 0, -2));
                yield return new WaitForSeconds(3f);
                AudioManager.Instance.PlaySoundOneShot("ZombieAttack", 2);
                zombie.GetComponent<Enemy>().AnimatorActiveCheck(EnemyAnimation.Attack);
                yield return new WaitForSeconds(1.5f);
                Camera.main.GetComponent<CameraMove>().SetTarget(sceneEventSystem.Player);
                yield return StartCoroutine(Boom());
                yield return StartCoroutine(KaTalk());
                break;
            }

            //좀비의 타겟을 분전반으로 고정
        }
    }

    private IEnumerator KaTalk()
    {
        yield return null;
        sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "갑자기 학교 전기가 나갔다. 전기는 학생식당에서 복구시킬 수 있는듯 하다.", 2f);
        yield return new WaitForSeconds(2.2f);
        sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "갑자기 강당에서 요란한 소리가 난다.", 1.5f);
        yield return new WaitForSeconds(1.6f);
        sceneEventSystem.DirectMode();
        sceneEventSystem.Player.GetComponent<PlayerInput>().enabled = true;

    }

    IEnumerator Boom()
    {
        yield return StartCoroutine(GameObject.Find("3F").GetComponent<ThirdFloor>().Fade(true));

        sceneEventSystem.Player.GetComponent<PlayerInput>().enabled = false;
        AudioManager.Instance.PlaySoundOneShot("Boom01", 2);

        zombie.SetActive(false);
        GameObject.Find("ElectricPuddle").SetActive(false);
        broadcastRoomDoor.transform.GetChild(0).GetComponent<Door>().IsUnlocked = true;
        broadcastRoomDoor.transform.GetChild(0).GetComponent<Door>().BehaviorByInteraction(null);
        yield return new WaitForSeconds(1f);
        broadcastRoomDoor.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        broadcastRoomDoor.transform.GetChild(0).gameObject.layer = 0;
        
        //조명어둡게
        sceneEventSystem.Player.transform.GetChild(3).gameObject.SetActive(true);
        if (!sceneEventSystem.Player.GetComponent<Inventory>().IsItemExist("Lighter"))
        {
            sceneEventSystem.Player.transform.GetChild(2).GetComponent<Light>().range = 10f;
        }
        else
        {
            sceneEventSystem.PushSystemMessage("가지고있는 라이터를 사용했다.", 1f);
            sceneEventSystem.Player.transform.GetChild(3).GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.8f);
        }


        yield return new WaitForSeconds(2);
        yield return StartCoroutine(GameObject.Find("3F").GetComponent<ThirdFloor>().Fade(false));
        GameObject.Find("3F").GetComponent<ThirdFloor>().MoveZombies();
        sceneEventSystem.IsBlackouted = true;
        yield return new WaitForSeconds(1);
        gameObject.layer = 0;
    }


    public void BehaviorByInteraction(GameObject player)
    {
        StartCoroutine(InteractBoard());
    }

    private IEnumerator InteractBoard()
    {
        sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "벽이 무너지며 분전반의 전선들이 끊어진 것 같다.", 2);
        yield return new WaitForSeconds(2);
        sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "직접 큰 충격을 가하면 감전될 것이 분명하다. 다른 방법이 없을까?", 2);
        yield return new WaitForSeconds(2);
    }
}
