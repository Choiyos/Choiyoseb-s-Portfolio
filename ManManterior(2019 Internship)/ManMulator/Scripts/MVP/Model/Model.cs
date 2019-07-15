using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Space Setting scene의 상태를 나타내는 정보 : ViewMode, EditMode
/// </summary>
public enum SettingState { ViewMode, EditMode }

/// <summary>
/// 방, 바닥, 벽, 문/창문 등 모든 정보를 담고있는 클래스.
/// 크기, 위치 등의 값은 실제 하이라키상의 Transfrom 정보를 이용한다.
/// </summary>
public partial class Model : Singleton<Model>
{
    #region Fields
    Room room = new Room();
    Floor floor = new Floor();
    SettingState state;

    GameObject textLabelPrefab;

    GameObject doorIconPrefab;

    GameObject swingDoorIconPrefab;
    GameObject slidingDoorIconPrefab;
    GameObject noDoorIconPrefab;

    GameObject windowIcon;
    GameObject defaultWindowIconPrefab;
    GameObject bigWindowIconPrefab;

    GameObject doorTargetPrefab;
    List<Door> doorList;
    List<Wall> wallList;
    List<Window> windowList;

    // 벽의 면적 정보를 담기 위해 오브젝트와 float값을 묶어서 저장
    Dictionary<Transform, float> generatedWallList = new Dictionary<Transform, float>();
    GameObject selectedElement;
    Decorator decorateContainer = new Decorator();

    GameObject doorPrefab;
    GameObject swingDoorPrefab;
    GameObject slidingDoorPrefab;
    GameObject noDoorPrefab;

    GameObject windowPrefab;
    GameObject defaltWindowPrefab;
    GameObject bigWindowPrefab;

    Material outwallMaterial;
    Material defaultMaterial;
    Material ceilingMaterial;
    
    // 공간설정 바닥 테두리에 적용할 단색 재질
    Material colorMaterialBlack;
    Material colorMaterialGray;
        
    float gridSize = 0.05f;
    #endregion

    #region Properties

    public Room Room { get => room; set => room = value; }
    public Floor Floor { get => floor; set => floor = value; }
    public SettingState State { get => state; set => state = value; }
    public GameObject DoorIcon { get => doorIconPrefab; set => doorIconPrefab = value; }
    public List<Door> DoorList { get => doorList; set => doorList = value; }
    public List<Wall> WallList { get => wallList; set => wallList = value; }
    public List<Window> WindowList { get => windowList; set => windowList = value; }
    public GameObject DoorTargetPrefab { get => doorTargetPrefab; set => doorTargetPrefab = value; }
    public GameObject SelectedElement { get => selectedElement; set => selectedElement = value; }
    public GameObject WindowIcon { get => windowIcon; set => windowIcon = value; }
    public Decorator DecorateContainer { get => decorateContainer; set => decorateContainer = value; }
    public GameObject DoorPrefab { get => doorPrefab; set => doorPrefab = value; }
    public GameObject WindowPrefab { get => windowPrefab; set => windowPrefab = value; }
    public Dictionary<Transform, float> GeneratedWallList { get => generatedWallList; set => generatedWallList = value; }
    public Material OutwallMaterial { get => outwallMaterial; set => outwallMaterial = value; }
    public Material CeilingMaterial { get => ceilingMaterial; set => ceilingMaterial = value; }
    public float GridSize { get => gridSize;}
    public GameObject SwingDoorIconPrefab { get => swingDoorIconPrefab;private set => swingDoorIconPrefab = value; }
    public GameObject SlidingDoorIconPrefab { get => slidingDoorIconPrefab; private set => slidingDoorIconPrefab = value; }
    public GameObject NoDoorIconPrefab { get => noDoorIconPrefab; private set => noDoorIconPrefab = value; }
    public GameObject SwingDoorPrefab { get => swingDoorPrefab; private set => swingDoorPrefab = value; }
    public GameObject SlidingDoorPrefab { get => slidingDoorPrefab; private set => slidingDoorPrefab = value; }
    public GameObject NoDoorPrefab { get => noDoorPrefab; private set => noDoorPrefab = value; }
    public GameObject BigWindowIconPrefab { get => bigWindowIconPrefab; private set => bigWindowIconPrefab = value; }
    public GameObject DefaultWindowIconPrefab { get => defaultWindowIconPrefab; set => defaultWindowIconPrefab = value; }
    public GameObject DefaltWindowPrefab { get => defaltWindowPrefab; set => defaltWindowPrefab = value; }
    public GameObject BigWindowPrefab { get => bigWindowPrefab; set => bigWindowPrefab = value; }
    public Material DefaultMaterial { get => defaultMaterial; set => defaultMaterial = value; }
    public Material ColorMaterialBlack { get => colorMaterialBlack; set => colorMaterialBlack = value; }
    public Material ColorMaterialGray { get => colorMaterialGray; set => colorMaterialGray = value; }
    public GameObject TextLabelPrefab { get => textLabelPrefab; set => textLabelPrefab = value; }


