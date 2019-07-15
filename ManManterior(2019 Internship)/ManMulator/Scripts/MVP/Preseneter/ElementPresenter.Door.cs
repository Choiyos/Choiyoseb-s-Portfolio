using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 요소설정 씬을 위한 프레젠터.
/// 문의 설치와 회전, 이동에 대한 부분을 담당하고있음.
/// </summary>
public partial class ElementPresenter : MonoBehaviour
{
    #region Fields
    // 문이 벽에서 떨어져야하는 최소 거리. 0으로 설정하면 벽의 모서리와 닿아서 설치 불가 판정이 난다.
    private const float doorPadding = 0.05f;
    Transform doorTransform;
    GameObject doorTargetPoint;

    [SerializeField]
    Button doorRotationButton = default, doorRemoveButton = default, doorMoveButton = default;
    
    [SerializeField]
    Button swingDoor = default, noDoor = default;
    GameObject doorPhase;
    #endregion

    #region Methods

    private void DoorInit()
    {
        doorTransform = GameObject.Find("Doors").transform;

        if (GameObject.Find("doorPhase") == null)
            doorPhase = new GameObject("doorPhase");
        else
            doorPhase = GameObject.Find("doorPhase");

        swingDoor.GetComponent<Image>().color = Color.cyan;
        noDoor.GetComponent<Image>().color = Color.white;
        model.DoorIcon = model.SwingDoorIconPrefab;
        if (doorTargetPoint == null)
        {
            doorTargetPoint = Instantiate(Model.Instance.DoorTargetPrefab);
            doorTargetPoint.SetActive(false);
        }
    }

    private void ReturnDoorPhase()
    {
        if (GameObject.Find("doorPhase") == null) doorPhase = new GameObject("doorPhase");
        else doorPhase = GameObject.Find("doorPhase");
        isClickable = true;
        swingDoor.GetComponent<Image>().color = Color.cyan;
        noDoor.GetComponent<Image>().color = Color.white;
        model.DoorIcon = model.SwingDoorIconPrefab;
        for (int i = 0; i < model.DoorList.Count; i++)
        {
            BindDoor(model.DoorList[i]);
        }
    }

    /// <summary>
    /// 기존에 존재하는 문들을 다시 바인드하기 위한 과정.
    /// </summary>
    /// <param name="door"></param>
    private void BindDoor(Model.Door door)
    {
        door.DoorObject.transform.GetChild(0).OnMouseDownAsObservable()
            .Where(x_ => isClickable && !isMoving)
            .Subscribe(x =>
            {
                CancelSelect();
                Select(door.DoorObject);
                labels.transform.GetChild(0).gameObject.SetActive(true);
                labels.transform.GetChild(1).gameObject.SetActive(true);
                labels.transform.GetChild(2).gameObject.SetActive(true);
                FlotDoorLabel(door.DoorObject, door.DoorAttachedWall);
                DoorButtonSetactive(true);

            })
            .AddTo(doorPhase);
    }

