using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 여러 스크립트에서 공통적으로 사용되거나, 로직이 길어서 본문 스크립트에서 분리하고 싶은 함수를 모아놓은 클래스.
/// </summary>
public static partial class Util
{
    #region 수치 표시

    /// <summary>
    /// <b>호출</b> : SpacePresenter.ShowFloorsLabel() <br></br>
    /// <b>참조</b> : AttachLabels(), Visualize(), Outliner.FindAllOutline() <br></br>
    /// 방의 각 벽 수치를 표시하는 함수.
    /// Outliner에서 방의 외곽선을 이루는 점 리스트를 받아오고, 각 점을 이어서 길이를 두 점 사이에 표시한다.
    /// </summary>
    /// <param name="room">수치 표시를 나타낼 방</param>
    /// <param name="labelTransform">라벨 오브젝트를 담을 부모 오브젝트</param>
    /// <param name="textPrefab">글자를 나타낼 프리팹</param>
    /// <param name="linePrefab">치수선 프리팹</param>
    /// <param name="isDebug">디버그 옵션</param>
    /// <param name="option">치수 표현 옵션(0 : 방 바깥쪽에 표시, 1: 벽 가운데에 표시, 2: 방 내부에 표시)</param>
    public static List<Transform> ShowRoomSize(List<Transform> subFloors, Transform labelTransform, GameObject textPrefab, GameObject linePrefab, bool isDebug = false, int option = 0)
    {
        List<Transform> labels = new List<Transform>();
        List<Dot> allDots = new List<Dot>();
        Outliner outliner = new Outliner();
        Transform roomLabelParent = new GameObject("RoomLabel").transform;
        roomLabelParent.parent = labelTransform;
        try
        {
            List<List<Dot>> outlineAllDots = outliner.FindAllOutline(subFloors, isDebug);
            foreach (var outlineDots in outlineAllDots)
            {
                AttachLabels(outlineDots, roomLabelParent, textPrefab, linePrefab, option);
                allDots.AddRange(outlineDots.ToArray());
            }

            return labels;
        }
        catch (System.Exception)
        {
            //Debug.Log(e);
        }

        // 디버그 옵션을 위한 부분 : 점 시각화를 통해 점 생성이 정상적인지 체크.
        Transform debugParent = GameObject.Find("DebugParent").transform;
        if (isDebug && debugParent != null)
        {
            for (int i = 0; i < debugParent.childCount; i++)
            {
                GameObject.Destroy(debugParent.GetChild(i).gameObject);
            }
            debugParent.position = Vector3.zero;
            Util.Visualize(allDots, debugParent);
            debugParent.position = Vector3.up * 3f;
        }

        return null;
    }

