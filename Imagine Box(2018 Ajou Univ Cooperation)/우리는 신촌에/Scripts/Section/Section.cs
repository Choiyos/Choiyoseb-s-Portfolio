using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Section : MonoBehaviour
{
    [Header("섹션 렌더러")]
    [SerializeField]
    SectionRendering renderer_;
    [Header("인접 섹션 리스트")]
    [SerializeField]
    private List<Section> sectionList;
    [Header("인접 섹션 개방성")]
    [SerializeField]
    private List<bool> sectionBoolList;
    [SerializeField]
    private List<GameObject> objectsInSection; // 해당 섹션에 있는 플레이어 or 적 오브젝트들.
    private List<Section> tempList;
    private PlayerProperty playerProperty;
    WaitForFixedUpdate waitTime;

    public List<GameObject> GetObjectsInSection
    {
        get
        {
            return objectsInSection;
        }
    }

    void OnEnable()
    {
        playerProperty = GameObject.Find("Player").GetComponent<PlayerProperty>();
        waitTime = new WaitForFixedUpdate();
        tempList = new List<Section>();
    }

    public void changeBoolList(Section sec, bool isOpen) // 섹션이 가지고 있는 개방성 정보를 변경.
    {
        int i;
        for (i = 0; i < sectionList.Count; i++)
        {
            if (sectionList[i] == sec)
                break;
        }
        sectionBoolList[i] = isOpen;
    }

    public IEnumerator RenderingCheck() // 자신에게 그리라는 표시(bool)를 남긴 후에, 자신과 연결된 개방된 섹션들에게도 동일한 메소드 요청.
    {
        yield return waitTime;
        if (renderer_.CheckedSection.ContainsKey(gameObject))
        {
            if (!renderer_.CheckedSection[gameObject]) // 다시 그리지 않아도 될 경우
            {
                renderer_.CheckedSection[gameObject] = true;
                List<Section> linkedSections = GetLinkedSection();
                for (int i = 0; i < linkedSections.Count; i++)
                {
                    yield return StartCoroutine(linkedSections[i].RenderingCheck());
                }
            }
        }
        else
        {
            if (playerProperty.UpAndDown) // 위쪽 방향으로 갔을 경우
            {
                int nextFloor = int.Parse(playerProperty.StandingSection.transform.parent.name.Substring(0, 1)) + 1;
                GameObject tempNextFloor = GameObject.Find(nextFloor.ToString() + "F");
                renderer_.delayCall(tempNextFloor.transform.GetChild(0).GetChild(0).gameObject);
            }
            else // 아래쪽 방향으로 갔을 경우
            {
                int nextFloor = int.Parse(playerProperty.StandingSection.transform.parent.name.Substring(0, 1)) - 1;
                GameObject tempNextFloor = GameObject.Find(nextFloor.ToString() + "F");
                renderer_.delayCall(tempNextFloor.transform.GetChild(0).GetChild(0).gameObject);
            }
        }
    }

    public List<Section> GetLinkedSection() // 자신의 섹션을 기준으로 개방된 섹션들의 리스트를 반환.
    {
        tempList.Clear();
        for (var i = 0; i < sectionList.Count; i++)
        {
            if (sectionBoolList[i])
            {
                tempList.Add(sectionList[i]);
            }
        }
        return tempList;
    }

    public void AddObjectInSection(GameObject obj)
    {
        if (!objectsInSection.Contains(obj))
            objectsInSection.Add(obj);
    }

    public void RemoveObjectInSection(GameObject obj)
    {
        if (objectsInSection.Contains(obj))
            objectsInSection.Remove(obj);
    }
}
