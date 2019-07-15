using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 꾸미기 씬에서 사용되는 main presenter.
/// 초기화(천장 오브젝트 생성, 각종 오브젝트 바인딩), 시점( 카메라 이동/회전) 처리.
/// </summary>
public partial class DecoratePresenter : MonoBehaviour
{
    #region Fields
    Model model;

    [SerializeField]
    Button fpsButton = default, tpsButton = default, topviewButton = default;
    [SerializeField]
    Button backButton = default;

    Transform floorsParent, wallsParent, ceilingParent, doorParent;

    // 문없음 관련 오브젝트를 담을 부모 오브젝트
    Transform nodoorMeshParent;
    // 문없음 관련 오브젝트와 면적 정보
    Dictionary<GameObject, float> nodoorMeshs;

    // 가구를 담을 부모 오브젝트
    Transform furnitureItemParent;
    // 천장 가구를 담을 부모 오브젝트
    Transform ceilingItemParent;
    // 천장 오브젝트와 적용한 재질 정보
    Dictionary<Transform, Material> ceilingItemsMaterial;

    [SerializeField]
    GameObject gizmo = default;

    // for debug
    bool isDebugMode = false;
    [SerializeField]
    GameObject debugMode = default;
    Text debugLog = default;
    

    #region Camera
    Camera fpsCamera, tpsCamera, topviewCamera;
    public bool isFree = true;
    [Header("회전속도")]
    public float speedR = 0.3f; // 회전속도
    [Header("이동속도")]
    public float speedP = 2f; // 이동속도 
    Vector3 tpsInitPos, tpsInitAngle;
    Vector3 topInitPos;

    GameObject minimap;
    FovVisualizer minimapDot;
    #endregion

    #endregion

    #region Methods


    #region Init

