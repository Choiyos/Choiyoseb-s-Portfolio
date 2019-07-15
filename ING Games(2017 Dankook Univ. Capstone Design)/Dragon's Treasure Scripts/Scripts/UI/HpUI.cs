using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpUI : MonoBehaviour
{
    // 인게임에서 체력 표시를 위한 스크립트.
    void Update()
    {
        GetComponent<Text>().text = "    " + GetComponentInParent<HpComponent>().HealthPoint.ToString();
    }
}
