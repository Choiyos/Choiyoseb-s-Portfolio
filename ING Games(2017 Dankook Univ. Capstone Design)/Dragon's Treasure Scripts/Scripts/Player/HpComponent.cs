using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class HpComponent : NetworkBehaviour
{
    // 캐릭터의 HP(생명력)속성을 위한 스크립트, 공격을 받았을 때의 처리와, 죽었을 경우에 대한 메세지 처리를 포함.
    [SyncVar]
    public float maxHealthPoint;
    [SyncVar]
    public float HealthPoint;
    public AudioSource damagedAudio;
    public AudioClip[] damagedAudioClip;
    [SerializeField]
    bool _isAttack = false;
    string PlayerID;
    private List<string> idList = new List<string>();
    float receiveDamage;

    void Update()
    {

    }

    [ClientCallback]
    void Start()
    {
        receiveDamage = 0;
    }

    [ClientCallback]
    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Weapon") // 근접 무기 피격 시
        {
            GameObject colPlayer = col.transform.root.gameObject;
            _isAttack = colPlayer.GetComponent<PlayerAnimation>().isAttack;
            PlayerID = colPlayer.GetComponent<NetworkIdentity>().netId.ToString();
            if (_isAttack)
            {
                if (!idList.Contains(PlayerID))
                {
                    StartCoroutine(addList(PlayerID, colPlayer.GetComponent<Player>().atkSpeed));
                    if (!GetComponent<RangedPlayerAnimation>() && GetComponent<PlayerAnimation>().characterClass == "Paladin" && GetComponent<Animator>().GetBool("isSkill"))
                        receiveDamage = col.GetComponent<AttackOntrigger>().damage * GetComponent<Player>().defPower;
                    else if (!GetComponent<PlayerAnimation>() && GetComponent<RangedPlayerAnimation>().characterClass == "Archer" && GetComponent<Animator>().GetBool("isSkill"))
                        receiveDamage = 0;
                    else
                        receiveDamage = col.GetComponent<AttackOntrigger>().damage;

                    if (HealthPoint - receiveDamage > 0)
                    {
                        StartCoroutine(hitAnimation());
                        if (isServer) CmdHpModification(-receiveDamage);
                        damagedAudio.clip = damagedAudioClip[0];
                        damagedAudio.PlayDelayed(0);
                    }
                    else
                    {
                        if (isServer) CmdHpModification(-receiveDamage);
                        CmdKillScoreUp(colPlayer);
                        CmdDeathScoreUp(gameObject);
                        CmdDie();
                    }
                }
            }
        }
        else if (col.tag == "RangedWeapon") // 원거리 무기 피격 시
        {
            if (!GetComponent<RangedPlayerAnimation>() && GetComponent<PlayerAnimation>().characterClass == "Paladin" && GetComponent<Animator>().GetBool("isSkill"))
                receiveDamage = col.GetComponent<RangedAttackTrigger>().damage * GetComponent<Player>().defPower;
            else if (!GetComponent<PlayerAnimation>() && GetComponent<RangedPlayerAnimation>().characterClass == "Archer" && GetComponent<Animator>().GetBool("isSkill"))
                receiveDamage = 0;
            else
                receiveDamage = col.GetComponent<RangedAttackTrigger>().damage;

            GameObject colPlayer = GameObject.Find(col.GetComponent<RangedAttackTrigger>().identify_Player);
            if (HealthPoint - receiveDamage > 0)
            {
                StartCoroutine(hitAnimation());
                if (isServer) CmdHpModification(-receiveDamage);
                damagedAudio.clip = damagedAudioClip[0];
                damagedAudio.PlayDelayed(0);
                Debug.Log(col.GetComponent<RangedAttackTrigger>().identify_Player + "가 " +
                    receiveDamage + "의 피해를 입혀 체력이 " + HealthPoint + "가 됨.");
            }
            else
            {
                if (isServer) CmdHpModification(-receiveDamage);
                CmdKillScoreUp(colPlayer);
                CmdDeathScoreUp(gameObject);
                Debug.Log(this.gameObject.name + " 죽음," + colPlayer.name + "의 killScore :" + colPlayer.GetComponent<Player>().killScore);
                CmdDie();
            }
        }
    }

    [ClientCallback]
    public void OnBurst(string colPlayer, float damage) // 범위 공격 스킬 처리
    {
        GameObject _colPlayer = GameObject.Find(colPlayer);
        if (!GetComponent<RangedPlayerAnimation>() && GetComponent<PlayerAnimation>().characterClass == "Paladin" && GetComponent<Animator>().GetBool("isSkill"))
            receiveDamage = damage * GetComponent<Player>().defPower;
        else if (!GetComponent<PlayerAnimation>() && GetComponent<RangedPlayerAnimation>().characterClass == "Archer" && GetComponent<Animator>().GetBool("isSkill"))
            receiveDamage = 0;
        else
            receiveDamage = damage;

        if (HealthPoint - receiveDamage > 0)
        {
            StartCoroutine(hitAnimation());
            CmdHpModification(-receiveDamage);
            damagedAudio.clip = damagedAudioClip[0];
            damagedAudio.PlayDelayed(0);
            Debug.Log(_colPlayer.name + "가 " + receiveDamage + "의 피해를 입혀 체력이 " + HealthPoint + "가 됨.");
        }
        else
        {
            CmdHpModification(-receiveDamage);
            if (colPlayer != transform.name) // 자살의 경우 killScore 처리 안함.
                CmdKillScoreUp(_colPlayer);
            CmdDeathScoreUp(gameObject);
            Debug.Log(gameObject.name + " 죽음, " + _colPlayer.name + "의 killScore :" +
                _colPlayer.GetComponent<Player>().killScore);
            CmdDie();
        }
    }

    IEnumerator hitAnimation()
    {
        GetComponent<Animator>().SetBool("isHit", true);
        yield return new WaitForSeconds(0.5f);
        GetComponent<Animator>().SetBool("isHit", false);
    }


    [Command]
    public void CmdDie()
    {
        damagedAudio.clip = damagedAudioClip[1];
        damagedAudio.PlayDelayed(0);
        GetComponent<Player>().RpcRespawn();
    }

    IEnumerator addList(string _name, float remainTime)
    {
        idList.Add(PlayerID);
        yield return new WaitForSeconds(remainTime);
        idList.Remove(_name);
        GetComponent<Animator>().SetBool("isHit", false);
    }

    [Command]
    public void CmdHpModification(float value)
    {
        RpcHpModification(value);
    }

    [ClientRpc]
    void RpcHpModification(float value)
    {
        HealthPoint += value;
        HealthPoint = Mathf.Clamp(HealthPoint, 0, GetComponent<HpComponent>().maxHealthPoint);
    }

    [Command]
    void CmdKillScoreUp(GameObject _colPlayer)
    {
        RpcKillScoreUp(_colPlayer);
    }

    [ClientRpc]
    void RpcKillScoreUp(GameObject _colPlayer)
    {
        _colPlayer.GetComponent<Player>().killScore++;
    }

    [Command]
    void CmdDeathScoreUp(GameObject _Player)
    {
        RpcDeathScoreUp(_Player);
    }

    [ClientRpc]
    void RpcDeathScoreUp(GameObject _Player)
    {
        _Player.GetComponent<Player>().deathScore++;
    }
}

