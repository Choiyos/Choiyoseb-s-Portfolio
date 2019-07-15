using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour, BehavioralObject
{
    [SerializeField]
    List<EnemyProperty> zombies;

    public void BehaviorByInteraction(GameObject player)
    {
        StartCoroutine(ActiveFirework());
    }

    IEnumerator ActiveFirework()
    {
        gameObject.tag = "Untagged";
        foreach (var zombie in zombies) // 좀비들의 어그로 현저히 감소.
        {
            zombie.ToRunRadius = 1;
            zombie.RunToAttackRadius = 1;
            zombie.RunToIdleRadius = 1;
        }
        yield return new WaitForSeconds(8);
        foreach (var zombie in zombies) // 좀비들의 원상 복구.
        {
            zombie.ToRunRadius = 350;
            zombie.RunToAttackRadius = 50;
            zombie.RunToIdleRadius = 500;
        }
        gameObject.SetActive(false);
    }
}
