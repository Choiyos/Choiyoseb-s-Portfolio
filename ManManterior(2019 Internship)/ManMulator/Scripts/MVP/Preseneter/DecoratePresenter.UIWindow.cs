using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx.Triggers;
using UniRx;

/// <summary>
/// Decorate 씬에서 사용되는 가구/바닥재/벽지 윈도우를 toggle로 관리하기 위해 만든 클래스.
/// </summary>
public class UIWindow
{
    public GameObject view;
    public Button toggleButton;
    GameObject toogleMode;
    // 현재 윈도우의 활성화 상태
    bool state = false;

    public bool State { get => state; set => state = value; }

    /// <summary>
    /// 토글 윈도우로 등록.
    /// </summary>
    /// <param name="_view">지정할 윈도우</param>
    /// <param name="_button">윈도우 토글 버튼</param>
    public UIWindow(GameObject _view, Button _button)
    {
        view = _view;
        toggleButton = _button;
    }
    /// <summary>
    /// 현재 윈도우를 토글할 때 해당하는 버튼의 색을 바꾸고 
    /// 윈도우의 위치를 화면 밖 / 화면 안으로 이동.
    /// 매번 목록을 로딩하지 않기 위해 위치이동으로 토글.
    /// </summary>
    /// <param name="on">현재 윈도우에 적용할 토글상태</param>
    public void Toggle(bool on)
    {
        toggleButton.GetComponent<Image>().color = on ? Color.yellow : Color.white;
        view.GetComponent<RectTransform>().localPosition = new Vector3(on ? -435 : -2000, -40, 0);
        State = on;
    }
    /// <summary>
    /// 윈도우 창 활성화 토글 함수.
    /// </summary>
    public void Toggle()
    {
        Toggle(!State);
    }

}