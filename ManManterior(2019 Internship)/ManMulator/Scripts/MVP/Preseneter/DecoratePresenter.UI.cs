using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Decorate Presenter 중 UI(가구/벽지/바닥재/견적 등)에 해당하는 기능을 분리.
/// 오브젝트 선택, 가구 추가/조작/삭제에 대한 내용 포함.
/// </summary>
public partial class DecoratePresenter : MonoBehaviour
{
    #region Fields

    //버튼그룹
    [SerializeField]
    GameObject addItemMode = default;
    [SerializeField]
    GameObject editFurnitureMode = default;

    // 각 UI 윈도우들
    [SerializeField]
    GameObject wallMaterialView = default;
    [SerializeField]
    GameObject floorMaterialView = default;    
    [SerializeField]
    GameObject estimateView = default;
    [SerializeField]
    GameObject furnitureView = default;
    [SerializeField]
    GameObject editFurnitureView = default;
    [SerializeField]
    GameObject detailView = default;

    // 간단한 tip 도움말을 표시할 부분
    [SerializeField]
    GameObject helpView = default;

    //버튼
    [SerializeField]
    Button setMaterialWallButton = default;
    [SerializeField]
    Button setMaterialFloorButton = default;
    [SerializeField]
    Button addFurnitureButton = default;

    [SerializeField]
    Button viewEstimateButton = default;

    [SerializeField]
    Button moveFurnitureButton = default;
    [SerializeField]
    Button rotateFurnitureButtonCW = default;
    [SerializeField]
    Button rotateFurnitureButtonRCW = default;
    [SerializeField]
    Button editFurnitureButton = default;
    [SerializeField]
    Button deleteFurnitureButton = default;

    // 견적 리스트에 적용한 아이템과 견적 UI 내 아이콘을 매치
    Dictionary<GameObject, GameObject> estimateList;
    // 가구와 가구가 배치된 벽을 매치
    Dictionary<GameObject, Transform> furnitureList; 

    // 가구 편집 윈도우에서 가구 표시에 사용하는 ortho 카메라
    Transform furnitureCamera = default;

    // 수치표시 라벨 프리팹
    [SerializeField]
    GameObject textLabel = default;

    //선택
    // 화면에서 클릭되어 선택된 오브젝트
    GameObject selectedObject; 
    // 선택할 수 있는 상태인지
    bool selectable = true;
    // 목록에서 클릭되어 선택된 재질 및 가구
    Item selectedItem =null; 
        
    // 토글 윈도우 리스트
    List<UIWindow> toggleViews = null;
    #endregion
    #region Init
    /// <summary>
    /// UI 초기화
    /// </summary>
    void InitUI()
    {
        estimateList = new Dictionary<GameObject, GameObject>();
        furnitureList = new Dictionary<GameObject, Transform>();
        toggleViews = new List<UIWindow>();
        
        toggleViews.Add(new UIWindow(furnitureView, addFurnitureButton));
        toggleViews.Add(new UIWindow(floorMaterialView, setMaterialFloorButton));
        toggleViews.Add(new UIWindow(wallMaterialView, setMaterialWallButton));

        editFurnitureMode.SetActive(false);       
        
        ToggleView(toggleViews, 3);
        addItemMode.SetActive(true);
        detailView.SetActive(false);        
        estimateView.SetActive(false);
        editFurnitureView.SetActive(false);

        furnitureCamera = GameObject.Find("FurnitureCamera").transform;
        helpView.SetActive(false);
    }

    /// <summary>
    /// 가구/벽지/바닥재 윈도우에 아이콘 배치 및 UI 바인딩.
    /// Item 클래스를 사용하여 꾸미기 정보를 참조한다.
    /// </summary>
    /// <param name="view">작업할 대상 윈도우</param>
    private void LoadMaterials(GameObject view)
    {
        Transform content = view.transform.GetChild(2).GetChild(0).GetChild(0);
        
        List<Material> materials = new List<Material>();
        List<Sprite> sprites = new List<Sprite>();
        List<GameObject> furnitures = new List<GameObject>();
        List<Item> items = new List<Item>();

        // 뷰 종류에 따라 재질또는 프리팹과 UI에 사용될 sprite를 가져옴
        if (view == floorMaterialView)
        {
            materials.AddRange(model.DecorateContainer.FloorMaterials);
            sprites.AddRange(model.DecorateContainer.FloorSprites);
            items.AddRange(GetItemArray(model.DecorateContainer.FloorSprites.Length, Item.Type.Material, DecorateType.Floor));
        }
        else if (view == wallMaterialView)
        {
            materials.AddRange(model.DecorateContainer.WallMaterials);
            sprites.AddRange(model.DecorateContainer.WallSprites);
            items.AddRange(GetItemArray(model.DecorateContainer.WallSprites.Length, Item.Type.Material, DecorateType.Wall));
        }
        else if(view == furnitureView)
        {
            furnitures.AddRange(model.DecorateContainer.FurnitureForFloor);
            sprites.AddRange(model.DecorateContainer.FurnitureForFloorSprites);
            items.AddRange(GetItemArray(model.DecorateContainer.FurnitureForFloorSprites.Length, Item.Type.Furniture, DecorateType.Floor));

            furnitures.AddRange(model.DecorateContainer.FurnitureForWall);
            sprites.AddRange(model.DecorateContainer.FurnitureForWallSprites);
            items.AddRange(GetItemArray(model.DecorateContainer.FurnitureForWallSprites.Length, Item.Type.Furniture, DecorateType.Wall));

            furnitures.AddRange(model.DecorateContainer.FurnitureForCeiling);
            sprites.AddRange(model.DecorateContainer.FurnitureForCeilingSprites);
            items.AddRange(GetItemArray(model.DecorateContainer.FurnitureForCeilingSprites.Length, Item.Type.Furniture, DecorateType.Ceiling));

            // 리소스에서 가져온 가구를 추가하기 전에 사용자 정의 가구를 먼저 따로 추가함
            string name = "사용자 정의 가구";
            string code = "999";
            string description = "사용자가 정의 가구입니다.";
            GameObject icon = Instantiate(model.DecorateContainer.ItemIconPrefab, content);
            Item item = new Item(Item.Type.Furniture , DecorateType.None);                        
            item.Set(code, name, 0, icon, model.DecorateContainer.CustomizeFurnitureSprite, model.DecorateContainer.CustomizeFurniture, description);
            //UI 바인딩
            BindItemIcon(icon.transform, item);            
        }

        if (sprites == null) return;

        // 상세설명은 임시로 벽지 데이터를 돌려씀
        string[] desc = model.DecorateContainer.WallScripts;

        for (int i = 0; i < sprites.Count; i++)
        {
            int index = i;
            // 항목의 정보는 리소스 폴더 내 sprite 파일명에서 가져옴 (ex: 코드_이름_10000)
            string code = sprites[index].name.Split('_')[0];
            string name = sprites[index].name.Split('_')[1];
            int cost = int.TryParse(sprites[index].name.Split('_')[2], out cost) ? cost : 0;
            string description = desc[index % desc.Length];
            GameObject icon = Instantiate(model.DecorateContainer.ItemIconPrefab, content);      
            if (items[index].type == Item.Type.Furniture) items[index].Set(code, name, cost, icon, sprites[index], furnitures[index], description);
            else items[index].Set(code, name, cost, icon, sprites[index], materials[index], description);
            //UI 바인딩
            BindItemIcon(icon.transform, items[index]);
        }
    }

