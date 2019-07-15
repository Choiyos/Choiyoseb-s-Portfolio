using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingText : MonoBehaviour
{
    Text text_;

    void Awake()
    {
        text_ = GetComponent<Text>();
        text_.DOColor(new Color(1, 1, 1, 0), 1).SetLoops(int.MaxValue, LoopType.Yoyo).SetEase(Ease.InOutQuad);
    }
}