    #endregion
    #region 벽 생성
    /// <summary>
    /// <b>호출</b> : SpacePresneter.MakeWallsAndFloors()<br></br>
    /// <b>참조</b> : IsClockwise(), FindRotateAngle(), CalcPadding(), Outliner.FindAllOutline() <br></br>
    /// 요소설정 단계에서 문/창문 설치를 위해 병합된 모든 방의 벽 윗부분(WallTop)을 생성하는 부분.
    /// Outliner로 각 방의 외곽선을 이루는 점을 수집한 뒤,
    /// 각 점 중간 부분에 두 점 사이 길이를 갖는 벽 오브젝트를 quad로 생성한다.
    /// 벽이 서로 직각으로 생성되는 경우 두 벽 사이에 빈 공간이 발생할 수 있는데, 
    /// 이 부분을 메꾸기 위해 정사각형의 wallPoint를 추가로 생성하여 배치한다.
    /// 벽의 방향을 비교하여 가로 벽인지 세로 벽인지 구분하여 요소설정 단계에서 이용할 수 있도록 모델 클래스에 정보를 저장한다.
    /// </summary>
    /// <param name="rooms">모든 방 리스트</param>
    /// <param name="border">벽 두께</param>
    /// <param name="height">벽 높이</param>
    /// <returns>생성된 WallTop 오브젝트들이 담긴 부모 오브젝트</returns>
    public static GameObject MakeWallTops(List<Transform> rooms, float border, float height)
    {
        // 오브젝트들을 담을 부모 오브젝트 생성        
        // 실제로 꾸미기가 가능한 내벽을 제외한 나머지 벽 오브젝트를 담을 부모 모브젝트
        Transform outwalls = new GameObject().transform;
        // 벽 윗부분
        Transform wallTopParent = new GameObject().transform;
        // 벽 윗부분 중에서 모서리 점 부분
        Transform wallPointParent = new GameObject().transform;
        outwalls.parent = rooms[0].parent;
        wallTopParent.parent = outwalls;
        wallPointParent.parent = outwalls;
        outwalls.name = "OutWalls";
        wallTopParent.name = "WallsTop";
        wallPointParent.name = "Points";

        foreach (var room in rooms)
        {
            List<Transform> subFloors = new List<Transform>();
            for (int i = 0; i < room.childCount; i++)
            {
                subFloors.Add(room.GetChild(i));
            }
            Outliner outliner = new Outliner();
            List<List<Dot>> allDots = outliner.FindAllOutline(subFloors);
            foreach (var dots in allDots)
            {
                //Visualize(dots, "outlineDots");

                float beforeAngle = 0;

                for (int i = 0; i < dots.Count; i++)
                {
                    Vector3 dot1 = dots[i].position;
                    Vector3 dot2 = dots[(i + 1) % dots.Count].position;

                    Transform wallTop = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;

                    wallTop.Rotate(Vector3.right * 90);

                    // 두 점의 x,z값을 비교하여 벽의 회전 각도(0,90,180,270)를 구한다
                    float angle = FindRotateAngle(dot1, dot2);
                    // 각도에 따라 두께만큼 이동할 방향을 계산한다.
                    Vector3 padding = CalcPadding(angle);

                    wallTop.localScale = new Vector3(Mathf.Max(Mathf.Abs(dot1.x - dot2.x), border), Mathf.Max(Mathf.Abs(dot1.z - dot2.z), border), height);
                    wallTop.GetComponent<Renderer>().material.color = Color.black;
                    wallTop.position = (dot1 + dot2) * 0.5f + Vector3.up * height + padding * border * 0.5f;
                    wallTop.parent = wallTopParent;
                    wallTop.GetComponent<Renderer>().material.color = Color.black;
                    wallTop.GetComponent<Renderer>().receiveShadows = false;
                    wallTop.gameObject.AddComponent<BoxCollider>();
                    wallTop.GetComponent<BoxCollider>().size = new Vector3(wallTop.localScale.x == border ? 2 : 1, wallTop.localScale.x == border ? 1 : 2, 0);
                    // 모델의 벽 리스트에 생성된 벽을 방향 정보와 함께 추가한다.
                    Model.Instance.WallList.Add(
                        new Model.Wall(wallTop.gameObject, wallTop.localScale.x == border
                        ? Model.Wall.WallDirection.Vertical : Model.Wall.WallDirection.Landscape));

                    //이전 angle과 비교했을때 반시계 방향으로 생성되면 -> 땜빵을 메꾸기 위해 가운데 모서리 생성            
                    if (i == 0)
                    {
                        beforeAngle = FindRotateAngle(dots[(i - 1 + dots.Count) % dots.Count].position, dots[i].position);
                    }
                    if (IsClockwise(angle, beforeAngle))
                    {
                        Transform point = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                        point.Rotate(Vector3.right * 90);
                        point.localScale = Vector3.one * border;
                        Vector3 padding2 = CalcPadding((angle + 90) % 360);
                        point.position = dot1 + Vector3.up * height + (padding - padding2) * border * 0.5f;
                        point.GetComponent<Renderer>().material.color = Color.black;
                        point.GetComponent<Renderer>().receiveShadows = false;
                        // 요소설정 단계에서 벽의 끝부분을 감지하기 위해 collider를 추가한다.
                        BoxCollider box = point.gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
                        box.size = new Vector3(1, 1.1f, 1.1f);
                        box = point.gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
                        box.size = new Vector3(1.1f, 1, 11.1f);
                        point.parent = wallPointParent;
                        point.tag = "CorssPoint";
                    }
                    beforeAngle = angle == -90 ? 270 : angle;
                }
            }

        }
        return wallTopParent.gameObject;
    }

