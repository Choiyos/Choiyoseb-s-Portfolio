using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InteractionButtonAnimation : MonoBehaviour
{
    void OnEnable()
    {
        transform.DOScale(new Vector3(1.1f, 1.1f, 1), 0.5f).SetEase(Ease.InOutQuad).SetLoops(int.MaxValue, LoopType.Yoyo);
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = new Vector3(1, 1, 1);
    }
}
