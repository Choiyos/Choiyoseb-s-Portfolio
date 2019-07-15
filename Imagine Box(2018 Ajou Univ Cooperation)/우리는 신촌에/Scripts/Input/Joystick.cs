using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


#region testing3 - 실험용 enum
public enum CardinalDirections
{
    N,
    NE,
    E,
    SE,
    S,
    SW,
    W,
    NW
}

#endregion


public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{

     Image bgImg; //조이스틱의 백그라운드 이미지
     Image joystickImg; //잡고 움직이는 조이스틱 이미지
    private Vector3 inputVector; //

    void Start()
    {
        bgImg = GetComponent<Image>();
        joystickImg = transform.GetChild(0).GetComponent<Image>();
    }


    /// <summary>
    ///사용자가 터치중일 때 계속해서 실행되는 함수 
    /// </summary>
    /// <param name="ped">터치의 좌표 등에 대한 정보가 담겨있음.</param>
    public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform
                                                                    , ped.position
                                                                    , ped.pressEventCamera
                                                                    , out pos))
        {
            pos.x = (pos.x / bgImg.rectTransform.sizeDelta.x);
            pos.y = (pos.y / bgImg.rectTransform.sizeDelta.y);
            
            inputVector = new Vector3(pos.x * 2, pos.y * 2, 0);
            
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;
            //Move Joystick IMG
            joystickImg.rectTransform.anchoredPosition =
                new Vector3(inputVector.x * (bgImg.rectTransform.sizeDelta.x / 2f)
                , inputVector.y * (bgImg.rectTransform.sizeDelta.y / 2f));
        }
    }

    //사용자가 터치를 하자마자 실행되는 함수.
    public virtual void OnPointerDown(PointerEventData ped)
    {
        OnDrag(ped);
    }

    //사용자가 터치를 끝냈을때 실행되는 함수.
    public virtual void OnPointerUp(PointerEventData ped)
    {
        inputVector = Vector3.zero;
        joystickImg.rectTransform.anchoredPosition = Vector3.zero;
    }

    //PlayerController 스크립트에서 inputVector.x 값을 받기 위해 사용될 함수
    public float GetHorizontalValue()
    {
        return inputVector.x;
    }

    //PlayerController 스크립트에서 inputVector.y 값을 받기 위해 사용될 함수
    public float GetVerticalValue()
    {
        return inputVector.y;
    }

    public float GetMoveSpeed()
    {
        return 0;
    }
}
