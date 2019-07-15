using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpeechBubbleAnimation : MonoBehaviour, IObjectPoolable
{
    [SerializeField]
    GameObject target; // 말풍선을 띄울 대상
    string text; // 말풍선 텍스트
    float time; // 말풍선 지속시간
    Transform headupPosition;

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnEnable()
    {
        StartCoroutine(FollowTarget());
        StartCoroutine(Dispose());
    }

    public void PoolObjectInit(params object[] list)
    {
        target = (GameObject)list[0]; // 말풍선의 타겟 오브젝트 설정
        text = (string)list[1]; // 말풍선의 텍스트 설정
        time = (float)list[2];
        gameObject.SetActive(true);

        headupPosition = target.transform.Find("HeadUp");
        transform.position = headupPosition.position;
        transform.GetChild(0).GetComponent<Text>().text = this.text;
    }

    IEnumerator FollowTarget()
    {
        transform.DOScale(new Vector2(10, 10), 0.3f).SetEase(Ease.InOutQuad);
        transform.DOScale(new Vector2(10.5f, 10.5f), 0.3f).SetEase(Ease.InOutQuad).SetLoops(int.MaxValue, LoopType.Yoyo).SetDelay(time - 1);

        while (true)
        {
            yield return null;
            transform.position = headupPosition.position;
        }
    }

    public IEnumerator Dispose() // 풀오브젝트 메모리 해제
    {
        yield return new WaitForSeconds(time);
        transform.DOScale(new Vector2(0, 0), 0.3f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(0.3f);
        transform.parent.GetComponent<ObjectPool>().ObjectPoolPush(gameObject);
        transform.DOKill();
    }
}
