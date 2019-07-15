using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SystemMessageAnimation : MonoBehaviour, IObjectPoolable
{
    Text message;
    float time;

    public IEnumerator Dispose()
    {
        yield return null;
        transform.parent.GetComponent<ObjectPool>().ObjectPoolPush(gameObject);
    }

    void OnEnable()
    {
        StartCoroutine(OnEnable_());
    }

    public void PoolObjectInit(params object[] list)
    {
        message = GetComponent<Text>();
        message.text = (string)list[0];
        time = (float)list[1];
        gameObject.SetActive(true);
    }

    public void PushSystemMessage() // 메세지를 Push하는 메소드
    {
        message.DOFade(1, 0.4f);
    }

    IEnumerator OnEnable_()
    {
        yield return StartCoroutine(transform.parent.GetComponent<SystemMessageQueue>().EnqueueMessage(gameObject));
    }

    public IEnumerator PopSystemMessage() // 메세지를 Pop하는 코루틴
    {
        yield return new WaitForSeconds(time);
        message.DOFade(0, 0.4f);
        yield return new WaitForSeconds(0.4f);
        StartCoroutine(Dispose());
    }
}
