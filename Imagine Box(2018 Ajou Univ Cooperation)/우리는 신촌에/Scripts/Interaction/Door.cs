using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour, BehavioralObject
{
    bool isOpen;
    [Header("연결 섹션")]
    [SerializeField]
    List<Section> sectionList = new List<Section>();

    [SerializeField]
    string lockedMessage = "문이 잠겨있다.";
    [Header("열쇠 이름")]
    [SerializeField]
    List <string> requiredItemList;

    [SerializeField]
    bool isUnlocked = false;
    SceneEventSystem sceneEventSystem; //우선은 직접참조.
    Inventory inventory;
    WaitForSeconds wait = new WaitForSeconds(1);

    public bool IsOpen
    {
        get { return this.isOpen; }
        set { isOpen = value; }
    }

    public List<Section> SectionList
    {
        get
        {
            return sectionList;
        }
    }

    public bool IsUnlocked
    {
        get
        {
            return isUnlocked;
        }
        set
        {
            isUnlocked = value;
        }
    }

    public string LockedMessage
    {
        get
        {
            return lockedMessage;
        }

        set
        {
            lockedMessage = value;
        }
    }

    void Awake()
    {
        isOpen = false;
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
        inventory = sceneEventSystem.Player.GetComponent<Inventory>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        CheckLockedDoor();

        if (IsUnlocked)
        {
            
            if (!isOpen)
            {
                isOpen = true;
                if(gameObject.activeInHierarchy)
                    StartCoroutine(OpenAnimation());
            }
            else
            {
                isOpen = false;
                if (gameObject.activeInHierarchy)
                    StartCoroutine(CloseAnimation());
            }
            callSection();
        }
        else
        {
            sceneEventSystem.PushSystemMessage(LockedMessage, 1);
        }
    }

    /// <summary>
    /// 문이 잠겨있는지, 열기위한 조건을 모두 충족하는지 확인하는 함수.
    /// </summary>
    void CheckLockedDoor()
    {
        if (!IsUnlocked) //문 열려있다면 패스
        {
            List<Item > foundItem = new List<Item>();
            foreach (string item in requiredItemList)
            {
                if (item != "" && inventory.IsItemExist(item))
                {
                    foundItem.Add(inventory.GetItem(item));
                    isUnlocked = true;
                }
                else
                {
                    isUnlocked = false;
                    break;
                }

            }
            if (IsUnlocked) {
                foreach (Item item in foundItem)
                {
                    sceneEventSystem.PushSystemMessage(item.itemKorName + " 아이템을 사용했다.", 1);
                    if(item.itemType == Item.ItemType.OneTimeKey)
                        inventory.RemoveItemByName(item.itemEngName);
                    //sceneEventSystem.
                }
            }
        }
    }

    IEnumerator OpenAnimation()
    {
        float value = transform.localPosition.y + 4;
        gameObject.layer = 0;
        transform.DOLocalMoveY(value, 1);
        yield return wait;
        gameObject.layer = 8;
    }

    IEnumerator CloseAnimation()
    {
        float value = transform.localPosition.y - 4;
        gameObject.layer = 0;
        transform.DOLocalMoveY(value, 1);
        yield return wait;
        gameObject.layer = 8;
    }

    void callSection() // 각 섹션에게 변경사항을 알리는 메소드
    {
        if (sectionList.Count != 0)
        {
            sectionList[0].changeBoolList(sectionList[1], isOpen);
            sectionList[1].changeBoolList(sectionList[0], isOpen);
        }
    }
}
