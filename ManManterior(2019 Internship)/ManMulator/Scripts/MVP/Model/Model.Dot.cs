using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공간 설정시 사용되는 점 정보 클래스.
/// 방향과 속성(교점or 꼭지점) 등의 정보를 담고 있어서 Outliner에서 외곽 점을 수집할 때 사용됨.
/// </summary>
public class Dot
{
    public Direction diretion;
    public string attribute;
    public Vector3 position;
    public float x, y;
    public Floor parentFloor;

    public Dot(Vector3 _position, Direction _direction)
    {
        position = _position;
        x = position.x;
        y = position.z;
        diretion = _direction;
        attribute = "Cross";
    }
    public Dot(Direction _direction, Vector3 _position, Floor _floor)
    {
        diretion = _direction;
        position = _position;
        x = position.x;
        y = position.z;
        parentFloor = _floor;
        attribute = "Corner";
    }
}
