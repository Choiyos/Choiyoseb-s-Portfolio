using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ExclamationAnimation : MonoBehaviour, IObjectPoolable
{
    GameObject target;
    Transform headUpPosition;

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnEnable()
    {
        StartCoroutine(Animation());
    }

    public void PoolObjectInit(params object[] list)
    {
        target = (GameObject)list[0]; // 느낌표의 타겟 오브젝트 설정
        headUpPosition = target.transform.GetChild(1);
        transform.position = headUpPosition.position;
        gameObject.SetActive(true);
    }

    IEnumerator Animation()
    {
        transform.DOScale(new Vector2(1.3f, 1.3f), 0.3f).SetEase(Ease.InOutQuad);
        transform.DOScale(new Vector2(1.5f, 1.5f), 0.3f).SetEase(Ease.InOutQuad).SetLoops(int.MaxValue, LoopType.Yoyo).SetDelay(0.3f);

        while (true)
        {
            yield return null;
            transform.position = headUpPosition.position;
        }
    }

    public IEnumerator Dispose()
    {
        transform.DOScale(new Vector2(0, 0), 0.3f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(0.3f);
        transform.parent.GetComponent<ObjectPool>().ObjectPoolPush(gameObject);
        StopAllCoroutines();
    }
}