    /// <summary>
    /// 동일한 속성을 가지는 Item 클래스 배열 생성
    /// </summary>
    /// <param name="length">배열 길이</param>
    /// <param name="type">아이템 타입(가구 or 재질)</param>
    /// <param name="subType">아이템 적용 타입(벽 or 천장 or 바닥)</param>
    /// <returns>생성한 아이템배열</returns>
    private Item[] GetItemArray(int length, Item.Type type, DecorateType subType)
    {
        Item[] _items = new Item[length];
        for (int i = 0; i < length; i++)
        {
            _items[i] = new Item(type, subType);
        }
        return _items;
    }

    /// <summary>
    /// 아이템 아이콘 UI 바인딩.
    /// 아이콘에 이미지, 속성을 적용하고, 클릭 시 아이템을 적용하는 기능을 바인딩한다.
    /// </summary>
    /// <param name="icon">바인딩할 대상 아이콘</param>
    /// <param name="item">아이템 정보</param>
    private void BindItemIcon(Transform icon, Item item)
    {        
        // 미리보기 이미지 sprite    
        icon.GetChild(0).GetComponent<Image>().sprite = item.Sprite;

        // 버튼을 클릭하면 해당 아이템을 선택한 상태로 바꾸고, 아이템이 적용할 수 있는 부분을 클릭시 아이템을 적용한다.
        icon.GetChild(0).GetComponent<Button>().OnClickAsObservable()
            .Where(_ => selectable)
            .Subscribe(_ =>
            {
                if (selectedItem !=null && selectedItem.SubType != DecorateType.None) selectedItem.Select(false);
                selectedItem = item;
                item.Select(true);
                ApplyItem(item);
                // 아이템 선택 시 상세정보 창이 뜨면서 선택한 항목 정보를 띄움
                OpenDetailView(item);
            });

        // 이름
        icon.GetChild(1).GetComponent<Text>().text = item.Name; 
        //icon.GetChild(2) 즐찾 버튼 ( 미구현 )

        //정보 버튼을 클릭하면 상세정보 창을 보여준다 (현재 선택한 상태와 동일하여 추후 삭제하거나 변경 필요)
        icon.GetChild(3).GetComponent<Button>().OnClickAsObservable()
            .Subscribe(_ =>
            {
                OpenDetailView(item);
            });
        // 코드 정보는 보이지 않지만 자식 오브젝트 내에 정보를 숨겨서 나중에 참조할 수 있도록 한다.
        icon.Find("Code").GetComponent<Text>().text = item.Code;
    }

    /// <summary>
    /// 상세 정보 창을 띄움. Item 클래스에서 참조할 항목의 정보를 받아와서 적용한다.
    /// </summary>
    /// <param name="item">상세정보를 참조한 항목</param>
    private void OpenDetailView(Item item)
    {
        detailView.SetActive(true);
        detailView.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = item.Sprite;
        detailView.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = item.Description;
    }

    #endregion

    #region Bind

    /// <summary>
    /// 씬 상에서 마우스/키보드 입력을 받기 위해 추가한 함수.
    /// Decorate Presenter(main)의 카메라 update바인딩 부분에 낑겨넣음. 추후 수정해도 될거같음.
    /// 씬 상에서 우클릭시 취소, 키보드 F12를 누르면 디버그 토글.
    /// </summary>
    private void BindCameraForUI()
    {
        if (Input.GetButtonDown("Fire2") && selectable)
        {
            Cancel();
        }

        // for debug
        if (Input.GetKeyDown(KeyCode.F12))
        {
            isDebugMode = !isDebugMode;
            debugMode.SetActive(isDebugMode);
        }
    }

