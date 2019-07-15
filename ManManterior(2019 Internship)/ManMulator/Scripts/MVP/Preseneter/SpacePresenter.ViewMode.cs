using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class SpacePresenter : MonoBehaviour
{
    #region ViewMode Buttons
    [SerializeField]
    Button addRoomButton = default;
    [SerializeField]
    Button editRoomButton = default;
    [SerializeField]
    Button removeRoomButton = default;
    [SerializeField]
    Button nextButton = default;
    [SerializeField]
    Button quitButton = default;

    #endregion

    #region Properties



    #endregion

    #region Methods

    private void ViewModeBind()
    {

        addRoomButton.OnClickAsObservable()
            .Subscribe(_ =>
            {

                CancelSelectRoom();
                SwitchMode(SettingState.EditMode);
                // 기존의 공간 재질 모두 disable처리.
                AddRoom();
                // 수치 표시의 변환
            });

        editRoomButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                GameObject selectedRoom = model.Room.SelectedRoom;
                SwitchMode(SettingState.EditMode);
                model.Room.SelectedRoom = selectedRoom;
                EditRoom();
            });

        removeRoomButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                RemoveRoom();
                
            });

        nextButton.OnClickAsObservable()            
            .Subscribe(_ =>
            {
                CancelSelectRoom();
                MakeWallsAndFloors();
            });

        quitButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                model.Room.Rooms.Clear();
                for (int i = 0; i < GameObject.Find("Rooms").transform.childCount; i++)
                {
                    Destroy(GameObject.Find("Rooms").transform.GetChild(i).gameObject);
                }
                SceneManager.LoadScene(0);

                //Application.Quit();
            });

    }

    /// <summary>
    /// 공간설정 종료 후 문/창문 설치를 위한 벽(wallTop)과 병합된 바닥 생성.
    /// </summary>
    void MakeWallsAndFloors()
    {
        //테두리 제거
        Transform borders = GameObject.Find("Borders").transform;
        for (int i = 0; i < borders.childCount; i++)
        {
            Destroy(borders.GetChild(i).gameObject);
        }
        foreach (var room in model.Room.Rooms)
        {
            for (int i = 0; i < room.childCount; i++)
            {
                FloatFloor(room.GetChild(i), 0);
            }          
        }

        float border = 0.1f;
        float height = 2.2f;

        // 공간설정 완료하고 벽 세우기
        Util.MakeWallTops(model.Room.Rooms, border, height);

        //병합된 바닥 생성하기
        Transform mergedFloorParent = new GameObject().transform;
        mergedFloorParent.parent = model.Room.Rooms[0].parent;
        mergedFloorParent.name = "MergedRooms";
        int number = 0;
        foreach (var room in model.Room.Rooms)
        {
            GameObject floor = MakeMesh.MakeRoomFloor(Util.GetChildTransform(room) ,out float area, isDebugMode); 
            model.Room.MergedRooms.Add(floor.transform,area);
            if (floor != null)
            {
                floor.name = "Room" + (number++).ToString();
                floor.transform.parent = mergedFloorParent;
                floor.GetComponent<MeshRenderer>().material = model.Floor.DefaultFloorMaterial;
            }
            room.gameObject.SetActive(false);
        }

        SceneManager.LoadScene("ElementSettingScene");
    }


    /// <summary>
    /// 편집모드 눌렀을 때에 실행될 함수.
    /// </summary>
    private void EditRoom()
    {
        for (int i = 0; i < model.Room.Rooms.Count; i++)
        {
            Transform room = model.Room.Rooms[i];
            if (room == model.Room.SelectedRoom.transform) continue;
            ChangeMaterial(
                room.GetComponentsInChildren<MeshRenderer>()
                , model.Room.DisabledRoomMaterial
            );
        }
    }

    /// <summary>
    /// 모델한테 바닥 프리팹에 대한 정보 요청.
    /// 뷰 모드에서 공간 추가를 눌렀을 때 바닥 생성해주면서 맨 처음만 Room이라는 부모 오브젝트 생성.
    /// Model.Floor에 생성된 Room오브젝트를 넣어두고 Floor추가할 때마다 해당 Transform을 부모로 삼음.
    /// </summary>
    private void AddRoom(bool init=false)
    {
        GameObject newRoom = new GameObject("Room");
        newRoom.transform.parent = GameObject.Find("Rooms").transform;
        model.Room.SelectedRoom = newRoom;
        AddFloor(init);

        // 공간 생성 전 기존 공간들 모두 재질 변경
        foreach (Transform room in model.Room.Rooms)
        {
            ChangeMaterial(
                room.GetComponentsInChildren<MeshRenderer>()
                , model.Room.DisabledRoomMaterial
            );
        }

        // 프리팹을 통해 새로운 바닥과 부모 오브젝트 생성
        model.Room.Rooms.Add(newRoom.transform);        
    }

    private void SelectRoom(GameObject floor)
    {
        CancelSelectRoom();
        if (model.Room.SelectedRoom != null)
        {
            ChangeMaterial(
                model.Room.SelectedRoom.GetComponentsInChildren<MeshRenderer>()
                , model.Room.DefaultRoomMaterial
                );
        }
        ChangeMaterial(
                floor.transform.parent.GetComponentsInChildren<MeshRenderer>()
                , model.Room.SelectedRoomMaterial
            );
        editRoomButton.gameObject.SetActive(true);
        removeRoomButton.gameObject.SetActive(true);
        ShowFloorsLabel(Util.GetChildTransform(floor.transform.parent));

        model.Room.SelectedRoom = floor.transform.parent.gameObject;
        foreach (var f in Util.GetChildTransform(model.Room.SelectedRoom.transform))
        {
            FloatFloor(f, 1);
        }
    }

    private void CancelSelectRoom()
    {
        if (model.Room.SelectedRoom != null)
        {
            ChangeMaterial(
                model.Room.SelectedRoom.GetComponentsInChildren<MeshRenderer>()
                , model.Room.DefaultRoomMaterial
                );
            editRoomButton.gameObject.SetActive(false);
            removeRoomButton.gameObject.SetActive(false);
            ClearAllLabels();
            foreach (var f in Util.GetChildTransform(model.Room.SelectedRoom.transform))
            {
                FloatFloor(f, 0);
            }
        }

        model.Room.SelectedRoom = null;
        model.Floor.SelectedFloor = null;
    }

    private void RemoveRoom()
    {
        if (model.Room.SelectedRoom == null) return;
        List<Transform> AllFloors = GetOtherFloors(model.Room.SelectedRoom.transform);
        // 마지막 1개 방은 삭제 불가.
        //Debug.Log("removeRoom :" + CheckRoomChunk(AllFloors));
        if (model.Room.Rooms.Count > 1 && CheckRoomChunk(AllFloors, true))
        {
            try
            {
                Transform room = model.Room.SelectedRoom.transform;
                foreach (var floor in Util.GetChildTransform(room))
                {
                    DeleteBorder(floor.gameObject);
                }
                model.Room.Rooms.Remove(room);
                CancelSelectRoom();
                Destroy(room.gameObject);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                //throw;
            }
        }
        else
        {
            ChangeMaterial(
           model.Room.SelectedRoom.GetComponentsInChildren<MeshRenderer>(),
           model.Floor.WarningFloorMaterial);
        }
    }

    #endregion

}
