using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    // Input에 따른 캐릭터의 움직임을 위한 스크립트.
    public void Move(Vector2 direction)
    {
        transform.position += transform.forward * direction.x * Time.deltaTime +
            transform.right * direction.y * Time.deltaTime;
    }
}