using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideUI : MonoBehaviour {
    // 씬의 변경마다 페이드 아웃 효과를 위한 스크립트.
    float elapsTime_alpha;

    public void fadeOut()
    {
        StartCoroutine(fadeOut_());
    }

    IEnumerator fadeOut_()
    {
        elapsTime_alpha = 1;
        while (true)
        {
            elapsTime_alpha -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
            GetComponentInChildren<Image>().color = new Color(1, 1, 1, elapsTime_alpha);

            if (GetComponentInChildren<Image>().color.a >= 0.999f)
                break;
        }
        this.gameObject.SetActive(false);
    }
}
