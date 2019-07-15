using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProperty : MonoBehaviour
{
    #region 적 기본 속성
    [Header("적 속성")]
    [SerializeField]
    EnemyState state;
    [SerializeField]
    float attackPoint;
    [SerializeField]
    Section standingSection; // 어느 섹션에 서 있는지
    #endregion
    #region 사거리 조건 확인 변수
    [Header("사거리 조건 확인 변수")]
    [SerializeField]
    private float toRunRadius; // Run으로 상태 전이를 위한 조건 변수
    [SerializeField]
    private float runToAttackRadius = 0.5f; // Run => Attack 상태 전이에서의 조건 변수
    [SerializeField]
    private float runToIdleRadius = 5f; // Run => Idle 상태 전이에서의 조건 변수
    #endregion
    #region 이동속도 변수
    [Header("이동속도 변수")]
    [SerializeField]
    private float moveSpeed = 0.8f; // Move 상태에서의 이동속도 변수
    [SerializeField]
    private float strollSpeed = 0.5f; // Move 상태에서의 이동속도 변수
    [SerializeField]
    private float runSpeed = 1.3f; // Run 상태에서의 이동속도 변수
    #endregion
    #region 사거리 조건 확인 변수
    [Header("Idle - Stroll 변수")]
    [SerializeField]
    private float strollDistance = 1; // Stroll 상태에서 한번에 움직일 거리 변수
    [SerializeField]
    private float strollRange = 1; // Stroll 상태에서 tempPos를 기준으로 움직일 수 있는 거리 제한
    [SerializeField]
    private float idleDurationTime = 2; // Idle 상태 지속 시간 변수
    #endregion
    #region 속성
    public float Damage
    {
        get { return this.attackPoint; }
    }

    public float ToRunRadius
    {
        get
        {
            return toRunRadius;
        }

        set
        {
            toRunRadius = value;
        }
    }

    public float RunToAttackRadius
    {
        get
        {
            return runToAttackRadius;
        }

        set
        {
            runToAttackRadius = value;
        }
    }

    public float RunToIdleRadius
    {
        get
        {
            return runToIdleRadius;
        }

        set
        {
            runToIdleRadius = value;
        }
    }

    public float MoveSpeed
    {
        get
        {
            return moveSpeed;
        }

        set
        {
            moveSpeed = value;
        }
    }

    public float StrollSpeed
    {
        get
        {
            return strollSpeed;
        }

        set
        {
            strollSpeed = value;
        }
    }

    public float RunSpeed
    {
        get
        {
            return runSpeed;
        }

        set
        {
            runSpeed = value;
        }
    }

    public float StrollDistance
    {
        get
        {
            return strollDistance;
        }

        set
        {
            strollDistance = value;
        }
    }

    public float StrollRange
    {
        get
        {
            return strollRange;
        }

        set
        {
            strollRange = value;
        }
    }

    public float IdleDurationTime
    {
        get
        {
            return idleDurationTime;
        }

        set
        {
            idleDurationTime = value;
        }
    }

    public EnemyState State
    {
        get
        {
            return state;
        }

        set
        {
            state = value;
        }
    }

    public Section StandingSection
    {
        get
        {
            return standingSection;
        }

        set
        {
            standingSection = value;
        }
    }
    #endregion
}