    /// <summary>
    /// <b>호출</b> : ElementPresenter.MakeWall()<br></br>
    /// <b>참조</b> : CalcPadding(),FindRotateAngle(), MakeMesh.MakeWalls(), Outliner.FindAllOutline()<br></br>
    /// 문/창문 위치 반영한 벽 생성 함수.
    /// Outliner로 방의 외곽선 점을 얻은 다음, 
    /// 그 점들을 따라 quad로 기준이 될 벽을 생성하고,
    /// MakeMesh.MakeWalls()으로 기준 벽에 문/창문 오브젝트의 정보를 반영하여 새로운 벽 오브젝트를 생성한 뒤,
    /// 생성된 오브젝트와 그 벽의 면적을 딕셔너리로 묶어서 저장하여 반환한다.
    /// </summary>
    /// <param name="rooms">모든 방 리스트</param>
    /// <param name="elements">모든 문/창문 오브젝트 리스트</param>
    /// <param name="height">벽 높이</param>
    /// <param name="material">생성된 벽에 적용할 기본 재질</param>
    /// <param name="wallParent">생성된 벽 오브젝트를 담을 부모 오브젝트</param>
    /// <param name="isDebug">디버그 옵션</param>
    /// <returns>생성된 벽 오브젝트와 벽의 면적을 담은 딕셔너리</returns>
    public static Dictionary<Transform, float> MakeWalls(List<Transform> rooms, List<Transform> elements, float height, Material material, out Transform wallParent, bool isDebug = false)
    {
        Dictionary<Transform, float> generatedWallList = new Dictionary<Transform, float>();
        wallParent = new GameObject().transform;
        wallParent.transform.parent = rooms[0].parent;
        wallParent.name = "Walls";
        foreach (var room in rooms)
        {
            // 방 외곽선을 이루는 점 수집
            List<Transform> subFloors = new List<Transform>();
            for (int i = 0; i < room.childCount; i++)
            {
                subFloors.Add(room.GetChild(i));
            }
            Outliner outliner = new Outliner();
            List<List<Dot>> allDots = outliner.FindAllOutline(subFloors);

            // 외곽선을 따라 벽 오브젝트 생성
            foreach (var dots in allDots)
            {
                for (int i = 0; i < dots.Count; i++)
                {
                    Vector3 dot1 = dots[i].position;
                    Vector3 dot2 = dots[(i + 1) % dots.Count].position;

                    // quad로 기준 벽을 생성
                    Transform wall = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                    float angle = FindRotateAngle(dot1, dot2);
                    Vector3 padding = CalcPadding(angle);
                    wall.position = (dot1 + dot2) * 0.5f + Vector3.up * height * 0.5f;
                    wall.GetComponent<Renderer>().material.color = Color.black;
                    wall.localScale = new Vector3(Mathf.Max(Mathf.Abs(dot1.x - dot2.x), Mathf.Abs(dot1.z - dot2.z)), height, 1);
                    wall.Rotate(Vector3.up * angle);

                    // 기준 벽과 문/창문 오브젝트 정보를 이용하여 새로운 벽 오브젝트 생성
                    Transform madeWall = MakeMesh.MakeWalls(wall, elements, material, out float area, isDebug).transform;
                    generatedWallList.Add(madeWall, area);

                    madeWall.parent = wallParent;
                    GameObject.Destroy(wall.gameObject);
                }
            }
        }
        return generatedWallList;
    }

    /// <summary>
    /// <b>호출</b> : ElementPresenter.MakeWall()<br></br>
    /// <b>참조</b> : FindRotateAngle(), MakeMesh.MakeWalls(), Outliner.FindAllOutline()<br></br>
    /// 외곽벽을 생성하는 함수.
    /// Outliner를 이용하여 전체 방의 외곽 점을 수집한 뒤, 점사이 공간에 quad로 임시 벽을 생성한다.
    /// 임시벽의 위치, 크기, 회전을 참조해서 문/창문 오브젝트를 고려한 벽을 MakeMesh 클래스를 이용하여 생성한다.
    /// </summary>
    /// <param name="allFloors">전체 바닥 정보</param>
    /// <param name="elements">문/창문 리스트</param>
    /// <param name="border">벽 두께</param>
    /// <param name="height">벽 높이</param>
    /// <param name="material">외곽벽 생성시 적용할 기본 오브젝트</param>
    /// <returns>생성된 외곽벽 오브젝트를 담은 부모 오브젝트</returns>
    public static GameObject MakeOutlineWalls(List<Transform> allFloors, List<Transform> elements, float border, float height, Material material)
    {
        // 위치,크기,회전 참조를 위해 quad로 임시 생성한 벽 오브젝트를 담을 부모 오브젝트
        Transform wallParent = new GameObject().transform;
        wallParent.name = "OutlineWalls";

        // 문/창문 오브젝트를 고려하여 다시 생성한 벽 오브젝트를 담을 부모 오브젝트
        Transform outlinewallParent = new GameObject().transform;
        outlinewallParent.name = "OutlineWalls";

        // 전체 방 외곽 점 수집
        Outliner outliner = new Outliner();
        List<List<Dot>> allDots = outliner.FindAllOutline(allFloors);

        foreach (var dots in allDots)
        {
            for (int i = 0; i < dots.Count; i++)
            {
                Vector3 dot1 = dots[i].position;
                Vector3 dot2 = dots[(i + 1) % dots.Count].position;
                float angle = FindRotateAngle(dot1, dot2);

                // 임시 벽 생성
                Transform wall = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                wall.position = (dot1 + dot2) * 0.5f + Vector3.up * height * 0.5f;
                wall.localScale = new Vector3(Mathf.Max(Mathf.Abs(dot1.x - dot2.x), Mathf.Abs(dot1.z - dot2.z)), height, 1);
                wall.Rotate(Vector3.up * ((angle + 180) % 360));
                wall.parent = wallParent;

                // 문/창문 고려한 벽 생성
                Transform madeWall = MakeMesh.MakeWalls(wall, elements, material, out float area).transform;
                madeWall.parent = outlinewallParent;
                madeWall.GetComponent<Renderer>().receiveShadows = false;
            }
        }

        GameObject.Destroy(wallParent.gameObject);
        return outlinewallParent.gameObject;
    }

