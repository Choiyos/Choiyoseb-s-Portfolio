using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionRendering : MonoBehaviour
{
    // 실시간으로 현재 플레이어 위치에 기반한 렌더링(Active)을 결정하고 가시화해주는 스크립트.
    // 섹션, 해당 섹션 위의 오브젝트들을 렌더링.
    [Header("모든 섹션 리스트")]
    [SerializeField]
    List<Section> sectionList;
    [Header("모든 문 리스트")]
    [SerializeField]
    List<Door> doorList;

    Dictionary<GameObject, bool> checkedSection; // 렌더링 순회를 할 때 방문여부를 확인하기 위한 딕셔너리.

    public Dictionary<GameObject, bool> CheckedSection
    {
        get
        {
            return checkedSection;
        }

        set
        {
            checkedSection = value;
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

    public void FloorInit(List<Section> sectionList_, List<Door> doorList_)
    {
        sectionList = sectionList_;
        doorList = doorList_;
        checkedSection = new Dictionary<GameObject, bool>();

        for (int i = 0; i < sectionList.Count; i++)
        {
            checkedSection.Add(sectionList[i].gameObject, false); // 딕셔너리 초기화
        }
    }

    public IEnumerator StartSectionRender(Section startSection)
    {
        for (int i = 0; i < checkedSection.Count; i++) // 딕셔너리 초기화
        {
            checkedSection[sectionList[i].gameObject] = false;
        }
        yield return StartCoroutine(startSection.RenderingCheck()); // 플레이어 기준으로부터 렌더링 여부 확인 시작

        RenderSection();
        RenderDoor();
    }

    public void ObjectsRender(Section section, bool value) // 현재 섹션 안에 있는 오브젝트들을 렌더링 활성화.
    {
        foreach (var item in section.GetObjectsInSection)
        {
            item.transform.GetChild(0).gameObject.SetActive(value);
        }
    }

    void DoRender(GameObject obj) // 섹션을 그리는 메소드
    {
        if (!obj.activeSelf)
        {
            obj.SetActive(true);
            ObjectsRender(obj.transform.parent.GetComponent<Section>(), true);
        }
    }

    void DoNotRender(GameObject obj) // 섹션을 그리지 않는 메소드
    {
        if (obj.activeSelf)
        {
            obj.SetActive(false);
            ObjectsRender(obj.transform.parent.GetComponent<Section>(), false);
        }
    }

    void RenderSection()
    {
        for (int i = 0; i < checkedSection.Count; i++) // 모든 연결된 섹션 정보를 바탕으로 렌더링 여부를 결정해 렌더링
        {
            if (checkedSection[sectionList[i].gameObject])
            {
                DoRender(sectionList[i].transform.GetChild(0).gameObject);
            }
            else
            {
                DoNotRender(sectionList[i].transform.GetChild(0).gameObject);
            }
        }
    }

    void RenderDoor()
    {
        for (int i = 0; i < doorList.Count; i++) // 섹션의 정보를 바탕으로 문의 렌더링 여부를 결정해 렌더링
        {
            if (checkedSection[doorList[i].SectionList[0].gameObject] || checkedSection[doorList[i].SectionList[1].gameObject])
            {
                doorList[i].gameObject.SetActive(true);
                doorList[i].transform.parent.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                doorList[i].gameObject.SetActive(false);
                doorList[i].transform.parent.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    public void delayCall(GameObject gameObject)
    {
        gameObject.SetActive(true);
        StartCoroutine(delayCall_(gameObject));
    }

    public IEnumerator delayCall_(GameObject gameObject)
    {
        yield return new WaitForSeconds(0.2f);
        gameObject.SetActive(true);
    }
}
