using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKinetic : MonoBehaviour
{
    // 플레이어의 밀림 방지용 스크립트.
    void OnTriggerExit(Collider other)
    {
        transform.parent.GetComponent<Rigidbody>().isKinematic = true;
        transform.parent.GetComponent<Rigidbody>().isKinematic = false;
    }
}
