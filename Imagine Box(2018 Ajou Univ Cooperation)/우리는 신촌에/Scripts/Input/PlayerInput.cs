using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Idle, Sneak, Walk, Run }
public enum PlayerAnimation { Idle, Move, Run }

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    Joystick joystick; //조이스틱 오브젝트
    GameObject lookDir;
    Animator animator;
    SpriteRenderer spriteRenderer;
    PlayerProperty property;
    IEnumerator routine;
    CardinalDirections dir; //플레이어가 바라보는 방향용 enum
    float movespeed; // 이동 속도
    float runMagnitude = 0.7f; //뛰는 상태 없애기 위한 변수.
    private Vector3 moveVector; //회전,이동 방향
    Sound[] sounds;

    void Start()
    {
        Init();
        SoundsInit();
    }

    void Update()
    {
        Move();
        Rotation();
        property.State = UpdateState();
        UpdateByState();
    }

    void Init()
    {
        property = GetComponent<PlayerProperty>();
        animator = transform.Find("Model").GetComponent<Animator>();
        lookDir = transform.Find("LookDir").gameObject;
        spriteRenderer = transform.Find("Model").GetComponent<SpriteRenderer>();
        property.State = PlayerState.Idle;
    }

    void SoundsInit()
    {
        sounds = new Sound[4];
        sounds[(int)PlayerState.Idle] = new Sound(0f, 1f);
        sounds[(int)PlayerState.Sneak] = new Sound(property.SneakVolume, property.SneakRange);
        sounds[(int)PlayerState.Walk] = new Sound(property.WalkVolume, property.WalkRange);
        sounds[(int)PlayerState.Run] = new Sound(property.RunVolume, property.RunRange);
    }

    private void Move()
    {
        moveVector = GetMoveInput();
        transform.Translate(moveVector.normalized * movespeed * Time.deltaTime);
    }

    private Vector3 GetMoveInput()
    {
        float h = joystick.GetHorizontalValue();
        float v = joystick.GetVerticalValue();
        Vector3 moveDir = new Vector3(h, 0, v);

        return moveDir;
    }

    private void Rotation()
    {
        if (moveVector != Vector3.zero)
            lookDir.transform.forward = moveVector;
        SpriteFlipUpdate();
    }

    void SpriteFlipUpdate()
    {
        float angle = lookDir.transform.localEulerAngles.y;
        spriteRenderer.flipX = (angle < 180) ? false : true;
    }

    PlayerState UpdateState()
    {
        if (moveVector.magnitude > runMagnitude)
        {
            return PlayerState.Run;
        }
        if (moveVector.magnitude > 0.3f)
        {
            return PlayerState.Walk;
        }
        if (moveVector.magnitude != 0.0f)
        {
            return PlayerState.Sneak;
        }
        return PlayerState.Idle;

    }

    public void RunningState(bool _flag)
    {
        if (!_flag)
            runMagnitude = 2f;
        else
            runMagnitude = 0.7f;
    }

    void UpdateByState()
    {
        SoundGenerator.SpreadSound(sounds[(int)property.State], transform.position); // 프로파일러 확인 결과 가비지 발생하는 코드임. 추후에 수정해야할 필요가 있음.
        switch (property.State)
        {
            case PlayerState.Idle:
                {
                    movespeed = 0;
                    animator.SetInteger("State", (int)PlayerAnimation.Idle);
                    AudioManager.Instance.StopStepSound();
                    break;
                }
            case PlayerState.Sneak:
                {
                    movespeed = property.SneakSpeed;
                    animator.SetInteger("State", (int)PlayerAnimation.Move);
                    AudioManager.Instance.PlayStepSound("Sneak");
                    break;
                }
            case PlayerState.Walk:
                {
                    movespeed = property.WalkSpeed;
                    animator.SetInteger("State", (int)PlayerAnimation.Move);
                    AudioManager.Instance.PlayStepSound("Walk1");
                    break;
                }
            case PlayerState.Run:
                {
                    movespeed = property.RunSpeed;
                    animator.SetInteger("State", (int)PlayerAnimation.Run);
                    AudioManager.Instance.PlayStepSound("Run1");
                    break;
                }
        }
    }
}
