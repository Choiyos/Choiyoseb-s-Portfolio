using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneUI : MonoBehaviour
{
    // 메인 로비씬에서 누르는 버튼의 분기에 따라 메세지를 던지는 기능의 스크립트.
    int clickLayer = 12;
    public GameObject LogInWindow;
    public GameObject AccountCreateWindow;
    public GameObject ETMLogo;
    public GameObject DragonImage;
    public GameObject[] TreasureImage;
    public GameObject SceneConverter;

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                int l = hitInfo.transform.gameObject.layer;
                if (l == clickLayer)
                {
                    Debug.Log(" hit object : " + hitInfo.collider.name);
                    switch (hitInfo.collider.name)
                    {
                        case "SignIn":
                            LogInWindow.GetComponent<ButtonUIMove>().OnClickEvent();
                            AccountCreateWindow.GetComponent<ButtonUIMove>().OnClickEvent();
                            break;
                        case "LogIn":
                            LogInWindow.GetComponent<ButtonUIMove>().OnClickEvent();
                            ETMLogo.GetComponent<ButtonUIMove>().OnClickEvent();
                            SceneConverter.GetComponent<SceneConverterUI>().fadeOut();
                            DragonImage.GetComponent<DragonImageUI>().fadeOut();
                            foreach (var TreasureImage_ in TreasureImage)
                            {
                                TreasureImage_.GetComponent<HideUI>().fadeOut();
                            }
                            break;
                        case "Create":
                            LogInWindow.GetComponent<ButtonUIMove>().OnClickEvent();
                            AccountCreateWindow.GetComponent<ButtonUIMove>().OnClickEvent();
                            break;
                        case "Back":
                            LogInWindow.GetComponent<ButtonUIMove>().OnClickEvent();
                            AccountCreateWindow.GetComponent<ButtonUIMove>().OnClickEvent();
                            break;
                        case "Quit":
                            Application.Quit();
                            break;
                    }
                }
            }
        }
    }
}
