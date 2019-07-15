using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Floor
{
    #region Fields

    private GameObject floorPrefab;
    private GameObject linePrefab;
    private GameObject selectedFloor;

    private Material selectedFloorMaterial;
    private Material disabledFloorMaterial;
    private Material defaultFloorMaterial;
    private Material warningFloorMaterial;

    public float x, y, w, h;
    public Dot leftUp, leftDown, rightUp, rightDown;

    #endregion

    #region Properties

    public GameObject SelectedFloor { get => selectedFloor; set => selectedFloor = value; }
    public GameObject FloorPrefab { get => floorPrefab; set => floorPrefab = value; }
    public GameObject LinePrefab { get => linePrefab; set => linePrefab = value; }
    public Material SelectedFloorMaterial { get => selectedFloorMaterial; set => selectedFloorMaterial = value; }
    public Material DisabledFloorMaterial { get => disabledFloorMaterial; set => disabledFloorMaterial = value; }
    public Material DefaultFloorMaterial { get => defaultFloorMaterial; set => defaultFloorMaterial = value; }
    public Material WarningFloorMaterial { get => warningFloorMaterial; set => warningFloorMaterial = value; }


    #endregion

    #region Methods
    public Floor(float _x, float _y, float _w, float _h)
    {
        x = _x;
        y = _y;
        w = _w;
        h = _h;

        leftUp = new Dot(Direction.Right, new Vector3(x, 0f, y + h), this);
        leftDown = new Dot(Direction.Up, new Vector3(x, 0f, y), this);
        rightUp = new Dot(Direction.Down, new Vector3(x + w, 0f, y + h), this);
        rightDown = new Dot(Direction.Left, new Vector3(x + w, 0f, y), this);
    }
    public Floor() { }

    #endregion








}