    #endregion      
    #region Axis

    /// <summary>
    /// 축 인덱스를 받아서 해당 축의 방향벡터를 반환하는 함수. 
    /// 함수 내에서 축 종류를 int형 인덱스로 처리하기 위해 사용.
    /// TransformPoint에 사용하기 위해 편의상 0.5f값을 곱해줌.
    /// </summary>
    /// <param name="index">축 인덱스 번호 (x=0, y=1, z=2)</param>
    /// <returns>해당 축 방향벡터</returns>
    public static Vector3 GetDirVector(int index)
    {
        switch (index)
        {
            case 0: return Vector3.right * 0.5f;
            case 1: return Vector3.up * 0.5f;
            case 2: return Vector3.forward * 0.5f;
            default: return Vector3.zero;
        }
    }

    /// <summary>
    /// 벽 생성시 사용하는 함수. 
    /// 벽을 이루는 두 선분이 순서대로 맞닿아 있을 때, 
    /// 두 선분의 각도를 통해 방향이 시계/반시계 방향인지 판단하는 함수.
    /// </summary>
    /// <param name="angle">현재 선분의 회전 각도</param>
    /// <param name="beforeAngle">이전 선분의 회전 각도</param>
    /// <returns>true - 시계방향 / false - 반시계방향</returns>
    static bool IsClockwise(float angle, float beforeAngle)
    {        
        if (beforeAngle == 0 && angle == 90) return true;
        if (beforeAngle == 90 && angle == 180) return true;
        if (beforeAngle == 180 && angle == 270) return true;
        if (beforeAngle == 270 && angle == 0) return true;
        return false;
    }
    
    /// <summary>
    /// 벽 생성시 각도에 따라 벽 두께만큼 원래 위치에서 이동해야 하는데,
    /// 그 방향(padding vector)을 계산해서 반환하는 함수.
    /// </summary>
    /// <param name="angle">현재 선분의 회전 각도</param>
    /// <returns>원래 위치 이동해야 하는 방향 벡터</returns>
    private static Vector3 CalcPadding(float angle)
    {
        Vector3 padding = Vector3.zero;
        if (angle == 0) padding = new Vector3(0, 0, 1);
        if (angle == 180) padding = new Vector3(0, 0, -1);
        if (angle == 270) padding = new Vector3(-1, 0, 0);
        if (angle == 90) padding = new Vector3(1, 0, 0);
        return padding;
    }
        
    /// <summary>
    /// 두 점이 이루는 선분의 y축 회전 각도를 반환하는 함수.
    /// </summary>
    /// <param name="dot1">첫번째 좌표</param>
    /// <param name="dot2">두번째 좌표</param>
    /// <returns>두 선분이 이루는 y축 각도</returns>
    private static float FindRotateAngle(Vector3 dot1, Vector3 dot2)
    {
        float angle = 0;
        if (IsBig(dot2.x, dot1.x)) { angle = 0; }
        if (IsBig(dot1.x, dot2.x)) { angle = 180; }
        if (IsBig(dot2.z, dot1.z)) { angle = 270; }
        if (IsBig(dot1.z, dot2.z)) { angle = 90; }
        return angle;
    }

    /// <summary>
    /// 오브젝트의 y축 회전 각도로 메인 축 인덱스(x = 0, z = 0) 찾기
    /// </summary>
    /// <param name="element">방향을 참조할 오브젝트</param>
    /// <returns>
    /// 0 or 2
    /// </returns>
    public static int Index(Transform element)
    {
        return (element.rotation.eulerAngles.y == 0 || element.rotation.eulerAngles.y == 180) ? 0 : 2;
    }
    #endregion
    #region compare

    /// <summary>
    /// float 값 비교를 위한 함수.
    /// a와 b 값이 같은지 비교. 
    /// 벽 두께(border)를 고려해서 벽 두께 만큼 떨어져 있어도 같다고 판정.
    /// (공간설정 바닥 검사를 위한 비교 시 벽 두께를 고려하기 위해 추가.)
    /// </summary>
    /// <param name="a">비교할 첫번째 값</param>
    /// <param name="b">비교할 두번째 값</param>
    /// <param name="border"> 고려할 벽 두께 </param>
    /// <returns>true - 같음 / false - 다름</returns>
    public static bool IsEquals(float a, float b, float border = 0)
    {
        return Mathf.Abs(a - b) < (0.0001f + border);
    }

