using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProperty : MonoBehaviour
{
    #region 플레이어 기본 속성
    [Header("플레이어 속성")]
    [SerializeField]
    bool isAlive;
    [SerializeField]
    PlayerState state;
    [SerializeField]
    float healthPoint;
    [SerializeField]
    float interactionRadius; // 상호작용 범위(각)
    [SerializeField]
    float interactionRange; // 상호작용 사거리
    [SerializeField]
    Section standingSection; // 플레이어가 서 있는 섹션
    [SerializeField]
    bool isWithchild; // 아이와 함께 있는지
    [SerializeField]
    bool upAndDown; // 층 이동의 방향여부 (true는 up, false는 down)

    #endregion
    #region 상태별 이동속도 변수
    [Header("속도")]
    [Range(0, 5)]
    [SerializeField]
    private float sneakSpeed = 0.3f;
    [Range(0.5f, 9)]
    [SerializeField]
    private float walkSpeed = 1f;
    [Range(1, 15)]
    [SerializeField]
    private float runSpeed = 1.6f;
    #endregion
    #region 상태별 소리 크기 변수
    [Header("소리 크기")]
    [SerializeField]
    private float sneakVolume = 0.5f;
    [SerializeField]
    private float walkVolume = 1f;
    [SerializeField]
    private float runVolume = 3f;
    #endregion
    #region 상태별 소리 범위 변수
    [Header("소리 범위")]
    [SerializeField]
    private float sneakRange = 0.3f;
    [SerializeField]
    private float walkRange = 0.8f;
    [SerializeField]
    private float runRange = 2f;
    #endregion
    #region 이동상태변환 비율
    [Header("0 ~  sneakRatio일 때 살펴 걷기")]
    [SerializeField]
    [Range(0, 0.5f)]
    private float sneakRatio;
    [Header("sneakRatio ~ walkRatio일 때 걷기.")]
    [Range(0.5f, 1)]
    [SerializeField]
    private float walkRatio;
    #endregion
    #region 속성
    public float HealthPoint
    {
        get
        {
            return healthPoint;
        }

        set
        {
            healthPoint = value;
        }
    }

    public float InteractionRadius
    {
        get
        {
            return interactionRadius;
        }

        set
        {
            interactionRadius = value;
        }
    }

    public float InteractionRange
    {
        get
        {
            return interactionRange;
        }

        set
        {
            interactionRange = value;
        }
    }

    public float SneakSpeed
    {
        get
        {
            return sneakSpeed;
        }

        set
        {
            sneakSpeed = value;
        }
    }

    public float WalkSpeed
    {
        get
        {
            return walkSpeed;
        }

        set
        {
            walkSpeed = value;
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

    public float SneakVolume
    {
        get
        {
            return sneakVolume;
        }

        set
        {
            sneakVolume = value;
        }
    }

    public float WalkVolume
    {
        get
        {
            return walkVolume;
        }

        set
        {
            walkVolume = value;
        }
    }

    public float RunVolume
    {
        get
        {
            return runVolume;
        }

        set
        {
            runVolume = value;
        }
    }

    public float SneakRange
    {
        get
        {
            return sneakRange;
        }

        set
        {
            sneakRange = value;
        }
    }

    public float WalkRange
    {
        get
        {
            return walkRange;
        }

        set
        {
            walkRange = value;
        }
    }

    public float RunRange
    {
        get
        {
            return runRange;
        }

        set
        {
            runRange = value;
        }
    }

    public float SneakRatio
    {
        get
        {
            return sneakRatio;
        }

        set
        {
            sneakRatio = value;
        }
    }

    public float WalkRatio
    {
        get
        {
            return walkRatio;
        }

        set
        {
            walkRatio = value;
        }
    }

    public PlayerState State
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

    public bool IsAlive
    {
        get
        {
            return isAlive;
        }

        set
        {
            isAlive = value;
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

    public bool IsWithchild
    {
        get
        {
            return isWithchild;
        }

        set
        {
            isWithchild = value;
        }
    }

    public bool UpAndDown
    {
        get
        {
            return upAndDown;
        }

        set
        {
            upAndDown = value;
        }
    }
    #endregion
}
