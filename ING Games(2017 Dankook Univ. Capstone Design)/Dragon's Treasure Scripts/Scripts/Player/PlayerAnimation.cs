using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAnimation : NetworkBehaviour
{
    // 근접 캐릭터 플레이어의 애니메이션을 담당하는 스크립트.
    public Animator animator;
    public bool isAttack = false;
    public float atkSpeed;
    public AudioSource Audio;
    public AudioClip[] attackAudioClip;
    public GameObject skillObject;
    public float characterMotionAdjust;
    public string characterClass;
    public GameObject weapon;

    void LateUpdate()
    {
        animator.SetFloat("Vertical", GameManager.Instance.InputController.Vertical);
        animator.SetFloat("Horizontal", GameManager.Instance.InputController.Horizontal);
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        if (Input.GetMouseButton(0) && !isAttack) // 근접 기본 공격
        {
            if (characterClass == "Paladin" && animator.GetCurrentAnimatorStateInfo(1).IsName("paladin_skill"))
                yield break;
            CmdIsAttack(true);
            animator.SetBool("isAttack", true);
            Audio.clip = attackAudioClip[0];
            Audio.PlayDelayed(characterMotionAdjust);
            yield return new WaitForSeconds(atkSpeed);
            animator.SetBool("isAttack", false);
            CmdIsAttack(false);
        }
        else if (Input.GetMouseButton(1) && characterClass == "Brutal" && !isAttack && !skillObject.GetComponent<SkillUI>().skill_cooling)
        {   // brutal 스킬
            CmdIsAttack(true);
            animator.SetBool("isSkill", true);
            StartCoroutine(skillObject.GetComponent<SkillUI>().Skill());
            float tempAtkpower = (weapon.GetComponent<AttackOntrigger>().damage + GetComponent<Player>().additive_Attackpower) * 0.6f; // 공격력 증가
            float tempSpeed = GetComponent<Player>().speed * 0.3f; // 이동속도 감소

            CmdAttackUp(gameObject, tempAtkpower);
            GetComponent<Player>().speed -= tempSpeed;
            Audio.clip = attackAudioClip[1];
            Audio.PlayDelayed(characterMotionAdjust);

            yield return new WaitForSeconds(3);

            CmdAttackUp(gameObject, -tempAtkpower);
            GetComponent<Player>().speed += tempSpeed;
            animator.SetBool("isSkill", false);
            CmdIsAttack(false);
        }
        else if (Input.GetMouseButton(1) && characterClass == "Paladin" && !animator.GetCurrentAnimatorStateInfo(1).IsName("paladin_attack")
            && !skillObject.GetComponent<SkillUI>().skill_cooling) // paladin 스킬
        {
            animator.SetBool("isSkill", true);
            StartCoroutine(skillObject.GetComponent<SkillUI>().Skill());
            CmdDefUp(gameObject, 0.6f); // 방어력 증가
            Audio.clip = attackAudioClip[1];
            Audio.PlayDelayed(0);

            yield return new WaitForSeconds(5);
            
            CmdDefUp(gameObject, -0.6f);
            animator.SetBool("isSkill", false);
        }
    }

    [Command]
    public void CmdIsAttack(bool value)
    {
        RpcIsAttack(value);
    }

    [ClientRpc]
    public void RpcIsAttack(bool value)
    {
        this.isAttack = value;
    }

    [Command]
    public void CmdAttackUp(GameObject gameObject, float damage)
    {
        RpcAttackUp(gameObject, damage);
    }

    [ClientRpc]
    public void RpcAttackUp(GameObject gameObject, float damage)
    {
        gameObject.GetComponent<Player>().additive_Attackpower += damage;
    }

    [Command]
    public void CmdDefUp(GameObject gameObject, float value)
    {
        RpcDefUp(gameObject, value);
    }

    [ClientRpc]
    public void RpcDefUp(GameObject gameObject, float value)
    {
        gameObject.GetComponent<Player>().defPower += value;
    }
}