    /// <summary>
    /// 버튼 및 윈도우 UI 바인딩하는 함수.
    /// 가구/벽지/바닥재 윈도우의 로딩 및 바인딩. 가구 편집 창 내부의 UI 바인딩 등이 실행됨.
    /// </summary>
    void BindUI()
    {
        BindView(wallMaterialView);
        BindView(floorMaterialView);
        BindView(estimateView);
        BindView(furnitureView);
        BindView(editFurnitureView, editFurnitureMode);

        // 가구/벽지/바닥재 아이콘 로딩
        LoadMaterials(furnitureView);
        LoadMaterials(wallMaterialView);
        LoadMaterials(floorMaterialView);

        //가구 편집 윈도우 관련 바인딩
        #region editFurnitureView bind

        Transform editFurniturePanel = editFurnitureView.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0);
        Toggle nameLabelToggle = editFurniturePanel.GetChild(0).Find("Toggle").GetComponent<Toggle>();
        InputField inputName = editFurniturePanel.GetChild(0).Find("InputField").GetComponent<InputField>();
        InputField inputX = editFurniturePanel.GetChild(1).Find("InputField").GetComponent<InputField>();
        InputField inputY = editFurniturePanel.GetChild(3).Find("InputField").GetComponent<InputField>();
        InputField inputZ = editFurniturePanel.GetChild(2).Find("InputField").GetComponent<InputField>();

        // 컬러단추 클릭시 해당 가구에 해당 색 적용
        Transform colorList = editFurniturePanel.GetChild(4).GetChild(1);
        InputField inputColor = editFurniturePanel.GetChild(5).Find("InputField").GetComponent<InputField>();
        for (int i = 0; i < colorList.childCount; i++)
        {
            Color color = colorList.GetChild(i).GetComponent<Image>().color;
            colorList.GetChild(i).GetComponent<Button>().OnClickAsObservable()
                .Subscribe(_ =>
                {
                    SetFurnitureColor(color);
                    inputColor.text = ColorUtility.ToHtmlStringRGB(color);
                });
        }
        // 색(숫자) 입력이 끝나면 가구에 색을 적용
        inputColor.onEndEdit.AddListener(delegate {
            Color color = Color.white;
            if (ColorUtility.TryParseHtmlString("#"+inputColor.text, out color))
            {   
                SetFurnitureColor(color);
            }
            else
            {
                inputColor.text = ColorUtility.ToHtmlStringRGB(selectedObject.GetComponentInChildren<Renderer>().material.color);
            }
        });

