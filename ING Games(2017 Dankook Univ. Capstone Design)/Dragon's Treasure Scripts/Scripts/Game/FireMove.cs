using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FireMove : NetworkBehaviour
{
    // 원거리 플레이어의 투사체 이동 기능을 하는 스크립트.
    public float speed;
    float _elapsedTime = 0f;
    public float lifetime;
    Vector3 forward_;

    void Awake()
    {
        forward_ = Vector3.forward;
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        transform.Translate(forward_ * Time.deltaTime * speed);
        if (_elapsedTime >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" || col.tag == "Map")
            Destroy(gameObject);
    }
}
