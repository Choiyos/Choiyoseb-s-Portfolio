using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// 요소설정 씬을 위한 프레젠터.
/// 문과 창문 설정 사이를 지원한다.
/// </summary>
public partial class ElementPresenter : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private GameObject backgroundImage = default;
    Model model;

    [SerializeField]
    Button backButton = default, nextButton = default;

    [SerializeField]
    GameObject labels = default;
    GameObject label1, label2, label3;
    TextMesh label1Text, label2Text, label3Text;
    Transform label1Line, label2Line, label3Line;
    // 콜라이더 충돌에서 쓰는 변수.
    bool isClickable = true;
    // 아이콘 이동중일 때 쓰는 변수.
    bool isMoving = false;
    [SerializeField]
    Button doorButton = default, windowButton = default;

    // for debug
    bool isDebugMode = false;
    [SerializeField]
    GameObject debugMode = default;
    Text debugLog = default;

    #region 설정 관련 변수들
    Transform previewIconParent;

    [SerializeField]
    Button settingButton = default;
    [SerializeField]
    GameObject settingView = default;
    #region 문 기본길이 관련 변수들
    [SerializeField]
    InputField doorDefaultLengthText = default;
    [SerializeField]
    float doorDefaultLength = 0.8f;

    #endregion

    #region 창문 기본길이 관련 변수들
    [SerializeField]
    InputField windowDefaultLengthText = default;
    [SerializeField]
    float windowDefaultLength = 1f;
    #endregion

    #endregion

    #region 아이콘 미리보기 관련 변수들
    List<Model.Wall> adjacencyWallList;
    Model.Wall previewAttachedWall;
    [SerializeField]
    float standardLength = 0.5f;
    GameObject previewIcon;
    #endregion

    #endregion

    #region Methods
    private void Awake()
    {
        Init();
        Bind();
    }

    private void Bind()
    {
        BackgroundBind();
        BindDoorButtons();
        BindWindowButtons();
        BindPreviewIcon();
        BindPreviewIconParent();
        BindUIButtons();

        label3.OnMouseUpAsObservable()
            .Subscribe(_ =>
            {
                label3.GetComponent<TextMesh>().color = Color.blue;
                GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bg.transform.SetParent(label3.transform);
                bg.transform.localPosition = Vector3.zero;
                bg.transform.localRotation = Quaternion.Euler(0, 0, 0);
                bg.transform.localScale = new Vector3(2, 0.5f, 1);
                StartCoroutine(InputText(label3.GetComponent<TextMesh>()));
            });

    }

    private void BindUIButtons()
    {
        backButton.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        model.DoorIcon = model.SwingDoorIconPrefab;
                        model.WindowIcon = model.DefaultWindowIconPrefab;

                        model.DoorList.Clear();
                        model.WindowList.Clear();
                        model.WallList.Clear();
                        model.Room.MergedRooms.Clear();
                        Transform rooms = GameObject.Find("Rooms").transform;
                        for (int i = 0; i < rooms.childCount; i++)
                        {
                            if (rooms.GetChild(i).name == "Room")
                            {
                                rooms.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                        Destroy(rooms.Find("OutWalls").gameObject);
                        Destroy(rooms.Find("MergedRooms").gameObject);
                        SceneManager.LoadScene("SpaceSettingScene");
                    });

        nextButton.OnClickAsObservable()
        .Subscribe(_ =>
        {
            editWindowSizeUI.SetActive(false);
            // 공간꾸미기 씬으로 전환.
            Loading();
            MakeWall();
            MakeElement();
            Destroy(windowPhase);
            Destroy(doorPhase);

            model.DoorIcon = model.SwingDoorIconPrefab;
            model.WindowIcon = model.DefaultWindowIconPrefab;

            SceneManager.LoadScene("DecorateSpaceScene");
        });


        doorButton.onClick.AsObservable()
            .Subscribe(_ =>
            {
                windowButton.GetComponent<Image>().color = Color.white;
                doorButton.GetComponent<Image>().color = Color.yellow;

                ChangePreviewIcon(model.SwingDoorIconPrefab);

                CancelSelect();
                GameObject.Find("AdditionalMode").transform.GetChild(1).gameObject.SetActive(false);
                GameObject.Find("AdditionalMode").transform.GetChild(0).gameObject.SetActive(true);
                ReturnDoorPhase();
                //BindDoorWalls(model.WallList);
                Destroy(windowPhase);
            });

        windowButton.OnClickAsObservable()
        .Subscribe(_ =>
        {
            windowButton.GetComponent<Image>().color = Color.yellow;
            doorButton.GetComponent<Image>().color = Color.white;
            ChangePreviewIcon(model.DefaultWindowIconPrefab);

            // 벽을 클릭하여 문 방향을 정하던 중 창문설정으로 넘어가면 생성중이던 문 삭제.
            if (doorTargetPoint.activeSelf)
            {
                doorTargetPoint.SetActive(false);
                labels.transform.GetChild(0).gameObject.SetActive(false);
                labels.transform.GetChild(1).gameObject.SetActive(false);
                labels.transform.GetChild(2).gameObject.SetActive(false);
                Transform doors = GameObject.Find("Doors").transform;
                for (int i = 0; i < doors.childCount; i++)
                {
                    if (!model.DoorList.Exists(x => x.DoorObject.transform == doors.GetChild(i)))
                    {
                        Destroy(doors.GetChild(i).gameObject);
                    }
                }
            }

            CancelSelect();
            // 창문설정으로 페이즈 변경.
            GameObject.Find("AdditionalMode").transform.GetChild(0).gameObject.SetActive(false);
            GameObject.Find("AdditionalMode").transform.GetChild(1).gameObject.SetActive(true);
            WindowInit();
            Destroy(doorPhase);
        });

        settingView.OnEnableAsObservable()
            .Subscribe(x =>
            {
                doorDefaultLengthText.onValueChanged.AsObservable()
                    .TakeUntil(settingView.OnDisableAsObservable())
                    .Subscribe(_ =>
                    {
                        float result = 0;

                        if (float.TryParse(doorDefaultLengthText.text, out result))
                        {
                            doorDefaultLength = result;
                        }
                    });

                windowDefaultLengthText.onValueChanged.AsObservable()
                    .TakeUntil(settingView.OnDisableAsObservable())
                    .Subscribe(_ =>
                    {
                        float result = 0;
                        if (float.TryParse(windowDefaultLengthText.text, out result))
                        {
                            windowDefaultLength = result;
                        }

                    });
            });

        settingButton.onClick.AsObservable()
            .Subscribe(_ =>
            {
                settingView.SetActive(true);
            });

        settingView.transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AsObservable()
            .Subscribe(_ =>
            {
                if (settingView.transform.GetChild(0).GetChild(0).name == "ExitButton")
                {
                    settingView.SetActive(false);
                }
            });
    }

    private void Init()
    {
        model = Model.Instance;

        #region 라벨 관련 초기화
        label1 = labels.transform.GetChild(0).gameObject;
        label2 = labels.transform.GetChild(1).gameObject;
        label3 = labels.transform.GetChild(2).gameObject;
        label1Text = label1.GetComponent<TextMesh>();
        label2Text = label2.GetComponent<TextMesh>();
        label3Text = label3.GetComponent<TextMesh>();
        label1Line = label1.transform.GetChild(0);
        label2Line = label2.transform.GetChild(0);
        label3Line = label3.transform.GetChild(0);
        #endregion

        #region 설정 창 관련 초기화
        doorDefaultLengthText.text = doorDefaultLength.ToString();
        windowDefaultLengthText.text = windowDefaultLength.ToString();
        #endregion

        #region 아이콘 미리보기 관련
        previewAttachedWall = null;
        previewIconParent = GameObject.Find("PreviewIcon").transform;
        #endregion

        //door 모드에서 시작
        windowButton.GetComponent<Image>().color = Color.white;
        doorButton.GetComponent<Image>().color = Color.yellow;

        DoorInit();
        // for debug
        debugMode.SetActive(false);
        debugLog = debugMode.transform.GetChild(0).GetComponent<Text>();
    }

    private void BackgroundBind()
    {
        backgroundImage.OnMouseDownAsObservable()
            .Where(_ => !EventSystem.current.IsPointerOverGameObject() && isClickable)
            .Subscribe(_ =>
            {
                CancelSelect();
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
                //휠 드래그시 카메라 확대/축소
                if (Camera.main.orthographicSize <= 7f && x < 0)
                {
                    Camera.main.orthographicSize = 7f;
                }
                else if (Camera.main.orthographicSize >= 12.5f && x > 0)
                {
                    Camera.main.orthographicSize = 12.5f;
                }
                else
                {
                    Camera.main.orthographicSize += x;
                }


                // for debug
                if (Input.GetKeyDown(KeyCode.F12))
                {
                    isDebugMode = !isDebugMode;
                    debugMode.SetActive(isDebugMode);
                }
            });

        string str;
        debugLog.UpdateAsObservable()
            .Where(_ => isDebugMode)
            .Subscribe(_ =>
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    str = "debugmode";
                    debugLog.text = str;
                }
            });
    }

    private void CancelSelect()
    {
        StopAllCoroutines();
        label3Text.color = Color.black;
        if (label3.transform.childCount > 1)
            Destroy(label3.transform.GetChild(1).gameObject);
        if (model.SelectedElement != null)
        {
            if (model.SelectedElement.name == "NoDoorIcon(Clone)")
                model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
            else
            {
                model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.black;
            }
            model.SelectedElement = null;
            DoorButtonSetactive(false);
            WindowButtonSetactive(false);
            editWindowSizeUI.SetActive(false);

            labels.transform.GetChild(0).gameObject.SetActive(false);
            labels.transform.GetChild(1).gameObject.SetActive(false);
            labels.transform.GetChild(2).gameObject.SetActive(false);

        }

    }

    public void Select(GameObject element)
    {
        element.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.cyan;
        model.SelectedElement = element;
    }

    void MakeWall()
    {
        // element를 transform으로 변환
        List<Transform> elementList = new List<Transform>();
        foreach (var door in model.DoorList)
        {
            // transform 정보를 가진 child를 넘겨줌
            elementList.Add(door.DoorObject.transform.GetChild(3));

        }
        foreach (var window in model.WindowList)
        {
            // transform 정보를 가진 child를 넘겨줌
            elementList.Add(window.WindowObject.transform.GetChild(3));
        }
        List<Transform> allFloors = new List<Transform>();
        foreach (var room in model.Room.Rooms)
        {
            allFloors.AddRange(Util.GetChildTransform(room));
        }

        float border = 0.1f;
        float height = 2.2f;

        // 문/창문 뚫린 벽 생성
        model.GeneratedWallList = Util.MakeWalls(model.Room.Rooms, elementList, height, model.DefaultMaterial, out Transform walls, isDebugMode);

        foreach (var floor in allFloors)
        {
            floor.localScale = new Vector3(floor.localScale.x + border * 2f, 1, floor.localScale.z + border * 2f);
        }

        Material outwallsMaterial = model.OutwallMaterial;
        //외곽 벽 두르기
        GameObject outwallsParent = GameObject.Find("OutWalls");
        GameObject outlineWall = Util.MakeOutlineWalls(allFloors, elementList, border, height, model.OutwallMaterial);
        outlineWall.transform.parent = outwallsParent.transform;

        Util.SetMaterial(outwallsParent, outwallsMaterial);

        foreach (var floor in allFloors)
        {
            floor.localScale = new Vector3(floor.localScale.x - border * 2f, 1, floor.localScale.z - border * 2f);
        }
    }

    /// <summary>
    /// 문 아이콘과 창문 아이콘의 자리에 3D 모델 오브젝트 생성하는 함수.
    /// </summary>
    private void MakeElement()
    {

        Transform doorParent = new GameObject().transform;
        doorParent.name = "DoorsParent";
        doorParent.parent = model.transform;
        Transform windowParent = new GameObject().transform;
        windowParent.name = "WindowsParent";
        windowParent.parent = model.transform;

        foreach (var door in model.DoorList)
        {
            // transform 정보를 가진 child를 넘겨줌

            if (door.DoorObject.name == "SwingDoorIcon(Clone)")
            {
                model.DoorPrefab = model.SwingDoorPrefab;
            }
            else if (door.DoorObject.name == "NoDoorIcon(Clone)")
            {
                model.DoorPrefab = model.NoDoorPrefab;

            }
            GameObject doorPrefab = Instantiate(model.DoorPrefab, new Vector3(door.DoorObject.transform.position.x, 0.06f, door.DoorObject.transform.position.z), new Quaternion(), doorParent);
            doorPrefab.transform.localRotation = door.DoorAttachedWall.Direction == Model.Wall.WallDirection.Vertical ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, 0, 0);
            doorPrefab.transform.localRotation = door.DoorObject.transform.localRotation;

            // 애니메이션을 위해 스케일 반영할 모델을 따로 뺌
            Transform doorModel = doorPrefab.transform.GetChild(0);
            doorModel.localScale = new Vector3((doorModel.localScale.x == 0 ? 1 : doorModel.localScale.x) * door.DoorObject.transform.localScale.x, doorModel.localScale.y, doorModel.localScale.z);

            // z 반전을  x -1 스케일로 변환
            Vector3 rotateAngle = door.DoorObject.transform.eulerAngles;
            Vector3 scale = doorModel.transform.localScale;
            if (Util.IsEquals(rotateAngle.z, 180)) { rotateAngle.z = 0; }
            else if (Util.IsEquals(rotateAngle.z, 0)) { scale.x *= -1; }
            doorPrefab.transform.eulerAngles = rotateAngle;
            doorModel.transform.localScale = scale;

            BoxCollider box = doorPrefab.GetComponent<BoxCollider>();
            if (box != null)
            {
                scale.x = Mathf.Abs(scale.x);
                scale.z = 0.2f;
                box.size = scale;
                box.center = Vector3.up * scale.y * 0.5f;
            }
            door.DoorModel = doorPrefab;
        }

        foreach (var window in model.WindowList)
        {
            // transform 정보를 가진 child를 넘겨줌

            if (window.WindowObject.name == "DefaultWindowIcon(Clone)")
            {
                model.WindowPrefab = model.DefaltWindowPrefab;
                GameObject windowPrefab =
                    Instantiate(
                      model.WindowPrefab
                    , new Vector3(window.WindowObject.transform.position.x
                        , window.WindowObject.transform.GetChild(3).position.y
                        , window.WindowObject.transform.position.z)
                    , new Quaternion(), windowParent);
                windowPrefab.transform.localScale =
                    new Vector3(model.WindowPrefab.transform.localScale.x
                    , window.WindowObject.transform.GetChild(3).localScale.y * model.WindowPrefab.transform.localScale.y
                    , window.WindowObject.transform.localScale.x);
                windowPrefab.transform.localRotation =
                    window.WindowAttachedWall.Direction == Model.Wall.WallDirection.Vertical
                    ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);
            }
            else if (window.WindowObject.name == "BigWindowIcon(Clone)")
            {
                model.WindowPrefab = model.BigWindowPrefab;
                float yScale = (window.WindowObject.transform.GetChild(3).localScale.y / 2) * model.WindowPrefab.transform.localScale.y;
                GameObject windowPrefab =
                    Instantiate(
                      model.WindowPrefab
                    , new Vector3(window.WindowObject.transform.position.x
                        , 0.05f
                        , window.WindowObject.transform.position.z)
                    , new Quaternion(), windowParent);
                windowPrefab.transform.localScale =
                    new Vector3(model.WindowPrefab.transform.localScale.x
                    , yScale
                    , window.WindowObject.transform.localScale.x);
                windowPrefab.transform.localRotation =
                    window.WindowAttachedWall.Direction == Model.Wall.WallDirection.Vertical
                    ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);

            }
        }
    }

    /// <summary>
    /// 라벨을 클릭했을 때 수치를 입력받기 위해 대기하는 함수.
    /// </summary>
    /// <param name="text">텍스트 컴포넌트 참조</param>
    /// <returns></returns>
    IEnumerator InputText(TextMesh text)
    {
        string str = "";
        bool blink = false;
        float currentTime = 0, blinkTime = 0.5f;
        bool isPeriod = false;
        while (true)
        {
            if (text == null) break;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) break;
            if (Input.GetKeyDown(KeyCode.Backspace) && str.Length > 0) str = str.Substring(0, str.Length - 1);
            if (int.TryParse(Input.inputString, out int result)) str += result;
            else if (!isPeriod && (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.KeypadPeriod)))
            {
                str += ".";
                isPeriod = true;
            }
            text.text = str + (blink ? "_" : "  ") + " m";
            if (currentTime < blinkTime) currentTime += Time.deltaTime;
            else { currentTime = 0; blink = !blink; }
            yield return null;
        }
        if (doorPhase != null)
        {
            UpdateDoorLabel(float.TryParse(str, out float result2) ? result2 : -1);
        }
        else
            UpdateWindow(float.TryParse(str, out float result2) ? result2 : -1);
        if (text != null)
        {
            //text.color = Color.black;
            Destroy(text.transform.GetChild(1).gameObject);
        }
        yield return null;
    }

    /// <summary>
    /// 아이콘 프리팹을 선택한 오브젝트로 바꿔치기 해주는 함수.
    /// </summary>
    /// <param name="iconPrefab"></param>
    void ChangePreviewIcon(GameObject iconPrefab)
    {
        Destroy(previewIcon);
        previewIcon = Instantiate(iconPrefab, previewIconParent);
        previewIcon.transform.GetChild(0).GetComponent<MeshRenderer>().material.color
            = iconPrefab.name.Contains("NoDoor") ? new Color(1, 1, 1, 0.6f) : new Color(0, 0, 0, 0.6f);
    }

    private void BindPreviewIconParent()
    {
        previewIconParent.OnMouseDownAsObservable()
            // 현재 preview Icon이 떠있는 상태인지.
            .Where(_ => previewIcon.activeSelf)
            .Subscribe(_ =>
            {
                ApplyElement();
            });
    }

    /// <summary>
    /// 미리보기중 마우스 클릭시 해당 부분에 아이콘을 적용하는 함수.
    /// 프로젝트 정리 이유로 여유롭지 못하여 Generate 함수들을 제대로 대체하지 못하였음.
    /// </summary>
    private void ApplyElement()
    {
        if (previewIcon.name.Contains("Door"))
        {
            previewIcon.SetActive(false);
            GenerateDoor(previewIcon.transform.position, previewAttachedWall);
        }
        else
        {
            previewIcon.SetActive(false);
            GenerateWindow(previewIcon.transform.position, previewAttachedWall);
        }
    }

    /// <summary>
    /// 요소 설정 씬에 들어오자 마자 다음 씬으로 넘어갈 때 까지 계속해서 실행되고 있는 함수.
    /// 벽 주변에 마우스를 가져다 대면 문이나 창문의 아이콘을 미리보기로 띄워줍니다.
    /// </summary>
    private void BindPreviewIcon()
    {
        List<Model.Wall> wallList = model.WallList;
        ChangePreviewIcon(model.SwingDoorIconPrefab);
        previewIcon.SetActive(false);
        // Update().
        this.UpdateAsObservable()
            // 문 설치중에는 미리보기 중지.
            .Where(_ => !doorTargetPoint.activeSelf
             &&!isMoving
             && !EventSystem.current.IsPointerOverGameObject())
            .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
            .Subscribe(position =>
            {
                if (isDebugMode)
                {
                    Debug.Log("ListCount : " + wallList.FindAll(x => x.Direction == Model.Wall.WallDirection.Landscape ?
                 (x.StartPoint.x < position.x && position.x < x.EndPoint.x)
                 && (Mathf.Abs(position.z - x.StartPoint.z) <= standardLength)
                 : (x.StartPoint.z < position.z && position.z < x.EndPoint.z)
                 && (Mathf.Abs(position.x - x.StartPoint.x) <= standardLength)).Count);

                    Debug.Log("ListCount Between Wall: " + wallList.FindAll(x => x.Direction == Model.Wall.WallDirection.Landscape ?
                    (x.StartPoint.x < position.x && position.x < x.EndPoint.x)
                    : (x.StartPoint.z < position.z && position.z < x.EndPoint.z)
                    ).Count);

                    Debug.Log("ListCount Length: " + wallList.FindAll(x => x.Direction == Model.Wall.WallDirection.Landscape ?
                  ((position.z - x.WallObject.transform.position.z) < standardLength) :
                 ((position.x - x.WallObject.transform.position.x) < standardLength)).Count);


                    Debug.Log(position.z - wallList[0].WallObject.transform.position.z);
                    Debug.Log(position.z - wallList[1].WallObject.transform.position.z);

                }

                // 마우스 기준 상하좌우에 걸치는 벽, 그리고 일정 범위 내에 있는 벽을 충족하는 벽 리스트.
                adjacencyWallList = wallList.FindAll(x => x.Direction == Model.Wall.WallDirection.Landscape ?
                 (x.StartPoint.x < position.x && position.x < x.EndPoint.x)
                 && (Mathf.Abs(position.z - x.StartPoint.z) <= standardLength)
                 : (x.StartPoint.z < position.z && position.z < x.EndPoint.z)
                 && (Mathf.Abs(position.x - x.StartPoint.x) <= standardLength)).OrderBy(x => (Mathf.Abs(position.z - x.StartPoint.z))).ToList();


                if (adjacencyWallList.Count != 0)
                {
                    foreach (Model.Wall wall in adjacencyWallList)
                    {
                        if (IsApplicable(wall, position))
                        {
                            previewAttachedWall = wall;
                            previewIcon.SetActive(true);
                            previewIcon.transform.rotation = wall.Direction == Model.Wall.WallDirection.Landscape ?
                            Quaternion.Euler(0, 0, 0)
                            : Quaternion.Euler(0, 90, 0);

                            previewIconParent.transform.position = wall.Direction == Model.Wall.WallDirection.Landscape ?
                                new Vector3(position.x, 2f, wall.WallObject.transform.position.z)
                            : new Vector3(wall.WallObject.transform.position.x, 2f, position.z);

                            previewIcon.transform.localScale = GameObject.Find("doorPhase") != null ?
                                previewIcon.name.Contains("Swing") ?
                                    new Vector3(doorDefaultLength, previewIcon.transform.localScale.y, doorDefaultLength)
                                    : new Vector3(doorDefaultLength, previewIcon.transform.localScale.y, previewIcon.transform.localScale.z)
                                : new Vector3(windowDefaultLength, previewIcon.transform.localScale.y, previewIcon.transform.localScale.z);

                            break;
                        }
                        else
                        {
                            previewIcon.SetActive(false);
                        }
                    }
                }
                else
                {
                    previewIcon.SetActive(false);
                }
            });
    }

    /// <summary>
    /// 벽의 마우스 위치에 요소를 생성 가능한지 여부를 반환해주는 함수.
    /// </summary>
    /// <param name="wall">선택된 벽</param>
    /// <param name="position">선택된 벽에서의 마우스 위치</param>
    /// <returns></returns>
    private bool IsApplicable(Model.Wall wall, Vector3 position)
    {
        float comparisonLength = GameObject.Find("doorPhase") != null ? doorDefaultLength : windowDefaultLength;

        if (wall.Direction == Model.Wall.WallDirection.Vertical)
        {
            #region 생선 전 검사 - 세로

            #region 클릭한 좌표와 겹치는 문이 있는지 상태 검사
            List<Model.Door> foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                    x.DoorObject.transform.position.x == wall.WallObject.transform.position.x);


            for (int i = 0; i < foundedDoorList.Count; i++)
            {
                // 위쪽이 StartPoint일 때.
                if (foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                || foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                {
                    if ((position.z + comparisonLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                        && position.z + comparisonLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                    || (position.z - comparisonLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                        && position.z - comparisonLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                    {
                        return false;
                    }
                }
                // 아래쪽이 StartPoint일 때.
                else
                {
                    if ((position.z + comparisonLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                        && position.z + comparisonLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                    || (position.z - comparisonLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                        && position.z - comparisonLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                    {
                        return false;
                    }
                }
            }
            #endregion

            #region 클릭한 좌표와 겹치는 창문이 있는지 상태 검사
            List<Model.Window> foundedWindowList = model.WindowList.FindAll(x =>
                    x.WindowObject.transform.position.x == wall.WallObject.transform.position.x);

            for (int i = 0; i < foundedWindowList.Count; i++)
            {
                // 위쪽이 StartPoint일 때.
                if (foundedWindowList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                || foundedWindowList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                {
                    if ((position.z + comparisonLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(1).position.z
                        && position.z + comparisonLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(2).position.z)
                    || (position.z - comparisonLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(1).position.z
                        && position.z - comparisonLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(2).position.z))
                    {
                        return false;
                    }
                }
                // 아래쪽이 StartPoint일 때.
                else
                {
                    if ((position.z + comparisonLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(1).position.z
                        && position.z + comparisonLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(2).position.z)
                    || (position.z - comparisonLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(1).position.z
                        && position.z - comparisonLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(2).position.z))
                    {
                        return false;
                    }
                }
            }
            #endregion

            #region 문의 양옆이 벽에 포함된 상태가 맞는지 검사

            // 아이콘 종류에 따라서 비교 대상이 달라져야함.
            if (position.z + comparisonLength / 2 > wall.EndPoint.z - doorPadding || position.z - comparisonLength / 2 < wall.StartPoint.z + doorPadding)
            {
                return false;
            }

            #endregion

            #region 문 영역에 수직방향 벽이 존재하지 않는지 검사
            // TODO : 문의 시작점과 끝점 사이에 다른 벽의 시작점 혹은 끝점이 있는지 검사 필요.
            // 방과 방 사이의 거리가 벌려질 것이기 때문에 해당 로직 구현 후에 맞춤 개발 해야할 듯.

            Transform points = GameObject.Find("Points").transform;
            if (isDebugMode)
            {
                Debug.Log("childCount : " + points.childCount);
            }
            for (int i = 0; i < points.childCount; i++)
            {
                if (isDebugMode)
                {
                    Debug.Log("points.GetChild(i).position.x : " + points.GetChild(i).position.x + "\n" +
                        " wall.EndPoint.x : " + (wall.EndPoint.x) + "\n");
                }
                if (Util.IsEquals(points.GetChild(i).position.x, wall.EndPoint.x))
                {
                    if (isDebugMode)
                    {
                        Debug.Log("positon z+ : " + (position.z + comparisonLength / 2) + "\n" +
                        "cross z- : " + (points.GetChild(i).position.z - 0.1f) + "\n" +
                        "positon z- : " + (position.z - comparisonLength / 2) + "\n" +
                        "cross z+ : " + (points.GetChild(i).position.z + 0.1f) + "\n");
                    }
                    if (position.z + comparisonLength / 2 > points.GetChild(i).position.z - 0.1f
                    && position.z - comparisonLength / 2 < points.GetChild(i).position.z + 0.1f)
                    {

                        return false;
                    }
                }
            }
            #endregion

            #endregion

            return true;
        }
        else
        {
            #region 생선 전 검사 - 가로

            #region 클릭한 좌표와 겹치는 문이 있는지 상태 검사
            List<Model.Door> foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                    x.DoorObject.transform.position.z == wall.WallObject.transform.position.z);


            for (int i = 0; i < foundedDoorList.Count; i++)
            {
                // 위쪽이 StartPoint일 때.
                if (foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 180, 0)
                || foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 0, 180))
                {
                    if ((position.x + comparisonLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                        && position.x + comparisonLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                    || (position.x - comparisonLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                        && position.x - comparisonLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                    {
                        return false;
                    }
                }
                // 아래쪽이 StartPoint일 때.
                else
                {
                    if ((position.x + comparisonLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                        && position.x + comparisonLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                    || (position.x - comparisonLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                        && position.x - comparisonLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                    {
                        return false;
                    }
                }
            }
            #endregion

            #region 클릭한 좌표와 겹치는 창문이 있는지 상태 검사
            List<Model.Window> foundedWindowList = model.WindowList.FindAll(z =>
                    z.WindowObject.transform.position.z == wall.WallObject.transform.position.z);

            for (int i = 0; i < foundedWindowList.Count; i++)
            {
                // 위쪽이 StartPoint일 때.
                if (foundedWindowList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                || foundedWindowList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                {
                    if ((position.x + comparisonLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(1).position.x
                        && position.x + comparisonLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(2).position.x)
                    || (position.x - comparisonLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(1).position.x
                        && position.x - comparisonLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(2).position.x))
                    {
                        return false;
                    }
                }
                // 아래쪽이 StartPoint일 때.
                else
                {
                    if ((position.x + comparisonLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(1).position.x
                        && position.x + comparisonLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(2).position.x)
                    || (position.x - comparisonLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(1).position.x
                        && position.x - comparisonLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(2).position.x))
                    {
                        return false;
                    }
                }
            }
            #endregion

            #region 문의 양옆이 벽에 포함된 상태가 맞는지 검사

            // 아이콘 종류에 따라서 비교 대상이 달라져야함.
            if (position.x + comparisonLength / 2 > wall.EndPoint.x - doorPadding || position.x - comparisonLength / 2 < wall.StartPoint.x + doorPadding)
            {
                return false;
            }

            #endregion

            #region 문 영역에 수직방향 벽이 존재하지 않는지 검사
            // TODO : 문의 시작점과 끝점 사이에 다른 벽의 시작점 혹은 끝점이 있는지 검사 필요.
            // 방과 방 사이의 거리가 벌려질 것이기 때문에 해당 로직 구현 후에 맞춤 개발 해야할 듯.

            Transform points = GameObject.Find("Points").transform;
            if (isDebugMode)
            {
                Debug.Log("childCount : " + points.childCount);
            }
            for (int i = 0; i < points.childCount; i++)
            {
                if (isDebugMode)
                {
                    Debug.Log("points.GetChild(i).position.z : " + points.GetChild(i).position.z + "\n" +
                        " wall.EndPoint.z : " + (wall.EndPoint.z) + "\n");
                }
                if (Util.IsEquals(points.GetChild(i).position.z, wall.EndPoint.z))
                {
                    if (isDebugMode)
                    {
                        Debug.Log("positon x+ : " + (position.x + comparisonLength / 2) + "\n" +
                        "cross x- : " + (points.GetChild(i).position.x - 0.1f) + "\n" +
                        "positon x- : " + (position.x - comparisonLength / 2) + "\n" +
                        "cross x+ : " + (points.GetChild(i).position.x + 0.1f) + "\n");
                    }
                    if (position.x + comparisonLength / 2 > points.GetChild(i).position.x - 0.1f
                    && position.x - comparisonLength / 2 < points.GetChild(i).position.x + 0.1f)
                    {

                        return false;
                    }
                }
            }
            #endregion

            #endregion

            return true;
        }
        
    }

    #endregion
}