    #endregion

    #region Methods

    /// <summary>
    /// 모델의 초기화. 씬에서 사용될 리소스 로딩.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        DoorList = new List<Door>();
        WallList = new List<Wall>();
        WindowList = new List<Window>();


        SelectedElement = null;
        Floor.FloorPrefab = Resources.Load<GameObject>("Prefabs/Floor");
        Floor.LinePrefab = Resources.Load<GameObject>("Prefabs/UI/Line");
        TextLabelPrefab = Resources.Load<GameObject>("Prefabs/UI/TextLabel");

        SwingDoorIconPrefab = Resources.Load<GameObject>("Prefabs/Elements/SwingDoorIcon");
        SlidingDoorIconPrefab = Resources.Load<GameObject>("Prefabs/Elements/SlidingDoorIcon");
        NoDoorIconPrefab = Resources.Load<GameObject>("Prefabs/Elements/NoDoorIcon");
        doorIconPrefab = swingDoorIconPrefab;

        doorTargetPrefab = Resources.Load<GameObject>("Prefabs/Elements/DoorTarget");

        DefaultWindowIconPrefab = Resources.Load<GameObject>("Prefabs/Elements/DefaultWindowIcon");
        BigWindowIconPrefab = Resources.Load<GameObject>("Prefabs/Elements/BigWindowIcon");
        windowIcon = DefaultWindowIconPrefab;

        SwingDoorPrefab = Resources.Load<GameObject>("Prefabs/Elements/SwingDoor");
        SlidingDoorPrefab = Resources.Load<GameObject>("Prefabs/Elements/SlidingDoor");
        NoDoorPrefab = Resources.Load<GameObject>("Prefabs/Elements/NoDoor");
        DoorPrefab = swingDoorPrefab;

        DefaltWindowPrefab = Resources.Load<GameObject>("Prefabs/Elements/DefaultWindow");
        BigWindowPrefab = Resources.Load<GameObject>("Prefabs/Elements/BigWindow");
        
        Room.DisabledRoomMaterial = Resources.Load<Material>("Materials/SpaceSetting/DisabledRoomMaterial");
        Room.SelectedRoomMaterial = Resources.Load<Material>("Materials/SpaceSetting/SelectedRoomMaterial");
        Room.DefaultRoomMaterial = Resources.Load<Material>("Materials/SpaceSetting/DefaultRoomMaterial");

        Floor.SelectedFloorMaterial = Resources.Load<Material>("Materials/SpaceSetting/SelectedFloorMaterial");
        Floor.DisabledFloorMaterial = Resources.Load<Material>("Materials/SpaceSetting/DisabledFloorMaterial");
        Floor.DefaultFloorMaterial = Resources.Load<Material>("Materials/SpaceSetting/DefaultFloorMaterial");
        Floor.WarningFloorMaterial = Resources.Load<Material>("Materials/SpaceSetting/WarningFloorMaterial");

