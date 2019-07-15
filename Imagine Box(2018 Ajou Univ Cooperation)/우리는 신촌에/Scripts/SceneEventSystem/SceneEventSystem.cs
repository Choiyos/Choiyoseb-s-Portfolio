using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class SceneEventSystem : MonoBehaviour
{
    bool isBlackouted; //정전상태 여부 확인용 변수.
    [SerializeField]
    GameObject fellowTeacher; //1층 동료 선생
    bool isInteractedThirdFloorCollabsedClass;
    //가장 최근 구조받고있는 아이
    string toBeSavedKid;

    #region 씬 이벤트에서 빈번하게 참조하는 변수들
    [SerializeField]
    GameObject player;
    [SerializeField]
    GameObject touchCanvas;
    [SerializeField]
    GameObject dialog;
    [SerializeField]
    GameObject joystick;
    [SerializeField]
    GameObject interactionButton;
    [SerializeField]
    GameObject systemButton;
    [SerializeField]
    GameObject inventoryButton;
    [SerializeField]
    GameObject directCanvas;
    [SerializeField]
    GameObject speechBubblesPool;
    [SerializeField]
    GameObject monologueBubblesPool;
    [SerializeField]
    GameObject systemMessagePool;
    [SerializeField]
    Image fade;
    [SerializeField]
    FirstFloor firstFloor;
    [SerializeField]
    PlayerProperty playerProperty;
    [SerializeField]
    List<bool> isTheFloorLocked; // 층이 잠겨 있는지 여부.

    public GameObject Player
    {
        get
        {
            return player;
        }

        set
        {
            player = value;
        }
    }

    public GameObject TouchCanvas
    {
        get
        {
            return touchCanvas;
        }

        set
        {
            touchCanvas = value;
        }
    }

    public GameObject Dialog
    {
        get
        {
            return dialog;
        }

        set
        {
            dialog = value;
        }
    }

    public GameObject Joystick
    {
        get
        {
            return joystick;
        }

        set
        {
            joystick = value;
        }
    }

    public GameObject InteractionButton
    {
        get
        {
            return interactionButton;
        }

        set
        {
            interactionButton = value;
        }
    }

    public GameObject SystemButton
    {
        get
        {
            return systemButton;
        }

        set
        {
            systemButton = value;
        }
    }

    public GameObject InventoryButton
    {
        get
        {
            return inventoryButton;
        }

        set
        {
            inventoryButton = value;
        }
    }

    public GameObject DirectCanvas
    {
        get
        {
            return directCanvas;
        }

        set
        {
            directCanvas = value;
        }
    }

    public GameObject SpeechBubblesPool
    {
        get
        {
            return speechBubblesPool;
        }

        set
        {
            speechBubblesPool = value;
        }
    }

    public GameObject SystemMessagePool
    {
        get
        {
            return systemMessagePool;
        }

        set
        {
            systemMessagePool = value;
        }
    }

    public FirstFloor FirstFloor
    {
        get
        {
            return firstFloor;
        }

        set
        {
            firstFloor = value;
        }
    }

    public List<bool> IsTheFloorLocked
    {
        get
        {
            return isTheFloorLocked;
        }

        set
        {
            isTheFloorLocked = value;
        }
    }

    public bool IsBlackouted
    {
        get
        {
            return isBlackouted;
        }

        set
        {
            isBlackouted = value;
        }
    }

    public string ToBeSavedKid
    {
        get
        {
            return toBeSavedKid;
        }

        set
        {
            toBeSavedKid = value;
        }
    }

    public bool IsInteractedThirdFloorCollabsedClass
    {
        get
        {
            return isInteractedThirdFloorCollabsedClass;
        }

        set
        {
            isInteractedThirdFloorCollabsedClass = value;
        }
    }
    #endregion

    public void OnPointerDown(PointerEventData eventData) // 터치 인식
    {
        List<RaycastResult> raycastList = new List<RaycastResult>();
        directCanvas.GetComponent<GraphicRaycaster>().Raycast(eventData, raycastList);
    }

    public IEnumerator SceneFade(bool InOrOut)
    {
        fade.gameObject.SetActive(true);
        if (InOrOut)
            fade.GetComponent<Image>().DOFade(1, 1).SetEase(Ease.InOutQuad);
        else
            fade.GetComponent<Image>().DOFade(0, 1).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(1);
        fade.gameObject.SetActive(false);
    }

    public IEnumerator SplitSound(string clip, float durationTime, float timeInterval, int count)
    {
        for (int i = 0; i < count; i++)
        {
            AudioManager.Instance.PlaySoundLoop(clip, durationTime);
            yield return new WaitForSeconds(timeInterval);
        }
    } // 다중 사운드 연출

    public void SpeechBubbles(GameObject target, string text, float time) // 말풍선을 띄움. target에 말풍선을 띄울 대상을 지정, text에 말풍선 내용을 지정, time에 지속시간을 지정
    {
        int count_temp = 0;
        for (int i = 0; i < speechBubblesPool.transform.childCount; i++)
        {
            if (speechBubblesPool.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                count_temp++;
                if (count_temp >= 2)
                    return;
            }
        }
        speechBubblesPool.GetComponent<ObjectPool>().ObjectPoolPop(target, text, time);
    }

    public void MonologueBubbles(GameObject target, string text, float time) // 혼잣말풍선을 띄움. target에 말풍선을 띄울 대상을 지정, text에 말풍선 내용을 지정, time에 지속시간을 지정
    {
        int count_temp = 0;
        for (int i = 0; i < monologueBubblesPool.transform.childCount; i++)
        {
            if (monologueBubblesPool.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                count_temp++;
                if (count_temp >= 2)
                    return;
            }
        }
        monologueBubblesPool.GetComponent<ObjectPool>().ObjectPoolPop(target, text, time);
    }

    public void PushSystemMessage(string message, float time) // 시스템 메세지를 띄움.
    {
        if (message != "")
            systemMessagePool.GetComponent<ObjectPool>().ObjectPoolPop(message, time);
    }

    public void DirectMode()
    {
        if (touchCanvas.activeSelf)
            touchCanvas.SetActive(false);
        else
            touchCanvas.SetActive(true);
    }

    /// <summary>
    /// 속도 비율을 줄이는 것이 아니라 Run만 불가능하게 변경해야함.
    /// </summary>
    /// <param name="player"></param>
    public void SaveStudent(PlayerInput playerInput)
    {
        playerInput.RunningState(false);
    }

    public void PlayerSpeedRestore(PlayerInput playerInput)
    {
        playerInput.RunningState(true);
    }

    /// <summary>
    /// 정전시나 방송실에서 1층으로 좀비를 몰았을 때 동료선생의 활성상태 변경.
    /// </summary>
    public void FellowTeacherActiveState(bool _flag)
    {
        fellowTeacher.SetActive(_flag);
    }
}
