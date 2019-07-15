using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Stroll, Move, Run, Attack }
public enum EnemyAnimation { Idle, Move, Attack }

public class Enemy : MonoBehaviour
{
    GameObject player;
    Animator animator;
    SpriteRenderer spriteRenderer;
    NavMeshAgent nav;
    Transform target; // 목표물 변수
    Vector3 targetPos; // 목표지점 변수
    float weightOfSound; // 소리의 가중치 변수, (소리의 크기/ 소음의 근원지까지의 거리)로 계산
    IEnumerator routine; // FSM 제어용 변수
    float elapsTime; // Stroll 이동을 위한 변수
    float strollTime; // stroll 시간 변수
    Vector3 tempPos; // Idle 이동을 위한 임시 변수
    EnemyProperty property;

    GameObject exclamationPool; // 느낌표 오브젝트 풀
    GameObject exclamationMark; // 느낌표 오브젝트

    public GameObject Player
    {
        set
        {
            player = value;
        }
    }

    void Awake()
    {
        Init();
        StartFSM();
    }

    public IEnumerator Dead()
    {
        StopCoroutine(routine);
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    IEnumerator FSM()
    {
        float distanceToPlayer;

        while (true)
        {
            yield return null;
            if (!player)
            {
                TransitionToIdle();
                AnimatorActiveCheck(EnemyAnimation.Idle);
                yield break; // 플레이어가 없을 경우 FSM 중단
            }
            else
            {
                distanceToPlayer = (transform.position - player.transform.position).sqrMagnitude;
                StateUpdate();
            }

            switch (property.State)
            {
                case EnemyState.Idle:
                    {
                        if (CalculateRange(distanceToPlayer, property.ToRunRadius)) // Idle 도중에 플레이어가 시야 안에 들어왔을 경우
                            TransitionToRun();
                        else if (elapsTime <= property.IdleDurationTime) // Idle 도중에 Idle 지속시간이 지나지 않은 경우
                            elapsTime += Time.deltaTime;
                        else // Idle 도중에 Idle 지속시간이 지난 경우
                            TransitionToStroll();
                        break;
                    }
                case EnemyState.Stroll:
                    {
                        if (CalculateRange(distanceToPlayer, property.ToRunRadius)) // Stroll 도중에 플레이어가 시야 안에 들어왔을 경우
                            TransitionToRun();
                        else if (elapsTime <= strollTime) // Stroll 도중에 아직 목표 거리에 다다르지 못했을 경우
                            elapsTime += Time.deltaTime;
                        else // Stroll 도중에 목표 거리에 도착했을 경우
                            TransitionToIdle();
                        break;
                    }
                case EnemyState.Move:
                    {
                        if (distanceToPlayer <= property.ToRunRadius) // Move 도중에 플레이어가 시야 안에 들어왔을 경우
                            TransitionToRun();
                        else if ((transform.position - targetPos).sqrMagnitude <= 0.5f) // Move 도중에 소음의 근원지에 도착했을 경우
                        {
                            TransitionToIdle();
                        }
                        else
                            SetMovepoint(targetPos);
                        break;
                    }
                case EnemyState.Run:
                    {
                        if (CalculateRange(distanceToPlayer, property.RunToAttackRadius) && IsSameSection()) // 플레이어가 공격 사거리 안에 있고 같은 공간일 경우
                            property.State = EnemyState.Attack;
                        else if (CalculateRange(distanceToPlayer, property.RunToIdleRadius)) // 플레이어가 포착 시야 안에 있을 경우
                            SetMovepoint(target.position);
                        else // 플레이어가 포착 시야 안에서 벗어날 경우
                            TransitionToIdle();
                        break;
                    }
                case EnemyState.Attack:
                    {
                        player.SendMessage("BeAttacked", gameObject, SendMessageOptions.DontRequireReceiver);
                        AudioManager.Instance.PlaySoundOneShot("ZombieAttack", 2);
                        yield return new WaitForSeconds(1f);
                        if (!player)
                        {
                            TransitionToIdle();
                        }
                        break;
                    }
            }
        }
    }

    public void Init()
    {
        property = GetComponent<EnemyProperty>();
        property.State = EnemyState.Idle;
        tempPos = transform.position;
        nav = GetComponent<NavMeshAgent>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");
        exclamationPool = GameObject.Find("ExclamationPool");
        exclamationMark = null;
        weightOfSound = 0;
        elapsTime = 0;
    }

    public void StartFSM() // FSM을 시작
    {
        routine = FSM();
        StartCoroutine(routine);
    }

    void SetMovepoint(Vector3 pos) // 이동 목적지를 초기화
    {
        spriteRenderer.flipX = (pos - transform.position).x > 0 ? true : false;
        nav.SetDestination(pos);
    }

    void TransitionToIdle() // Idle상태로 전이
    {
        property.State = EnemyState.Idle;
        elapsTime = 0;
        if (exclamationMark)
        {
            StartCoroutine(exclamationMark.GetComponent<ExclamationAnimation>().Dispose()); // 느낌표 해제
            exclamationMark = null;
        }
        tempPos = transform.position;
    }

    void TransitionToStroll() // Stroll상태로 전이
    {
        elapsTime = 0;
        property.State = EnemyState.Stroll;
        strollTime = property.StrollDistance / property.StrollSpeed;

        float distanceToOrigin = (transform.position - tempPos).sqrMagnitude;

        // Stroll 범위를 계산해서 (기존 위치 기준에서 일정 거리 랜덤으로 이동) or (원래 제자리 방향으로 이동)
        Vector3 newDir = CalculateRange(distanceToOrigin, property.StrollRange) ?
            transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized : transform.position + (tempPos - transform.position).normalized;

        SetMovepoint(newDir);
    }

    void TransitionToRun() // 플레이어에게 돌진
    {
        elapsTime = 0;
        property.State = EnemyState.Run;
        target = player.transform;

        if (!exclamationMark)
            exclamationMark = exclamationPool.GetComponent<ObjectPool>().ObjectPoolPop(gameObject); // 느낌표 띄움
    }

    bool IsSameSection() // 플레이어와 자신이 같은 섹션인지를 검사
    {
        return property.StandingSection == player.GetComponent<PlayerProperty>().StandingSection ? true : false;
    }

    bool CalculateRange(float distance, float range) // 거리와 범위를 비교해 결과 반환.
    {
        return (distance <= range) ? true : false;
    }

    public void AnimatorActiveCheck(EnemyAnimation anim) // 애니메이터의 액티브 상태를 체크 후 true일 경우 애니메이션 변경.
    {
        if (animator.gameObject.activeSelf)
            animator.SetInteger("AnimState", (int)anim);
        else
            return;
    }

    public void ListeningToSounds(Sound sound, Vector3 origin) // 소리를 듣는 콜백, 가중치를 실시간으로 계산해 행동을 결정함 
    {
        float distanceToSound = sound.Volume / (transform.position - origin).sqrMagnitude;
        if (CalculateWeightOfSound(distanceToSound))
        {
            weightOfSound = distanceToSound;
            targetPos = origin;
            property.State = EnemyState.Move;
        }
    }

    bool CalculateWeightOfSound(float distanceToSound) // Run, Attack 상태가 아니면서, 최근에 들린 소리의 가중치가 기존 가중치보다 클 경우 true 반환.
    {
        if (property.State == EnemyState.Run) return false;
        if (property.State == EnemyState.Attack) return false;
        if (weightOfSound >= distanceToSound) return false;
        return true;
    }

    void StateUpdate() // 상태에 따라 속성을 조정하는 업데이트
    {
        switch (property.State)
        {
            case EnemyState.Idle:
                AnimatorActiveCheck(EnemyAnimation.Idle);
                weightOfSound = 0;
                nav.speed = 0;
                break;
            case EnemyState.Stroll:
                AnimatorActiveCheck(EnemyAnimation.Move);
                nav.speed = property.StrollSpeed;
                break;
            case EnemyState.Move:
                AnimatorActiveCheck(EnemyAnimation.Move);
                nav.speed = property.MoveSpeed;
                break;
            case EnemyState.Run:
                AnimatorActiveCheck(EnemyAnimation.Move);
                nav.speed = property.RunSpeed;
                break;
            case EnemyState.Attack:
                AnimatorActiveCheck(EnemyAnimation.Attack);
                nav.speed = 0.1f;
                break;
        }
    }
}
