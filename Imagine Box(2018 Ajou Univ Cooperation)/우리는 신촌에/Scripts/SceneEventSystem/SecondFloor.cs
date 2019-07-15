using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondFloor : MonoBehaviour
{
    [Header("섹션 리스트")]
    [SerializeField]
    List<Section> sectionList;
    [Header("문 리스트")]
    [SerializeField]
    List<Door> doorList;
    [SerializeField]
    SceneEventSystem sceneEventSystem;
    [SerializeField]
    Door schoolOfficeStorageDoor; // 교무실 문
    [SerializeField]
    List<Enemy> schoolOfficeStorageZombies; // 교무실 창고 좀비들
    [SerializeField]
    List<Enemy> musicRoomZombies; // 음악실 좀비들
    [SerializeField]
    TV tv; // 교무실 이벤트 tv
    [SerializeField]
    Door musicRoomStorageDoor; // 음악실 창고 문
    [SerializeField]
    Enemy movedZombie; // 2층 최초 진입 시 이벤트 좀비
    [SerializeField]
    GameObject movedZombiePos; // 2층 최초 진입 시 이벤트 좀비 목적지
    [SerializeField]
    GameObject soundSource; // 2층 최초 진입 시 이벤트 소리 근원지
    [SerializeField]
    StudentOfficeDoor studentOfficeDoor; // 2층 학생회실 문
    [SerializeField]
    GameObject tvSoundSource;
    [SerializeField]
    CurrentFloorUI currentFloorUI;
    [SerializeField]
    GameObject tvScreen;
    WaitForFixedUpdate waitTime;
    SectionRendering sectionRendering;

    bool isBroadcasted = false;

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

    public bool IsBroadcasted
    {
        get
        {
            return isBroadcasted;
        }

        set
        {
            isBroadcasted = value;
        }
    }

    public void FloorInit()
    {
        sectionRendering = sceneEventSystem.GetComponent<SectionRendering>();
        sectionRendering.FloorInit(sectionList, doorList);
    }

    void OnEnable()
    {
        Transform zombies = GameObject.Find("2FZombies").transform;
        waitTime = new WaitForFixedUpdate();
        sectionRendering = sceneEventSystem.GetComponent<SectionRendering>();
        sectionRendering.FloorInit(sectionList, doorList);
        currentFloorUI.UIupdate("2F");

        for (int i = 0; i < zombies.childCount; i++)
        {
            if (zombies.GetChild(i).gameObject.activeInHierarchy)
                zombies.GetChild(i).GetComponent<Enemy>().StartFSM();
        }

        if (!IsBroadcasted)
            StartCoroutine(MovedZombieDirect());
    }

    IEnumerator MovedZombieDirect() // 2층에서 동쪽 계단의 좀비 한마리의 이동을 연출
    {
        yield return new WaitForSeconds(0.5f);
        movedZombie.transform.position = movedZombiePos.transform.position;
        movedZombie.ListeningToSounds(new Sound(int.MaxValue, 1000), soundSource.transform.position);
    }

    public void SchoolOfficeEvent() // 교무실에서 열쇠 꾸러미를 픽했을 때, 좀비 5마리가 우당탕탕 뛰쳐나오는 이벤트 연출
    {
        StartCoroutine(SchoolOfficeEvent_());
    }

    IEnumerator SchoolOfficeEvent_()
    {
        sceneEventSystem.DirectMode();
        Camera.main.GetComponent<CameraMove>().SetTarget(tvSoundSource);
        schoolOfficeStorageDoor.IsUnlocked = true;
        schoolOfficeStorageDoor.BehaviorByInteraction(null);
        yield return new WaitForSeconds(0.5f);

        SoundGenerator.SpreadSound(new Sound(9999, 50), tvSoundSource.transform.position); // 좀비들 tv 근처로 우르르 몰려나옴

        yield return new WaitForSeconds(3);
        Camera.main.GetComponent<CameraMove>().SetTarget(sceneEventSystem.Player);
        sceneEventSystem.DirectMode();

        while (true)
        {
            yield return waitTime;
            if (tv.IsOn)
            {
                SoundGenerator.SpreadSound(new Sound(9999, 50), tvSoundSource.transform.position); // tv 켜진 후 소음 발생
                tvScreen.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                for (int i = 0; i < schoolOfficeStorageZombies.Count; i++) // 좀비들의 어그로 조정
                {
                    schoolOfficeStorageZombies[i].GetComponent<EnemyProperty>().ToRunRadius = 0;
                }
            }
            else
            {
                tvScreen.GetComponent<MeshRenderer>().materials[0].color = Color.black;
                for (int i = 0; i < schoolOfficeStorageZombies.Count; i++) // 좀비들의 어그로 조정
                {
                    schoolOfficeStorageZombies[i].GetComponent<EnemyProperty>().ToRunRadius = 350;
                }
            }
        }
    }

    public void MusicRoomEvent() // 음악실에서 문을 열었을 때, 이벤트 연출
    {
        StartCoroutine(MusicRoomEvent_());
    }

    IEnumerator MusicRoomEvent_()
    {
        sceneEventSystem.DirectMode();
        Camera.main.GetComponent<CameraMove>().SetTarget(musicRoomZombies[0].transform.position);
        yield return new WaitForSeconds(3f);
        Camera.main.GetComponent<CameraMove>().SetTarget(sceneEventSystem.Player);
        sceneEventSystem.DirectMode();

        yield return new WaitUntil(() => musicRoomStorageDoor.IsUnlocked);

        sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "빠루 끝을 문 사이에 넣고 힘을 줘서 문을 뜯어냈다.", 2);
        yield return new WaitForSeconds(2f);
        SoundGenerator.SpreadSound(new Sound(9999, 350), musicRoomStorageDoor.transform.position); // 음악실 문을 따고 소음 발생
    }

    public void StudentOffice() // 학생 회실에서 문을 열었을 때, 이벤트 연출
    {
        if (GetComponent<Door>().enabled)
            studentOfficeDoor.GetComponent<Door>().BehaviorByInteraction(sceneEventSystem.Player);
        else
            studentOfficeDoor.GetComponent<StudentOfficeDoor>().BehaviorByInteraction(sceneEventSystem.Player);
    }
}
