using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 층끼리 정보 전송을 담당하는 임시 스크립트.
/// </summary>
public class InterfloorInfoTransfer : MonoBehaviour {

    //3층 무너진 교실 문과 상호작용했는지 여부.
    [SerializeField]
    bool isInteractedThirdFloorCollabsedClass = false;


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
}
