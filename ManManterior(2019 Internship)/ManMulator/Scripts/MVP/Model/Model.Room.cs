using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
[System.Serializable]
public class Room
{

    #region Fields

    GameObject selectedRoom;
    List<Transform> rooms = new List<Transform>();
    Dictionary<Transform, float> mergedRooms = new Dictionary<Transform, float>(); // 병합된 바닥 + 면적 정보
    Material disabledRoomMaterial;
    Material selectedRoomMaterial;
    Material defaultRoomMaterial;

    #endregion

    #region Properties

    public List<Transform> Rooms { get => rooms; set => rooms = value; }
    public GameObject SelectedRoom { get => selectedRoom; set => selectedRoom = value; }
    public Material DisabledRoomMaterial { get => disabledRoomMaterial; set => disabledRoomMaterial = value; }
    public Material SelectedRoomMaterial { get => selectedRoomMaterial; set => selectedRoomMaterial = value; }
    public Material DefaultRoomMaterial { get => defaultRoomMaterial; set => defaultRoomMaterial = value; }    
    public Dictionary<Transform, float> MergedRooms { get => mergedRooms; set => mergedRooms = value; }

    #endregion

}