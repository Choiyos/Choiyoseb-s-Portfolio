using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragonImageUI : MonoBehaviour
{
    // 메인 로비씬 용 오브젝트의 애니메이션 기능을 하는 스크립트.
    public float movSensitive;
    float velocity;
    float elapsTime_Image;
    float elapsTime_alpha;
    RectTransform transform_;

    void Awake()
    {
        transform_ = GetComponent<RectTransform>();
    }

    void Update()
    {
        elapsTime_Image += Time.deltaTime;
        transformMove();
    }

    void transformMove()
    {
        velocity = movSensitive * Mathf.Sin(3 * elapsTime_Image);
        transform_.Translate(0, velocity, 0);
    }

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
