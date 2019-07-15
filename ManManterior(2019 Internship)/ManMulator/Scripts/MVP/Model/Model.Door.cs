using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


partial class Model
{
    /// <summary>
    /// 문 설정, 문 모델 배치 시 참조로 사용되는 클래스
    /// </summary>
    [Serializable]
    public class Door 
    {
        #region Fields
        // 문 설정 시 사용되는 문 아이콘 오브젝트
        GameObject doorObject;
        // 3D 씬에서 실제 배치한 문 모델 오브젝트
        GameObject doorModel;
        Vector3 startPoint, endPoint;
        Wall doorAttachedWall;
        #endregion

        #region Properties
        public GameObject DoorObject { get => doorObject; set => doorObject = value; }
        public Vector3 StartPoint { get => startPoint; set => startPoint = value; }
        public Vector3 EndPoint { get => endPoint; set => endPoint = value; }
        public Wall DoorAttachedWall { get => doorAttachedWall; set => doorAttachedWall = value; }
        public GameObject DoorModel { get => doorModel; set => doorModel = value; }


        #endregion

        #region Methods

        public Door(GameObject _doorObject, Wall _wallAttachedDoor)
        {
            startPoint = _doorObject.transform.GetChild(1).position;
            endPoint = _doorObject.transform.GetChild(2).position;
            doorObject = _doorObject;
            doorAttachedWall = _wallAttachedDoor;
        }

        #endregion
    }
}