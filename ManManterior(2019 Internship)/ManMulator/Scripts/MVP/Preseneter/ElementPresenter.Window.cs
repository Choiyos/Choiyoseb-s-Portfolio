using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public partial class ElementPresenter : MonoBehaviour
{

    #region Fields

    Transform windowTransform;
    [SerializeField]
    Button windowRemoveButton = default, windowMoveButton = default;
    GameObject windowPhase;
    

    #region 창문 종류 버튼
    [SerializeField]
    Button defaultWindow = default, bigWindow = default;
    #endregion

    #region Window Detail Setting
    [SerializeField]
    private float wallHeight = 2f;
    // 창문의 높이 배율.(높이가 곱해질 1M단위의 상수,  (창문 Scale.y = 0.55) == 1M)
    const float windowOriginHeight = 0.5f;
    private const float windowPadding = 0.05f;
    [SerializeField]
    Button windowDetailButton = default;
    [SerializeField]
    InputField paddingInputField = default, heightInputField = default;
    [SerializeField]
    Button editWindowSizeCloseButton = default, editWindowSizeApplyButton = default;
    [SerializeField]
    GameObject windowObject = default;
    [SerializeField]
    GameObject editWindowSizeUI = default;
    #endregion

    [SerializeField]
    GameObject loadingPanel = default;

    #endregion

    #region Methods

    private void WindowInit()
    {
        if (GameObject.Find("windowPhase") == null) windowPhase = new GameObject("windowPhase");
        else windowPhase = GameObject.Find("windowPhase");
        BindWindowWalls(model.WallList);
        defaultWindow.GetComponent<Image>().color = Color.cyan;
        bigWindow.GetComponent<Image>().color = Color.white;
        model.WindowIcon = model.DefaultWindowIconPrefab;
        if (model.WindowList.Count == 0)
        {
            windowTransform = GameObject.Find("Windows").transform;
        }
        else
        {
            BindExistWindow();
        }
        
        
    }

    private void BindExistWindow()
    {
        for (int i = 0; i < model.WindowList.Count; i++)
        {
            BindWindow(model.WindowList[i]);
        }
    }

    void BindWindow(Model.Window window)
    {
        #region 창문 Select 구독
        window.WindowObject.transform.GetChild(0).OnMouseDownAsObservable()
        .Where(x_ => isClickable && !isMoving)
        .Subscribe(_window =>
        {
            CancelSelect();
            Select(window.WindowObject);
            labels.transform.GetChild(0).gameObject.SetActive(true);
            labels.transform.GetChild(1).gameObject.SetActive(true);
            labels.transform.GetChild(2).gameObject.SetActive(true);
            FlotWindowLabel(window.WindowObject, window.WindowAttachedWall);
            WindowButtonSetactive(true);
        })
        .AddTo(windowPhase);
        #endregion
    }

    private void BindWindowButtons()
    {
        windowRemoveButton.onClick.AsObservable()
        .Subscribe(_ =>
        {
            RemoveWindow(model.SelectedElement);
            CancelSelect();
        });

        windowMoveButton.onClick.AsObservable()
        .Subscribe(_ =>
        {
            WindowButtonSetactive(false);
            Model.Wall attachWall = model.WindowList.Find(y => y.WindowObject == model.SelectedElement).WindowAttachedWall;

            var overlapNMouseDown = model.SelectedElement.transform.GetChild(0)
            .OnMouseDownAsObservable().Merge(backgroundImage.OnMouseDownAsObservable())
            .Where(flag => isClickable);

            labels.transform.GetChild(0).gameObject.SetActive(true);
            labels.transform.GetChild(1).gameObject.SetActive(true);
            labels.transform.GetChild(2).gameObject.SetActive(true);
            isMoving = true;
            //마우스 커서 중심으로 벽의 양 끝 Clamp
            model.SelectedElement.UpdateAsObservable()
            .TakeUntil(overlapNMouseDown).DoOnCompleted(() => isMoving = false)
            .Select(x => Camera.main.ScreenToWorldPoint(Input.mousePosition))
            .Subscribe(x =>
            {
                // 1+0.1 >> 창문 너비/2 + 벽 두께.
                // 3항연산자, 가로 벽이면 x축 Clamp, 세로 벽이면 z축 Clamp.


                model.SelectedElement.transform.position = 
                Mathf.Abs(attachWall.WallObject.transform.localScale.y - 0.1f) < 0.01f
                ?
                new Vector3(
                    Mathf.Clamp(
                        Util.GetGridValue (x.x)
                        , attachWall.StartPoint.x + ((model.SelectedElement != null) ? model.SelectedElement.transform.localScale.x / 2 : 1f) + windowPadding
                        , attachWall.EndPoint.x - ((model.SelectedElement != null) ? model.SelectedElement.transform.localScale.x / 2 : 1f) - windowPadding)
                    , 1f
                    , attachWall.StartPoint.z
                    )
                :
                new Vector3(
                        attachWall.StartPoint.x
                    , 1f
                    , Mathf.Clamp(
                        Util.GetGridValue(x.z)
                        , attachWall.StartPoint.z + ((model.SelectedElement != null) ? model.SelectedElement.transform.localScale.x / 2 : 1f) + windowPadding
                        , attachWall.EndPoint.z - ((model.SelectedElement != null) ? model.SelectedElement.transform.localScale.x / 2 : 1f) - windowPadding)
                );
                // 다른 요소와 겹칠때 생성 불가능.

                #region 콜라이더 겹침
                IDisposable onEnter = model.SelectedElement.transform.GetChild(0).OnTriggerStayAsObservable()
                .Select(collision => collision)
                .Where(collision => collision.name == "Window" || collision.name == "Door" || collision.tag == "CorssPoint")
                .TakeUntil(overlapNMouseDown)
                .Subscribe(a =>
                {
                    model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(200, 0, 0, 0.7f);
                    isClickable = false;
                }).AddTo(model.SelectedElement);
                IDisposable onExit = model.SelectedElement.transform.GetChild(0).OnTriggerExitAsObservable()
                .Select(collision => collision)
                .Where(collision => collision.name == "Window" || collision.name == "Door" || collision.tag == "CorssPoint")
                .TakeUntil(overlapNMouseDown)
                .Subscribe(a =>
                {
                    model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.7f);
                    isClickable = true;
                }).AddTo(model.SelectedElement);
                #endregion

                FlotWindowLabel(model.SelectedElement, attachWall);
            });
        });

        windowDetailButton.onClick.AsObservable()
            .Subscribe(_ => {
                Transform tempWindowCollider = model.WindowList.Find(x => x.WindowObject == model.SelectedElement).WindowObject.transform.GetChild(3);

                windowObject.transform.localScale = new Vector3(windowObject.transform.localScale.x
                    , tempWindowCollider.localScale.y*windowOriginHeight
                    , windowObject.transform.localScale.z * model.SelectedElement.transform.localScale.x);
                editWindowSizeUI.SetActive(true);

                windowObject.transform.position = new Vector3(windowObject.transform.position.x
                , tempWindowCollider.position.y-0.5f
                    , windowObject.transform.position.z);

                paddingInputField.text = Util.GetGridValue((tempWindowCollider.position.y - 0.5f - (0.5f * (tempWindowCollider.localScale.y - 1)))).ToString();
                heightInputField.text = (Util.GetGridValue(tempWindowCollider.localScale.y)).ToString();

            });

        editWindowSizeCloseButton.onClick.AsObservable()
            .Subscribe(_=> {
                editWindowSizeUI.SetActive(false);
            });

        editWindowSizeUI.OnDisableAsObservable()
            .Subscribe(_ => {

                paddingInputField.text = 1.ToString();
                heightInputField.text = 1.ToString();

                windowObject.transform.localScale = new Vector3(windowObject.transform.localScale.x
                    , windowOriginHeight
                    ,1);
                windowObject.transform.position = new Vector3(windowObject.transform.position.x
                    , 1
                    , windowObject.transform.position.z);
            });

        editWindowSizeApplyButton.onClick.AsObservable()
            .Subscribe(_ => {

                float windowPadding = float.Parse(paddingInputField.text);
                float windowHeight = float.Parse(heightInputField.text);
                float clamppedHeight = Util.GetGridValue(windowOriginHeight * Mathf.Clamp(windowHeight, 0.1f, wallHeight));
                float clamppedPadding =Util.GetGridValue( Mathf.Clamp(windowPadding, -0.5f, (wallHeight - clamppedHeight / windowOriginHeight)<0.01?0: (wallHeight - clamppedHeight / windowOriginHeight)) );
                float additionalPadding = Util.GetGridValue((0.5f * (Mathf.Clamp(clamppedHeight / windowOriginHeight - 1,-99, clamppedHeight / windowOriginHeight - 1))));

                if (isDebugMode)
                {
                    Debug.Log("windowPadding : " + windowPadding);
                    Debug.Log("windowHeight : " + windowHeight);
                    Debug.Log("clamppedHeight : " + clamppedHeight);
                    Debug.Log("clamppedPadding : " + clamppedPadding);
                    Debug.Log("heightInputField : " + clamppedHeight / windowOriginHeight);
                }
                paddingInputField.text = clamppedPadding.ToString();
                heightInputField.text =(Util.GetGridValue(clamppedHeight / windowOriginHeight)).ToString();

                windowObject.transform.localScale = new Vector3(windowObject.transform.localScale.x
                    , clamppedHeight
                        , windowObject.transform.localScale.z);
                windowObject.transform.position = new Vector3(windowObject.transform.position.x
                    , clamppedPadding + additionalPadding
                        , windowObject.transform.position.z);

                // 선택 요소에 정보 입력.
                Transform tempWindowCollider = model.WindowList.Find(x => x.WindowObject == model.SelectedElement).WindowObject.transform.GetChild(3);
                tempWindowCollider.localScale = new Vector3(tempWindowCollider.localScale.x
                    , clamppedHeight / windowOriginHeight
                        , tempWindowCollider.localScale.z);
                tempWindowCollider.position = new Vector3(tempWindowCollider.position.x
                    , clamppedPadding + additionalPadding + 0.5f
                            , tempWindowCollider.position.z);
            });


        defaultWindow.onClick.AsObservable()
            .Subscribe(_ =>
            {
                defaultWindow.GetComponent<Image>().color = Color.cyan;
                bigWindow.GetComponent<Image>().color = Color.white;

                model.WindowIcon = model.DefaultWindowIconPrefab;

                ChangePreviewIcon(model.DefaultWindowIconPrefab);

            });


        bigWindow.onClick.AsObservable()
            .Subscribe(_ =>
            {
                defaultWindow.GetComponent<Image>().color = Color.white;
                bigWindow.GetComponent<Image>().color = Color.cyan;
                model.WindowIcon = model.BigWindowIconPrefab;

                ChangePreviewIcon(model.BigWindowIconPrefab);


            });
    }


    private void BindWindowWalls(List<Model.Wall> walls)
    {
        foreach (Model.Wall wall in walls)
        {
            wall.WallObject.OnMouseDownAsObservable()
                .Where(_ => !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
                .Subscribe(position =>
                {
                    GenerateWindow(position, wall);
                })
                .AddTo(windowPhase);

        }
    }

    private void GenerateWindow(Vector3 position, Model.Wall wall)
    {
        switch (wall.Direction)
        {
            #region 세로
            case Model.Wall.WallDirection.Vertical:

                #region 생선 전 검사

                #region 클릭한 좌표와 겹치는 창문이 있는지 상태 검사
                List<Model.Window> foundedList = model.WindowList.FindAll(x =>
                        x.WindowObject.transform.position.x == wall.WallObject.transform.position.x);

                for (int i = 0; i < foundedList.Count; i++)
                {
                    // 위쪽이 StartPoint일 때.
                    if (foundedList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                    || foundedList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                    {
                        if ((position.z + windowDefaultLength/2 <= foundedList[i].WindowObject.transform.GetChild(1).position.z
                            && position.z + windowDefaultLength/2 >= foundedList[i].WindowObject.transform.GetChild(2).position.z)
                        || (position.z - windowDefaultLength/2 <= foundedList[i].WindowObject.transform.GetChild(1).position.z
                            && position.z - windowDefaultLength/2 >= foundedList[i].WindowObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                    // 아래쪽이 StartPoint일 때.
                    else
                    {
                        if ((position.z + windowDefaultLength/2 >= foundedList[i].WindowObject.transform.GetChild(1).position.z
                            && position.z + windowDefaultLength/2 <= foundedList[i].WindowObject.transform.GetChild(2).position.z)
                        || (position.z - windowDefaultLength/2 >= foundedList[i].WindowObject.transform.GetChild(1).position.z
                            && position.z - windowDefaultLength/2 <= foundedList[i].WindowObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                }
                #endregion

                #region 클릭한 좌표와 겹치는 문이 있는지 상태 검사
                // 생성 위치 주변에 문이 있는지 검사.
                List<Model.Door> foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                x.DoorObject.transform.position.x == wall.WallObject.transform.position.x);


                for (int i = 0; i < foundedDoorList.Count; i++)
                {
                    // 위쪽이 StartPoint일 때.
                    if (foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                    || foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                    {
                        if ((position.z + windowDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && position.z + windowDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                        || (position.z - windowDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && position.z - windowDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                    // 아래쪽이 StartPoint일 때.
                    else
                    {
                        if ((position.z + windowDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && position.z + windowDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                        || (position.z - windowDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && position.z - windowDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                }


                #endregion

                #region 문의 양옆이 벽에 포함된 상태가 맞는지 검사
                if (position.z + windowDefaultLength / 2 > wall.EndPoint.z - windowPadding || position.z - windowDefaultLength / 2 < wall.StartPoint.z + windowPadding)
                {
                    return;
                }

                #endregion

                #region 창문 영역에 수직방향 벽이 존재하지 않는지 검사
                // TODO : 문의 시작점과 끝점 사이에 다른 벽의 시작점 혹은 끝점이 있는지 검사 필요.
                // 방과 방 사이의 거리가 벌려질 것이기 때문에 해당 로직 구현 후에 맞춤 개발 해야할 듯.

                Transform points = GameObject.Find("Points").transform;
                for (int i = 0; i < points.childCount; i++)
                {
                    if (Util.IsEquals(wall.EndPoint.x, points.GetChild(i).position.x))
                        if (position.z + windowDefaultLength / 2 > points.GetChild(i).position.z- windowPadding
                        && position.z - windowDefaultLength / 2 < points.GetChild(i).position.z + windowPadding)
                        {
                            return;
                        }
                }


                #endregion

                #endregion

                #region 창문 생성과 마우스 위치에 따른 창문 방향 변경

                GameObject window = Instantiate(model.WindowIcon
                       , new Vector3(wall.WallObject.transform.position.x, 1f, Util.GetGridValue(position.z))
                       , Quaternion.Euler(0, 90, 0)
                       , windowTransform);
                window.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 1f);
                window.transform.localScale = new Vector3(windowDefaultLength, window.transform.localScale.y, windowDefaultLength);
                Model.Window tempWindow = new Model.Window(window, wall);

                #region 창문 양옆 수치표시
                labels.transform.GetChild(0).gameObject.SetActive(true);
                labels.transform.GetChild(1).gameObject.SetActive(true);
                labels.transform.GetChild(2).gameObject.SetActive(true); FlotWindowLabel(tempWindow.WindowObject, wall);
                #endregion
                

                window.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.black;
                model.WindowList.Add(tempWindow);
                

                CancelSelect();
                Select(window);
                labels.transform.GetChild(0).gameObject.SetActive(true);
                labels.transform.GetChild(1).gameObject.SetActive(true);
                labels.transform.GetChild(2).gameObject.SetActive(true); FlotWindowLabel(tempWindow.WindowObject, wall);
                WindowButtonSetactive(true);


                #region 창문 Select 구독
                BindWindow(tempWindow);
                #endregion

                #endregion

                break;
            #endregion

            #region 가로
            case Model.Wall.WallDirection.Landscape:

                #region 생성 전 검사

                #region 클릭한 좌표와 겹치는 창문이 있는지 상태 검사
                foundedList = model.WindowList.FindAll(x =>
                        x.WindowObject.transform.position.z == wall.WallObject.transform.position.z);

                for (int i = 0; i < foundedList.Count; i++)
                {
                    // 오른쪽이 StartPoint일 때.
                    if (foundedList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 180, 0)
                    || foundedList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 0, 180))
                    {
                        if ((position.x + windowDefaultLength/2 <= (foundedList[i].WindowObject.transform.GetChild(1).position.x- windowPadding)
                            && position.x + windowDefaultLength/2 >=( foundedList[i].WindowObject.transform.GetChild(2).position.x+ windowPadding))
                        || (position.x - windowDefaultLength/2 <= (foundedList[i].WindowObject.transform.GetChild(1).position.x- windowPadding)
                            && position.x - windowDefaultLength/2 >= (foundedList[i].WindowObject.transform.GetChild(2).position.x+ windowPadding)))
                        {
                            return;
                        }
                    }
                    // 왼쪽이 StartPoint일 때.
                    else
                    {
                        if ((position.x + windowDefaultLength/2 >= (foundedList[i].WindowObject.transform.GetChild(1).position.x - windowPadding)
                            && position.x + windowDefaultLength/2 <=( foundedList[i].WindowObject.transform.GetChild(2).position.x + windowPadding))
                        || (position.x - windowDefaultLength/2 >= (foundedList[i].WindowObject.transform.GetChild(1).position.x - windowPadding)
                            && position.x - windowDefaultLength/2 <= (foundedList[i].WindowObject.transform.GetChild(2).position.x + windowPadding)))
                        {
                            return;
                        }
                    }
                }
                #endregion

                #region 클릭한 좌표와 겹치는 문이 있는지 상태 검사
                foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                        x.DoorObject.transform.position.z == wall.WallObject.transform.position.z);

                for (int i = 0; i < foundedDoorList.Count; i++)
                {
                    // 오른쪽이 StartPoint일 때.
                    if (foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 180, 0)
                    || foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 0, 180))
                    {
                        if ((position.x + windowDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && position.x + windowDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                        || (position.x - windowDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && position.x - windowDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                        {
                            return;
                        }
                    }
                    // 왼쪽이 StartPoint일 때.
                    else
                    {
                        if ((position.x + windowDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && position.x + windowDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                        || (position.x - windowDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && position.x - windowDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                        {
                            return;
                        }
                    }
                }
                #endregion

                #region 문의 양옆이 벽에 포함된 상태가 맞는지 검사
                if (position.x +windowDefaultLength/2 > wall.EndPoint.x - windowPadding || position.x -windowDefaultLength/2 < wall.StartPoint.x + windowPadding)
                {
                    return;
                }

                #endregion

                #region 창문 영역에 수직방향 벽이 존재하지 않는지 검사
                // TODO : 문의 시작점과 끝점 사이에 다른 벽의 시작점 혹은 끝점이 있는지 검사 필요.
                // 방과 방 사이의 거리가 벌려질 것이기 때문에 해당 로직 구현 후에 맞춤 개발 해야할 듯.

                points = GameObject.Find("Points").transform;
                for (int i = 0; i < points.childCount; i++)
                {
                    if (Util.IsEquals(wall.EndPoint.z, points.GetChild(i).position.z))
                        if (position.x + windowDefaultLength / 2 > points.GetChild(i).position.x - windowPadding
                        && position.x - windowDefaultLength / 2 < points.GetChild(i).position.x + windowPadding)
                        {
                            return;
                        }
                }
                #endregion

                #endregion

                #region 창문 생성과 마우스 위치에 따른 창문 방향 변경

                window = Instantiate(model.WindowIcon
                        , new Vector3(Util.GetGridValue(position.x), 1f, wall.WallObject.transform.position.z)
                        , Quaternion.Euler(0, 0, 0)
                        , windowTransform);
                window.transform.localScale = new Vector3(windowDefaultLength, window.transform.localScale.y, windowDefaultLength);
                tempWindow = new Model.Window(window, wall);

                #region 창문 양옆 수치표시
                labels.transform.GetChild(0).gameObject.SetActive(true);
                labels.transform.GetChild(1).gameObject.SetActive(true);
                labels.transform.GetChild(2).gameObject.SetActive(true);
                FlotWindowLabel(tempWindow.WindowObject, wall);
                #endregion

                window.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.black;
                model.WindowList.Add(tempWindow);

                CancelSelect();
                Select(window);
                labels.transform.GetChild(0).gameObject.SetActive(true);
                labels.transform.GetChild(1).gameObject.SetActive(true);
                labels.transform.GetChild(2).gameObject.SetActive(true);
                FlotWindowLabel(tempWindow.WindowObject, wall);
                WindowButtonSetactive(true);

                #region 창문 Select 구독
                BindWindow(tempWindow);
                #endregion

                #endregion

                break;
            #endregion

            default:
                break;
        }
    }

    private void RemoveWindow(GameObject selectedElement)
    {
        model.WindowList.Remove(model.WindowList.Find(x => x.WindowObject == selectedElement));

        Destroy(selectedElement);
    }

    private void WindowButtonSetactive(bool _flag)
    {
        windowRemoveButton.gameObject.SetActive(_flag);
        windowMoveButton.gameObject.SetActive(_flag);
        if (_flag && model.SelectedElement.name == "DefaultWindowIcon(Clone)")
        {
            windowDetailButton.gameObject.SetActive(_flag);
        }
        else
        {
            windowDetailButton.gameObject.SetActive(false);
        }
            
        
    }

    void FlotWindowLabel(GameObject element, Model.Wall wall)
    {

        Vector3 windowLabelPadding = Vector3.zero;

        Vector3 label1_dot1 = wall.StartPoint;
        float windowScaleX = ((model.SelectedElement != null) ? model.SelectedElement.transform.localScale.x / 2 : 0.5f);
        Vector3 label1_dot2 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
                new Vector3(
                    element.transform.position.x - windowScaleX
                    , 0f
                    , element.transform.position.z)
                : new Vector3(element.transform.position.x, 0f, element.transform.position.z - windowScaleX);


        label1Text.text = Util.GetGridLabel(Vector3.Distance(new Vector3(label1_dot1.x, 0, label1_dot1.z), new Vector3(label1_dot2.x, 0, label1_dot2.z))) + "m";
        Vector3 padding = Vector3.zero;
        float angle = 0;
        if (label1_dot2.x - label1_dot1.x > 0) { angle = 0; padding = new Vector3(0, 0, 1); windowLabelPadding = new Vector3(0, 0, 0.75f);  }
        if (label1_dot2.x - label1_dot1.x < 0) { angle = 180; padding = new Vector3(0, 0, -1); windowLabelPadding = new Vector3(0, 0, 0.75f); }
        if (label1_dot2.z - label1_dot1.z > 0) { angle = 270; padding = new Vector3(-1, 0, 0); windowLabelPadding = new Vector3(-0.75f, 0, 0); }
        if (label1_dot2.z - label1_dot1.z < 0) { angle = 90; padding = new Vector3(1, 0, 0); windowLabelPadding = new Vector3(-0.75f, 0, 0);  }
        label1.transform.rotation = Quaternion.Euler(new Vector3(90, angle, 0));
        label1.transform.position = (label1_dot1 + label1_dot2) * 0.5f + padding * 0.5f + Vector3.up * 2;



        float length1 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.01f ?
    (element.transform.position.x - windowScaleX) - wall.StartPoint.x :
    (element.transform.position.z - windowScaleX) - wall.StartPoint.z;

        label1Line.transform.localScale = new Vector3(length1, 1, 1);
        //

        Vector3 label2_dot1 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
                new Vector3(
                    element.transform.position.x + windowScaleX
                    , 0f
                    , element.transform.position.z)
                : new Vector3(
                    element.transform.position.x
                    , 0f
                    , element.transform.position.z + windowScaleX);

        Vector3 label2_dot2 = wall.EndPoint;

        label2Text.text = Util.GetGridLabel(Vector3.Distance(new Vector3(label2_dot1.x, 0, label2_dot1.z), new Vector3(label2_dot2.x, 0, label2_dot2.z))) + "m";
        label2.transform.rotation = Quaternion.Euler(new Vector3(90, angle, 0));
        label2.transform.position = (label2_dot1 + label2_dot2) * 0.5f + padding * 0.5f + Vector3.up * 2;


        float length2 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
            (element.transform.position.x + windowScaleX) - wall.EndPoint.x :
            (element.transform.position.z + windowScaleX) - wall.EndPoint.z;


        label2Line.transform.localScale = new Vector3(length2, 1, 1);
        //
        Vector3 label3_dot1 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
        new Vector3(
            element.transform.position.x + windowScaleX
            , 0f
            , element.transform.position.z)
        : new Vector3(
            element.transform.position.x
            , 0f
            , element.transform.position.z + windowScaleX);

        Vector3 label3_dot2 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
                new Vector3(
                    element.transform.position.x - windowScaleX
                    , 0f
                    , element.transform.position.z)
                : new Vector3(
                    element.transform.position.x
                    , 0f
                    , element.transform.position.z - windowScaleX);

        label3Text.text = Util.GetGridLabel(Vector3.Distance(new Vector3(label3_dot1.x, 0, label3_dot1.z), new Vector3(label3_dot2.x, 0, label3_dot2.z))) + "m";
        label3.transform.rotation = Quaternion.Euler(new Vector3(90, angle, 0));
        label3.transform.position = (label3_dot1 + label3_dot2) * 0.5f + padding * 0.5f + Vector3.up * 2+ windowLabelPadding;


        float length3 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
            (element.transform.position.x + windowScaleX) - (element.transform.position.x - windowScaleX) :
            (element.transform.position.z + windowScaleX) - (element.transform.position.z - windowScaleX);


        label3Line.transform.localScale = new Vector3(length3, 1, 1);
    }

    private void UpdateWindow(float length)
    {
        if (model.SelectedElement == null) return;
        if (length < 0.001f) return;
        Vector3 scale = model.SelectedElement.transform.localScale;
        scale.x = Util.GetGridValue(length);

        #region 창문 수치변경시 벽 바깥으로 넘어갈 때.
        Model.Wall wall = model.WindowList.Find(y => y.WindowObject == model.SelectedElement).WindowAttachedWall;
        switch (wall.Direction)
        {
            case Model.Wall.WallDirection.Vertical:
                if (model.SelectedElement.transform.position.z + length / 2 > wall.EndPoint.z - windowPadding
                    || model.SelectedElement.transform.position.z - length / 2 < wall.StartPoint.z + windowPadding)
                {
                    label3Text.color = Color.red;
                    //label3Text = null;
                    return;
                }
                break;
            case Model.Wall.WallDirection.Landscape:
                if (model.SelectedElement.transform.position.x + length / 2 > wall.EndPoint.x - windowPadding
                    || model.SelectedElement.transform.position.x - length / 2 < wall.StartPoint.x + windowPadding)
                {
                    label3Text.color = Color.red;
                    //label3Text = null;
                    return;
                }
                break;
            default:
                break;

        }
        #endregion

        #region 창문 수치변경시 양옆에 창문이나 문 있는지 확인
        switch (wall.Direction)
        {
            case Model.Wall.WallDirection.Vertical:
                List<Model.Window> foundedList = model.WindowList.FindAll(x =>
                x.WindowObject.transform.position.x == wall.WallObject.transform.position.x
                &&x.WindowObject!=model.SelectedElement);

                for (int i = 0; i < foundedList.Count; i++)
                {
                    // 위쪽이 StartPoint일 때.
                    if (foundedList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                    || foundedList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                    {
                        if ((model.SelectedElement.transform.position.z + length / 2 <= foundedList[i].WindowObject.transform.GetChild(1).position.z
                            && model.SelectedElement.transform.position.z + length / 2 >= foundedList[i].WindowObject.transform.GetChild(2).position.z)
                        || (model.SelectedElement.transform.position.z - length / 2 <= foundedList[i].WindowObject.transform.GetChild(1).position.z
                            && model.SelectedElement.transform.position.z - length / 2 >= foundedList[i].WindowObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                    // 아래쪽이 StartPoint일 때.
                    else
                    {
                        if ((model.SelectedElement.transform.position.z + length / 2 >= foundedList[i].WindowObject.transform.GetChild(1).position.z
                            && model.SelectedElement.transform.position.z + length / 2 <= foundedList[i].WindowObject.transform.GetChild(2).position.z)
                        || (model.SelectedElement.transform.position.z - length / 2 >= foundedList[i].WindowObject.transform.GetChild(1).position.z
                            && model.SelectedElement.transform.position.z - length / 2 <= foundedList[i].WindowObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }

                    if ((model.SelectedElement.transform.position.z + length / 2 >= foundedList[i].WindowObject.transform.GetChild(1).position.z
                        && model.SelectedElement.transform.position.z + length / 2 >= foundedList[i].WindowObject.transform.GetChild(2).position.z)
                    && (model.SelectedElement.transform.position.z - length / 2 <= foundedList[i].WindowObject.transform.GetChild(1).position.z
                        && model.SelectedElement.transform.position.z - length / 2 <= foundedList[i].WindowObject.transform.GetChild(2).position.z))
                    {
                        return;
                    }
                }


                // 생성 위치 주변에 문이 있는지 검사.
                List<Model.Door> foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                x.DoorObject.transform.position.x == wall.WallObject.transform.position.x);


                for (int i = 0; i < foundedDoorList.Count; i++)
                {
                    // 위쪽이 StartPoint일 때.
                    if (foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                    || foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                    {
                        if ((model.SelectedElement.transform.position.z + length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && model.SelectedElement.transform.position.z + length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                        || (model.SelectedElement.transform.position.z - length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && model.SelectedElement.transform.position.z - length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                    // 아래쪽이 StartPoint일 때.
                    else
                    {
                        if ((model.SelectedElement.transform.position.z + length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && model.SelectedElement.transform.position.z + length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                        || (model.SelectedElement.transform.position.z - length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && model.SelectedElement.transform.position.z - length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }

                    if ((model.SelectedElement.transform.position.z + length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                        && model.SelectedElement.transform.position.z + length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                    && (model.SelectedElement.transform.position.z - length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                        && model.SelectedElement.transform.position.z - length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                    {
                        return;
                    }
                }
                break;
            case Model.Wall.WallDirection.Landscape:

                foundedList = model.WindowList.FindAll(x =>
                x.WindowObject.transform.position.z == wall.WallObject.transform.position.z
                && x.WindowObject != model.SelectedElement);

                for (int i = 0; i < foundedList.Count; i++)
                {
                    // 오른쪽이 StartPoint일 때.
                    if (foundedList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 180, 0)
                    || foundedList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 0, 180))
                    {
                        if ((model.SelectedElement.transform.position.x + length / 2 <= (foundedList[i].WindowObject.transform.GetChild(1).position.x - windowPadding)
                            && model.SelectedElement.transform.position.x + length  / 2 >= (foundedList[i].WindowObject.transform.GetChild(2).position.x + windowPadding))
                        || (model.SelectedElement.transform.position.x - length  / 2 <= (foundedList[i].WindowObject.transform.GetChild(1).position.x - windowPadding)
                            && model.SelectedElement.transform.position.x - length  / 2 >= (foundedList[i].WindowObject.transform.GetChild(2).position.x + windowPadding)))
                        {
                            return;
                        }
                    }
                    // 왼쪽이 StartPoint일 때.
                    else
                    {
                        if ((model.SelectedElement.transform.position.x + length  / 2 >= (foundedList[i].WindowObject.transform.GetChild(1).position.x - windowPadding)
                            && model.SelectedElement.transform.position.x + length  / 2 <= (foundedList[i].WindowObject.transform.GetChild(2).position.x + windowPadding))
                        || (model.SelectedElement.transform.position.x - length  / 2 >= (foundedList[i].WindowObject.transform.GetChild(1).position.x - windowPadding)
                            && model.SelectedElement.transform.position.x - length  / 2 <= (foundedList[i].WindowObject.transform.GetChild(2).position.x + windowPadding)))
                        {
                            return;
                        }
                    }
                    if ((model.SelectedElement.transform.position.x + length / 2 >= foundedList[i].WindowObject.transform.GetChild(1).position.x
                        && model.SelectedElement.transform.position.x + length / 2 >= foundedList[i].WindowObject.transform.GetChild(2).position.x)
                    && (model.SelectedElement.transform.position.x - length / 2 <= foundedList[i].WindowObject.transform.GetChild(1).position.x
                        && model.SelectedElement.transform.position.x - length / 2 <= foundedList[i].WindowObject.transform.GetChild(2).position.x))
                    {
                        return;
                    }
                }

                foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                        x.DoorObject.transform.position.z == wall.WallObject.transform.position.z);

                for (int i = 0; i < foundedDoorList.Count; i++)
                {
                    // 오른쪽이 StartPoint일 때.
                    if (foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 180, 0)
                    || foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 0, 180))
                    {
                        if ((model.SelectedElement.transform.position.x + length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && model.SelectedElement.transform.position.x + length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                        || (model.SelectedElement.transform.position.x - length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && model.SelectedElement.transform.position.x - length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                        {
                            return;
                        }
                    }
                    // 왼쪽이 StartPoint일 때.
                    else
                    {
                        if ((model.SelectedElement.transform.position.x + length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && model.SelectedElement.transform.position.x + length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                        || (model.SelectedElement.transform.position.x - length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && model.SelectedElement.transform.position.x - length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                        {
                            return;
                        }
                    }

                    if ((model.SelectedElement.transform.position.x + length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && model.SelectedElement.transform.position.x + length / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                        && (model.SelectedElement.transform.position.x - length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && model.SelectedElement.transform.position.x - length / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                    {
                        return;
                    }
                }
                break;
            default:
                break;
        }
        #endregion

        model.SelectedElement.transform.localScale = scale;
        FlotWindowLabel(model.SelectedElement,wall);
    }

    void Loading()
    {

        DontDestroyOnLoad(loadingPanel);
        loadingPanel.SetActive(true);
        Slider loadingBar = GameObject.Find("LoadingBar").GetComponent<Slider>();
        Text text = loadingPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        float loadingTime = 2;
        float animateCycle = 1f;
        float currentTime = 0;
        var update = new SingleAssignmentDisposable();
        string str = "로딩중";
        text.text = str;
        update.Disposable = loadingBar.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (currentTime > loadingTime)
                {
                    update.Dispose();
                    Destroy( loadingPanel);

                }
                else
                {
                    currentTime += Time.deltaTime;
                    loadingBar.value = currentTime / loadingTime;
                    if (currentTime - (currentTime / animateCycle) == 0)
                    {
                        //str += ".";
                        text.text = str;
                    }
                }

            });
    }

    #endregion
}