        // 적용 버튼 클릭시 가구 수치 및 이름 적용
        editFurnitureView.transform.GetChild(3).GetChild(0).GetComponent<Button>().OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (float.TryParse(inputX.text, out float x)) UpdateFurnitureSize(x, 0);
                if (float.TryParse(inputY.text, out float y)) UpdateFurnitureSize(y, 1);
                if (float.TryParse(inputZ.text, out float z)) UpdateFurnitureSize(z, 2);
                selectedObject.transform.GetComponentInChildren<Text>().text = inputName.text;
                editFurnitureView.SetActive(false);
                editFurnitureMode.SetActive(true);
                SetLayer(selectedObject, 0);
            });

        // 가구 이름 라벨 표시 토글
        nameLabelToggle.onValueChanged.AddListener(delegate
        {
            selectedObject.transform.Find("Label").gameObject.SetActive(nameLabelToggle.isOn);
        });

        // 이름, 각 수치 입력이 끝나면 적용
        inputName.onEndEdit.AddListener(delegate {           
            selectedObject.transform.GetComponentInChildren<Text>().text = inputName.text;
        });
        inputX.onEndEdit.AddListener(delegate {
            if (!float.TryParse(inputX.text, out float x)) { inputX.text = Util.GetGridLabel(selectedObject.transform.GetChild(2).localScale.x); return; }
            UpdateFurnitureSize(x, 0);
            inputX.text = Util.GetGridLabel(x);
        });
        inputY.onEndEdit.AddListener(delegate {
            if (!float.TryParse(inputY.text, out float y)) { inputY.text = Util.GetGridLabel(selectedObject.transform.GetChild(2).localScale.y); return; }
            UpdateFurnitureSize(y, 1);
            inputY.text = Util.GetGridLabel(y);
        });
        inputZ.onEndEdit.AddListener(delegate {
            if (!float.TryParse(inputZ.text, out float z)) { inputZ.text = Util.GetGridLabel(selectedObject.transform.GetChild(2).localScale.z); return; }
            UpdateFurnitureSize(z, 2);
            inputZ.text = Util.GetGridLabel(z);
        });

        #endregion
        
        viewEstimateButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                estimateView.SetActive(true);
            });

        setMaterialFloorButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ToggleView(toggleViews, 1);
            });

        setMaterialWallButton.OnClickAsObservable()
            .Subscribe(_ =>
            {                
                ToggleView(toggleViews, 2);                
            });

        addFurnitureButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ToggleView(toggleViews, 0);
            });

        #region Detail view
        // detailview 목록으로 돌아가기 버튼
        detailView.transform.GetChild(0).Find("BackButton").GetComponent<Button>().OnClickAsObservable()
            .Subscribe(_ =>
            {
                detailView.SetActive(false);
                CancelItem();
            });
        detailView.transform.GetChild(0).Find("ExitButton").GetComponent<Button>().OnClickAsObservable()
            .Subscribe(_ =>
            {
                detailView.SetActive(false);
                CancelItem();
            }); 
        #endregion
        #region Furniture edit mode

        moveFurnitureButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ShowHelp("가구를 드래그하여 이동하세요");
                Highlight(SelectMode.Edit);
                selectable = false;
                editFurnitureMode.SetActive(false);
            });

        rotateFurnitureButtonCW.OnClickAsObservable()
            .Subscribe(_ =>
            {   
                selectedObject.transform.GetChild(1).Rotate(Vector3.up * 45);
                Highlight(SelectMode.Select);
            });

        rotateFurnitureButtonRCW.OnClickAsObservable()
            .Subscribe(_ =>
            {
                selectedObject.transform.GetChild(1).Rotate(Vector3.up * -45);
                Highlight(SelectMode.Select);
            });

        editFurnitureButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Vector3 localscale = selectedObject.transform.GetChild(1).localScale;
                nameLabelToggle.isOn = selectedObject.transform.Find("Label").gameObject.activeSelf;
                inputX.text = localscale.x.ToString();
                inputY.text = localscale.y.ToString();
                inputZ.text = localscale.z.ToString();
                inputName.text = selectedObject.transform.GetChild(0).GetChild(0).GetComponent<Text>().text;
                inputColor.text = ColorUtility.ToHtmlStringRGB(selectedObject.GetComponentInChildren<Renderer>().material.color);                
                AttachFurnitureCam();

                editFurnitureView.SetActive(true);
                editFurnitureMode.SetActive(false);
                addItemMode.SetActive(true);
            });

        deleteFurnitureButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                GameObject deleteItem = selectedObject;
                Cancel();
                if (estimateList.ContainsKey(deleteItem))
                {
                    GameObject delIcon = estimateList[deleteItem];
                    estimateList.Remove(deleteItem);
                    Destroy(delIcon);
                }
                furnitureList.Remove(deleteItem);
                Destroy(deleteItem);
                CalculateEstimate();
                editFurnitureMode.SetActive(false);
                addItemMode.SetActive(true);

            });

        #endregion
    }

    /// <summary>
    /// 윈도우 종료버튼 클릭 시 작동 바인딩
    /// </summary>
    /// <param name="view">적용할 윈도우</param>
    /// <param name="toggleObject">창을 닫을 때 활성화될 오브젝트</param>
    private void BindView(GameObject view, GameObject toggleObject=null)
    {
        Button button = view.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        button.OnClickAsObservable()
            .Subscribe(_ =>
            {                                
                //가구 변경 창을 종료할 때에는 가구에 적용되었던 설정을 되돌리고 선택취소
                if(view == editFurnitureView) SetLayer(selectedObject, 0);
                if (toggleViews.FindAll(x => x.view == view).Count > 0)
                {
                    CancelItem();
                    ToggleView(toggleViews, 3);
                    return;
                }
                Cancel();
                if (toggleObject != null) toggleObject.SetActive(true);
                button.transform.parent.parent.gameObject.SetActive(false);
            });
    }

    #endregion

    #region Methods

    #region Furniture
    
    /// <summary>
    /// 가구 추가하는 함수.
    /// 가구 리스트에서 추가할 가구 아이콘을 클릭 시 실행됨.
    /// 해당하는 가구가 적용되는 위치(ex: 의자의 경우 바닥)에 마우스가 이동하면 해당 위치에 가구가 따라감.
    /// 원하는 위치에서 클릭하면 가구가 그 위치에 적용됨.
    /// 그 전에 오른쪽 클릭이나 다른 버튼을 클릭하면 취소됨.
    /// </summary>
    /// <param name="_item">추가할 가구 정보</param>
    private void AddFurniture(Item _item)
    {        
        DecorateType type = _item.SubType;
        string name = _item.Name;
        ShowHelp("가구를 놓을 위치를 클릭하세요");        
        // 천장 가구인데 현재 top/tps뷰일 경우 가구 배치를 위해 천장 콜라이더를 켬
        ToggleCeilingCollider(type, true);
        GameObject item = Instantiate(_item.Furniture);
        item.SetActive(false);
        item.transform.parent = type == DecorateType.Ceiling? ceilingItemParent: furnitureItemParent;
        item.name = name;

        // 가구 오브젝트가 laycast를 받지 않도록 레이어 변경
        SetLayer(item, 2);
        foreach (var i in furnitureList) { SetLayer(i.Key, 2); }        
        AttachLabel(item, name, (type == DecorateType.None ? true : false));
        
        selectedObject = item;

        // 가구 배치 완료되었는지
        bool settle = false;

        RaycastHit hit;

        var updateDisposable = new SingleAssignmentDisposable();
        var updateDisposable2 = new SingleAssignmentDisposable();

        // 가구 배치 상태에서 마우스를 따라다니다가 적절한 위치에 클릭이 되면 종료.
        updateDisposable.Disposable = this.UpdateAsObservable()
        .Select(move => CurrentCamera().ScreenPointToRay(Input.mousePosition))
        .Where(__ => !selectable && !EventSystem.current.IsPointerOverGameObject())        
        .Subscribe(move =>
        {
            // 마우스 위치를 따라 가구 이동
            Physics.Raycast(move, out hit, 100);
            SettleFurniture(type, item, hit);

            // 클릭 시 가구 배치
            if (Input.GetButtonDown("Fire1"))
            {
                Physics.Raycast(move, out hit, 100);
                if (hit.transform != null)
                {
                    settle = true;
                    selectable = true;                    
                    BindItem(item.transform.GetChild(1), DecorateType.Furniture);
                    furnitureList.Add(item, hit.transform);

                    foreach (var i in furnitureList) { SetLayer(i.Key, 0); }
                    ToggleCeilingCollider(type, false);

                    Select(item, DecorateType.Furniture);                    
                    CancelItem();
                    detailView.SetActive(false);
                    updateDisposable2.Dispose();
                    updateDisposable.Dispose();

                }
            }

            // 중간에 오른쪽 클릭시 가구 추가 취소
            if (Input.GetButtonDown("Fire2")) selectable = true;          
        });
        
        // 다른 UI 클릭 등 외부요인에 의해 가구 추가 취소시 정리
        updateDisposable2.Disposable = this.UpdateAsObservable()
            .Where(__ => selectable && !settle)
            .Subscribe(__ => 
            {
                foreach (var i in furnitureList) { SetLayer(i.Key, 0); }
                ToggleCeilingCollider(type, false);

                Destroy(item);
                selectedObject = null;
                CancelItem();
                detailView.SetActive(false);
                updateDisposable.Dispose();
                updateDisposable2.Dispose();
            });
    }


    /// <summary>
    /// 가구의 타입에 맞는 위치에 마우스가 위치하면, 그 부분에 가구를 배치하는 함수.
    /// 벽 가구의 경우, normal 방향을 확인하여 회전 후 적용.
    /// </summary>
    /// <param name="type">가구 배치되는 종류(ex: 벽지)</param>
    /// <param name="item">적용할 가구 오브젝트</param>
    /// <param name="hit">마우스가 가리키는 지점 정보</param>
    private void SettleFurniture(DecorateType type, GameObject item, RaycastHit hit)
    {
        if (hit.transform == null) return;
        DecorateType hitType = FindType(hit.transform);        
        if (type != DecorateType.None && type != hitType) return;
        // 마우스가 가리키는 타입이 None일 경우(사용자가구) 마우스가 가리키는 지점을 가구의 타입으로 한다.
        if (type == DecorateType.None)
        {
            if(!(hitType == DecorateType.Ceiling || hitType == DecorateType.Wall || hitType == DecorateType.Floor)) return;
            type = hitType;
        }
        if(type != FindType(hit.transform)) return;

        item.transform.position = Util.GetGridPos(hit.point);

        // 타입에 따라 normal을 고려하여 가구 오브젝트에 회전 적용.
        switch (type)
        {
            case DecorateType.Floor:
                item.transform.eulerAngles = Vector3.zero;
                break;
            case DecorateType.Wall:
                float angle = 0;
                // normal 방향에 따라 y 축 회전
                if (hit.normal == -Vector3.forward) angle = 0;
                if (hit.normal == -Vector3.right) angle = 90;
                if (hit.normal == Vector3.forward) angle = 180;
                if (hit.normal == Vector3.right) angle = 270;
                item.transform.eulerAngles = Vector3.right * -90 + Vector3.up * angle;                
                break;
            case DecorateType.Ceiling:
                item.transform.eulerAngles = Vector3.right * 180;
                break;                        
        }
        if (!item.activeSelf) item.SetActive(true);
        Highlight(SelectMode.Edit, item.transform, type);
    }

    /// <summary>
    /// 가구 오브젝트에 이름을 나타낼 라벨 오브젝트 붙임.
    /// </summary>
    /// <param name="item">가구 오브젝트</param>
    /// <param name="name">가구 이름</param>
    /// <param name="type">가구 타입</param>
    private void AttachLabel(GameObject item, string name, bool type)
    {
        GameObject label = Instantiate(textLabel, item.transform);
        label.name = "Label";
        Vector2 offset = new Vector2(1, -1);
        Text text = label.GetComponentInChildren<Text>();
        text.text = name;        
        label.transform.SetSiblingIndex(0);

        // 현재 뷰의 종류에 따라 글자 크기와 각도를 적용.
        label.UpdateAsObservable()
            .Subscribe(___ =>
            {
                label.transform.position = item.transform.GetChild(1).position;

                if (CurrentCamera() == fpsCamera)
                {
                    text.fontSize = 10;
                    label.transform.eulerAngles = CurrentCamera().transform.eulerAngles;
                }
                else if (CurrentCamera() == tpsCamera)
                {
                    text.fontSize = (int) CurrentCamera().fieldOfView;
                    label.transform.eulerAngles = new Vector3(90, CurrentCamera().transform.eulerAngles.y, CurrentCamera().transform.eulerAngles.z);
                }
                else if (CurrentCamera() == topviewCamera)
                {
                    text.fontSize = (int)(CurrentCamera().orthographicSize * 5f);
                    label.transform.eulerAngles = new Vector3(90, CurrentCamera().transform.eulerAngles.y, 0);
                }
                // 글자 라벨에 적용된 외곽선 offset을 크기에 따라 적용
                text.GetComponent<Outline>().effectDistance = offset * text.fontSize * 0.05f;

            });
        if (!type) label.gameObject.SetActive(false);
    }

    /// <summary>
    /// 가구 위치 이동.
    /// </summary>
    void SetFurniture()
    {
        // 가구가 배치된 오브젝트(ex: 의자가 놓여진 방 바닥)
        Transform target = furnitureList[selectedObject];
        DecorateType type = FindType(target);
        Vector3 scale = selectedObject.transform.GetChild(1).localScale;

        // 천장가구(조명)일 경우, 현재 뷰에 따라 필요시 천장 콜라이더 킴
        ToggleCeilingCollider(type, true);        
        foreach (var item in furnitureList) { SetLayer(item.Key, 2); }

        // 가구 드래그 시 가구 이동
        var update = new SingleAssignmentDisposable();
        update.Disposable = target.UpdateAsObservable()
            .Select(move => CurrentCamera().ScreenPointToRay(Input.mousePosition))
            .TakeUntil(selectedObject.transform.GetChild(1).OnMouseUpAsObservable())
            .DoOnCompleted(() =>
            {
                foreach (var item in furnitureList) { SetLayer(item.Key, 0); }
                selectable = true;
                Highlight(SelectMode.Select);
                editFurnitureMode.SetActive(true);
                ToggleCeilingCollider(type, false);
                update.Dispose();
            })
            .Subscribe(move =>
            {
                Physics.Raycast(move, out RaycastHit hit, 100);
                SettleFurniture(type, selectedObject, hit);
            });
    }

    /// <summary>
    /// 뷰의 종류에 따라 천장 콜라이더 toggle.
    /// 천장에 적용되는 가구(전등)를 top/tps에서 추가/편집할 경우 천장 콜라이더를 사용해야 함.
    /// </summary>
    /// <param name="type">현재 가구 타입</param>
    /// <param name="on">활성화 상태</param>
    private void ToggleCeilingCollider(DecorateType type, bool on)
    {
        if (type == DecorateType.Ceiling && CurrentCamera() != fpsCamera)
        {
            ToggleCeiling(on, on);
        }
    }

    /// <summary>
    /// 가구 편집 윈도우의 크기 값을 반영하여 가구 크기 변경.
    ///  item : 위치
    ///   ㄴ label : 라벨링
    ///   ㄴ model : 크기, 회전
    /// </summary>
    /// <param name="value"></param>
    /// <param name="index"></param>
    void UpdateFurnitureSize(float value, int index)
    {
        Vector3 scale = selectedObject.transform.GetChild(1).localScale;
        scale[index] = Util.GetGridValue(value);
        selectedObject.transform.GetChild(1).localScale = scale;     
        selectedObject.transform.GetChild(1).localPosition = Vector3.up * scale.y * 0.5f;

        // 변경된 가구 크기에 맞춰서 가구를 비추는 ortho 카메라 재설정
        AttachFurnitureCam();
    }

    /// <summary>
    /// 가구 편집 윈도우에서 선택한 색으로 가구 색 설정.
    /// </summary>
    /// <param name="color">변경할 가구 색</param>
    private void SetFurnitureColor(Color color)
    {
        MeshRenderer[] renderers = selectedObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            if (r.name.CompareTo("Label") == 0 || (r.transform.parent != null && r.transform.parent.name.CompareTo("Label") == 0)) continue;
            r.material.color = color;
        }
    }

    /// <summary>
    /// UI에 모델을 비출 ortho 카메라를 설정.
    /// </summary>
    private void AttachFurnitureCam()
    {
        // 선택한 오브젝트가 ortho카메라에 보일 수 있도록 layer 설정
        SetLayer(selectedObject, 2);
        Transform target = selectedObject.transform.GetChild(1);
        furnitureCamera.parent.parent.position = target.position;

        // 가구 타입에 따른 회전 각도 적용
        if(FindType(furnitureList[selectedObject]) == DecorateType.Wall)
        {
            furnitureCamera.parent.parent.eulerAngles = target.parent.eulerAngles;
            furnitureCamera.parent.localEulerAngles = target.localEulerAngles;
        }
        else
        {
            furnitureCamera.parent.parent.localEulerAngles = target.localEulerAngles;
        }
        // 가구 크기에 따라 뷰 크기 조절
        furnitureCamera.GetComponent<Camera>().orthographicSize =
            Mathf.Max(
                target.localScale.x,
                target.localScale.y,
                target.localScale.z
            );
    }

    #endregion

    /// <summary>
    /// 선택한 항목 적용.<br></br>
    /// 1.항목의 종류가 재질일 경우 <br></br>
    /// 1-1.그리고 선택된 오브젝트가 없을 경우 : 다음부터 클릭한 오브젝트에 항목 적용<br></br>
    /// 1-2.선택된 오브젝트가 있을경우: 같은 타입이면 적용, 아니면 경고문 띄움<br></br>
    /// 2. 항목의 종류가 가구일 경우 : 가구를 추가한다.
    /// </summary>
    /// <param name="_item">적용할 항목</param>
    private void ApplyItem(Item _item)
    {        
        if (_item.type == Item.Type.Material)
        {
            // 항목의 종류가 재질이고 선택 되어있는 오브젝트가 없다면, 다음에 클릭한 (해당종류의)오브젝트에 항목 적용
            //(ex: 벽지 선택 후 벽 선택 시 벽지 적용)
            if (selectedObject == null)
            {
                if (_item.SubType == DecorateType.Wall) ShowHelp("선택한 재질을 적용할 벽을 선택하세요");
                else if (_item.SubType == DecorateType.Floor) ShowHelp("선택한 재질을 적용할 바닥을 선택하세요");

                if (selectedItem.SubType != DecorateType.None) selectedItem.Select(false);
                selectedItem = _item;
                selectedItem.Select(true);
                
                return;
            }

            // 선택되어 있는 오브젝트가 있고, 항목과 해당 오브젝트의 종류가 같으면 해당 아이템 적용.
            DecorateType type = FindType(selectedObject.transform);
            if (type == _item.SubType || (type == DecorateType.Ceiling && _item.SubType== DecorateType.Wall))
            {
                // 해당 오브젝트(ex:벽)의 면적을 찾아 평단가로 계산 후 견적에 반영한다.
                float area = 0;               
                if(selectedObject.transform.parent == nodoorMeshParent) area = nodoorMeshs[selectedObject];
                else if(type == DecorateType.Floor) area = model.Room.MergedRooms[selectedObject.transform];
                else if(type == DecorateType.Wall) area = model.GeneratedWallList[selectedObject.transform];
                else if(type == DecorateType.Ceiling) area = model.Room.MergedRooms[GameObject.Find("MergedRooms").transform.GetChild(selectedObject.transform.GetSiblingIndex())];
                //1제곱미터 = 0.3025평
                float totalArea = (area * 0.3025f);
                AddItem(selectedObject, _item.Name + " (" + totalArea.ToString("0.00") + "㎡)", _item.Cost * totalArea, _item.Code); // 평당 가격으로  환산
                selectedObject.GetComponent<MeshRenderer>().material = _item.Material;
            }
            else
            {
                // 선택된 아이템과 오브젝트가 맞지 않을 경우 경고문 출력
                if (_item.SubType == DecorateType.Wall) ShowHelp("벽이 아닙니다");
                else if (_item.SubType == DecorateType.Floor) ShowHelp("바닥이 아닙니다");
            }
        }
        else
        {
            // 항목이 가구일 경우 가구를 추가한다
            if (!selectable) return;
            selectable = false;
            AddFurniture(_item);
            _item.Select(false);
            selectedItem = null;
        }

    }

    #region Select

    /// <summary>
    /// 오브젝트를 선택한 경우 처리.
    /// 1. 가구를 선택한 경우 : 가구 편집 버튼 그룹 표시<br></br>
    /// 2. 벽/천장/바닥을 선택한 경우 : 해당 오브젝트에 적용할 수 있는 벽지/바닥재 윈도우 표시
    /// </summary>
    /// <param name="item">선택한 오브젝트</param>
    /// <param name="type">오브젝트 타입</param>
    void Select(GameObject item, DecorateType type)
    {
        // 천장에도 벽지 적용하기 위해 조건문 추가
        if (selectedItem != null && selectedItem.SubType != DecorateType.None 
            && (type != selectedItem.SubType && !(type == DecorateType.Ceiling && selectedItem.SubType == DecorateType.Wall)))
        {
            DebugMsg("타입이 안맞음 : "+ selectedItem.SubType+ " &&" + type);
            return;
        }
        Item currentItem = selectedItem;        
        Cancel();
        selectedObject = item;
        selectedItem = currentItem;

        // 가구일 경우 토글 뷰를 모두 닫고 가구 편집 버튼 그룹을 표시.
        if(type == DecorateType.Furniture)
        {
            ToggleView(toggleViews, 3, true);
            addItemMode.SetActive(false);
            editFurnitureMode.SetActive(true);
        }
        else
        {
            //나머지(천장/벽/바닥)일 경우 해당 오브젝트에 적용할 수 있는 재질(벽지or 바닥재) 리스트 윈도우를 켬
            if (selectedItem == null) ToggleView(toggleViews,type == DecorateType.Ceiling? 2 : (int)type);
            else ApplyItem(selectedItem);
        }
        Highlight(SelectMode.Select);
    }

    /// <summary>
    /// 선택 취소 처리.
    /// </summary>
    void Cancel()
    {        
        if (selectedObject == null) return;
        if (!selectable) selectable = true;
        // 선택시 적용했던 아웃라인 효과 제거
        MeshRenderer[] renderers = selectedObject.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Destroy(renderers[i].GetComponent<cakeslice.Outline>());
        }
        // 선택시 열렸던 윈도우 창 닫기
        editFurnitureView.SetActive(false);
        SetLayer(selectedObject, 0);
        selectedObject = null;
        addItemMode.SetActive(true);
        editFurnitureMode.SetActive(false);

    }

    /// <summary>
    /// 선택한 오브젝트에 아웃라인 효과 주기. 
    /// 가구의 경우 조건검사를 통해 범위를 벗어났을 때 빨간색(NotAble), 이동 중일때 파란색(Edit)으로 표시.
    /// </summary>
    /// <param name="mode">선택모드(select or edit)</param>
    /// <param name="target">가구 오브젝트의 경우 검사할 참조 오브젝트/다른 오브젝트의 경우 선택 오브젝트 자신</param>
    /// <param name="type">가구 오브젝트 검사시 사용할 가구 타입</param>
    /// <param name="check">조건검사 수행 여부</param>
    private void Highlight(SelectMode mode, Transform target = null, DecorateType type = DecorateType.None, bool check = true)
    {
        if (selectedObject == null) return;
        if (target == null) target = selectedObject.transform;

        //가구인 경우 검사를 수행하고 모드를 결정(지정한 선택 모드 or NotAble)
        if (FindType(target) == DecorateType.Furniture && check)
        {            
            Vector3 p0 = target.position;
            Vector3 dir = p0 - target.GetChild(1).position;
            Vector3 delta = -dir.normalized * 0.05f;
            Physics.Raycast(p0 + delta, dir, out RaycastHit hit, 0.1f);
            if (type == DecorateType.None && furnitureList.ContainsKey(target.gameObject)) type = FindType(furnitureList[target.gameObject]);
            if (!checkFuniture(target, type, hit)) mode = SelectMode.NotAble;            
        }

        // 모드에 맞는 색으로 아웃라인 효과 지정
        MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            if (r.name.CompareTo("Label") == 0 || (r.transform.parent != null && r.transform.parent.name.CompareTo("Label") == 0)) continue;
            if (r.gameObject.GetComponent<cakeslice.Outline>() == null) r.gameObject.AddComponent<cakeslice.Outline>();
            r.gameObject.GetComponent<cakeslice.Outline>().color = (int)mode;
        }
    }

    /// <summary>
    /// 지정한 오브젝트및 자식 오브젝트에 레이어를 적용.
    /// laycast를 무시하거나, 가구 편집 윈도우에서 ortho 카메라에 비추기 위해 사용.
    /// </summary>
    /// <param name="target">레이어를 변경할 오브젝트</param>
    /// <param name="layer">변경할 레이어</param>
    private void SetLayer(GameObject target, int layer)
    {
        target.layer = layer;
        for (int i = 0; i < target.transform.childCount; i++)
        {
            if (target.transform.GetChild(i).name.CompareTo("Label") == 0
                || (target.transform.GetChild(i).transform.parent != null
                && target.transform.GetChild(i).transform.parent.name.CompareTo("Label") == 0)) continue;
            target.transform.GetChild(i).gameObject.layer = layer;
        }
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r.name.CompareTo("Label") == 0 || (r.transform.parent != null && r.transform.parent.name.CompareTo("Label") == 0)) continue;
            r.gameObject.layer = layer;
        }
    }


    #endregion
    #region Estimate
    /// <summary>
    /// 재질 및 가구 추가시 견적에 반영하고 견적 UI 아이콘 바인딩.
    /// </summary>
    /// <param name="item">추가할 항목</param>
    /// <param name="name">추가할 항목의 이름</param>
    /// <param name="cost">가격</param>
    /// <param name="code">코드</param>
    void AddItem(GameObject item, string name, float cost, string code)
    {
        // 이미 견적에 해당 오브젝트에 적용된 사항이 있다면 제거하고 적용(ex. 이미 다른 벽지가 적용된 벽에 새 벽지 적용시 이전벽지 삭제)
        if (estimateList.ContainsKey(item))
        {
            if(estimateList[item].transform.Find("Code").GetComponent<Text>().text != code)
            {
                GameObject delIcon = estimateList[item];
                estimateList.Remove(item);
                Destroy(delIcon);
            }else
            {
                return;
            }            
        }        
        // 견적 아이콘 생성 및 바인딩
        Transform estIcon = Instantiate(model.DecorateContainer.EstimateIconPrefab, estimateView.transform.GetChild(2).GetChild(0).GetChild(0)).transform;
        estimateList.Add(item, estIcon.gameObject);        
        estIcon.GetChild(0).GetComponent<Text>().text = name;
        estIcon.GetChild(1).GetComponent<Text>().text = Util.ToCost((int)Mathf.Round(cost));
        // 삭제 버튼
        estIcon.GetChild(2).GetComponent<Button>().OnClickAsObservable()
            .Subscribe(_ =>
            {
                DeleteItem(item);
            });
        // 선택 토글
        estIcon.GetChild(3).GetComponent<Toggle>().onValueChanged.AddListener(delegate {
            CalculateEstimate();
        });
        estIcon.Find("Code").GetComponent<Text>().text = code;
        // 새 항목 추가 후 견적 다시 계산
        CalculateEstimate();
    }

    /// <summary>
    /// 견적에서 아이템 삭제 시 실제 아이템도 삭제 적용하는 함수.
    /// </summary>
    /// <param name="item">삭제 대상 오브젝트</param>
    private void DeleteItem(GameObject item)
    {
        Cancel();
        GameObject delIcon = estimateList[item];
        estimateList.Remove(item);
        if (FindType(item.transform) == DecorateType.Furniture)
        {
            // 가구라면 삭제       
            furnitureList.Remove(item);
            Destroy(item);
        }
        else
        {   
            // 벽지라면 원복               
            item.GetComponent<MeshRenderer>().material = model.Floor.DefaultFloorMaterial;            
        }
        // 계산 오류발생으로 의도적 지연 후 다시 견적 계산
        Invoke("CalculateEstimate", 0.0001f);
        Destroy(delIcon);
    }

    /// <summary>
    /// 견적에 있는 모든 항목 합산 후 견적 윈도우에 업데이트
    /// </summary>
    void CalculateEstimate()
    {
        int totalEstimate = 0;
        foreach (var estIcon in estimateList.Values)
        {
            if (estIcon.transform.GetChild(3).GetComponent<Toggle>().isOn)
            {
                int cost = Util.GetCost(estIcon.transform.GetChild(1).gameObject.GetComponent<Text>().text);
                totalEstimate += cost;
            }
        }
        estimateView.transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<Text>().text = Util.ToCost(totalEstimate);
    }
    #endregion

    /// <summary>
    /// 도움말 윈도우에 간단한 tip이나 메시지를 표시하는 함수.
    /// </summary>
    /// <param name="msg">표시할 메시지</param>
    void ShowHelp(string msg)
    {
        helpView.SetActive(true);
        helpView.GetComponentInChildren<Text>().text = msg;        
        Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(_ =>
        {
            helpView.SetActive(false);
        }).AddTo(this);
    }

    /// <summary>
    /// 벽지/바닥재/가구 윈도우 활성화 토글하는 함수.
    /// </summary>
    /// <param name="toggleViews">토글대상 윈도우 리스트</param>
    /// <param name="index">활성화할 윈도우 인덱스</param>
    /// <param name="noCancel">창 닫지 않음 옵션</param>
    void ToggleView(List<UIWindow> toggleViews, int index, bool noCancel = false)
    {
        if (selectedObject != null && index == (int)FindType(selectedObject.transform)) noCancel = true;

        int i = 0;        
        foreach (var view in toggleViews)
        {
            if (i++ == index && view.State) continue;
            view.Toggle(false);
        }
        detailView.SetActive(false);
        if (!noCancel) Cancel();
        CancelItem();

        if (index < toggleViews.Count)
        {
            if (noCancel) toggleViews[index].Toggle(true);
            else toggleViews[index].Toggle();
        }
    }

    /// <summary>
    /// 선택한 항목(재질,가구) 취소 처리.
    /// </summary>
    private void CancelItem()
    {        
        if (!selectable) selectable = true;
        if (selectedItem == null) return;
        selectedItem.Select(false);
        selectedItem = null;
        Cancel();
    }

    #endregion

}
