using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Arrow { Up, Down }

public class FirstFloor : MonoBehaviour
{
    // 성준's 메세지 - 자물쇠 관련 변수 및 뺄수 있는 모든 시리얼라이즈 변수는 빼길. 그리고 변수는 항상 위쪽에 몰아넣고, 다음에 속성 메소드, 다음에 Mono 메소드(Awake, Start, Update 등)이 오도록 하고,
    //                 그 다음으로 필요한 메소드를 되도록 호출 순으로 배치할 것. 안 중요해 보이지만 협업에서 가독성을 위해 코드 정리는 필수!
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
    Door infirmaryDoor; // 양호실 문
    [SerializeField]
    CurrentFloorUI currentFloorUI;
    #region 자물쇠 관련  변수
    [SerializeField]
    Text[] cipherText;//자물쇠 네개의 숫자 텍스트를 담아놓을 배열.
    [SerializeField]
    GameObject lockWindow;
    bool isLockOpened = false;
    [SerializeField]
    [Header("자물쇠 비밀번호")]
    string cipher = "1234";

    [SerializeField]
    StudentOfficeDoor studentOfficeDoor;
    bool isSprinkle = false;
    #endregion
    #region 아이구출관련 변수
    [SerializeField]
    GameObject interactionButton;
    [SerializeField]
    [Header("아이구출시 이동속도 감소 비율")]
    [Range(0, 1)]
    float reductionRate;
    #endregion
    #region 수위아저씨한테 전화 관련 변수
    [SerializeField]
    [Header("수위아저씨 벨소리 범위")]
    float bellSoundRange = 20.0f;
    [SerializeField]
    bool isSavedGuardNumber = false;
    [SerializeField]
    bool isOpenedCafeteria = false;
    [SerializeField]
    GameObject callButton;
    #endregion

    public bool IsLockOpened
    {
        get
        {
            return isLockOpened;
        }

        set
        {
            isLockOpened = value;
        }
    }

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

    public bool IsSprinkle
    {
        get
        {
            return isSprinkle;
        }

        set
        {
            isSprinkle = value;
        }
    }

    public void FloorInit()
    {
        sectionRendering = sceneEventSystem.GetComponent<SectionRendering>();
        sectionRendering.FloorInit(sectionList, doorList);
    }

    void OnEnable()
    {
        sectionRendering = sceneEventSystem.GetComponent<SectionRendering>();
        sectionRendering.FloorInit(sectionList, doorList);
        Transform zombies = GameObject.Find("1FZombies").transform;
        if (currentFloorUI.GetComponent<Text>().text != "1F")
            currentFloorUI.UIupdate("1F");
        for (int i = 0; i < zombies.childCount; i++)
        {
            if (zombies.GetChild(i).gameObject.activeInHierarchy)
                zombies.GetChild(i).GetComponent<Enemy>().StartFSM();
        }
    }


    #region 자물쇠 열기 관련 코드
    public void TryUnlock()
    {
        string inputCipher = String.Empty;
        foreach (Text item in cipherText)
        {
            inputCipher = string.Concat(inputCipher, item.text);
        }
        if (inputCipher == cipher)
        {
            StartCoroutine(OpenMessage());
            OpenSuccess();
        }
        else
        {
            sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "열리지 않는다.", 1.5f);
        }
        Time.timeScale = 1;
        lockWindow.SetActive(false);
    }

    private IEnumerator OpenMessage()
    {
        sceneEventSystem.PushSystemMessage("자물쇠가 열렸다.", 1);
        yield return new WaitForSeconds(1);
        sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "여러가지 스위치가 보인다.\n방화 셔터라고 적힌 스위치를 조작했다.", 2);
        yield return new WaitForSeconds(2);
        sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "복도 쪽에서 무언가 올라가는 소리가 들린다.", 1.5f);
        yield return new WaitForSeconds(1.5f);
        sceneEventSystem.IsTheFloorLocked[0] = true; // 2층 계단 개방.
    }

    private void OpenSuccess()
    {
        IsLockOpened = true;

        //방화셔터 올리는 작업.
    }

    public void Cipher(string value)
    {
        string[] temp = value.Split('_');
        string order = temp[0];
        string upOrDown = temp[1];
        switch (upOrDown)
        {
            case "UP":
                cipherText[int.Parse(order) - 1].text = int.Parse(cipherText[int.Parse(order) - 1].text) == 9
                    ? (0).ToString() : ((int.Parse(cipherText[int.Parse(order) - 1].text)) + 1).ToString();
                break;
            case "DOWN":
                cipherText[int.Parse(order) - 1].text = int.Parse(cipherText[int.Parse(order) - 1].text) == 0
                    ? (9).ToString() : ((int.Parse(cipherText[int.Parse(order) - 1].text)) - 1).ToString();
                break;
            default:
                break;
        }
    }

    public void LockWindowOpen()
    {
        if (!IsLockOpened)
        {
            foreach (Text item in cipherText)
            {
                item.text = "0";
            }
            lockWindow.SetActive(true);
            Time.timeScale = 0;
        }
        else if (IsSprinkle)
        {
            //정전상태 해제, 방화셔터 올림
            sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "방화셔터를 올리고 전기를 복구시켰다.", 1.5f);
            sceneEventSystem.Player.transform.GetChild(3).gameObject.SetActive(false);
            sceneEventSystem.IsTheFloorLocked[0] = true;
            sceneEventSystem.IsBlackouted = false;
            IsSprinkle = false;

            if (studentOfficeDoor.gameObject)
            {
                studentOfficeDoor.GetComponent<Door>().IsUnlocked = true;
                DestroyImmediate(studentOfficeDoor);
            }

        }
        else
        {
            sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "이미 열려있다.", 1.5f);
        }
    }
    #endregion

    #region 식당문열고, 수위한테 전화하기 관련 코드
    public void CallToGuards()
    {
        AudioManager.Instance.PlaySoundOneShot("BGM002", 5);
        SoundGenerator.SpreadSound(new Sound(9999, bellSoundRange), GameObject.Find("GuardZombie").transform.position);
        callButton.SetActive(false);
    }

    public void EventTrigger(string cond) // 이벤트 발생시 해당 조건을 스위치
    {
        switch (cond)
        {
            case "cafeteriaKey":
                OpenCafeteriaDoor();
                break;
            case "Computer":
                SavePhoneNumber();
                break;
        }
    }

    void SavePhoneNumber()
    {
        isSavedGuardNumber = true;
        CheckCallIconActiveCondition();
    }

    void OpenCafeteriaDoor()
    {
        isOpenedCafeteria = true;
        CheckCallIconActiveCondition();
    }

    private void CheckCallIconActiveCondition()
    {
        if (isSavedGuardNumber && isOpenedCafeteria)
        {
            callButton.SetActive(true);
        }
    }
    #endregion

}
