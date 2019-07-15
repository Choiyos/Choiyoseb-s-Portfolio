using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 대부분의 로직이 구현되는 클래스.
/// 추후에 RoomPresenter같은 구체적인 네이밍이 되어야 할 듯.
/// View들을 모두 SerializeField로 받고 Model의 Singleton을 이용해
/// 둘의 관계를 이어준다.
/// </summary>
public partial class SpacePresenter : MonoBehaviour
{
    #region Fields

    [SerializeField]
    GameObject backgroundImage = default;

    GameObject viewModeObject = default;
    GameObject editModeObject = default;

    [SerializeField]
    Button settingButton = default;

    [SerializeField]
    GameObject settingView = default;

    // 스냅기능을 위해 모든 방의 개별 아웃라인 점들을 모아놓은 리스트.
    List<Dot> snapDots;
    // 바닥 클릭한 순간의 점들을 분류해놓은 리스트.
    List<Dot> xRightDots, xLeftDots, yUpDots, yDownDots;

    Dictionary<Dot, Transform> roomContainsDot;

    Model model;

    Transform labelTransfrom;

    // 테두리 관련
    Dictionary<GameObject, Transform> borders;
    [SerializeField]
    Transform borderParent = default;

    //클릭시 위치 저장
    Vector3 deltaPos;

    float gridSize;

    // for debug
    bool isDebugMode = false;
    [SerializeField]
    GameObject debugMode = default;
    Text debugLog = default;
    Transform debugParent = null;

    //option    
    [SerializeField]
    Toggle snapToggle = default;
    Dropdown labelOption;

    #endregion

    #region Methods

    #region Initiation

    private void Awake()
    {
        Init();
        Bind();
    }

    private void Init()
    {
        model = Model.Instance;
        gridSize = model.GridSize;
        model.State = SettingState.ViewMode;
        viewModeObject = addRoomButton.transform.parent.gameObject;
        editModeObject = addFloorButton.transform.parent.gameObject;
        labelTransfrom = GameObject.Find("Labels").transform;
        labelOption = GameObject.Find("LabelTypeList").GetComponent<Dropdown>();
        borders = new Dictionary<GameObject, Transform>();
        if (model.Room.Rooms.Count == 0)
        {
            DontDestroyOnLoad(GameObject.Find("Rooms"));

            // 초기 방 생성
            AddRoom(true);
            CancelSelectFloor();
            CancelSelectRoom();
        }
        else
        {
            for (int i = 0; i < model.Room.Rooms.Count; i++)
            {
                for (int j = 0; j < model.Room.Rooms[i].childCount; j++)
                {
                    BindFloor(model.Room.Rooms[i].GetChild(j).gameObject);
                    AttachBordersFloor(model.Room.Rooms[i].GetChild(j).gameObject);
                }
            }
        }

        settingView.SetActive(false);

        debugMode.SetActive(false);
        debugLog = debugMode.transform.GetChild(0).GetComponent<Text>();
    }

