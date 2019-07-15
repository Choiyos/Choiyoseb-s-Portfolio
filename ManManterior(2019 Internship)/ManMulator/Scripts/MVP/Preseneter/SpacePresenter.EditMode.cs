using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class SpacePresenter : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private const float snapLength = 0.3f;
    #region EditMode Buttons
    [SerializeField]
    Button addFloorButton = default;
    [SerializeField]
    Button removeFloorButton = default;
    [SerializeField]
    Button completeButton = default;
    [SerializeField]
    Button cancelButton = default;

    #endregion
    GameObject editingLabel;

    bool isMoving = false;

    #endregion
    #region Methods

    private void EditModeBind()
    {
        addFloorButton.OnClickAsObservable()
              .Subscribe(_ =>
              {
                  AddFloor();
              });

        removeFloorButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                RemoveFloor();
            });

        cancelButton.OnClickAsObservable()
            .Subscribe(_ =>
            {

            });

        completeButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (CheckRoom())
                {
                    SwitchMode(SettingState.ViewMode);
                }
            });
        snapToggle.isOn = true;

    }

    #region Floor

    private void RemoveFloor()
    {
        if (model.Floor.SelectedFloor == null) return;
        //1. 마지막 방의 마지막 바닥은 삭제 불가.
        //2 .삭제해도 섬공간 안생겨야함.
        //3. 삭제했을때 다른방과 연결 끊기면 안됨.
        if (!(model.Room.Rooms.Count == 1 && model.Room.SelectedRoom.transform.childCount == 1)
            && CheckDeleteFloor(model.Floor.SelectedFloor.transform))
        {
            // 현재 방의 마지막 바닥이면 방을 삭제
            if (model.Room.SelectedRoom.transform.childCount == 1)
            {
                RemoveRoom();
                SwitchMode(SettingState.ViewMode);

            }
            else
            {
                Transform floor = model.Floor.SelectedFloor.transform;
                DeleteBorder(floor.gameObject);
                floor.parent = null;
                Destroy(floor.gameObject);
                CancelSelectFloor();
            }

        }
        else
        {
            ChangeMaterial(
           model.Room.SelectedRoom.GetComponentsInChildren<MeshRenderer>(),
           model.Floor.WarningFloorMaterial);
            return;
        }
    }

    /// <summary>
    /// 편집모드에서 바닥 추가를 눌렀을 때 실행되는 메소드.
    /// </summary>
    private void AddFloor(bool init = false)
    {
        if (isMoving) return;
        if (FindDouble()) return;

        GameObject newFloor = Instantiate(
                model.Floor.FloorPrefab
                , model.Room.SelectedRoom.transform);

        BindFloor(newFloor);
        AttachBordersFloor(newFloor);
        SelectFloor(newFloor);

        if (init) return;
        newFloor.SetActive(false);
        borders[newFloor].gameObject.SetActive(false);

        RefreshDots(newFloor);
        ClearAllLabels();
        isMoving = true;

        // 바닥 생성시 클릭하여 위치를 적용하기 전까지 마우스를 따라다님
        var followMouse = new SingleAssignmentDisposable();
        followMouse.Disposable = this.UpdateAsObservable()
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Select(move => Camera.main.ScreenToWorldPoint(Input.mousePosition))
            .Subscribe(move =>
            {

                if (model.Floor.SelectedFloor == null || newFloor == null)
                {
                    isMoving = false;
                    followMouse.Dispose();
                    return;
                }

                if (!newFloor.activeSelf)
                {
                    newFloor.SetActive(true);
                    borders[newFloor].gameObject.SetActive(true);
                }

                Util.SetFloorPosition(newFloor, new Vector3(move.x, newFloor.transform.position.y, move.z) - deltaPos, gridSize);
                if (snapToggle.isOn) SnapFloor(newFloor);
                UpdateBorder(newFloor);

                if (Input.GetButtonDown("Fire1"))
                {
                    isMoving = false;
                    CheckFloor(newFloor);
                    followMouse.Dispose();
                }
            });
    }

    private void SelectFloor(GameObject floor)
    {
        if (model.Floor.SelectedFloor == null || model.Floor.SelectedFloor != floor) CancelSelectFloor();
        if (model.Floor.SelectedFloor != null)
        {
            ChangeMaterial(
            new MeshRenderer[] { model.Floor.SelectedFloor.GetComponent<MeshRenderer>() }
            , model.Floor.DefaultFloorMaterial
            );
        }
        model.Floor.SelectedFloor = floor;
        ChangeMaterial(
            new MeshRenderer[] { floor.GetComponent<MeshRenderer>() }
            , model.Floor.SelectedFloorMaterial
            );
        removeFloorButton.interactable = true;
        ClearAllLabels();
        ShowFloorLabel(floor.transform);
        FloatFloor(floor.transform, 2);
    }

    /// <summary>
    /// 입력된 수치를 바닥에 반영하여 크기 변경.
    /// 수치 변경 시 바닥의 좌하단을 기준점으로 고정하고 오른쪽과 위쪽 방향으로 길이가 늘어나는 방향으로 적용.
    /// </summary>
    /// <param name="length"></param>
    /// <param name="axis"></param>
    private void UpdateFloor(float length, int axis)
    {
        if (model.Floor.SelectedFloor == null) return;
        if (length < 0.05f) return;

        GameObject floor = model.Floor.SelectedFloor;
        Vector3 scale = floor.transform.localScale;
        Vector3 originScale = scale;
        scale[axis] = Util.GetGridValue(length);
        floor.transform.localScale = scale;
        floor.transform.position = floor.transform.position + (scale - originScale) / 2;
        ClearAllLabels();
        ShowFloorLabel(floor.transform);
        CheckFloor(floor);
        UpdateBorder(floor);
    }

    private void CancelSelectFloor()
    {
        if (editingLabel != null) editingLabel = null;
        GameObject floor = model.Floor.SelectedFloor;
        ChangeMaterial(
            model.Room.SelectedRoom.GetComponentsInChildren<MeshRenderer>()
            , model.Floor.DefaultFloorMaterial
            );
        //removeFloorButton.gameObject.SetActive(false);
        removeFloorButton.interactable = false;
        ClearAllLabels();
        ShowAllLabels();
        if (!(floor == null || floor.transform.parent == null))
        {
            FloatFloor(floor.transform, 1);
        }
        model.Floor.SelectedFloor = null;
    }

    #endregion

    #region 바닥검사

    private bool CheckFloor(GameObject floor)
    {
        if (floor == null) return false;
        // 바닥이 방에서 따로 떨어지지 않고 && 다른 방의 바닥과 겹치지 않음
        if (CheckRoomChunk(Util.GetChildTransform(floor.transform.parent))
            && CheckRoomRoomIntersect(new List<Transform> { floor.transform }, GetOtherFloors(floor.transform.parent)))
        {
            ChangeMaterial(
                new MeshRenderer[] { floor.transform.GetComponent<MeshRenderer>() },
                model.Floor.SelectedFloorMaterial);

            return true;
        }
        else
        {
            ChangeMaterial(
                new MeshRenderer[] { floor.transform.GetComponent<MeshRenderer>() },
                model.Floor.WarningFloorMaterial);
            return false;
        }
    }

    // 지금 이 방바닥을 지워도 1.섬공간은 안생기는가 2.다른방과 연결이 끊어지지않는가
    // true : 조건 통과
    bool CheckDeleteFloor(Transform curFloor)
    {
        Transform curRoom = model.Room.SelectedRoom.transform;
        List<Transform> floors = Util.GetChildTransform(curRoom);
        floors.Remove(curFloor);

        List<Transform> allFloors = Util.GetAllFloors(model.Room.Rooms);
        allFloors.Remove(curFloor);
        return CheckRoomChunk(floors) && CheckRoomChunk(allFloors, true);
    }

    /// <summary>
    /// 현재 방이 한 덩어리로 되어 있는지 검사
    /// 바닥이 겹치는 여부를 검사하여 각 바닥의 연결관계를 리스트로 저장하고,
    /// 그 리스트를 탐색하여 모든 바닥이 한 덩어리로 이루어져있는지 검사.
    /// </summary>
    /// <param name="curFloors">현재 방을 구성하는 바닥 리스트</param>
    /// <param name="edge">모서리 맞닿음 검사</param>
    /// <returns>true - 방이 한 덩어리로 되어있음(섬공간x) or false</returns>
    bool CheckRoomChunk(List<Transform> curFloors, bool edge = false)
    {
        if (curFloors.Count < 2) return true;
        List<Queue<int>> connectedFloors = new List<Queue<int>>();        
        for (int i = 0; i < curFloors.Count; i++)
        {
            Queue<int> cFloors = new Queue<int>();
            int countIntersect = 0;
            Floor floor1 = Util.ConvertFloorToFloor(curFloors[i]);
            for (int j = 0; j < curFloors.Count; j++)
            {
                if (i == j) continue;
                Floor floor2 = Util.ConvertFloorToFloor(curFloors[j]);
                if (Util.GetIntersect(floor1, floor2, edge))
                {
                    countIntersect++;
                    cFloors.Enqueue(j);
                }
            }
            connectedFloors.Add(cFloors);
            if (countIntersect == 0) return false;
        }
        if (connectedFloors.Count == 0) return false;
        return Util.FindGroupNumber(connectedFloors);
    }
    #endregion

    #region 방검사

    private bool CheckRoom()
    {
        if (model.Room.SelectedRoom == null) return false;
        if (CheckRoomChunk(Util.GetChildTransform(model.Room.SelectedRoom.transform))
            && CheckRoomChunk(Util.GetAllFloors(model.Room.Rooms), true)
            && CheckRoomRoomIntersect(Util.GetChildTransform(model.Room.SelectedRoom.transform), GetOtherFloors(model.Room.SelectedRoom.transform))
            && !FindDouble())
        {
            CancelSelectFloor();
            return true;
        }
        else
        {
            ChangeMaterial(
                model.Room.SelectedRoom.transform.GetComponentsInChildren<MeshRenderer>(),
                model.Floor.WarningFloorMaterial);
            return false;
        }
    }

    // 다른 방의 바닥을 수집하여 다른방과 현재 방 내의 바닥간의 조건검사를 위해 사용
    private List<Transform> GetOtherFloors(Transform curRoom)
    {
        List<Transform> otherFloors = new List<Transform>();
        foreach (var room in model.Room.Rooms)
        {
            if (room == curRoom) continue;
            otherFloors.AddRange(Util.GetChildTransform(room));
        }

        return otherFloors;
    }

    // 해당 방의 바닥이 다른 방에 겹치지 않은지 검사O(n^2)
    // true : 통과(겹쳐지지 않음)
    bool CheckRoomRoomIntersect(List<Transform> curFloors, List<Transform> otherFloors)
    {
        if (model.Room.Rooms.Count == 1 || otherFloors == null || otherFloors.Count() == 0) return true;

        foreach (var curFloor in curFloors)
        {
            int countIntersect = 0;
            foreach (var floor in otherFloors)
            {
                if (Util.GetIntersect(Util.ConvertFloorToFloor(floor), Util.ConvertFloorToFloor(curFloor)))
                {
                    countIntersect++;
                }
            }
            if (countIntersect > 0) return false;
        }
        return true;
    }

    #endregion

    #region Snap

    /// <summary>
    /// 매개변수 좌표 사이의 거리가 조건에 부합하는 지 검사하는 함수.
    /// </summary>
    /// <param name="a">대상1 좌표의 한 축(x||y||z)</param>
    /// <param name="b">대상2 좌표의 한 축(x||y||z)</param>
    /// <param name="length">두 좌표 사이의 조건 거리</param>
    /// <returns></returns>
    bool IsClose(float a, float b, float length)
    {
        return (Vector3.Distance(new Vector3(a, 0, 0), new Vector3(b, 0, 0)) < length);
    }


    /// <summary>
    /// 바닥이 드래그 중일 때, 클릭시 저장했던 방향별 스냅할 수 있는 점들을 반복하여 순회함.
    /// 모든 점마다 현재 바닥의 네 꼭짓점과 거리 등의 조건을 검사하고, 조건에 부합하면
    /// 자석효과처럼 드래그중인 바닥을 대상 바닥의 변에 밀착시킴.
    /// </summary>
    /// <param name="floor"></param>
    void SnapFloor(GameObject floor)
    {
        Floor tempfloor = Util.ConvertFloorToFloor(floor.transform);

        Vector3 originPosition = floor.transform.position;

        // 오른쪽 변 스냅검사.
        for (int i = 0; i < xRightDots.Count; i++)
        {

            if (IsClose(xRightDots[i].x, tempfloor.leftUp.x, snapLength))
            {
                if (IsIncludeDot(tempfloor, xRightDots[i], Direction.Down)
                    || (xRightDots[i].diretion == Direction.Down && NotIncludeDot(tempfloor, xRightDots, xRightDots[i], Direction.Down)))
                {
                    floor.transform.position = SnappedPosiiton(floor, xRightDots[i], Direction.Down);
                }
            }
        }
        // 왼쪽 변 스냅검사.
        for (int i = 0; i < xLeftDots.Count; i++)
        {
            if (IsClose(xLeftDots[i].x, tempfloor.rightUp.x, snapLength))
            {
                if (IsIncludeDot(tempfloor, xLeftDots[i], Direction.Up)
                    || (xLeftDots[i].diretion == Direction.Up && NotIncludeDot(tempfloor, xLeftDots, xLeftDots[i], Direction.Up)))
                {
                    floor.transform.position = SnappedPosiiton(floor, xLeftDots[i], Direction.Up);
                }
            }
        }
        // 위쪽 변 스냅검사.
        for (int i = 0; i < yUpDots.Count; i++)
        {
            if (IsClose(yUpDots[i].y, tempfloor.rightDown.y, snapLength))
            {
                if (IsIncludeDot(tempfloor, yUpDots[i], Direction.Right)
                    || (yUpDots[i].diretion == Direction.Right && NotIncludeDot(tempfloor, yUpDots, yUpDots[i], Direction.Right)))
                {
                    floor.transform.position = SnappedPosiiton(floor, yUpDots[i], Direction.Right);
                }
            }
        }
        // 아래쪽 변 스냅검사.
        for (int i = 0; i < yDownDots.Count; i++)
        {
            if (IsClose(yDownDots[i].y, tempfloor.rightUp.y, snapLength))
            {

                if (IsIncludeDot(tempfloor, yDownDots[i], Direction.Left)
                    || (yDownDots[i].diretion == Direction.Left && NotIncludeDot(tempfloor, yDownDots, yDownDots[i], Direction.Left)))
                {
                    floor.transform.position = SnappedPosiiton(floor, yDownDots[i], Direction.Left);
                }
            }
        }

    }

    /// <summary>
    /// 조건에 부합하면 스냅됐을 때의 위치를 계산해 반환해주는 함수.
    /// </summary>
    /// <param name="floor">드래그중인 바닥</param>
    /// <param name="dot">현재 검사중인 점</param>
    /// <param name="direction">점의 방향</param>
    /// <returns>
    /// 상/하인지, 좌/우인지에 따라 Vector3의 값을 다르게 return한다.
    /// 상/하 내에서도 위쪽 스냅인지 아래쪽 스냅인지에 따라 -1 혹은 1을 Multiply하여 구분하였다.
    /// 또한 모든 스냅에서 현재 움직이고 있는 바닥과 스냅된 바닥이 같은 방인지를 구분하기 위해 
    /// roomContainsDot Dictionary를 이용해 Padding 여부를 결정하였다.
    /// </returns>
    private Vector3 SnappedPosiiton(GameObject floor, Dot dot, Direction direction)
    {
        return (direction == Direction.Up || direction == Direction.Down)
            ? new Vector3(dot.x + ((direction == Direction.Up) ? -1 : 1) * floor.transform.localScale.x / 2
                + ((direction == Direction.Up) ? -1 : 1) * (floor.transform.parent == roomContainsDot[dot] ? 0 : 0.1f)
                , floor.transform.position.y, floor.transform.position.z)
            : new Vector3(floor.transform.position.x, floor.transform.position.y
                    , dot.position.z + ((direction == Direction.Left) ? -1 : 1) * floor.transform.localScale.z / 2
                    + ((direction == Direction.Left) ? -1 : 1) * (floor.transform.parent == roomContainsDot[dot] ? 0 : 0.1f));
    }

    /// <summary>
    /// 점이 바닥의 양옆 사이 점에 포함될 때의 경우.
    /// </summary>
    /// <param name="tempfloor">드래그중인 바닥</param>
    /// <param name="dot">현재 검사중인 점</param>
    /// <param name="direction">점의 방향</param>
    /// <returns></returns>
    private bool IsIncludeDot(Floor tempfloor, Dot dot, Direction direction)
    {
        return (direction == Direction.Up || direction == Direction.Down)
            ? (tempfloor.leftDown.y <= dot.y && dot.y <= tempfloor.leftUp.y)
            : (tempfloor.leftDown.x <= dot.x && dot.x <= tempfloor.rightDown.x);
    }

    /// <summary>
    /// 점이 바닥 양옆 사이 점의 바깥 범위에 있을 경우.
    ///  > 드래그중인 바닥보다 스냅하려는 바닥 변의 길이가 더 길 경우.
    /// </summary>
    /// <param name="tempfloor">드래그중인 바닥</param>
    /// <param name="dotList">스냅 되는 방향의 모든 점 리스트</param>
    /// <param name="dot">현재 검사중인 점</param>
    /// <param name="direction">점의 방향</param>
    /// <returns>
    /// 바닥의 수가 많아질 때 가끔 catch 문으로 잡히는 경우가 있음, 
    /// 성능상으로는 크리티컬하지 않으나, List의 Find중 float비교연산 때문에 오류가 생기는 듯 함.
    /// </returns>
    private bool NotIncludeDot(Floor tempfloor, List<Dot> dotList, Dot dot, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                try
                {
                    bool flag1 = dot.y <= tempfloor.leftDown.y;
                    bool flag2 = dotList.FindAll(x => Util.IsEquals(x.x, dot.x) && (x.y > dot.y))
                        .OrderBy(x => -1 * x.y).First().y > tempfloor.leftUp.y;
                    return flag1 && flag2;
                }
                catch (Exception)
                {
                    Debug.Log("UP");
                    return false;
                }
            case Direction.Down:
                try
                {
                    bool flag3 = dot.y >= tempfloor.leftUp.y;
                    bool flag4 = dotList.FindAll(x => Util.IsEquals(x.x, dot.x) && (x.y < dot.y))
                        .OrderBy(x => -1 * x.y).First().y < tempfloor.leftDown.y;
                    return flag3 && flag4;
                }
                catch (Exception)
                {
                    Debug.Log("Down");
                    return false;

                }
            case Direction.Left:
                try
                {
                    bool flag5 = (dot.x >= tempfloor.rightDown.x);

                    bool flag6 = dotList.FindAll(x => Util.IsEquals(x.y, dot.y) && (x.x < dot.x))
                            .OrderBy(x => -1 * x.x).First().x < tempfloor.leftDown.x;
                    return flag5 && flag6;
                }
                catch (Exception)
                {
                    Debug.Log("Left");
                    return false;

                }
            case Direction.Right:
                try
                {
                    bool flag7 = (dot.x <= tempfloor.leftUp.x);
                    bool flag8 = dotList.FindAll(x => Util.IsEquals(x.y, dot.y) && (x.x > dot.x))
                            .OrderBy(x => -1 * x.x).First().x > tempfloor.rightDown.x;
                    return flag7 && flag8;
                }
                catch (Exception)
                {
                    Debug.Log("Right");
                    return false;

                }
            default:
                return false;
        }

    }
    #endregion

    #region 수치변경

    //EditMode에서 floor 수치표시 & 수치 변경 기능이 있는 input box attach
    private void ShowFloorLabel(Transform floor)
    {
        if (!floor.gameObject.activeSelf || isMoving) return;
        if (labelTransfrom.childCount > 1) ClearAllLabels();
        ShowFloorsLabel(new List<Transform>() { floor });

        for (int i = 0; i < labelTransfrom.GetChild(0).childCount; i++)
        {
            GameObject label = labelTransfrom.GetChild(0).GetChild(i).gameObject;
            label.AddComponent<BoxCollider>();
            label.GetComponent<BoxCollider>().size = new Vector3(1, 1, 0.5f);
            label.name = "editableLabel" + label.name;

            string str = "";
            bool blink = false;
            float currentTime = 0, blinkTime = 0.5f;
            bool isPeriod = false;
            GameObject bg = null;
            var labelClick = new SingleAssignmentDisposable();
            labelClick.Disposable = label.OnMouseDownAsObservable()
                .Where(_ => model.State == SettingState.EditMode && editingLabel == null)
                .Subscribe(_ =>
                {
                    label.GetComponentInChildren<Text>().color = Color.blue;
                    bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    bg.transform.SetParent(label.transform);
                    bg.transform.localPosition = Vector3.back * 0.1f;
                    bg.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    bg.transform.localScale = new Vector3(1, 0.3f, 1);

                    str = "";
                    blink = false;
                    currentTime = 0;
                    blinkTime = 0.5f;
                    isPeriod = false;
                    editingLabel = label.gameObject;
                });
            Text text = label.GetComponentInChildren<Text>();
            string originalLength = text.text;

            var labelEdit = new SingleAssignmentDisposable();
            labelEdit.Disposable = label.UpdateAsObservable()
                .Where(_ => editingLabel == label)
                .Subscribe(_ =>
                {
                    if (Input.GetKeyDown(KeyCode.Backspace) && str.Length > 0)
                    {
                        str = str.Substring(0, str.Length - 1);
                        if (!str.Contains(".")) isPeriod = false;
                    }
                    if (int.TryParse(Input.inputString, out int result)) str += result;
                    else if ((Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.KeypadPeriod)) && !isPeriod)
                    {
                        str += ".";
                        isPeriod = true;
                    }
                    text.text = str + (blink ? "_" : "  ") + " m";
                    if (bg != null) bg.transform.localScale = new Vector3(0.5f + str.Length * 0.17f, 0.3f, 1);

                    if (currentTime < blinkTime) currentTime += Time.deltaTime;
                    else { currentTime = 0; blink = !blink; }

                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        int axis = int.Parse(label.name.Split('_')[1]);
                        if (float.TryParse(str, out float result2) && result2 > 0.049f && result2 < 25) UpdateFloor(result2, axis);
                        else text.text = originalLength;
                        if (text != null)
                        {
                            text.color = Color.black;
                            if (bg != null) Destroy(bg);
                            editingLabel = null;
                            labelEdit.Dispose();
                        }
                    }
                });

        }


    }
    #endregion

    #endregion
}