    /// <summary>
    /// float 값 비교를 위한 함수.
    /// a가 b보다 큰 지 비교.
    /// 벽 두께(border)를 고려해서 벽 두께 만큼 떨어져 있어도 같다고 판정.
    /// (공간설정 바닥 검사를 위한 비교 시 벽 두께를 고려하기 위해 추가.)
    /// </summary>
    /// <param name="a">비교할 첫번째 값</param>
    /// <param name="b">비교할 두번째 값</param>
    /// <param name="border"> 고려할 벽 두께</param>
    /// <returns> true or false </returns>
    public static bool IsBig(float a, float b, float border = 0)
    {
        return (a - b) > (IsEquals(border, 0) ? 0.0001f : border - 0.0001f);
    }

    #endregion
    #region convert

    /// <summary>
    /// 설정된 그리드 값으로 좌표 값을 반올림하여 적용하는 함수.
    /// 기본 그리드 값 = 0.05m
    /// </summary>
    /// <param name="position">대상 좌표</param>
    /// <param name="gridSize">그리드 단위 값</param>
    /// <returns>그리드가 적용된 좌표</returns>
    public static Vector3 GetGridPos(Vector3 position, float gridSize = 0.05f)
    {
        position.x = GetGridValue(position.x, gridSize);
        position.y = GetGridValue(position.y, gridSize);
        position.z = GetGridValue(position.z, gridSize);
        return position;
    }

    /// <summary>
    /// float 값을 지정된 그리드 단위로 반올림하는 함수.
    /// 기본 그리드 값 = 0.05m
    /// </summary>
    /// <param name="value">변환할 대상 값</param>
    /// <param name="gridSize">그리드 크기</param>
    /// <returns>그리드 단위로 반올림된 float 값</returns>
    public static float GetGridValue(float value, float gridSize = 0.05f)
    {
        value = Mathf.Round(value / gridSize) * gridSize;
        return Mathf.Round(value * 1000f) / 1000f;
    }

    /// <summary>
    /// 바닥의 transform 정보를 Floor 클래스로 변환하는 클래스.
    /// 추후 Floor 클래스 안에 정리하는 것이 나을 듯.
    /// </summary>
    /// <param name="curFloor">변환할 바닥 transfrom</param>
    /// <returns>바닥 정보를 갖는 floor 클래스 오브젝트 </returns>
    public static Floor ConvertFloorToFloor(Transform curFloor)
    {
        return new Floor(
                        curFloor.position.x - curFloor.localScale.x / 2
                        , curFloor.position.z - curFloor.localScale.z / 2
                        , Mathf.Abs(curFloor.localScale.x)
                        , Mathf.Abs(curFloor.localScale.z));
    }

    /// <summary>
    /// 부모 transfrom을 받아 자식 transform 리스트를 반환하는 함수.
    /// 방에 속하는 바닥 리스트를 불러올 때 사용.
    /// </summary>
    /// <param name="room">자식 리스트를 받아올 부모</param>
    /// <returns>자식 transform 리스트</returns>
    public static List<Transform> GetChildTransform(Transform room)
    {
        List<Transform> floors = new List<Transform>();
        for (int i = 0; i < room.childCount; i++)
        {
            floors.Add(room.GetChild(i));
        }
        return floors;
    }

    /// <summary>
    /// 모든 방 리스트에서 모든 방 바닥을 불러오는 함수.
    /// 전체 방 외곽 벽 생성시 사용.
    /// </summary>
    /// <param name="rooms">모든 방 리스트</param>
    /// <returns>모든 바닥 리스트</returns>
    public static List<Transform> GetAllFloors(List<Transform> rooms)
    {
        List<Transform> allFloors = new List<Transform>();
        foreach (var room in rooms)
        {
            allFloors.AddRange(GetChildTransform(room));
        }
        return allFloors;
    }
        
    /// <summary>
    /// int 금액을 string 원으로 환산
    /// 견적에서 사용.
    /// </summary>
    /// <param name="value">int 금액</param>
    /// <returns>string 금액 (ex : "1,000 원")</returns>
    public static string ToCost(int value)
    {
        return string.Format("{0:#,###}", value) + " 원";
    }
    /// <summary>
    /// string 금액에서 int 금액 환산
    /// 견적 계산시 사용
    /// </summary>
    /// <param name="cost">string 금액</param>
    /// <returns>int 금액</returns>
    public static int GetCost(string cost)
    {
        return int.TryParse(Regex.Replace(cost, @"\D", string.Empty), out int value) ? value : 0;
    }
        
