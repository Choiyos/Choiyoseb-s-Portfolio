using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprinkler : MonoBehaviour, BehavioralObject
{
    [SerializeField]
    SceneEventSystem sceneEventSystem;
    [SerializeField]
    List<EnemyProperty> zombies;

    public void BehaviorByInteraction(GameObject player)
    {
        if (!sceneEventSystem.IsBlackouted)
        {
            // 정전 중이 아니라면,
            sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "알코올 램프가 있다. 불을 붙이면 큰 화재가 날 것만 같다.", 3);
        }
        else
        {
            if (sceneEventSystem.Player.GetComponent<Inventory>().IsItemExist("Lighter"))
            {
                // 정전 중이라면,
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "알코올 램프에 라이터로 불을 붙였다. 화재가 발생해 스프링 쿨러가 작동했다.", 3);
                StartCoroutine(OnFire());
            }
            else
            {
                sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "불을 붙일 무언가가 필요하다.", 3);
            }
        }
    }

    IEnumerator OnFire() // 화재 발생 이벤트
    {
        yield return null;
        foreach (var zombie in zombies) // 좀비들의 어그로 현저히 감소.
        {
            zombie.ToRunRadius = 50;
            zombie.RunToAttackRadius = 25;
            zombie.RunToIdleRadius = 30;
        }
        sceneEventSystem.IsTheFloorLocked[0] = false;
        sceneEventSystem.FirstFloor.IsSprinkle = true;

        gameObject.layer = 0;
    }
}
