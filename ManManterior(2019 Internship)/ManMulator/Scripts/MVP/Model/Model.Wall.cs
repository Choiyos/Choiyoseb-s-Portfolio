using UnityEngine;

partial class Model
{
    [System.Serializable]
    public class Wall
    {
        #region Fields
        public enum WallDirection { Vertical, Landscape }
        GameObject wallObject;
        WallDirection direction;
        Vector3 startPoint, endPoint;
       
        #endregion

        #region Properties

        public GameObject WallObject { get => wallObject; }
        public WallDirection Direction { get => direction; set => direction = value; }
        public Vector3 StartPoint { get => startPoint; }
        public Vector3 EndPoint { get => endPoint;  }


        #endregion

        #region Methods

        public Wall(GameObject _wallObject, WallDirection _direction)
        {
            wallObject = _wallObject;
            direction = _direction;
            if (_direction == WallDirection.Vertical)
            {
                startPoint = _wallObject.transform.TransformPoint(new Vector3(0, -0.5f, 0));
                endPoint = _wallObject.transform.TransformPoint(new Vector3(0, 0.5f, 0));
            }
            else
            {
                startPoint = _wallObject.transform.TransformPoint(new Vector3(-0.5f, 0, 0));
                endPoint = _wallObject.transform.TransformPoint(new Vector3(0.5f,0, 0));

            }
        }

        #endregion

    }
}