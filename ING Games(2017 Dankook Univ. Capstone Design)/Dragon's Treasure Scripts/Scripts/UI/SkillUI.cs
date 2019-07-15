using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    // 인게임에서 스킬 사용대기 시간을 표시하는 기능의 스크립트.
    public float cooltime;
    public Text cooltimeText;
    public bool skill_cooling = false;

    public IEnumerator Skill()
    {
        GetComponent<Image>().fillAmount = 1;
        cooltimeText.gameObject.SetActive(true);
        while (true)
        {
            if (GetComponent<Image>().fillAmount == 0)
            {
                skill_cooling = false;
                cooltimeText.gameObject.SetActive(false);
                yield break;
            }
            yield return null;
            skill_cooling = true;
            GetComponent<Image>().fillAmount -= Time.deltaTime/cooltime;
            cooltimeText.text = (GetComponent<Image>().fillAmount * cooltime).ToString("N0");
        }
    }
}