    private void Bind()
    {
        BackgroundBind();
        ViewModeBind();
        EditModeBind();

        settingView.transform.GetChild(0).Find("ExitButton").GetComponent<Button>().OnClickAsObservable()
            .Subscribe(_ =>
            {
                settingView.SetActive(false);
            });
        settingButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                settingView.SetActive(!settingView.activeSelf);
            });
        labelOption.OnValueChangedAsObservable()
            .Where(_ => !isMoving)
            .Subscribe(_ => {
                ClearAllLabels();
                ShowAllLabels();
            });
    }

    private void BackgroundBind()
    {
        backgroundImage.OnMouseDownAsObservable()
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ =>
            {
                CancelBG();
            });


        // 에딧모드에서 배경 더블클릭시 -> 뷰모드
        var clickStream = backgroundImage.UpdateAsObservable()
            .Where((_) =>
            {
                if (Input.GetMouseButtonDown(0) && model.State == SettingState.EditMode
                && !EventSystem.current.IsPointerOverGameObject())
                {
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f);
                    if (hit.transform != null && hit.transform == backgroundImage.transform) return true;
                }
                return false;
            });

        clickStream
            .Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(200)))
            .Where(x => x.Count >= 2 && !EventSystem.current.IsPointerOverGameObject()
                        && model.State == SettingState.EditMode)
            .Subscribe(_ =>
            {
                if (CheckRoom()) SwitchMode(SettingState.ViewMode);
            });

        backgroundImage.OnMouseDragAsObservable()
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Select(_ => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")))
            .Subscribe(move =>
            {
                Camera.main.transform.position -= new Vector3(move.x * Time.deltaTime * 5.0f, 0, move.y * Time.deltaTime * 5.0f);
            });
        
        backgroundImage.UpdateAsObservable()
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Select(x_ => Input.GetAxis("Mouse ScrollWheel") * -3.0f)
            .Subscribe(x =>
            {
                //휠 드래그시 카메라 확대/축소 before : 7
                if (Camera.main.orthographicSize <= 2f && x < 0)
                {
                    Camera.main.orthographicSize = 2f;
                }
                else if (Camera.main.orthographicSize >= 12.5f && x > 0)
                {
                    Camera.main.orthographicSize = 12.5f;
                }
                else
                {
                    Camera.main.orthographicSize += x;
                }

            });

        // 키입력
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (model.State == SettingState.EditMode)
                {
                    if (Input.GetKeyDown(KeyCode.Delete) && !isMoving) { RemoveFloor(); return; }
                    if (Input.GetKeyDown(KeyCode.Escape) && model.Floor.SelectedFloor == null)
                    {
                        if (CheckRoom()) SwitchMode(SettingState.ViewMode);
                        return;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Delete) && !isMoving) { RemoveRoom(); return; }
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        if (model.Room.SelectedRoom == null) return;
                        GameObject selectedRoom = model.Room.SelectedRoom;
                        SwitchMode(SettingState.EditMode);
                        model.Room.SelectedRoom = selectedRoom;
                        EditRoom();
                        return;
                    }
                }

                if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Fire2")) && !isMoving) { CancelBG(); return; }

                // for debug
                if (Input.GetKeyDown(KeyCode.F12))
                {
                    isDebugMode = !isDebugMode;
                    debugMode.SetActive(isDebugMode);

                    if (debugParent == null) debugParent = new GameObject("DebugParent").transform;
                    debugParent.gameObject.SetActive(isDebugMode);

                    if (GameObject.Find("DebugOutline") != null) GameObject.Find("DebugOutline").SetActive(debugMode);
                }

                if (isDebugMode)
                {
                    if (Input.GetKeyDown(KeyCode.F11)) { debugParent.gameObject.SetActive(!debugParent.gameObject.activeSelf); return; }

                    if (Input.GetKeyDown(KeyCode.Alpha1)) { Debug.Log("label0"); labelOption.value = 0; ClearAllLabels(); ShowAllLabels(); return; }
                    if (Input.GetKeyDown(KeyCode.Alpha2)) { Debug.Log("label1"); labelOption.value = 1; ClearAllLabels(); ShowAllLabels(); return; }
                    if (Input.GetKeyDown(KeyCode.Alpha3)) { Debug.Log("label2"); labelOption.value = 2; ClearAllLabels(); ShowAllLabels(); return; }

                }

            });

        // debug 모드일때 floor 좌표/크기 정보 표시
        debugLog.UpdateAsObservable()
            .Where(_ => isDebugMode)
            .Subscribe(_ =>
            {
                string str = "";
                int i = 0, j = 0;
                foreach (var room in model.Room.Rooms)
                {
                    str += "room" + i + " : \n";
                    foreach (var floor in Util.GetChildTransform(room))
                    {
                        str += "floor" + j + " pos = " + floor.position.ToString("0.###") + ", scale = " + floor.localScale.ToString("0.###") + " \n";
                        j++;
                    }
                    i++;
                }

                debugLog.text = str;

                if (Input.GetKeyDown(KeyCode.C))
                {
                    GUIUtility.systemCopyBuffer = debugLog.text;
                    Debug.Log("copy to clipboard :" + debugLog.text);
                }
            });
    }
    
    private void BindFloor(GameObject floor)
    {
        var sceneChange = Observable.EveryUpdate()
            .Where(_ => UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 1);
        // 바닥 클릭시
        floor.transform.OnMouseDownAsObservable()
            .Where(_ => !floor.GetComponent<MeshRenderer>().material.name.Contains("Disabled")
            && !EventSystem.current.IsPointerOverGameObject())
            .TakeUntil(sceneChange)
            .Subscribe(_ =>
            {
                deltaPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - floor.transform.position;
                deltaPos.y = 0;
                ClearAllLabels();
                Select(floor);

                RefreshDots(floor);
            }).AddTo(this);

        // 바닥 더블클릭        
        var clickStream = floor.UpdateAsObservable()
            .Where((_) =>
            {
                if (Input.GetMouseButtonDown(0) && model.State == SettingState.ViewMode
                && !EventSystem.current.IsPointerOverGameObject())
                {
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f);
                    if (hit.transform != null && hit.transform == floor.transform) return true;
                }
                return false;
            });
        clickStream
            .Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(200)))
            .Where(x => x.Count >= 2
                        && model.Room.SelectedRoom != null
                        && !floor.GetComponent<MeshRenderer>().material.name.Contains("Disabled")
                        && !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ =>
            {
                // 뷰모드에서 바닥 더블클릭시 -> 편집모드
                if (model.State == SettingState.ViewMode)
                {
                    GameObject selectedRoom = model.Room.SelectedRoom;
                    SwitchMode(SettingState.EditMode);
                    model.Room.SelectedRoom = selectedRoom;
                    EditRoom();

                    //Select(floor);
                }
                // 편집모드에서 바닥 더블클릭시 -> 선택취소
                if (model.State == SettingState.EditMode)
                {
                    CheckRoom();
                }

            }).AddTo(this);



        // 바닥 드래그시
        floor.OnMouseDragAsObservable()
            .Where(_ => !floor.GetComponent<MeshRenderer>().material.name.Contains("Disabled")
            //&& !EventSystem.current.IsPointerOverGameObject()
            && model.State == SettingState.EditMode && !isMoving)
            .TakeUntil(sceneChange)
            .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
            .Subscribe(move =>
            {
                ClearAllLabels();
                Util.SetFloorPosition(floor, new Vector3(move.x, floor.transform.position.y, move.z) - deltaPos, gridSize);
                if (snapToggle.isOn) SnapFloor(floor);
                UpdateBorder(floor);
            }).AddTo(this);

        // 바닥 클릭/드래그하고 나서
        floor.OnMouseUpAsObservable()
            .Where(_ => !floor.GetComponent<MeshRenderer>().material.name.Contains("Disabled")
            && model.State == SettingState.EditMode && !isMoving)
            .TakeUntil(sceneChange)
            .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
            .Subscribe(move =>
            {
                CheckFloor(floor);
                ShowFloorLabel(floor.transform);
            }).AddTo(this);
    }

    /// <summary>
    /// 스냅기능을 위해 바닥에 변경사항이 생길 때마다 Dot 리스트를 갱신해주는 함수.
    /// </summary>
    /// <param name="floor"></param>
    private void RefreshDots(GameObject floor)
    {
        snapDots = new List<Dot>();
        roomContainsDot = new Dictionary<Dot, Transform>();
        xRightDots = new List<Dot>();
        xLeftDots = new List<Dot>();
        yUpDots = new List<Dot>();
        yDownDots = new List<Dot>();
        Outliner outliner = new Outliner();
        List<Dot> tempOutLine = new List<Dot>();


        for (int i = 0; i < Model.Instance.Room.Rooms.Count; i++)
        {
            List<Transform> floors = new List<Transform>();
            for (int j = 0; j < Model.Instance.Room.Rooms[i].childCount; j++)
            {
                if (floor != Model.Instance.Room.Rooms[i].GetChild(j).gameObject)
                {
                    floors.Add(Model.Instance.Room.Rooms[i].GetChild(j));
                }
            }

            if (floors.Count > 0)
            {
                tempOutLine = outliner.FindOutline(floors);
                snapDots.AddRange(tempOutLine);

                for (int k = 0; k < tempOutLine.Count; k++)
                {
                    try
                    {
                        roomContainsDot.Add(tempOutLine[k], Model.Instance.Room.Rooms[i]);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                }
            }
        }

        Floor tempfloor = Util.ConvertFloorToFloor(floor.transform);
        // 바닥에서 4방향으로 나누고, 해당 부분을 리스트로 따로 담음.
        xRightDots = snapDots.FindAll(x => (x.diretion == Direction.Down
        || x.diretion == Direction.Right && x.attribute == "Cross"
        || x.diretion == Direction.Left && x.attribute == "Corner"));

        xLeftDots = snapDots.FindAll(x => (x.diretion == Direction.Up
        || x.diretion == Direction.Left && x.attribute == "Cross"
        || x.diretion == Direction.Right && x.attribute == "Corner"));

        yUpDots = snapDots.FindAll(x => (x.diretion == Direction.Right
        || x.diretion == Direction.Up && x.attribute == "Cross"
        || x.diretion == Direction.Down && x.attribute == "Corner"));

        yDownDots = snapDots.FindAll(x => (x.diretion == Direction.Left
        || x.diretion == Direction.Down && x.attribute == "Cross"
        || x.diretion == Direction.Up && x.attribute == "Corner"));

        if (isDebugMode)
        {
            Debug.Log("---DotListCount---");
            Debug.Log("xRightDots : " + xRightDots.Count);
            Debug.Log("xLeftDots : " + xLeftDots.Count);
            Debug.Log("yUpDots : " + yUpDots.Count);
            Debug.Log("yDownDots : " + yDownDots.Count);
            Debug.Log("------------------");

        }
    }

    private void Select(GameObject floor)
    {
        switch (model.State)
        {
            case SettingState.ViewMode:
                SelectRoom(floor);
                break;
            case SettingState.EditMode:
                SelectFloor(floor);
                break;
            default:
                break;
        }
    }

    #endregion

    #region 공통모듈

    /// <summary>
    /// 뷰 모드와 편집 모드 사이의 변경(버튼 그룹 활성화/비활성화). 
    /// </summary>
    /// <param name="mode"></param>
    private void SwitchMode(SettingState mode)
    {
        model.State = mode;

        switch (mode)
        {
            case SettingState.ViewMode:
                CancelSelectRoom();
                viewModeObject.SetActive(true);
                editModeObject.SetActive(false);
                nextButton.interactable = true;
                break;

            case SettingState.EditMode:

                ShowAllLabels();
                viewModeObject.SetActive(false);
                editModeObject.SetActive(true);
                nextButton.interactable = false;
                break;
            default:
                break;
        }
        
        List<Transform> curfloors = model.Room.SelectedRoom == null ? new List<Transform>() : Util.GetChildTransform(model.Room.SelectedRoom.transform);
        foreach (var r in borderParent.GetComponentsInChildren<Renderer>())
        {
            if (mode == SettingState.ViewMode)
            {
                r.material = model.ColorMaterialBlack;
            }
            else
            {
                // 해당 테두리에 속한 바닥이 현재 편집중인 방이 아니면 테두리를 회색으로 적용
                r.material = (curfloors.Exists(x => borders[x.gameObject] == r.transform.parent)) ? model.ColorMaterialBlack : model.ColorMaterialGray;
            }
        }

        CleanSelect();
    }

    private void CleanSelect()
    {
        foreach (Transform room in model.Room.Rooms)
        {
            ChangeMaterial(
                room.GetComponentsInChildren<MeshRenderer>()
                , model.Room.DefaultRoomMaterial
            );
        }
    }

    private void ChangeMaterial(MeshRenderer[] floors, Material _material)
    {
        foreach (MeshRenderer floor in floors)
        {
            floor.material = _material;
        }
    }

    #region 수치표시 관련

    /// <summary>
    /// 모든 방 수치 라벨 표시하는 함수.
    /// </summary>
    private void ShowAllLabels()
    {
        foreach (var room in model.Room.Rooms)
        {
            ShowFloorsLabel(Util.GetChildTransform(room));
        }
    }
    
    /// <summary>
    /// 방 수치 라벨 표시하는 함수.
    /// </summary>
    /// <param name="roomfloors"></param>
    private void ShowFloorsLabel(List<Transform> roomfloors)
    {
        Util.ShowRoomSize(roomfloors, labelTransfrom, model.TextLabelPrefab, model.Floor.LinePrefab, isDebugMode, labelOption.value);
    }

    /// <summary>
    /// 라벨 일괄 제거
    /// </summary>
    private void ClearAllLabels()
    {
        Destroy(GameObject.Find("Visualize_Line"));
        for (int i = 0; i < labelTransfrom.childCount; i++)
        {
            Destroy(labelTransfrom.GetChild(i).gameObject);
        }

        if (isDebugMode && debugParent != null)
        {
            for (int i = 0; i < debugParent.childCount; i++)
            {
                Destroy(debugParent.GetChild(i).gameObject);
            }
        }
    }

    private void CancelBG()
    {
        switch (model.State)
        {
            case SettingState.ViewMode:
                CancelSelectRoom();
                break;
            case SettingState.EditMode:
                CheckRoom();
                break;
            default:
                break;
        }

        // 에디터 상에서 바닥을 매뉴얼로 옮길 경우 편의성을 위해 추가
        if (isDebugMode)
        {
            foreach (var floor in Util.GetAllFloors(model.Room.Rooms))
            {
                UpdateBorder(floor.gameObject);
            }
        }
    }

    #endregion

    #region 테두리 관련

    /// <summary>
    /// 활성/비활성화 된 바닥을 y축으로 올리기/내리기
    /// </summary>
    /// <param name="floor"></param>
    /// <param name="level"></param>
    private void FloatFloor(Transform floor, int level)
    {
        floor.position = new Vector3(floor.position.x, level, floor.position.z);
        UpdateBorder(floor.gameObject, level);
    }

    GameObject AttachBordersFloor(GameObject floor)
    {
        GameObject border = Util.MakeBorder(floor.transform, borderParent, model.ColorMaterialBlack);
        borders.Add(floor, border.transform);
        return border;
    }

    void DeleteBorder(GameObject floor)
    {
        GameObject border = borders[floor].gameObject;
        borders.Remove(floor);
        Destroy(border);
    }


    /// <summary>
    /// 바닥을 동일한 위치에 또 생성하려고 하는지 검사.
    /// </summary>
    /// <returns></returns>
    private static bool FindDouble()
    {
        List<Transform> floors = Util.GetAllFloors(Model.Instance.Room.Rooms);
        foreach (var floor1 in floors)
        {
            foreach (var floor2 in floors)
            {
                if (floor1 == floor2) continue;
                if (Util.Equals(floor1.position.x, floor2.position.x) && Util.Equals(floor1.position.z, floor2.position.z)
                    && Util.Equals(floor1.localScale.x, floor2.localScale.x) && Util.Equals(floor1.localScale.z, floor2.localScale.z)) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 바닥 크기 및 상태 변경시 바닥에 해당하는 테두리를 바닥에 맞게 적용
    /// </summary>
    /// <param name="floor"></param>
    /// <param name="level"></param>
    void UpdateBorder(GameObject floor, int level = 2)
    {
        if (floor == null) return;
        if (!borders.ContainsKey(floor)) return;
        Transform borderTransform = borders[floor];
        if (borderTransform == null)
        {
            Debug.Log("sadf");
            return;
        }
        float border = 0.1f * 2;
        float height = 0.1f;
        borderTransform.localScale = floor.transform.localScale;
        borderTransform.GetChild(0).localScale = new Vector3(1 + border / floor.transform.localScale.x, border / floor.transform.localScale.z, 1);
        borderTransform.GetChild(1).localScale = new Vector3(1 + border / floor.transform.localScale.x, border / floor.transform.localScale.z, 1);
        borderTransform.GetChild(2).localScale = new Vector3(1 + border / floor.transform.localScale.z, border / floor.transform.localScale.x, 1);
        borderTransform.GetChild(3).localScale = new Vector3(1 + border / floor.transform.localScale.z, border / floor.transform.localScale.x, 1);
        if (floor.transform.position.y == 2) borderTransform.position = new Vector3(floor.transform.position.x, 1, floor.transform.position.z) + Vector3.down * height;
        else borderTransform.position = floor.transform.position + Vector3.down * height;

    }
    #endregion

    #endregion

    #endregion


}
