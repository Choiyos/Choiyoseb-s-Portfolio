using System;
using UnityEngine;

partial class Model
{
    /// <summary>
    /// 창문 설정, 창문 모델 배치 시 참조로 사용되는 클래스
    /// </summary>
    [Serializable]
    public class Window
    {

        #region Fields
        GameObject windowObject;

        Vector3 startPoint, endPoint;
        Wall windowAttachedWall;
        #endregion

        #region Properties
        public GameObject WindowObject { get => windowObject; set => windowObject = value; }
        public Wall WindowAttachedWall { get => windowAttachedWall; set => windowAttachedWall = value; }


        #endregion

        #region Methods

        public Window(GameObject _windowObject, Wall _wallAttachedWindow)
        {
            startPoint = _windowObject.transform.GetChild(1).position;
            endPoint = _windowObject.transform.GetChild(2).position;
            windowObject = _windowObject;
            windowAttachedWall = _wallAttachedWindow;
        }

        #endregion
    }

}
