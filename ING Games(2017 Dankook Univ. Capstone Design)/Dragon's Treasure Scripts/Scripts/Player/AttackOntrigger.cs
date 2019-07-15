using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackOntrigger : MonoBehaviour {
    // 근접 공격 캐릭터들의 무기 충돌 처리를 위한 스크립트.
    public float damage;
    float iniDamage;

    void Awake()
    {
        iniDamage = damage;
    }

    void Update()
    {
        damage = iniDamage + gameObject.transform.root.GetComponent<Player>().additive_Attackpower;
    }
}
