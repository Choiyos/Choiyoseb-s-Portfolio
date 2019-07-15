using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThirdFloor : MonoBehaviour
{

    [Header("섹션 리스트")]
    [SerializeField]
    List<Section> sectionList;
    [Header("문 리스트")]
    [SerializeField]
    List<Door> doorList;

    [SerializeField]
    SceneEventSystem sceneEventSystem;
    SectionRendering sectionRendering;

    [SerializeField]
    GameObject player;

    [SerializeField]
    GameObject signalButton;

    [SerializeField]
    GameObject broadcastZombiesParent;
    List<GameObject> broadcastZombies;
    [SerializeField]
    GameObject broadcastButtons;
    [SerializeField]
    GameObject broadcastUI;
    [SerializeField]
    GameObject[] broadcastZombiePositions;
    [SerializeField]
    GameObject[] hallwayZombies;

    IEnumerator playerMonitoring;

    [SerializeField]
    GameObject firstFloorZombies;

    [SerializeField]
    GameObject storageZombie;

    [SerializeField]
    GameObject storageDoor;

    [SerializeField]
    GameObject auditoriumDoor;

    [SerializeField]
    CurrentFloorUI currentFloorUI;

    GameObject fade;
    public List<Section> SectionList
    {
        get
        {
            return sectionList;
        }

        set
        {
            sectionList = value;
        }
    }

    public List<Door> DoorList
    {
        get
        {
            return doorList;
        }

        set
        {
            doorList = value;
        }
    }

    private void Awake()
    {
        ButtonSetting();
        broadcastZombies = new List<GameObject>();
        for (int i = 0; i < 8; i++)
        {
            broadcastZombies.Add(broadcastZombiesParent.transform.GetChild(i).gameObject);
        }
        fade = GameObject.Find("Canvas").transform.GetChild(1).GetChild(0).gameObject;
    }
    public IEnumerator Fade(bool _flag)
    {
        fade.gameObject.SetActive(true);
        if (_flag)
            fade.GetComponent<Image>().DOFade(1, 1).SetEase(Ease.InOutQuad);
        else
            fade.GetComponent<Image>().DOFade(0, 1).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(1);
        fade.gameObject.SetActive(_flag);
    }
    private void ButtonSetting()
    {

        broadcastButtons.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => BroadcastToFloor(0));
        broadcastButtons.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => BroadcastToFloor(1));
        broadcastButtons.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => BroadcastToFloor(2));

        broadcastButtons.transform.GetChild(broadcastButtons.transform.childCount - 1).GetComponent<Button>().onClick.AddListener(() => ShowBroadcastUI(false));
    }

    public void FloorInit()
    {
        sectionRendering = sceneEventSystem.GetComponent<SectionRendering>();
        sectionRendering.FloorInit(sectionList, doorList);

    }

    public void ShowBroadcastUI(bool _flag)
    {
        broadcastUI.SetActive(_flag);
    }

    private void BroadcastToFloor(int _index)
    {
        Transform zombiePosition = null;
        sceneEventSystem.TouchCanvas.SetActive(false);
        StartCoroutine(GameObject.Find("3F").GetComponent<ThirdFloor>().Fade(true));

        zombiePosition = broadcastZombiePositions[_index].transform;
        if (_index == 0)
        {
            sceneEventSystem.FellowTeacherActiveState(false);
        }
        else
        {
            sceneEventSystem.FellowTeacherActiveState(true);
        }
        for (int i = 0; i < zombiePosition.transform.childCount; i++)
        {

            broadcastZombies[i].SetActive(false);
            broadcastZombies[i].transform.position = zombiePosition.GetChild(i).position;
            broadcastZombies[i].transform.parent = zombiePosition.GetChild(i);
            broadcastZombies[i].SetActive(true);
        }
        if (hallwayZombies.Length != 0)
        {
            foreach (GameObject item in hallwayZombies)
            {
                item.SetActive(false);
            }
        }
        ShowBroadcastUI(false);
        broadcastZombiesParent.SetActive(true);

        StartCoroutine(GameObject.Find("3F").GetComponent<ThirdFloor>().Fade(false));
        sceneEventSystem.TouchCanvas.SetActive(true);
    }

    void OnEnable()
    {
        StartCoroutine(sceneEventSystem.SceneFade(false));
        playerMonitoring = PlayerMonitoring();
        Transform zombies = GameObject.Find("3FZombies").transform;
        currentFloorUI.UIupdate("3F");
        for (int i = 0; i < zombies.childCount; i++)
        {
            if (zombies.GetChild(i).gameObject.activeInHierarchy)
                zombies.GetChild(i).GetComponent<Enemy>().StartFSM();
        }
        FloorInit();
    }


    public void ActiveSignalButton()
    {
        signalButton.SetActive(true);
        StartCoroutine(playerMonitoring);
    }

    private IEnumerator PlayerMonitoring()
    {
        while (true)
        {
            yield return null;

            if (player.GetComponent<PlayerProperty>().StandingSection.name == "3FHallway")
            {
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "아이를 구해야한다.", 1.5f);
                StartCoroutine(BackStep());
            }
        }
    }

    private IEnumerator BackStep()
    {
        player.transform.GetChild(0).GetComponent<Animator>().SetInteger("State", 1);
        float value = player.transform.localPosition.x + 5;
        player.GetComponent<PlayerInput>().enabled = false;
        player.transform.DOLocalMoveX(value, 2);
        yield return new WaitForSeconds(2);
        player.GetComponent<PlayerInput>().enabled = true;
        player.transform.GetChild(0).GetComponent<Animator>().SetInteger("State", 0);
    }

    public void StopMonitoring()
    {
        StopCoroutine(playerMonitoring);
        signalButton.SetActive(false);
    }

    public void MoveZombies()
    {
        //문열고 좀비가 창고 밖으로 나옴.
        storageDoor.GetComponent<Door>().IsUnlocked = true;
        storageDoor.GetComponent<Door>().BehaviorByInteraction(null);
        auditoriumDoor.GetComponent<Door>().IsUnlocked = true;
        auditoriumDoor.GetComponent<Door>().BehaviorByInteraction(null);
        float value = auditoriumDoor.transform.localPosition.y + 4;
        auditoriumDoor.transform.DOLocalMoveY(value, 1);
        firstFloorZombies.SetActive(true);
        storageZombie.SetActive(true);
    }
}