        ColorMaterialBlack = Resources.Load<Material>("Materials/SpaceSetting/Black");
        ColorMaterialGray = Resources.Load<Material>("Materials/SpaceSetting/Gray");

        OutwallMaterial = Resources.Load<Material>("Materials/DecorateSpace/OutWallMaterial");
        CeilingMaterial = Resources.Load<Material>("Materials/DecorateSpace/CeilingMaterial");
        DefaultMaterial = Resources.Load<Material>("Materials/DecorateSpace/DefaultMaterial");

        #region Decorator load

        DecorateContainer.ItemIconPrefab = Resources.Load<GameObject>("Prefabs/UI/Icon");
        DecorateContainer.EstimateIconPrefab = Resources.Load<GameObject>("Prefabs/UI/EstimateIcon");

        Object[] tmp = Resources.LoadAll("Decoration/Materials/Floor", typeof(Material));
        decorateContainer.FloorMaterials = new Material[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.FloorMaterials[i] = tmp[i] as Material;
        }

        tmp = Resources.LoadAll("Decoration/Sprites/Floor", typeof(Sprite));
        decorateContainer.FloorSprites = new Sprite[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.FloorSprites[i] = tmp[i] as Sprite;
        }

        tmp = Resources.LoadAll("Decoration/Materials/Wall", typeof(Material));
        decorateContainer.WallMaterials = new Material[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.WallMaterials[i] = tmp[i] as Material;
        }

        tmp = Resources.LoadAll("Decoration/Sprites/Wall", typeof(Sprite));
        decorateContainer.WallSprites = new Sprite[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.WallSprites[i] = tmp[i] as Sprite;
        }
        
        tmp = Resources.LoadAll("Decoration/Descriptions/Wall", typeof(TextAsset));
        decorateContainer.WallScripts = new string[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.WallScripts[i] = (tmp[i] as TextAsset).text;
        }

        DecorateContainer.CustomizeFurniture = Resources.Load<GameObject>("Decoration/Furnitures/Etc/Customize");
        DecorateContainer.CustomizeFurnitureSprite = Resources.Load<Sprite>("Decoration/Sprites/Furniture/Etc/Customize");

        tmp = Resources.LoadAll("Decoration/Furnitures/Ceiling", typeof(GameObject));
        decorateContainer.FurnitureForCeiling = new GameObject[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.FurnitureForCeiling[i] = tmp[i] as GameObject;
        }

        tmp = Resources.LoadAll("Decoration/Sprites/Furniture/Ceiling", typeof(Sprite));
        decorateContainer.FurnitureForCeilingSprites = new Sprite[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.FurnitureForCeilingSprites[i] = tmp[i] as Sprite;
        }

        tmp = Resources.LoadAll("Decoration/Furnitures/Floor", typeof(GameObject));
        decorateContainer.FurnitureForFloor = new GameObject[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.FurnitureForFloor[i] = tmp[i] as GameObject;
        }

        tmp = Resources.LoadAll("Decoration/Sprites/Furniture/Floor", typeof(Sprite));
        decorateContainer.FurnitureForFloorSprites = new Sprite[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.FurnitureForFloorSprites[i] = tmp[i] as Sprite;
        }


        tmp = Resources.LoadAll("Decoration/Furnitures/Wall", typeof(GameObject));
        decorateContainer.FurnitureForWall = new GameObject[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.FurnitureForWall[i] = tmp[i] as GameObject;
        }

        tmp = Resources.LoadAll("Decoration/Sprites/Furniture/Wall", typeof(Sprite));
        decorateContainer.FurnitureForWallSprites = new Sprite[tmp.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            decorateContainer.FurnitureForWallSprites[i] = tmp[i] as Sprite;
        }
        #endregion
    }


    #endregion
}