    /// <summary>
    /// 숫자 값을 소수점(2자리)을 가진 string으로 변환.
    /// 수치 라벨 표시에 사용.
    /// </summary>
    /// <param name="value">표시할 수치</param>
    /// <returns>포맷이 적용된 표시할 값 값</returns>
    public static string GetGridLabel(float value)
    {
        return string.Format("{0:0.##}", GetGridValue(value));
    }
    #endregion
    #region check
    /// <summary>
    /// 바닥 두 개의 변이 나란히 붙어있는 지 검사하는 함수.
    /// 벽 두께를 고려해서 벽두께만큼 떨어져있으면 붙어있다고 간주한다.
    /// </summary>
    /// <param name="r1">검사할 첫번째 바닥</param>
    /// <param name="r2">검사할 두번째 바닥</param>
    /// <param name="border">벽두께</param>
    /// <returns>true - 두 바닥이 나란히 붙어있음 / false - 변이 붙어있지 않음</returns>
    public static bool GetSide(Floor r1, Floor r2, float border)
    {
        if (IsEquals(r1.x, (r2.x + r2.w), border) && (IsBig((r2.y + r2.h), r1.y) && IsBig((r1.y + r1.h), r2.y))) return true;
        if (IsEquals((r1.x + r1.w), r2.x, border) && (IsBig((r2.y + r2.h), r1.y) && IsBig((r1.y + r1.h), r2.y))) return true;
        if (IsEquals(r1.y, (r2.y + r2.h), border) && (IsBig((r2.x + r2.w), r1.x) && IsBig((r1.x + r1.w), r2.x))) return true;
        if (IsEquals((r1.y + r1.h), r2.y, border) && (IsBig((r2.x + r2.w), r1.x) && IsBig((r1.x + r1.w), r2.x))) return true;
        return false;
    }

    /// <summary>
    /// 두 바닥이 서로 꼭지점끼리만 붙어있는지 검사.
    /// </summary>
    /// <param name="r1">검사할 첫번째 바닥</param>
    /// <param name="r2">검사할 두번째 바닥</param>
    /// <returns>true - 두 바닥이 꼭지점으로만 붙어있음 / false - 그렇지 않음</returns>
    static bool IsCorner(Floor r1, Floor r2)
    {
        if (IsEquals(r1.x, (r2.x + r2.w)) && IsEquals(r1.y, (r2.y + r2.h))) return true;
        if (IsEquals(r1.x, (r2.x + r2.w)) && IsEquals((r1.y + r1.h), r2.y)) return true;
        if (IsEquals((r1.x + r1.w), r2.x) && IsEquals(r1.y, (r2.y + r2.h))) return true;
        if (IsEquals((r1.x + r1.w), r2.x) && IsEquals((r1.y + r1.h), r2.y)) return true;
        return false;
    }
    
    /// <summary>
    /// 두 바닥이 겹쳐져 있는지 검사.
    /// 모서리만 겹치는 것은 겹치는 것으로 간주하지 않는다.
    /// 바닥검사의 두 경우에서 사용.<br></br>
    /// 1. 같은 방 내의 바닥끼리 검사 : 두 바닥의 내부가 겹쳐야만 true.<br></br>
    /// 2. 서로 다른 방 내의 바닥끼리 검사 : 두 바닥의 변과 내부가 겹치면 true,
    /// </summary>
    /// <param name="r1">검사할 첫번째 바닥</param>
    /// <param name="r2">검사할 두번째 바닥</param>
    /// <param name="includeEdge">변을 포함하여 검사할지 여부</param>
    /// <returns>겹침 여부 true or false</returns>
    public static bool GetIntersect(Floor r1, Floor r2, bool includeEdge = false)
    {
        float border = 0.1f;
        if (IsCorner(r1, r2)) return false;
        // includeEdge ? true : 모서리 border 포함 + 내부 겹침 / false : 꼭지점/모서리 제외 내부만 겹침
        if (includeEdge && GetSide(r1, r2, border)) return true;
        if (IsBig(r1.x, (r2.x + r2.w))) return false;
        if (IsBig(r2.x, (r1.x + r1.w))) return false;
        if (IsBig(r1.y, (r2.y + r2.h))) return false;
        if (IsBig(r2.y, (r1.y + r1.h))) return false;
        return true;
    }


