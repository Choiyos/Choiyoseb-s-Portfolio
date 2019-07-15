using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RangedPlayerAnimation : NetworkBehaviour
{
    // 원거리 캐릭터 플레이어의 애니메이션을 담당하는 스크립트.
    Animator animator;
    public AudioSource[] Audio;
    public AudioClip[] AudioClip;
    public bool isAttack = false;
    public GameObject firePrefeb;
    public GameObject granadePrefeb;
    public GameObject magicPrefeb;
    public GameObject dirGameobj;
    public GameObject skillObject;
    public float characterMotionAdjust;
    public string characterClass;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        animator.SetFloat("Vertical", GameManager.Instance.InputController.Vertical);
        animator.SetFloat("Horizontal", GameManager.Instance.InputController.Horizontal);
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        yield return null;
        if (Input.GetMouseButton(0) && !GetComponent<Animator>().GetBool("isAttack") && !GetComponent<Animator>().GetBool("isSkill"))
        {
            animator.SetBool("isAttack", true);
            Audio[0].clip = AudioClip[0];
            Audio[0].PlayDelayed(characterMotionAdjust);
            yield return new WaitForSeconds(GetComponent<Player>().atkSpeed);
            animator.SetBool("isAttack", false);
            CmdFire(dirGameobj.transform.forward);
        }
        else if (Input.GetMouseButton(1) && characterClass == "Archer" && !animator.GetCurrentAnimatorStateInfo(1).IsName("archer_attack")
            && !skillObject.GetComponent<SkillUI>().skill_cooling)
        { // archer 스킬
            animator.SetBool("isSkill", true);
            StartCoroutine(skillObject.GetComponent<SkillUI>().Skill());
            Audio[0].clip = AudioClip[1];
            Audio[0].PlayDelayed(0);
            float elapsTime = 0;
            while (true)
            {
                if (elapsTime >= 0.6f) break;
                elapsTime += Time.deltaTime;
                gameObject.transform.position += 6.5f * gameObject.transform.forward * Time.deltaTime;
                yield return null;
            }
            animator.SetBool("isSkill", false);
        }
        else if (Input.GetMouseButton(1) && characterClass == "Soldier" && !animator.GetBool("isAttack") && !skillObject.GetComponent<SkillUI>().skill_cooling)
        { // soldier 스킬
            animator.SetBool("isSkill", true);
            StartCoroutine(skillObject.GetComponent<SkillUI>().Skill());
            Audio[0].clip = AudioClip[1];
            Audio[0].PlayDelayed(characterMotionAdjust);
            yield return new WaitForSeconds(1.5f);
            CmdGrande(dirGameobj.transform.forward);
            yield return new WaitForSeconds(0.5f);
            animator.SetBool("isSkill", false);
        }
        else if (Input.GetMouseButton(1) && characterClass == "Magician" && !animator.GetCurrentAnimatorStateInfo(1).IsName("magician_attack") 
            && !skillObject.GetComponent<SkillUI>().skill_cooling && !animator.GetCurrentAnimatorStateInfo(1).IsName("magician_skill"))
        { // magician 스킬
            Collider[] Enemies;
            animator.SetBool("isSkill", true);
            StartCoroutine(skillObject.GetComponent<SkillUI>().Skill());
            Audio[1].clip = AudioClip[2];
            Audio[1].PlayDelayed(0);
            yield return new WaitForSeconds(0.5f);
            CmdMagic(dirGameobj.transform.forward);
            Audio[0].clip = AudioClip[1];
            Audio[0].PlayDelayed(0);
            yield return new WaitForSeconds(3.6f);
            Enemies = Physics.OverlapSphere(gameObject.transform.position, 5);
            foreach (Collider enemies in Enemies)
            {
                if (enemies.gameObject.layer == 9 && enemies.tag == "Player")
                {
                    enemies.GetComponent<HpComponent>().OnBurst(gameObject.name, 100);
                }
            }
            yield return new WaitForSeconds(0.8f);
            animator.SetBool("isSkill", false);
        }
    }

    [Command]
    void CmdFire(Vector3 _dir)
    {
        RpcFire(_dir);
    }

    [ClientRpc]
    void RpcFire(Vector3 _dir)
    {
        GameObject missile = Instantiate(firePrefeb, transform.position + _dir * 1.7f + new Vector3(0, 0.5f, 0),
            transform.rotation);
        missile.GetComponent<RangedAttackTrigger>().damage += GetComponent<Player>().additive_Attackpower;
        missile.GetComponent<RangedAttackTrigger>().identify_Player = gameObject.name;
        NetworkServer.Spawn(missile);
    }

    [Command]
    void CmdGrande(Vector3 _dir)
    {
        RpcGranade(_dir);
    }

    [ClientRpc]
    void RpcGranade(Vector3 _dir)
    {
        GameObject missile = Instantiate(granadePrefeb, transform.position + _dir * 1.7f + new Vector3(0, 0.5f, 0),
            transform.rotation);
        missile.GetComponent<RangedAttackTrigger>().identify_Player = gameObject.name;
        NetworkServer.Spawn(missile);
    }

    [Command]
    void CmdMagic(Vector3 _dir)
    {
        RpcMagic(_dir);
    }

    [ClientRpc]
    void RpcMagic(Vector3 _dir)
    {
        GameObject missile = Instantiate(magicPrefeb, transform.position + new Vector3(0, 1.2f, 0), transform.rotation, gameObject.transform);
        NetworkServer.Spawn(missile);
    }
}