    private void BindDoorButtons()
    {
        doorRemoveButton.onClick.AsObservable()
        .Subscribe(_ =>
            {
                RemoveDoor(model.SelectedElement);
                CancelSelect();
            });

        doorMoveButton.onClick.AsObservable()
        .Subscribe(_ =>
        {
            DoorButtonSetactive(false);
            Model.Wall attachWall = model.DoorList.Find(y => y.DoorObject == model.SelectedElement).DoorAttachedWall;
            // 문을 다시 클릭하거나 배경 이미지를 클릭하는 것을 식별하는 이벤트.
            var overlapNMouseDown = model.SelectedElement.transform.GetChild(0)
            .OnMouseDownAsObservable().Merge(backgroundImage.OnMouseDownAsObservable())
            .Where(flag => isClickable);

            labels.transform.GetChild(0).gameObject.SetActive(true);
            labels.transform.GetChild(1).gameObject.SetActive(true);
            labels.transform.GetChild(2).gameObject.SetActive(true);
            isMoving = true;
            //마우스 커서 중심으로 벽의 양 끝 Clamp
            model.SelectedElement.UpdateAsObservable()
            // 해당 이벤트가 발생하면 flag 값을 조정하고 구독을 취소.
            .TakeUntil(overlapNMouseDown).DoOnCompleted(() => isMoving = false)
            // 마우스 위치를 좌표로 받아옴.
            .Select(x => Camera.main.ScreenToWorldPoint(Input.mousePosition))
            .Subscribe(x =>
            {
                // 0.5+0.1 >> 문 너비/2 + 벽 두께.
                // 3항연산자, 가로 벽이면 x축 Clamp, 세로 벽이면 z축 Clamp.

                model.SelectedElement.transform.position =
                Mathf.Abs(attachWall.WallObject.transform.localScale.y - 0.1f) < 0.01f
                ?
                new Vector3(
                    Mathf.Clamp(
                        Util.GetGridValue(x.x)
                        , attachWall.StartPoint.x + model.SelectedElement.transform.localScale.x / 2 +  doorPadding 
                        , attachWall.EndPoint.x - model.SelectedElement.transform.localScale.x / 2 - doorPadding )
                    , 1f
                    , attachWall.StartPoint.z
                    )
                :
                new Vector3(
                        attachWall.StartPoint.x
                    , 1f
                    , Mathf.Clamp(
                        Util.GetGridValue(x.z)
                        , attachWall.StartPoint.z + model.SelectedElement.transform.localScale.x / 2 + doorPadding 
                        , attachWall.EndPoint.z - model.SelectedElement.transform.localScale.x / 2 -  doorPadding) 
                );

                #region 콜라이더 겹침
                // 다른문과 겹칠때 생성 불가능.
                IDisposable onEnter = model.SelectedElement.transform.GetChild(0).OnTriggerStayAsObservable()
                .Select(collision => collision)
                .Where(collision => collision.name == "Door" || collision.name == "Window" || collision.tag == "CorssPoint")
                .TakeUntil(overlapNMouseDown)
                .Subscribe(a =>
                {
                    model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(200, 0, 0, 0.7f);
                    isClickable = false;
                }).AddTo(model.SelectedElement);
                IDisposable onExit = model.SelectedElement.transform.GetChild(0).OnTriggerExitAsObservable()
                .Select(collision => collision)
                .Where(collision => collision.name == "Door" || collision.name == "Window" || collision.tag == "CorssPoint")
                .TakeUntil(overlapNMouseDown)
                .Subscribe(a =>
                {
                    model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color =model.SelectedElement.name== "NoDoorIcon(Clone)"? new Color(1, 1, 1, 0.7f) : new Color(0, 0, 0, 0.7f);
                    isClickable = true;
                }).AddTo(model.SelectedElement);
                #endregion

                // 지속적으로 수치표시 갱신.
                FlotDoorLabel(model.SelectedElement, attachWall);
            });
        });

        doorRotationButton.onClick.AsObservable()
        .Subscribe(x =>
            {
                DoorButtonSetactive(false);
                Model.Wall attachWall = model.DoorList.Find(y => y.DoorObject == model.SelectedElement).DoorAttachedWall;
                // 회전 중 취소했을 때를 위한 원래의 회전 값 저장.
                Quaternion originRotation = model.SelectedElement.transform.localRotation;
                isClickable = false;
                bool isCollision = true;
                // 삭제-생성이 아닌 기존 오브젝트 수정.
                switch (attachWall.Direction)
                {
                    case Model.Wall.WallDirection.Vertical:


                        #region 문 콜라이더 충돌 검사
                        isCollision = true;
                        IDisposable onEnter = model.SelectedElement.transform.GetChild(0).OnTriggerStayAsObservable()
                        .Select(collision => collision)
                        .Where(collision => collision.name == "Door" || collision.tag == "CorssPoint")
                        .Subscribe(_ =>
                        {
                            model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(200, 0, 0, 0.7f);
                            isCollision = false;
                        })
                        .AddTo(model.SelectedElement);
                        IDisposable onExit = model.SelectedElement.transform.GetChild(0).OnTriggerExitAsObservable()
                        .Select(collision => collision)
                        .Where(collision => collision.name == "Door" || collision.tag == "CorssPoint")
                        .Subscribe(_ =>
                        {
                            model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.7f);
                            isCollision = true;
                        })
                        .AddTo(model.SelectedElement);

                        #endregion

                        doorTargetPoint.transform.position = new Vector3(model.SelectedElement.transform.position.x, 5f, model.SelectedElement.transform.position.z);
                        doorTargetPoint.SetActive(true);

                        var verticalTarget = new SingleAssignmentDisposable();
                        verticalTarget.Disposable =
                        doorTargetPoint.OnMouseOverAsObservable()
                        .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
                        .Subscribe(doorTaretPosition =>
                        {

                            if (doorTaretPosition.x > doorTargetPoint.transform.position.x
                            && doorTaretPosition.z > doorTargetPoint.transform.position.z)
                            {
                                // 오른쪽 위.
                                model.SelectedElement.transform.localRotation = Quaternion.Euler(0, 90, 0);
                            }
                            else if (doorTaretPosition.x > doorTargetPoint.transform.position.x
                            && doorTaretPosition.z < doorTargetPoint.transform.position.z)
                            {
                                // 오른쪽 아래.
                                model.SelectedElement.transform.localRotation = Quaternion.Euler(0, 90, 180);
                            }
                            else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
                           && doorTaretPosition.z > doorTargetPoint.transform.position.z)
                            {
                                // 왼쪽 위.
                                model.SelectedElement.transform.localRotation = Quaternion.Euler(0, -90, 180);
                            }
                            else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
                           && doorTaretPosition.z < doorTargetPoint.transform.position.z)
                            {
                                // 왼쪽 아래.
                                model.SelectedElement.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            }
                        })
                        .AddTo(model.SelectedElement);

                        var doorClick = new SingleAssignmentDisposable();
                        doorClick.Disposable = doorTargetPoint.OnMouseDownAsObservable()
                        .Where(_ => isCollision)
                        .First()
                        .Subscribe(_ =>
                        {
                            model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.black;
                            doorTargetPoint.SetActive(false);
                            verticalTarget.Dispose();
                            doorClick.Dispose();
                            onEnter.Dispose();
                            onExit.Dispose();
                            labels.transform.GetChild(0).gameObject.SetActive(true);
                            labels.transform.GetChild(1).gameObject.SetActive(true);
                            labels.transform.GetChild(2).gameObject.SetActive(true);
                            DoorButtonSetactive(true);
                            isClickable = true;

                            return;

                        }).AddTo(model.SelectedElement);

                        // 문 생성중 취소. > 문 회전중에는 회전 누르기 전의 상태로 돌아가야함.
                        this.UpdateAsObservable()
                        .TakeWhile(_ => !doorClick.IsDisposed)
                        .Where(_ => Input.GetKey(KeyCode.Escape))
                        .Subscribe(_ =>
                        {
                            doorTargetPoint.SetActive(false);
                            labels.transform.GetChild(0).gameObject.SetActive(false);
                            labels.transform.GetChild(1).gameObject.SetActive(false);
                            labels.transform.GetChild(2).gameObject.SetActive(false);
                            model.SelectedElement.transform.localRotation = originRotation;
                            isClickable = true;
                            isCollision = true;
                            verticalTarget.Dispose();
                            doorClick.Dispose();
                            onEnter.Dispose();
                            onExit.Dispose();
                            CancelSelect();
                            return;
                        }).AddTo(model.SelectedElement);
                        break;

                    case Model.Wall.WallDirection.Landscape:

                        doorTargetPoint.transform.position = new Vector3(model.SelectedElement.transform.position.x, 5f, model.SelectedElement.transform.position.z);
                        doorTargetPoint.SetActive(true);


                        #region 문 콜라이더 충돌 검사
                        isCollision = true;
                        onEnter = model.SelectedElement.transform.GetChild(0).OnTriggerStayAsObservable()
                        .Select(collision => collision)
                        .Where(collision => collision.name == "Door" || collision.tag == "CorssPoint")
                        .Subscribe(_ =>
                        {
                            model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(200, 0, 0, 0.7f);
                            isCollision = false;
                        }).AddTo(model.SelectedElement);
                        onExit = model.SelectedElement.transform.GetChild(0).OnTriggerExitAsObservable()
                        .Select(collision => collision)
                        .Where(collision => collision.name == "Door" || collision.tag == "CorssPoint")
                        .Subscribe(_ =>
                        {
                            model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.7f);
                            isCollision = true;
                        }).AddTo(model.SelectedElement);
                        #endregion