    /// <summary>
    /// 점 p가 오브젝트 내에 포함되는지 확인
    /// 오브젝트의 회전을 고려하여 메인축과 sub축을 찾아서 최대최소값을 구하고, 각 축에서 점의 좌표값과 비교한다.
    /// </summary>
    /// <param name="cube">정육면체 부피를 가지는 오브젝트</param>
    /// <param name="point">검사할 점</param>
    /// <param name="index">검사할 메인 축</param>
    /// <returns>포함여부 true or false</returns>
    public static bool GetIntersect(Transform cube, Vector3 point, int index)
    {
        // 검사하기 위한 두번째 축 찾기
        int index2 = index == 0 ? 2 : 0;
        // 메인축,sub축에 따른 최대/최소 값
        float max1 = Mathf.Max(cube.TransformPoint(Vector3.right * 0.5f)[index], cube.TransformPoint(Vector3.right * -0.5f)[index]);
        float maxY = Mathf.Max(cube.TransformPoint(Vector3.up * 0.5f).y, cube.TransformPoint(Vector3.up * -0.5f).y);
        float max2 = Mathf.Max(cube.TransformPoint(Vector3.forward * 0.5f)[index2], cube.TransformPoint(Vector3.forward * -0.5f)[index2]);
        float min1 = Mathf.Min(cube.TransformPoint(Vector3.right * 0.5f)[index], cube.TransformPoint(Vector3.right * -0.5f)[index]);
        float minY = Mathf.Min(cube.TransformPoint(Vector3.up * 0.5f).y, cube.TransformPoint(Vector3.up * -0.5f).y);
        float min2 = Mathf.Min(cube.TransformPoint(Vector3.forward * 0.5f)[index2], cube.TransformPoint(Vector3.forward * -0.5f)[index2]);

        if (IsBig(point[index], max1) || IsBig(min1, point[index])) return false;
        if (IsBig(point.y, maxY) || IsBig(minY, point.y)) return false;
        if (IsBig(point[index2], max2) || IsBig(min2, point[index2])) return false;
        return true;
    }
    /// <summary>
    /// 점이 바닥의 변 위치에 있는지 검사.
    /// </summary>
    /// <param name="r1">검사할 바닥</param>
    /// <param name="point">검사할 점</param>
    /// <returns>변 위치에 있는지 여부 true or false</returns>
    public static bool GetSide(Floor r1, Vector3 point)
    {
        if (IsEquals(r1.x, point.x) && (IsBig(point.z, r1.y) && IsBig((r1.y + r1.h), point.z))) return true;
        if (IsEquals((r1.x + r1.w), point.x) && (IsBig(point.z, r1.y) && IsBig((r1.y + r1.h), point.z))) return true;
        if (IsEquals(r1.y, point.z) && (IsBig(point.x, r1.x) && IsBig((r1.x + r1.w), point.x))) return true;
        if (IsEquals(r1.y + r1.h, point.z) && (IsBig(point.x, r1.x) && IsBig((r1.x + r1.w), point.x))) return true;
        return false;
    }

    /// <summary>
    /// 바닥 내부에 점이 겹치는지 검사
    /// </summary>
    /// <param name="floor">검사할 바닥</param>
    /// <param name="point">검사할 점</param>
    /// <returns>겹침 여부 true or false</returns>
    public static bool GetIntersect(Floor floor, Vector3 point)
    {
        if (IsBig(floor.x, point.x)) return false;
        if (IsBig(point.x, (floor.x + floor.w))) return false;
        if (IsBig(floor.y, point.z)) return false;
        if (IsBig(point.z, (floor.y + floor.h))) return false;
        return true;
    }

    /// <summary>
    /// 점이 바닥의 꼭지점 자리에 있는지 검사
    /// </summary>
    /// <param name="floor">검사할 바닥</param>
    /// <param name="point">검사할 점</param>
    /// <returns>겹침 여부 true or false</returns>
    public static bool IsCorner(Floor floor, Vector3 point)
    {
        if (IsEquals(floor.x, point.x) && IsEquals(floor.y, point.z)) return true;
        if (IsEquals(floor.x, point.x) && IsEquals((floor.y + floor.h), point.z)) return true;
        if (IsEquals((floor.x + floor.w), point.x) && IsEquals(floor.y, point.z)) return true;
        if (IsEquals((floor.x + floor.w), point.x) && IsEquals((floor.y + floor.h), point.z)) return true;
        return false;
    }

