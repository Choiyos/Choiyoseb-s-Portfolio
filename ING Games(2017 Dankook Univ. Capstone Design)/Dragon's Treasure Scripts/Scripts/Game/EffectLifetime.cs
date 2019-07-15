using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLifetime : MonoBehaviour {
    // 파티클 이펙트의 지속 시간을 조정하는 스크립트.
    float _elapsedTime = 0f;
    public float lifetime = 3f;

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= lifetime)
            Destroy(gameObject);
    }
}
