using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneConverterUI : MonoBehaviour
{
    // 씬을 변경해주는 애니메이션 작업을 하는 스크립트.
    float elapsTime;

    public void fadeOut()
    {
        StartCoroutine(fadeOut_());
    }

    IEnumerator fadeOut_()
    {
        elapsTime = 0;
        while (true)
        {
            elapsTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
            GetComponentInChildren<Image>().color = new Color(0, 0, 0, elapsTime);

            if (GetComponentInChildren<Image>().color.a >= 0.999f)
                break;
        }
        yield return new WaitForSeconds(1);
        
        while (true)
        {
            elapsTime -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
            GetComponentInChildren<Image>().color = new Color(0, 0, 0, elapsTime);

            if (GetComponentInChildren<Image>().color.a <= 0.85f)
                break;
        }
    }
}
