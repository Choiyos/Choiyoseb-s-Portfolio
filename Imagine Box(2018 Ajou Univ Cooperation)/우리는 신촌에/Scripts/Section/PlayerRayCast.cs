using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{
    // 플레이어 기준에서 Raycast를 던지고 현재 자신이 위치한 섹션을 알아내는 기능.
    RaycastHit hit;
    [SerializeField]
    SectionRendering renderer_;
    PlayerProperty playerProperty;
    IEnumerator updateCoroutine;

    public Transform GetSection
    {
        get { return hit.transform.parent.parent; }
    }

    void Awake()
    {
        playerProperty = GetComponent<PlayerProperty>();
        StartUpdate();
    }

    public void StopUpdate()
    {
        StopCoroutine(updateCoroutine);
    }

    public void StartUpdate()
    {
        updateCoroutine = update();
        StartCoroutine(updateCoroutine);
    }

    IEnumerator update()
    {
        WaitForSeconds waitTime = new WaitForSeconds(0.1f);
        while (true)
        {
            yield return waitTime;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 50, 1 << 9)) // hit할 경우 section 검색 성공.
            {
                Section section = hit.transform.parent.parent.GetComponent<Section>();
                playerProperty.StandingSection = section;
                yield return StartCoroutine(renderer_.StartSectionRender(section)); // 렌더러에게 해당 섹션을 기준으로 그리라고 요청.
            }
        }
    }
}