    /// <summary>
    /// 점이 바닥의 모서리에 있는것이 아닌지 검사.
    /// 점을 중심으로 작은 사각형을 만들고 각 꼭지점을 검사해서 전체가 바닥에 걸리는지 검사.
    /// </summary>
    /// <param name="floors">검사할 바닥 리스트</param>
    /// <param name="point">검사할 점</param>
    /// <returns>점이 모서리에 있는지 여부에 따라 true of false</returns>
    public static bool CheckIsDotOutside(List<Floor> floors, Vector3 point)
    {
        bool ur = false, ul = false, dr = false, dl = false;
        ur = floors.FindAll(x => Util.GetIntersect(x, point + Vector3.right * 0.05f + Vector3.forward * 0.05f)).Count > 0;
        dr = floors.FindAll(x => Util.GetIntersect(x, point + Vector3.right * 0.05f + Vector3.back * 0.05f)).Count > 0;
        ul = floors.FindAll(x => Util.GetIntersect(x, point + Vector3.left * 0.05f + Vector3.forward * 0.05f)).Count > 0;
        dl = floors.FindAll(x => Util.GetIntersect(x, point + Vector3.left * 0.05f + Vector3.back * 0.05f)).Count > 0;
        return ur && dr && ul && dl;
    }
        
    /// <summary>
    /// 바닥 검사에 사용.
    /// 모든 바닥의 연결정보 리스트를 탐색해서 모든 인덱스가 한 덩어리로 연결되어있는지 검사.
    /// 모든 바닥이 연결되어 있는 경우(섬 공간이 없는 경우)에는 모든 인덱스가 한 덩어리로 연결되어 있다.
    /// </summary>
    /// <param name="connected">바닥 연결 정보 리스트</param>
    /// <returns>바닥이 모두 붙어있으면(한 덩어리이면) true, 아니면 false</returns>
    public static bool FindGroupNumber(List<Queue<int>> connected)
    {
        bool[] visited = new bool[connected.Count];
        int start = 0;
        visited[start] = true;
        int current;

        // 현재 바닥(start)에 연결된 바닥(current) 탐색
        Queue<int> q = connected[start];
        while (q.Count > 0)
        {
            current = q.Dequeue();
            if (!visited[current])
            {
                visited[current] = true;
                // current에 연결된 바닥(item) 중 아직 탐색되지 않은 바닥 추가
                foreach (var item in connected[current])
                {
                    if (!visited[item] && !q.Contains(item)) q.Enqueue(item);
                }
            }
        }

        // 덩어리 1개일때만 true. 2개 이상이면 false 
        // -> 한번 탐색했을때 모든 바닥이 방문되지 않으면 2덩어리 이상이므로 false
        int sum = 0;
        foreach (var item in visited)
        {
            if (item) sum++;
        }
        //Debug.Log(sum +"/"+visited.Length);
        // 탐색이 끝났는데 전체 탐색을 못한거면 섬 공간이 존재한다는 뜻이므로 false
        if (sum < visited.Length) return false;
        else return true;
    }
    #endregion
    #region Apply

    /// <summary>
    /// 좌측 하단으로 정의된 좌표를 실제 바닥 오브젝트에 적용하는 함수.
    /// </summary>
    /// <param name="floor">대상 바닥 오브젝트</param>
    /// <param name="position">좌측 하단 기준으로 정의된 좌표</param>
    /// <param name="gridSize">좌표값 반올림을 위한 그리드 크기</param>
    public static void SetFloorPosition(GameObject floor, Vector3 position, float gridSize)
    {
        Vector3 deltaVector = Vector3.right * 0.5f * floor.transform.localScale.x
            + Vector3.forward * 0.5f * floor.transform.localScale.z;
        Vector3 positionLD = GetGridPos(position + deltaVector, gridSize);
        floor.transform.position = GetGridPos(positionLD - deltaVector, gridSize * 0.5f);
    }

    /// <summary>
    /// 자식 오브젝트의 모든 재질을 지정된 재질로 변경
    /// </summary>
    /// <param name="parent">변경할 부모 오브젝트</param>
    /// <param name="material">변경할 재질</param>
    public static void SetMaterial(GameObject parent, Material material)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r.name.CompareTo("Label") == 0 || (r.transform.parent != null && r.transform.parent.name.CompareTo("Label") == 0)) continue;
            r.material = material;
        }
    }

    /// <summary>
    /// mesh를 반대 방향(-normal)으로 뒤집는 함수.
    /// 병합되어 생성된 바닥 mesh를 뒤집어서 천장 오브젝트를 생성할 때 사용.
    /// </summary>
    /// <param name="mesh">뒤집을 대상 mesh</param>
    /// <returns>뒤집기가 적용된 mesh</returns>
    public static Mesh FilpMesh(Mesh mesh)
    {
        // filp normal                            
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
            normals[i] = -normals[i];
        mesh.normals = normals;

        for (int m = 0; m < mesh.subMeshCount; m++)
        {
            int[] triangles = mesh.GetTriangles(m);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i + 0];
                triangles[i + 0] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
            mesh.SetTriangles(triangles, m);
        }
        return mesh;
    }
    #endregion
}
