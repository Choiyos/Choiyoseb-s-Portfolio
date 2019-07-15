using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstDamage : MonoBehaviour
{
    // Solider의 특수 기술인 수류탄 투척에 관련한 스크립트.
    Collider[] burstCheck;
    public string _identify_player;

    void Awake()
    {
        StartCoroutine(Burst());
    }

    IEnumerator Burst()
    {
        GetComponent<AudioSource>().PlayDelayed(0);
        yield return null;
        yield return null;
        burstCheck = Physics.OverlapSphere(transform.position, 2.3f);
        for (int i = 0; i < burstCheck.Length; i++)
        {
            if (burstCheck[i].transform.tag == "Player")
            {
                burstCheck[i].GetComponent<HpComponent>().OnBurst(_identify_player, 100);
            }
        }
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