                        var landscapeTarget = new SingleAssignmentDisposable();
                        landscapeTarget.Disposable =
                        doorTargetPoint.OnMouseOverAsObservable()
                        .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
                            .Subscribe(doorTaretPosition =>
                            {
                                if (doorTaretPosition.x > doorTargetPoint.transform.position.x
                                && doorTaretPosition.z > doorTargetPoint.transform.position.z)
                                {
                                    // 오른쪽 위.
                                    model.SelectedElement.transform.localRotation = Quaternion.Euler(0, 0, 180);
                                }
                                else if (doorTaretPosition.x > doorTargetPoint.transform.position.x
                                && doorTaretPosition.z < doorTargetPoint.transform.position.z)
                                {
                                    // 오른쪽 아래.
                                    model.SelectedElement.transform.localRotation = Quaternion.Euler(0, 180, 0);
                                }
                                else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
                               && doorTaretPosition.z > doorTargetPoint.transform.position.z)
                                {
                                    // 왼쪽 위.
                                    model.SelectedElement.transform.localRotation = Quaternion.Euler(0, 0, 0);
                                }
                                else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
                               && doorTaretPosition.z < doorTargetPoint.transform.position.z)
                                {
                                    // 왼쪽 아래.
                                    model.SelectedElement.transform.localRotation = Quaternion.Euler(0, 180, 180);
                                }
                            }).AddTo(model.SelectedElement);

                        // 방향 결정 후 클릭하면 클릭한 자리에 문 적용.
                        doorClick = new SingleAssignmentDisposable();
                        doorClick.Disposable = doorTargetPoint.OnMouseDownAsObservable()
                        .Where(_ => isCollision)
                        .First()
                        .Subscribe(_ =>
                        {
                            model.SelectedElement.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.black;
                            doorTargetPoint.SetActive(false);
                            landscapeTarget.Dispose();
                            doorClick.Dispose();
                            onEnter.Dispose();
                            onExit.Dispose();
                            labels.transform.GetChild(0).gameObject.SetActive(true);
                            labels.transform.GetChild(1).gameObject.SetActive(true);
                            labels.transform.GetChild(2).gameObject.SetActive(true);
                            DoorButtonSetactive(true);
                            isClickable = true;
                            return;

                        }).AddTo(model.SelectedElement);

                        // 문 생성중 취소. > 문 회전중에는 회전 누르기 전의 상태로 돌아가야함.
                        this.UpdateAsObservable()
                        .TakeWhile(_ => !doorClick.IsDisposed)
                        .Where(_ => Input.GetKey(KeyCode.Escape))
                        .Subscribe(_ =>
                        {
                            doorTargetPoint.SetActive(false);
                            doorClick.Dispose();
                            onEnter.Dispose();
                            onExit.Dispose();
                            landscapeTarget.Dispose();
                            labels.transform.GetChild(0).gameObject.SetActive(false);
                            labels.transform.GetChild(1).gameObject.SetActive(false);
                            labels.transform.GetChild(2).gameObject.SetActive(false);
                            model.SelectedElement.transform.localRotation = originRotation;
                            isClickable = true;
                            isCollision = true;
                            CancelSelect();
                            return;
                        }).AddTo(model.SelectedElement);


                        break;
                    default:
                        break;
                }
            });

        swingDoor.onClick.AsObservable()
            .Subscribe(_ =>
            {
                swingDoor.GetComponent<Image>().color = Color.cyan;
                noDoor.GetComponent<Image>().color = Color.white;

                model.DoorIcon = model.SwingDoorIconPrefab;

                ChangePreviewIcon(model.SwingDoorIconPrefab);
            });

        noDoor.onClick.AsObservable()
            .Subscribe(_ =>
            {
                swingDoor.GetComponent<Image>().color = Color.white;
                noDoor.GetComponent<Image>().color = Color.cyan;
                model.DoorIcon = model.NoDoorIconPrefab;

                ChangePreviewIcon(model.NoDoorIconPrefab);
            });
    }

    private void RemoveDoor(GameObject _doorObject)
    {
        model.DoorList.Remove(model.DoorList.Find(x => x.DoorObject == _doorObject));

        Destroy(_doorObject);
    }

    /// <summary>
    /// 벽을 마우스클릭하면 문이 생성되도록 하는 함수, 미리보기가 적용된 시점에서 사용하지 않음.
    /// </summary>
    /// <param name="walls"></param>
    //private void BindDoorWalls(List<Model.Wall> walls)
    //{
    //    foreach (Model.Wall wall in walls)
    //    {
    //        // 바닥 주위의 벽을 마우스 클릭하면 설정해놓은 문의 방향대로 마우스 위치에 생성.
    //        // 문의 시작점과 끝점 사이에 점이 있는지 여부 체크필요.
    //        // 문과 직교하는 벽이 존재한다면 문을 빨간색으로 표시하고 생성되지 않음.
    //        wall.WallObject.OnMouseDownAsObservable()
    //            .Where(_ =>!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()&&!doorTargetPoint.activeSelf)
    //            .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
    //            .Subscribe(position =>
    //            {
    //                #region x, z축을 분리하지 않고 통합 처리하기 위한 시도 (추후 수정)
    //                //            List<Model.Door> temp = Model.Instance.DoorList.FindAll(x =>
    //                //wall.WallObject.transform.InverseTransformPoint(x.DoorObject.transform.position).x == wall.WallObject.transform.localPosition.x);

    //                //Debug.Log(wall.WallObject.transform.TransformPoint(Vector3.zero));
    //                //Debug.Log(wall.WallObject.transform.TransformDirection(90, 0, -90));
    //                //Debug.Log(wall.WallObject.transform.TransformPoint(wall.WallObject.transform.InverseTransformPoint(position.x, 0, position.z)));
    //                //Debug.Log(wall.WallObject.transform.InverseTransformPoint(new Vector3(position.x, 0, position.z)));
    //                //Debug.Log(wall.WallObject.transform.InverseTransformPoint(position.x, position.y, 0));

    //                //GameObject door = Instantiate(Model.Instance.DoorIconPrefab
    //                //       , wall.WallObject.transform.TransformPoint(wall.WallObject.transform.InverseTransformPoint(position.x, 0, position.z))
    //                //           , Quaternion.Euler(0, 90, 0)
    //                //           , doorTransform);
    //                //door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.7f);
    //                //Model.Instance.DoorList.Add(new Model.Door(door));

    //                //GameObject doorTargetPoint = Instantiate(doorTargetPrefab, new Vector3(position.x, model.DoorIconPrefab.transform.localScale.x/2, position.z), new Quaternion(0, 0, 0, 0));

    //                //doorTargetPoint.OnMouseOverAsObservable()
    //                //.Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
    //                //.Subscribe(doorTaretPosition =>
    //                //{
    //                //    if (doorTaretPosition.x > doorTargetPoint.transform.position.x
    //                //    && doorTaretPosition.z > doorTargetPoint.transform.position.z)
    //                //    {
    //                //        door.transform.localRotation = Quaternion.Euler(0, 90, 0);
    //                //    }
    //                //    else if (doorTaretPosition.x > doorTargetPoint.transform.position.x
    //                //    && doorTaretPosition.z < doorTargetPoint.transform.position.z)
    //                //    {
    //                //        door.transform.localRotation = Quaternion.Euler(0, 90, 180);
    //                //    }
    //                //    else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
    //                //   && doorTaretPosition.z > doorTargetPoint.transform.position.z)
    //                //    {
    //                //        door.transform.localRotation = Quaternion.Euler(0, -90, 180);
    //                //    }
    //                //    else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
    //                //   && doorTaretPosition.z < doorTargetPoint.transform.position.z)
    //                //    {
    //                //        door.transform.localRotation = Quaternion.Euler(0, -90, 0);
    //                //    }
    //                //})
    //                //.AddTo(this); 
    //                #endregion
    //                GenerateDoor(position, wall);
    //            })
    //            .AddTo(doorPhase);

    //    }
    //}
    
    
    private void GenerateDoor(Vector3 position, Model.Wall wall)
    {
        switch (wall.Direction)
        {
            #region 세로
            case Model.Wall.WallDirection.Vertical:

                #region 생선 전 검사

                #region 클릭한 좌표와 겹치는 문이 있는지 상태 검사
                List<Model.Door> foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                        x.DoorObject.transform.position.x == wall.WallObject.transform.position.x);


                for (int i = 0; i < foundedDoorList.Count; i++)
                {
                    // 위쪽이 StartPoint일 때.
                    if (foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                    || foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                    {
                        if ((position.z + doorDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && position.z + doorDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                        || (position.z - doorDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && position.z - doorDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                    // 아래쪽이 StartPoint일 때.
                    else
                    {
                        if ((position.z + doorDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && position.z + doorDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z)
                        || (position.z - doorDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.z
                            && position.z - doorDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.z))
                        {
                            return;
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
                        if ((position.z + doorDefaultLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(1).position.z
                            && position.z + doorDefaultLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(2).position.z)
                        || (position.z - doorDefaultLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(1).position.z
                            && position.z - doorDefaultLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                    // 아래쪽이 StartPoint일 때.
                    else
                    {
                        if ((position.z + doorDefaultLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(1).position.z
                            && position.z + doorDefaultLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(2).position.z)
                        || (position.z - doorDefaultLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(1).position.z
                            && position.z - doorDefaultLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(2).position.z))
                        {
                            return;
                        }
                    }
                }
                #endregion

                #region 문의 양옆이 벽에 포함된 상태가 맞는지 검사
                if (position.z + doorDefaultLength / 2 > wall.EndPoint.z - doorPadding || position.z - doorDefaultLength / 2 < wall.StartPoint.z + doorPadding)
                {
                    return;
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
                            Debug.Log("positon z+ : "+ (position.z + doorDefaultLength / 2)+"\n"+
                            "cross z- : " + (points.GetChild(i).position.z - 0.1f) + "\n"+
                            "positon z- : " +( position.z - doorDefaultLength / 2) + "\n"+
                            "cross z+ : " + (points.GetChild(i).position.z + 0.1f) + "\n");
                        }
                        if (position.z + doorDefaultLength / 2 > points.GetChild(i).position.z - 0.1f
                        && position.z - doorDefaultLength / 2 < points.GetChild(i).position.z + 0.1f)
                        {

                            return;
                        }
                    }
                }
                #endregion

                #endregion

                #region 문 생성과 마우스 위치에 따른 문 방향 변경

                GameObject door = Instantiate(Model.Instance.DoorIcon
                       , new Vector3(wall.WallObject.transform.position.x, 1f, Util.GetGridValue(position.z))
                       , Quaternion.Euler(0, 90, 0)
                       , doorTransform);
                door.transform.localScale = new Vector3(doorDefaultLength, door.transform.localScale.y, doorDefaultLength);
                door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.7f);

                Model.Door tempDoor = new Model.Door(door, wall);




                #region 문 콜라이더 충돌 검사

                IDisposable onEnter = tempDoor.DoorObject.transform.GetChild(0).OnTriggerStayAsObservable()
                .Select(collision => collision)
                .Where(collision => collision.name == "Door" || collision.name == "Window" || collision.tag == "CorssPoint")
                .Subscribe(_ =>
                {
                    door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(200, 0, 0, 0.7f);
                    isClickable = false;
                })
                .AddTo(door);
                IDisposable onExit = door.transform.GetChild(0).OnTriggerExitAsObservable()
                .Select(collision => collision)
                .Where(collision => collision.name == "Door" || collision.name == "Window" || collision.tag == "CorssPoint")
                .Subscribe(_ =>
                {
                    door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.7f);
                    isClickable = true;
                })
                .AddTo(door);

                #endregion

                CancelSelect();

                #region 문 양옆 수치표시
                labels.transform.GetChild(0).gameObject.SetActive(true);
                labels.transform.GetChild(1).gameObject.SetActive(true);
                labels.transform.GetChild(2).gameObject.SetActive(true);
                FlotDoorLabel(tempDoor.DoorObject, wall);
                #endregion

                var verticalTarget = new SingleAssignmentDisposable();

                if (door.name == "SwingDoorIcon(Clone)")
                {
                    doorTargetPoint.transform.position = new Vector3(position.x, 5f, position.z);
                    doorTargetPoint.SetActive(true);


                    verticalTarget.Disposable =
                    doorTargetPoint.OnMouseOverAsObservable()
                    .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
                    .Subscribe(doorTaretPosition =>
                    {

                        if (doorTaretPosition.x > doorTargetPoint.transform.position.x
                        && doorTaretPosition.z > doorTargetPoint.transform.position.z)
                        {
                            // 오른쪽 위.
                            door.transform.localRotation = Quaternion.Euler(0, 90, 0);
                        }
                        else if (doorTaretPosition.x > doorTargetPoint.transform.position.x
                        && doorTaretPosition.z < doorTargetPoint.transform.position.z)
                        {
                            // 오른쪽 아래.
                            door.transform.localRotation = Quaternion.Euler(0, 90, 180);
                        }
                        else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
                       && doorTaretPosition.z > doorTargetPoint.transform.position.z)
                        {
                            // 왼쪽 위.
                            door.transform.localRotation = Quaternion.Euler(0, -90, 180);
                        }
                        else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
                       && doorTaretPosition.z < doorTargetPoint.transform.position.z)
                        {
                            // 왼쪽 아래.
                            door.transform.localRotation = Quaternion.Euler(0, -90, 0);
                        }
                    })
                    .AddTo(door);


                    var doorClick = new SingleAssignmentDisposable();
                    doorClick.Disposable = doorTargetPoint.OnMouseDownAsObservable()
                    .Where(_ => isClickable)
                    .First()
                    .Subscribe(_ =>
                    {

                        if (isDebugMode)
                        {
                            Debug.Log("DoorMouse Down");
                        }
                        door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.black;
                        doorTargetPoint.SetActive(false);
                        verticalTarget.Dispose();
                        onEnter.Dispose();
                        onExit.Dispose();
                        doorClick.Dispose();
                        Model.Instance.DoorList.Add(tempDoor);

                        CancelSelect();
                        Select(door);
                        labels.transform.GetChild(0).gameObject.SetActive(true);
                        labels.transform.GetChild(1).gameObject.SetActive(true);
                        labels.transform.GetChild(2).gameObject.SetActive(true);
                        FlotDoorLabel(tempDoor.DoorObject, wall);
                        DoorButtonSetactive(true);


                        #region 문 Select 구독
                        BindDoor(tempDoor);
                        #endregion

                        return;

                    }).AddTo(door);

                    // 문 생성중 취소.
                    this.UpdateAsObservable()
                    .TakeWhile(_ => !doorClick.IsDisposed)
                    .Where(_ => Input.GetKey(KeyCode.Escape))
                    .Subscribe(_ =>
                    {
                        isClickable = true;
                        doorTargetPoint.SetActive(false);
                        labels.transform.GetChild(0).gameObject.SetActive(false);
                        labels.transform.GetChild(1).gameObject.SetActive(false);
                        labels.transform.GetChild(2).gameObject.SetActive(false);
                        Destroy(door);

                        return;
                    }).AddTo(door);

                }
                else
                {
                    BindDoor(tempDoor);
                    onEnter.Dispose();
                    onExit.Dispose();
                    Model.Instance.DoorList.Add(tempDoor);
                    CancelSelect();
                    Select(door);
                    labels.transform.GetChild(0).gameObject.SetActive(true);
                    labels.transform.GetChild(1).gameObject.SetActive(true);
                    labels.transform.GetChild(2).gameObject.SetActive(true);
                    FlotDoorLabel(tempDoor.DoorObject, wall);
                    DoorButtonSetactive(true);
                    door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                }
                #endregion

                break;
            #endregion

            #region 가로
            case Model.Wall.WallDirection.Landscape:

                #region 생성 전 검사

                #region 클릭한 좌표와 겹치는 문이 있는지 상태 검사
                foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                        x.DoorObject.transform.position.z == wall.WallObject.transform.position.z);

                for (int i = 0; i < foundedDoorList.Count; i++)
                {
                    // 오른쪽이 StartPoint일 때.
                    if (foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 180, 0)
                    || foundedDoorList[i].DoorObject.transform.localRotation == Quaternion.Euler(0, 0, 180))
                    {
                        if ((position.x + doorDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && position.x + doorDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                        || (position.x - doorDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && position.x - doorDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                        {
                            return;
                        }
                    }
                    // 왼쪽이 StartPoint일 때.
                    else
                    {
                        if ((position.x + doorDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && position.x + doorDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x)
                        || (position.x - doorDefaultLength / 2 >= foundedDoorList[i].DoorObject.transform.GetChild(1).position.x
                            && position.x - doorDefaultLength / 2 <= foundedDoorList[i].DoorObject.transform.GetChild(2).position.x))
                        {
                            return;
                        }
                    }
                }
                #endregion

                #region 클릭한 좌표와 겹치는 창문이 있는지 상태 검사
                 foundedWindowList = model.WindowList.FindAll(x =>
                        x.WindowObject.transform.position.z == wall.WallObject.transform.position.z);

                for (int i = 0; i < foundedWindowList.Count; i++)
                {
                    // 위쪽이 StartPoint일 때.
                    if (foundedWindowList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, 90, 0)
                    || foundedWindowList[i].WindowObject.transform.localRotation == Quaternion.Euler(0, -90, 180))
                    {
                        if ((position.x + windowDefaultLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(1).position.x
                            && position.x + windowDefaultLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(2).position.x)
                        || (position.x - windowDefaultLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(1).position.x
                            && position.x- windowDefaultLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(2).position.x))
                        {
                            return;
                        }
                    }
                    // 아래쪽이 StartPoint일 때.
                    else
                    {
                        if ((position.x + windowDefaultLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(1).position.x
                            && position.x + windowDefaultLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(2).position.x)
                        || (position.x - windowDefaultLength / 2 >= foundedWindowList[i].WindowObject.transform.GetChild(1).position.x
                            && position.x - windowDefaultLength / 2 <= foundedWindowList[i].WindowObject.transform.GetChild(2).position.x))
                        {
                            return;
                        }
                    }
                }
                #endregion

                #region 문의 양옆이 벽에 포함된 상태가 맞는지 검사
                if (position.x + doorDefaultLength / 2 > wall.EndPoint.x - doorPadding || position.x - doorDefaultLength / 2 < wall.StartPoint.x + doorPadding)
                {
                    return;
                }

                #endregion

                #region 문 영역에 수직방향 벽이 존재하지 않는지 검사
                // TODO : 문의 시작점과 끝점 사이에 다른 벽의 시작점 혹은 끝점이 있는지 검사 필요.
                // 방과 방 사이의 거리가 벌려질 것이기 때문에 해당 로직 구현 후에 맞춤 개발 해야할 듯.


                points = GameObject.Find("Points").transform;
                for (int i = 0; i < points.childCount; i++)
                {
                    if (Util.IsEquals(points.GetChild(i).position.z, wall.EndPoint.z))
                        if (position.x + doorDefaultLength / 2 > points.GetChild(i).position.x - 0.1f
                        && position.x - doorDefaultLength / 2 < points.GetChild(i).position.x + 0.1f)
                        {
                            return;
                        }
                }
                #endregion

                #endregion

                #region 문 생성과 마우스 위치에 따른 문 방향 변경

                door = Instantiate(Model.Instance.DoorIcon
                        , new Vector3(Util.GetGridValue(position.x), 1f, wall.WallObject.transform.position.z)
                        , Quaternion.Euler(0, 0, 0)
                        , doorTransform);
                door.transform.localScale = new Vector3(doorDefaultLength, door.transform.localScale.y, doorDefaultLength);
                door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.7f);
                tempDoor = new Model.Door(door, wall);




                #region 문 콜라이더 충돌 검사
                isClickable = true;
                onEnter = door.transform.GetChild(0).OnTriggerStayAsObservable()
                .Select(collision => collision)
                .Where(collision => collision.name == "Door" || collision.tag == "CorssPoint")
                .Subscribe(_ =>
                {
                    door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(200, 0, 0, 0.7f);
                    isClickable = false;
                }).AddTo(door);
                onExit = door.transform.GetChild(0).OnTriggerExitAsObservable()
                .Select(collision => collision)
                .Where(collision => collision.name == "Door" || collision.tag == "CorssPoint")
                .Subscribe(_ =>
                {
                    door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.7f);
                    isClickable = true;
                }).AddTo(door);
                #endregion

                CancelSelect();

                #region 문 양옆 수치표시
                labels.transform.GetChild(0).gameObject.SetActive(true);
                labels.transform.GetChild(1).gameObject.SetActive(true);
                labels.transform.GetChild(2).gameObject.SetActive(true);
                FlotDoorLabel(tempDoor.DoorObject, wall);
                #endregion

                // 문 방향을 정하기 위한 부분.
                var landscapeTarget = new SingleAssignmentDisposable();
                if (door.name == "SwingDoorIcon(Clone)")
                {
                    doorTargetPoint.transform.position = new Vector3(position.x, 5f, position.z);
                    doorTargetPoint.SetActive(true);

                    // 마우스 위치에 따라서 문을 회전시킨다.
                    landscapeTarget.Disposable =
                    doorTargetPoint.OnMouseOverAsObservable()
                    .Select(_ => Camera.main.ScreenToWorldPoint(Input.mousePosition))
                    .Subscribe(doorTaretPosition =>
                    {
                        if (doorTaretPosition.x > doorTargetPoint.transform.position.x
                        && doorTaretPosition.z > doorTargetPoint.transform.position.z)
                        {
                            // 오른쪽 위.
                            door.transform.localRotation = Quaternion.Euler(0, 0, 180);
                        }
                        else if (doorTaretPosition.x > doorTargetPoint.transform.position.x
                        && doorTaretPosition.z < doorTargetPoint.transform.position.z)
                        {
                            // 오른쪽 아래.
                            door.transform.localRotation = Quaternion.Euler(0, 180, 0);
                        }
                        else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
                       && doorTaretPosition.z > doorTargetPoint.transform.position.z)
                        {
                            // 왼쪽 위.
                            door.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        }
                        else if (doorTaretPosition.x < doorTargetPoint.transform.position.x
                       && doorTaretPosition.z < doorTargetPoint.transform.position.z)
                        {
                            // 왼쪽 아래.
                            door.transform.localRotation = Quaternion.Euler(0, 180, 180);
                        }
                    }).AddTo(door);

                    // 방향 결정 후 클릭하면 해당 위치에 문 생성.
                    var doorClick = new SingleAssignmentDisposable();
                    doorClick.Disposable = doorTargetPoint.OnMouseDownAsObservable()
                    .Where(_ => isClickable)
                    .First()
                    .Subscribe(_ =>
                    {
                        if (isDebugMode)
                        {
                            Debug.Log("DoorMouse Down");
                        }

                        door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.black;
                        doorTargetPoint.SetActive(false);
                        landscapeTarget.Dispose();
                        onEnter.Dispose();
                        onExit.Dispose();
                        doorClick.Dispose();
                        model.DoorList.Add(tempDoor);


                        CancelSelect();
                        Select(door);
                        labels.transform.GetChild(0).gameObject.SetActive(true);
                        labels.transform.GetChild(1).gameObject.SetActive(true);
                        labels.transform.GetChild(2).gameObject.SetActive(true);
                        FlotDoorLabel(tempDoor.DoorObject, wall);
                        DoorButtonSetactive(true);

                        #region 문 Select 구독
                        BindDoor(tempDoor);
                        #endregion

                        return;

                    }).AddTo(door);

                    // 문 생성중 취소.                            
                    this.UpdateAsObservable()
                    .TakeWhile(_ => !doorClick.IsDisposed)
                    .Where(_ => Input.GetKey(KeyCode.Escape))
                    .Subscribe(_ =>
                    {
                        isClickable = true;
                        labels.transform.GetChild(0).gameObject.SetActive(false);
                        labels.transform.GetChild(1).gameObject.SetActive(false);
                        labels.transform.GetChild(2).gameObject.SetActive(false);
                        Destroy(door);
                        doorTargetPoint.SetActive(false);
                        return;
                    }).AddTo(door);
                }
                else
                {
                    BindDoor(tempDoor);
                    onEnter.Dispose();
                    onExit.Dispose();
                    Model.Instance.DoorList.Add(tempDoor);
                    CancelSelect();
                    Select(door);
                    labels.transform.GetChild(0).gameObject.SetActive(true);
                    labels.transform.GetChild(1).gameObject.SetActive(true);
                    labels.transform.GetChild(2).gameObject.SetActive(true);
                    FlotDoorLabel(tempDoor.DoorObject, wall);
                    DoorButtonSetactive(true);
                    door.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                }
                #endregion
                break;
            #endregion

            default:
                break;
        }
    }


    private void DoorButtonSetactive(bool _flag)
    {
        if (_flag && model.SelectedElement.name == "SwingDoorIcon(Clone)")
        {
            doorRotationButton.gameObject.SetActive(_flag);
        }
        else
        {
            doorRotationButton.gameObject.SetActive(false);
        }
        doorRemoveButton.gameObject.SetActive(_flag);
        doorMoveButton.gameObject.SetActive(_flag);
    }

    /// <summary>
    /// 문 양옆에서 벽까지의 거리와 문 자체의 폭에 대한 길이 정보를 띄워주는 함수.
    /// </summary>
    /// <param name="element">문 오브젝트</param>
    /// <param name="wall">문이 붙어있는 벽</param>
    void FlotDoorLabel(GameObject element, Model.Wall wall)
    {
        Vector3 doorLabelPadding = Vector3.zero;
        float doorScaleX = ((model.SelectedElement != null) ? model.SelectedElement.transform.localScale.x / 2 : doorDefaultLength/2);

        Vector3 label1_dot1 = wall.StartPoint;
        Vector3 label1_dot2 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.01f ?
                new Vector3(
                    element.transform.position.x - doorScaleX
                    , 0f
                    , element.transform.position.z)
                : new Vector3(element.transform.position.x, 0f, element.transform.position.z - doorScaleX);


        label1Text.text = Util.GetGridLabel(Vector3.Distance(new Vector3(label1_dot1.x, 0, label1_dot1.z), new Vector3(label1_dot2.x, 0, label1_dot2.z))) + "m";
        Vector3 padding = Vector3.zero;
        float angle = 0;
        if (label1_dot2.x - label1_dot1.x > 0) { angle = 0; padding = new Vector3(0, 0, 1); doorLabelPadding = new Vector3(0, 0, (element.name.Contains("Swing") ? doorScaleX * 2 : 1)); }
        if (label1_dot2.x - label1_dot1.x < 0) { angle = 180; padding = new Vector3(0, 0, -1); doorLabelPadding = new Vector3(0, 0, (element.name.Contains("Swing") ? doorScaleX * 2 : 1)); }
        if (label1_dot2.z - label1_dot1.z > 0) { angle = 270; padding = new Vector3(-1, 0, 0); doorLabelPadding = new Vector3((element.name.Contains("Swing") ? -doorScaleX * 2 : -1), 0, 0); }
        if (label1_dot2.z - label1_dot1.z < 0) { angle = 90; padding = new Vector3(1, 0, 0); doorLabelPadding = new Vector3((element.name.Contains("Swing") ? -doorScaleX * 2 :- 1), 0, 0); }
        label1.transform.rotation = Quaternion.Euler(new Vector3(90, angle, 0));
        label1.transform.position = (label1_dot1 + label1_dot2) * 0.5f + padding * 0.5f + Vector3.up * 2;



        float length1 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.01f ?
        (element.transform.position.x - doorScaleX) - wall.StartPoint.x :
        (element.transform.position.z - doorScaleX) - wall.StartPoint.z;

        label1Line.transform.localScale = new Vector3(length1, 1, 1);


        Vector3 label2_dot1 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.01f ?
            new Vector3(
                element.transform.position.x + doorScaleX
                , 0f
                , element.transform.position.z)
            : new Vector3(
                element.transform.position.x
                , 0f
                , element.transform.position.z + doorScaleX);

        Vector3 label2_dot2 = wall.EndPoint;

        label2Text.text = Util.GetGridLabel(Vector3.Distance(new Vector3(label2_dot1.x, 0, label2_dot1.z), new Vector3(label2_dot2.x, 0, label2_dot2.z))) + "m";
        label2.transform.rotation = Quaternion.Euler(new Vector3(90, angle, 0));
        label2.transform.position = (label2_dot1 + label2_dot2) * 0.5f + padding * 0.5f + Vector3.up * 2;


        float length2 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.01f ?
            (element.transform.position.x + doorScaleX) - wall.EndPoint.x :
            (element.transform.position.z + doorScaleX) - wall.EndPoint.z;


        label2Line.transform.localScale = new Vector3(length2, 1, 1);


        Vector3 label3_dot1 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
            new Vector3(
                element.transform.position.x + doorScaleX
                , 0f
                , element.transform.position.z)
            : new Vector3(
                element.transform.position.x
                , 0f
                , element.transform.position.z + doorScaleX);

        Vector3 label3_dot2 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
            new Vector3(
                element.transform.position.x - doorScaleX
                , 0f
                , element.transform.position.z)
            : new Vector3(
                element.transform.position.x
                , 0f
                , element.transform.position.z - doorScaleX);

        label3Text.text = Util.GetGridLabel(Vector3.Distance(new Vector3(label3_dot1.x, 0, label3_dot1.z), new Vector3(label3_dot2.x, 0, label3_dot2.z))) + "m";
        label3.transform.rotation = Quaternion.Euler(new Vector3(90, angle, 0));
        label3.transform.position = (label3_dot1 + label3_dot2) * 0.5f + padding * 0.5f + Vector3.up * 3 + doorLabelPadding;


        float length3 = Mathf.Abs(wall.WallObject.transform.localScale.y - 0.1f) < 0.001f ?
            (element.transform.position.x + doorScaleX - (element.transform.position.x - doorScaleX)) :
            (element.transform.position.z + doorScaleX - (element.transform.position.z - doorScaleX));


        label3Line.transform.localScale = new Vector3(length3, 1, 1);
    }

    private void UpdateDoorLabel(float length)
    {
        if (model.SelectedElement == null) return;
        if (length < 0.001f) return;
        Vector3 scale = new Vector3(Util.GetGridValue(length), model.SelectedElement.transform.localScale.y, Util.GetGridValue(length));

        #region 문 수치변경시 벽 바깥으로 넘어갈 때.
        Model.Wall wall = model.DoorList.Find(y => y.DoorObject == model.SelectedElement).DoorAttachedWall;
        switch (wall.Direction)
        {
            case Model.Wall.WallDirection.Vertical:
                if (model.SelectedElement.transform.position.z + length / 2 > wall.EndPoint.z - 0.1f
                    || model.SelectedElement.transform.position.z - length / 2 < wall.StartPoint.z + 0.1f)
                {
                    label3Text.color = Color.red;
                    return;
                }
                break;
            case Model.Wall.WallDirection.Landscape:
                if (model.SelectedElement.transform.position.x + length / 2 > wall.EndPoint.x - 0.1f
                    || model.SelectedElement.transform.position.x - length / 2 < wall.StartPoint.x + 0.1f)
                {
                    label3Text.color = Color.red;
                    return;
                }
                break;
            default:
                break;

        }
        #endregion

        #region 문 수치변경시 양옆에 창문이나 문 있는지 확인
        switch (wall.Direction)
        {
            case Model.Wall.WallDirection.Vertical:

                // 생성 위치 주변에 문이 있는지 검사.
                List<Model.Door> foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                x.DoorObject.transform.position.x == wall.WallObject.transform.position.x
                && x.DoorObject != model.SelectedElement);


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
                foundedDoorList = Model.Instance.DoorList.FindAll(x =>
                        x.DoorObject.transform.position.z == wall.WallObject.transform.position.z
                && x.DoorObject != model.SelectedElement);

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
        FlotDoorLabel(model.SelectedElement, wall);
    }
    #endregion


}
