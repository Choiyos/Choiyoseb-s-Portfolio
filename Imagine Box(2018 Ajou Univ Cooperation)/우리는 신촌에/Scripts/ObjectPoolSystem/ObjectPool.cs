using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectPoolable
{
    void PoolObjectInit(params object[] list); // 풀오브젝트 초기화
    IEnumerator Dispose(); // 풀오브젝트 메모리 해제
}

public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    Queue<GameObject> objectPool;

    //#region 풀 오브젝트 요소            ---> 우선 설계를 풀오브젝트를 미리 넉넉히 준비해놓는 방식으로 구현.
    //GameObject exclamationMark;              그 이유는 중요성을 생각해봤을때 (런타임상에서 부하) > (로딩에서의 부하)
    //GameObject marker;                       라고 판단했기 때문. 그래서 충분히 개발기획이 이루어져 적정수준으로 오브젝트풀의
    //GameObject SpeechBubble;                 오브젝트 개수를 유지하도록 해야할 것. 추후에 예외처리로 풀오브젝트 요소를
    //#endregion                               가지고 있을 것인지에 대한 것은 논의가 필요.

    public void Awake()
    {
        objectPool = new Queue<GameObject>();
        //#region 풀 오브젝트 요소 초기화
        //exclamationMark = Resources.Load<GameObject>("Prefabs/PoolObject/ExclamationMark");
        //marker = Resources.Load<GameObject>("Prefabs/PoolObject/Mark");
        //SpeechBubble = Resources.Load<GameObject>("Prefabs/PoolObject/SpeechBubble");
        //#endregion

        for (int i = 0; i < transform.childCount; i++)
        {
            objectPool.Enqueue(transform.GetChild(i).gameObject);
        }
    }

    public void ObjectPoolPush(GameObject obj)
    {
        obj.SetActive(false);
        objectPool.Enqueue(obj);
    }

    public GameObject ObjectPoolPop(params object[] list)
    {
        if (objectPool.Count != 0)
        {
            GameObject obj;
            obj = objectPool.Dequeue();
            obj.GetComponent<IObjectPoolable>().PoolObjectInit(list); // 풀 오브젝트 초기화
            return obj;
        }
        else
        {
            Debug.Log("There's no Object in ObjectPool!"); // 예외 처리 필요
            return null;
        }
    }
}