    private void Awake()
    {
        Init();
        Bind();

        // 초기 카메라 tps 모드로 세팅
        ToggleCamera(tpsCamera, true);
    }
    /// <summary>
    /// 씬의 각종 게임오브젝트와 참조 변수 연결.
    /// 천장 오브젝트 생성
    /// </summary>
    private void Init()
    {
        model = Model.Instance;     
        fpsCamera = GameObject.Find("FPS Camera").GetComponent<Camera>();
        tpsCamera = GameObject.Find("TPS Camera").GetComponent<Camera>();
        topviewCamera = GameObject.Find("TopView Camera").GetComponent<Camera>();

        tpsInitPos = tpsCamera.transform.position;
        tpsInitAngle = tpsCamera.transform.parent.eulerAngles;
        topInitPos = topviewCamera.transform.position;

        minimap = GameObject.Find("Minimap");
        minimapDot = GameObject.Find("Minimap_dot").GetComponent<FovVisualizer>();
        minimap.SetActive(minimapDot.isOn);

        floorsParent = GameObject.Find("MergedRooms").transform;
        wallsParent = GameObject.Find("Walls").transform;
        ceilingParent = new GameObject("Ceilings").transform;        
        doorParent = GameObject.Find("DoorsParent").transform;
        nodoorMeshParent = new GameObject("NoDoorMeshParent").transform;
        nodoorMeshs = new Dictionary<GameObject, float>();

        furnitureItemParent = new GameObject().transform;
        furnitureItemParent.name = "FurnitureParent";
        debugLog = debugMode.transform.GetChild(0).GetComponent<Text>();

        // 천장생성
        MakeCeiling();

        // WallsTop Collider off
        foreach (var col in GameObject.Find("WallsTop").GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        InitUI();
    }

    /// <summary>
    /// 카메라 전환 함수.
    /// 씬 전환시 이동 및 회전이 초기값으로 초기화.
    /// </summary>
    /// <param name="cam">전환할 카메라</param>
    /// <param name="init">초기화 플래그</param>
    private void ToggleCamera(Camera cam, bool init=false)
    {
        if (CurrentCamera() == cam && !init) return;
        bool isFPS = false;
        if (cam == fpsCamera) isFPS = true;
        ToggleCeiling(isFPS);
        minimap.SetActive(isFPS);
        minimapDot.isOn = isFPS;
        Cancel();

        //tps 카메라의 경우 이전 카메라의 회전을 반영
        fpsCamera.transform.eulerAngles = CurrentCamera() == tpsCamera? Vector3.up * tpsCamera.transform.eulerAngles.y : Vector3.zero ;
        tpsCamera.transform.position = tpsInitPos;
        tpsCamera.transform.parent.eulerAngles = tpsInitAngle;
        topviewCamera.transform.position = topInitPos;

        fpsCamera.depth = cam == fpsCamera ? 1 : 0;
        tpsCamera.depth = cam == tpsCamera ? 1 : 0;
        topviewCamera.depth = cam == topviewCamera ? 1 : 0;

        fpsButton.GetComponent<Image>().color = cam == fpsCamera ? Color.yellow : Color.white;
        topviewButton.GetComponent<Image>().color = cam == topviewCamera ? Color.yellow : Color.white;
        tpsButton.GetComponent<Image>().color = cam == tpsCamera ? Color.yellow : Color.white;
    }

    /// <summary>
    /// 디버그용 함수.
    /// 로그를 표시하거나 클릭한 위치의 오브젝트 정보를 반환.
    /// </summary>
    /// <param name="str">로그 메시지</param>
    private void DebugMsg(string str="")
    {
        if (!isDebugMode) return;
        if (str == "")
        {
            str += Input.mousePosition.ToString();
            str += " / ";
            Physics.Raycast(CurrentCamera().ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f);
            if (hit.transform != null) str += hit.transform.name;
            if (selectedObject != null) str += FindType(selectedObject.transform);
        }        
        debugLog.text = str;
        Debug.Log(str);
    }

    /// <summary>
    /// 천장 생성 함수.
    /// 병합된 바닥을 복제하고 mesh의 normal을 뒤집어서 천장 생성.
    /// </summary>
    void MakeCeiling()
    {
        float height = 2.2f;
        float border = 0.1f;
        
        ceilingParent.parent = model.Room.Rooms[0].parent;
        ceilingItemParent = new GameObject("CeilingItems").transform;
        Transform shadowParent = new GameObject("ShadowMaskParent").transform;

        List<Transform> floors = Util.GetAllFloors(model.Room.Rooms);
        foreach (var floor in floors)
        {
            // 그림자 차폐를 위한 천장 추가
            Transform shadowMask = Instantiate(model.Floor.FloorPrefab).transform;
            shadowMask.parent = shadowParent;
            shadowMask.name = "ShadowMaskCeiling";
            shadowMask.position = floor.position + Vector3.up * (height + 0.1f);
            shadowMask.localScale = new Vector3(floor.localScale.x + border * 2, 1, floor.localScale.z + border * 2);
            shadowMask.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            shadowMask.GetComponent<Renderer>().receiveShadows = false;
            shadowMask.gameObject.GetComponent<Collider>().enabled = false;
        }

        // 조명 설치를 위한 천장 추가
        foreach (var room in model.Room.MergedRooms.Keys)
        {
            // 바닥 복사
            GameObject tmp = Instantiate(room.gameObject, ceilingParent);
            tmp.transform.position += Vector3.up * height;
            if (tmp.GetComponent<MeshCollider>() == null) tmp.AddComponent<MeshCollider>();
            tmp.GetComponent<MeshCollider>().sharedMesh = Util.FilpMesh(tmp.GetComponent<MeshFilter>().mesh);
            
            // top/tps 뷰에서 조명 설치를 위한 박스 콜라이더 추가
            int index = room.transform.GetSiblingIndex();
            foreach (var floor in Util.GetChildTransform(model.Room.Rooms[index]))
            {
                BoxCollider col = tmp.AddComponent<BoxCollider>();
                Vector3 scale = floor.localScale;
                scale.y = 0.1f;
                col.size = scale;
                col.center = floor.position + Vector3.up * -0.05f;
            }       
        }
    }

    #endregion
    #region Bind

    /// <summary>
    /// 버튼 및 카메라 바인딩.
    /// </summary>
    private void Bind()
    {
        // fps 뷰로 전환시 이동하고 싶은 위치 지정하여 해당 위치로 카메라 이동
        fpsButton.onClick.AsObservable()
            .Subscribe(_ =>
            {
                if (CurrentCamera() == fpsCamera) return;
                ShowHelp("살펴보고 싶은 위치를 클릭하세요");
                selectable = false;
                GameObject temp = Instantiate(gizmo);
                temp.SetActive(false);
                temp.layer = 2;
                RaycastHit hit;

                // 바닥 위에 마우스 이동 시 이동 기즈모 표시. 
                // 클릭 시 fps카메라 위치를 이동 후 전환
                var updateDisposable = new SingleAssignmentDisposable();
                updateDisposable.Disposable = this.UpdateAsObservable()
                .Select(move => CurrentCamera().ScreenPointToRay(Input.mousePosition))
                .Where(__ => !EventSystem.current.IsPointerOverGameObject())
                .Subscribe(move =>
                {
                    Physics.Raycast(move, out hit, 100);
                    if (hit.transform != null && FindType(hit.transform)== DecorateType.Floor)
                    {
                        temp.transform.position = hit.point+Vector3.up * 0.1f;
                        temp.SetActive(true);                        
                    }
                    if (Input.GetButtonDown("Fire1"))
                    {
                        selectable = true;
                        fpsCamera.transform.position = temp.transform.position;
                        ToggleCamera(fpsCamera);
                        Destroy(temp);
                        updateDisposable.Dispose();                        
                    }
                });
                
            });

        tpsButton.onClick.AsObservable()
            .Subscribe(_ =>
            {
                ToggleCamera(tpsCamera);
            });

        topviewButton.onClick.AsObservable()
            .Subscribe(_ =>
            {
                ToggleCamera(topviewCamera);
            });

        // 각 카메라가 활성화 중일 때 이동,회전,확대 기능 바인딩
        fpsCamera.UpdateAsObservable()
            .Where(_=>fpsCamera.depth ==1)
            .Subscribe(_ => {
                if (isFree)
                {
                    float xRot = 0;
                    float yRot = 0;
                    if (Input.GetMouseButton(1))
                    {
                        yRot = Input.GetAxis("Mouse X");
                        xRot = Input.GetAxis("Mouse Y");
                    }

                    float h = Input.GetAxis("Horizontal");
                    float v = Input.GetAxis("Vertical");


                    //if (Input.GetButtonDown("Fire2")) yRot += 180;        // 뒤로돌기
                    fpsCamera.transform.localRotation *= Quaternion.Euler(-xRot * speedR, yRot * speedR, 0);
                    float xAngle = 0;
                    if (fpsCamera.transform.eulerAngles.x>270)
                    {
                        xAngle = Mathf.Clamp(fpsCamera.transform.eulerAngles.x,275,360);
                    }
                    else
                    {
                        xAngle = Mathf.Clamp(fpsCamera.transform.eulerAngles.x, 0, 85);
                    }
                    fpsCamera.transform.eulerAngles = new Vector3(xAngle, fpsCamera.transform.eulerAngles.y, 0);

                    Vector3 pos = fpsCamera.transform.position + fpsCamera.transform.forward * v * speedP * Time.deltaTime + fpsCamera.transform.right * h * speedP*Time.deltaTime;
                    pos = new Vector3(pos.x, 1.5f, pos.z);
                    fpsCamera.transform.position = pos;
                }
                BindCameraForUI();
            });

        tpsCamera.UpdateAsObservable()
            .Where(_ => tpsCamera.depth == 1 && !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ =>
            {
                if (isFree)
                {
                    float xRot = 0;
                    float yRot = 0;
                    if (Input.GetMouseButton(1))
                    {
                        yRot = Input.GetAxis("Mouse X");
                        xRot = Input.GetAxis("Mouse Y");
                    }

                    float zoom = Input.GetAxis("Mouse ScrollWheel");
                    if (tpsCamera.fieldOfView >= 10
                        && tpsCamera.fieldOfView <= 100)
                    {
                        tpsCamera.fieldOfView -= zoom * 10;
                        if (tpsCamera.fieldOfView > 100) tpsCamera.fieldOfView = 100;
                        if (tpsCamera.fieldOfView < 10) tpsCamera.fieldOfView = 10;
                    }

                    float h = Input.GetAxis("Horizontal");
                    float v = Input.GetAxis("Vertical");

                    tpsCamera.transform.parent.localRotation *= Quaternion.Euler(-xRot * speedR, yRot * speedR, 0);

                    float xAngle = 0;

                    
                    if (tpsCamera.transform.parent.eulerAngles.x > 270)
                    {
                        xAngle = Mathf.Clamp(tpsCamera.transform.parent.eulerAngles.x, 275, 360);
                    }
                    else
                    {
                        xAngle = Mathf.Clamp(tpsCamera.transform.parent.eulerAngles.x, 0, 40);
                    }

                    tpsCamera.transform.parent.eulerAngles = new Vector3(xAngle, tpsCamera.transform.parent.eulerAngles.y, 0);

                    Vector3 pos = tpsCamera.transform.position + tpsCamera.transform.up * v * Time.deltaTime*10 + tpsCamera.transform.right * h *Time.deltaTime*10;
                    tpsCamera.transform.position = pos;

                }
                BindCameraForUI();
            }).AddTo(this); ;

        topviewCamera.UpdateAsObservable()
            .Where(_ => topviewCamera.depth == 1 && !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ =>
            {
                if (isFree)
                {
                    float h = 0, v = 0;
                    if (Input.GetMouseButton(1))
                    {
                        h = Input.GetAxis("Mouse X");
                        v = Input.GetAxis("Mouse Y");
                    }

                    float zoom = Input.GetAxis("Mouse ScrollWheel") * 3;

                    //휠 드래그시 카메라 확대/축소
                    if (topviewCamera.orthographicSize <= 5f && zoom > 0)
                    {
                        topviewCamera.orthographicSize = 5f;
                    }
                    else if (topviewCamera.orthographicSize >= 12.5f && zoom < 0)
                    {
                        topviewCamera.orthographicSize = 12.5f;
                    }
                    else
                    {
                        topviewCamera.orthographicSize -= zoom;
                    }

                    Vector3 pos = topviewCamera.transform.position + topviewCamera.transform.up * (-v) * 0.3f + topviewCamera.transform.right * (-h) * 0.3f;
                    pos = new Vector3(pos.x, 10f, pos.z);
                    topviewCamera.transform.position = pos;
                }
                BindCameraForUI();
            }).AddTo(this); ;

        // Element 씬으로 되돌아감 : 충돌을 방지하기 위해 필요없는 오브젝트를 제거하고 관련 리스트 초기화.
        backButton.onClick.AsObservable()
            .Subscribe(_ =>
            {
                Transform rooms = GameObject.Find("Rooms").transform;

                Destroy(rooms.Find("OutWalls").Find("OutlineWalls").gameObject);
                Destroy(rooms.Find("Walls").gameObject);
                Destroy(rooms.GetChild(rooms.childCount - 1).gameObject);
                for (int i = 0; i < floorsParent.childCount; i++)
                {
                    floorsParent.GetChild(i).GetComponent<MeshRenderer>().material = model.Floor.DefaultFloorMaterial;
                }

                model.DoorList.Clear();
                model.WindowList.Clear();
                for (int i = 0; i < model.gameObject.transform.childCount; i++)
                {
                    Destroy( model.transform.GetChild(i).gameObject);
                }
                Transform outwalls = rooms.Find("OutWalls");
                for (int i = 0; i < outwalls.GetChild(0).childCount; i++)
                {
                    outwalls.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().material.color = Color.black;
                }
                for (int i = 0; i < outwalls.GetChild(1).childCount; i++)
                {
                    outwalls.GetChild(1).GetChild(i).GetComponent<MeshRenderer>().material.color = Color.black;
                }
                Cancel();

                // wallstop 콜라이더 on
                foreach (var col in GameObject.Find("WallsTop").GetComponentsInChildren<Collider>())
                {
                    col.enabled = true;
                }

                SceneManager.LoadScene("ElementSettingScene");

            }).AddTo(this); ;
        
        BindObjects();
        BindUI();

        // for debug : 디버그 모드일때 화면 클릭시 해당 위치의 오브젝트 정보 표시
        debugMode.SetActive(false);
        debugLog.UpdateAsObservable()
            .Where(_ => isDebugMode)
            .Subscribe(_ =>
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    DebugMsg();
                }
            });
    }

    /// <summary>
    /// 오브젝트 이벤트 바인딩
    /// </summary>
    void BindObjects()
    {
        BindItems(floorsParent, DecorateType.Floor);
        BindItems(wallsParent, DecorateType.Wall);
        BindItems(ceilingParent, DecorateType.Ceiling);
        BindItems(doorParent, DecorateType.Door); // for 문열기
        BindItems(nodoorMeshParent, DecorateType.None);
    }
    
    /// <summary>
    /// 부모 오브젝트로부터 자식 오브젝트를 받아와서 이벤트 바인딩 적용
    /// </summary>
    /// <param name="itemsParent">이벤트를 바인딩할 오브젝트들을 담은 부모 오브젝트</param>
    /// <param name="type">바인딩할 오브젝트 타입</param>
    private void BindItems(Transform itemsParent, DecorateType type)
    {
        for (int i = 0; i < itemsParent.childCount; i++)
        {
            BindItem(itemsParent.GetChild(i), type);
        }
    }
    /// <summary>
    /// 오브젝트에 이벤트를 바인딩하는 함수.
    /// 오브젝트와 항목 선택 상태에 따라 클릭 시 선택/취소 되거나, 재질이 적용되도록 함.
    /// sliding door의 경우 애니메이션을 위해 문짝 모델을 분리하여 별도의 클릭 이벤트를 바인딩 해줌.
    /// no door의 경우 이벤트 바인딩을 하는 대신에
    /// 재질 적용릉 위해 벽지/천장/바닥 부분을 땜빵하는 Quad 모델을 Probuilder로 다시 생성함.
    /// 이때 다시 생성된 mesh들은 nodoor mesh 부모 오브젝트 아래로 들어가서 벽지 또는 바닥재가 적용될 수 있도록 바인딩한다.
    /// </summary>
    /// <param name="item">바인딩 대상 오브젝트</param>
    /// <param name="type">오브젝트 타입</param>
    private void BindItem(Transform item, DecorateType type)
    {
        if (type == DecorateType.None) type = FindType(item);
        GameObject itemObject = (type == DecorateType.Furniture ? item.parent.gameObject : item.gameObject);
        
        #region for door
        Transform d = null;
        float dir = 0;
        Vector3 angle = Vector3.zero;
        Vector3 localPosition = Vector3.zero;
        // Sliding Door용 X좌표.
        float defaultX = 0;
        float defaultY = 0;
        bool isMoving = false;
        bool isOpen = false;
        float startPos = 0, endPos = 0;
        float animationTime = 0.5f;
        float currentTime = 0;

        if (type == DecorateType.Door)
        {
            if (type == DecorateType.Door && item.name == "NoDoor(Clone)")
            {
                GameObject mesh;
                float area;
                mesh = MakeMesh.MakeQuad(item.GetChild(0).Find("WallL").gameObject, model.Floor.DefaultFloorMaterial, out area);
                mesh.transform.parent = nodoorMeshParent;
                nodoorMeshs.Add(mesh, area);
                mesh = MakeMesh.MakeQuad(item.GetChild(0).Find("WallR").gameObject, model.Floor.DefaultFloorMaterial, out area);
                mesh.transform.parent = nodoorMeshParent;
                nodoorMeshs.Add(mesh, area);
                mesh = MakeMesh.MakeQuad(item.GetChild(0).Find("Floor").gameObject, model.Floor.DefaultFloorMaterial, out area);
                mesh.transform.parent = nodoorMeshParent;
                nodoorMeshs.Add(mesh, area);
                mesh = MakeMesh.MakeQuad(item.GetChild(0).Find("Ceiling").gameObject, model.Floor.DefaultFloorMaterial, out area);
                mesh.transform.parent = nodoorMeshParent;
                nodoorMeshs.Add(mesh, area);
                return;
            }

            // 애니메이션과 클릭 이벤트가 적용될 문짝 오브젝트
            d = item.GetChild(0).GetChild(0);
            d.parent = item.transform;
            dir = item.GetChild(0).localScale.x > 0 ? 1 : -1;
            angle = d.eulerAngles;
            localPosition = d.localPosition;
            defaultX = localPosition.x;
            defaultY = angle.y;

            item.UpdateAsObservable()
                .Where(condition => isMoving)
                .Subscribe(_ =>
                {
                    if (currentTime > animationTime)
                    {
                        isMoving = false;
                        currentTime = 0;
                    }
                    else
                    {
                        currentTime += Time.deltaTime;
                        angle.y = Mathf.Lerp(startPos, endPos, currentTime / animationTime);
                        d.eulerAngles = angle;

                    }
                });
            Destroy(item.GetComponent<BoxCollider>());
            item = item.GetComponentInChildren<MeshCollider>().transform;
        }
        #endregion

        // 오브젝트 클릭시
        if (item.GetComponent<Collider>() == null) item.gameObject.AddComponent<MeshCollider>();        
        item.OnMouseDownAsObservable()
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ =>
            {
                if (selectedObject == null || selectedObject != itemObject)
                {
                    // 처음 클릭시
                    if (!selectable) return;
                    else Select(itemObject, type);
                }
                else if (selectedObject == itemObject)
                {
                    // 두번째 클릭시
                    switch (type)
                    {
                        case DecorateType.Furniture:
                            if(!selectable) SetFurniture();
                            else Cancel();
                            break;
                        case DecorateType.Door:
                            // 문 열기/닫기
                            isMoving = true;
                            if (!isOpen)
                            {
                                isOpen = true;
                                if(item.parent.name == "SwingDoor(Clone)")
                                {
                                    startPos = defaultY;
                                    endPos = defaultY + 90 * dir;
                                }
                            }
                            else
                            {
                                isOpen = false;
                                if (item.parent.name == "SwingDoor(Clone)")
                                {
                                    startPos = defaultY + 90 * dir;
                                    endPos = defaultY;
                                }
                            }
                            break;
                        default:
                            if (!selectable) return;
                            // 다른 경우에는 선택취소
                            Cancel();
                            break;
                    }
                }
            }).AddTo(this);
    }


    #endregion
    #region 시점관련

    Camera CurrentCamera()
    {
        if (fpsCamera.depth == 1) return fpsCamera;
        else if (tpsCamera.depth == 1) return tpsCamera;
        else if (topviewCamera.depth == 1) return topviewCamera;
        return null;
    }

    /// <summary>
    /// 뷰 전환시 천장 오브젝트의 재질을 변경하고(top/tps뷰에서 반투명) 천장 콜라이더를 토글하는 함수.
    /// </summary>
    /// <param name="on">천장 표시 여부</param>
    /// <param name="boxCol">천장 콜라이더 활성화 여부</param>
    void ToggleCeiling(bool on, bool boxCol=false)
    {
        // on : fps(true) / top/tps(false)
        Material material = model.CeilingMaterial;
        if (!on)
        {
            // 기존 머터리얼을 저장
            Renderer[] renderers = ceilingItemParent.GetComponentsInChildren<Renderer>();
            Dictionary<Transform, Material> materials = new Dictionary<Transform, Material>();
            foreach (var r in renderers)
            {
                if (r.name.CompareTo("Label") == 0 || (r.transform.parent != null && r.transform.parent.name.CompareTo("Label") == 0)) continue;
                if (r.material.name.Substring(0, 7).CompareTo("Ceiling") == 0) continue;
                materials.Add(r.transform, r.material);
                float alpha = material.color.a;
                Color color = r.material.color;
                color.a = alpha;
                r.material = material;
                r.material.color = color;
            }
            if (materials.Count > 0) ceilingItemsMaterial = materials;
        }
        if (on)
        {
            Renderer[] renderers = ceilingItemParent.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.name.CompareTo("Label") == 0 || (r.transform.parent != null && r.transform.parent.name.CompareTo("Label") == 0)) continue;
                if (ceilingItemsMaterial != null && ceilingItemsMaterial.ContainsKey(r.transform)) r.material = ceilingItemsMaterial[r.transform];
            }
        }

        foreach (var col in ceilingParent.GetComponentsInChildren<BoxCollider>())
        {
            col.enabled = boxCol;
        }

        // 문 콜라이더 끔
        foreach (var door in model.DoorList)
        {
            if(door.DoorModel.GetComponentInChildren<MeshCollider>()!=null) door.DoorModel.GetComponentInChildren<MeshCollider>().enabled = on;
        }
    }

    #endregion

    DecorateType FindType(Transform target)
    {
        if (target.parent == null) return DecorateType.None;
        else if (target.parent == furnitureItemParent) return DecorateType.Furniture;
        else if (target.parent == ceilingItemParent) return DecorateType.Furniture;
        else if (target.parent == ceilingParent) return DecorateType.Ceiling;
        else if (target.parent == floorsParent) return DecorateType.Floor;
        else if (target.parent == wallsParent) return DecorateType.Wall;
        else if (target.parent == doorParent) return DecorateType.Door;
        else if (target.parent == nodoorMeshParent)
        {
            if (target.name.Contains("Wall")) return DecorateType.Wall;
            else if (target.name.Contains("Floor")) return DecorateType.Floor;
            else if (target.name.Contains("Ceiling")) return DecorateType.Ceiling;
        }
        return DecorateType.None;
    }

    /// <summary>
    /// 해당 가구 오브젝트가 가능한 위치 범위를 벗어났는지 체크하는 함수.    
    /// </summary>
    /// <param name="furniture"></param>
    /// <param name="type"></param>
    /// <param name="hit">가구와 가구가 배치된 오브젝트(ex: 벽)가 맞닿는 위치와 normal 정보</param>
    /// <returns></returns>
    bool checkFuniture(Transform furniture, DecorateType type, RaycastHit hit)
    {
        if (hit.normal == null) return false;
        Vector3 delta = hit.normal * 0.05f;
        Vector3 p0 = hit.point;
        Vector3 dir = -hit.normal;

        Vector3 p1 = p0 + (furniture.GetChild(1).TransformPoint(new Vector3(-0.5f, -0.5f, -0.5f)) - furniture.transform.position);
        Vector3 p2 = p0 + (furniture.GetChild(1).TransformPoint(new Vector3(-0.5f, -0.5f, 0.5f)) - furniture.transform.position);
        Vector3 p3 = p0 + (furniture.GetChild(1).TransformPoint(new Vector3(0.5f, -0.5f, -0.5f)) - furniture.transform.position);
        Vector3 p4 = p0 + (furniture.GetChild(1).TransformPoint(new Vector3(0.5f, -0.5f, 0.5f)) - furniture.transform.position);
        

        // position +- furniture scale*0.5 위치에 lay를 쏴서 네개 다  target에 맞으면 true
        // 벽의 경우 p1~4가 바뀜
        RaycastHit _hit1, _hit2, _hit3, _hit4;
        Physics.Raycast(p1 + delta, dir, out _hit1, 0.1f);
        Physics.Raycast(p2 + delta, dir, out _hit2, 0.1f);
        Physics.Raycast(p3 + delta, dir, out _hit3, 0.1f);
        Physics.Raycast(p4 + delta, dir, out _hit4, 0.1f);
        if (_hit1.transform == null || _hit2.transform == null || _hit3.transform == null || _hit4.transform == null) return false;
        if (FindType(_hit1.transform) == type && _hit1.transform == _hit2.transform && _hit1.transform == _hit3.transform && _hit1.transform == _hit4.transform) return true;
        else return false;
    }
    #endregion
}
