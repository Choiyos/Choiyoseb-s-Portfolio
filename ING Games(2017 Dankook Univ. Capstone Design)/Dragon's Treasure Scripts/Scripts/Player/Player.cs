using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(MoveController))]
public class Player : NetworkBehaviour
{
    // 캐릭터의 속성들과 죽음 및 리스폰, 움직임, 아이템에 따른 상호작용 등을 처리하는 전반적인 플레이어 기능을 담당하는 스크립트.
    #region 움직임 위한 변수들
    [SerializeField]
    bool cursorLock = false;
    private float verticalRotation = 0;
    private float mouseSensitivity = 1;
    public float rotSentsitive = 1;
    private MoveController m_MoveController;
    public GameObject child;

    #endregion

    #region 리스폰 위한 변수
    [SyncVar]
    private bool _isDead = false;
    [SerializeField]
    private Behaviour[] disableOnDeath;
    Transform spawnPoint;
    private bool[] wasEnabled;
    public float respawnTime = 4f;
    public GameObject effectPrefeb;
    #endregion

    #region 캐릭터 속성 변수
    public float speed; // 이동 속도
    public float additive_Attackpower = 0; // 추가 공격력
    public float atkSpeed; // 공격 속도
    public float defPower; // 방어력
    public int killScore = 0; // 킬 스코어, 동기화 완료
    public int deathScore = 0; // 데스 스코어, 동기화 완료
    #endregion

    #region 리스폰 위한 함수들

    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    public void Setup()
    {
        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        SetDefaults();
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        if (!isDead)
            Die();
    }

    private void Die()
    {
        isDead = true;
        StartCoroutine(DieAnimation());

        if (GetComponent<KeyComponent>().numOfKey > 0)
        {
            GetComponent<KeyComponent>().GetOfKey(-1);
            KeySpawn.Instance.CmdSpawn(transform.position);
        }
        additive_Attackpower = 0; // 추가 공격력 초기화
        speed = 3.5f; // 이동 속도 초기화

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        Collider _col = GetComponent<CapsuleCollider>();
        if (_col != null)
            _col.enabled = false;

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        respawnTime += 4;
        SetDefaults();
        Transform _spawnPoint = spawnPoint.GetChild(Random.Range(0, spawnPoint.childCount));
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
        yield return new WaitForSeconds(0.2f);
        CmdRespawnEffect(_spawnPoint.position);
    }

    IEnumerator DieAnimation()
    {
        GetComponent<Animator>().SetBool("isDead", true);
        GetComponent<Animator>().SetBool("isDead2", true);
        yield return new WaitForSeconds(0.5f);
        GetComponent<Animator>().SetBool("isDead2", false);

    }

    public void SetDefaults()
    {
        GetComponent<Animator>().SetBool("isDead", false);
        isDead = false;
        GetComponent<HpComponent>().HealthPoint = GetComponent<HpComponent>().maxHealthPoint;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        Collider _col = GetComponent<CapsuleCollider>();
        if (_col != null)
            _col.enabled = true;
    }


    #endregion

    #region 움직임 위한 함수들
    public MoveController MoveController
    {
        get
        {
            if (m_MoveController == null)
            {
                m_MoveController = GetComponent<MoveController>();
            }
            return m_MoveController;
        }
    }
    InputController playerInput;

    void Awake()
    {
        playerInput = GameManager.Instance.InputController;
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        spawnPoint = GameObject.Find("PlayerSpawnPoint").transform;
    }

    void Update()
    {
        float horizontalRotation = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        verticalRotation -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -35, 0);
        transform.Rotate(0, horizontalRotation, 0);
        child.transform.localEulerAngles = new Vector3(verticalRotation, 0, 0);


        Vector2 direction = new Vector2(playerInput.Vertical * speed, playerInput.Horizontal * speed);
        if (!GetComponent<PlayerAnimation>())
        {
            if (GetComponent<RangedPlayerAnimation>().characterClass == "Archer" && !GetComponent<Animator>().GetBool("isSkill"))
                MoveController.Move(direction);
            else if (GetComponent<RangedPlayerAnimation>().characterClass != "Archer")
                MoveController.Move(direction);
        }
        else
            MoveController.Move(direction);
    }
    #endregion

    [Command]
    public void CmdAttackPowerIncrease(float value)
    {
        RpcAttackPowerIncrease(value);
    }

    [ClientRpc]
    public void RpcAttackPowerIncrease(float value)
    {
        GetComponent<Player>().additive_Attackpower += value;
    }

    [Command]
    public void CmdSpeedIncrease(float value)
    {
        RpcSpeedIncrease(value);
    }

    [ClientRpc]
    public void RpcSpeedIncrease(float value)
    {
        GetComponent<Player>().speed += value;
    }

    [Command]
    public void CmdRespawnEffect(Vector3 pos)
    {
        RpcRespawnEfeect(pos);
    }

    [ClientRpc]
    public void RpcRespawnEfeect(Vector3 pos)
    {
        GameObject respawnEffect = Instantiate(effectPrefeb, pos, transform.rotation, gameObject.transform);
        NetworkServer.Spawn(respawnEffect);
    }
}