using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowUIBackground : MonoBehaviour, IPointerDownHandler
{
    // 해당 스크립트를 특정 UI에 넣을 경우, 플레이 상에서 해당 오브젝트를 클릭했을 때 부모 오브젝트의 acitve상태가 false.
    // 모바일 게임 UI 기능에서 바깥쪽을 클릭했을 때, 해당 UI가 꺼지는 기능이 많을 것으로 보이고 간단하게 유용하게 쓰일 것이라고 생각함.
    
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.parent.gameObject.SetActive(false);
    }
}
