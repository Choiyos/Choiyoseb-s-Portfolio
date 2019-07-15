using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour
{
    // 인게임에서 열쇠 개수 표시를 위한 스크립트.
    void Update()
    {
        GetComponent<Text>().text = GetComponentInParent<KeyComponent>().numOfKey.ToString();
    }
}
