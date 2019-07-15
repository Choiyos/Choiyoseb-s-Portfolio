using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // 3인칭 카메라의 이동을 위한 스크립트.
    public GameObject player;
    Vector3 playerPos;
    public float movSensitive;

    void Update()
    {
        playerPos = player.transform.position + player.transform.forward * 3.5f;
        transform.rotation = Quaternion.LookRotation(playerPos - transform.position);
    }
}
