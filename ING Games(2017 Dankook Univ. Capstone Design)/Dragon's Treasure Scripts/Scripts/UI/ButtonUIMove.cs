using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonUIMove : MonoBehaviour
{
    // 메인 로비씬에서의 버튼 클릭시에 애니메이션 처리를 위한 스크립트.
    public bool buttonSwitch;
    public GameObject MainPos;
    public GameObject BackPos;

    public void OnClickEvent()
    {
        if (buttonSwitch == true)
        {
            buttonSwitch = false;
            StartCoroutine(moveBack());
        }
        else
        {
            buttonSwitch = true;
            StartCoroutine(moveActive());
        }
    }

    IEnumerator moveActive()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            transform.position = Vector3.Lerp(transform.position, MainPos.transform.position, 4 * Time.deltaTime);
            if (Vector3.Distance(transform.position, MainPos.transform.position) < 0.01f)
            {
                yield break;
            }
        }
    }

    IEnumerator moveBack()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            transform.position = Vector3.Lerp(transform.position, BackPos.transform.position, 4 * Time.deltaTime);
            if (Vector3.Distance(transform.position, BackPos.transform.position) < 0.01f)
            {
                yield break;
            }
        }
    }
}
