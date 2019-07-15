using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 라벨 표시 및 디버그를 위한 시각화 함수를 모아놓음.
/// </summary>
public static partial class Util
{
    #region 라벨표시 함수
    
    /// <summary>
    /// 주어진 점 리스트를 따라서 선분을 그리고 라벨 수치를 표시하는 함수.
    /// 방 수치표시에 사용.
    /// </summary>
    /// <param name="allDots">선분을 그릴 외곽 점 리스트</param>
    /// <param name="labelTransform">라벨 오브젝트를 넣을 부모 오브젝트</param>
    /// <param name="textPrefab">라벨 프리팹</param>
    /// <param name="linePrefab">수치선 프리팹</param>
    /// <param name="option">라벨표시 방법 옵션</param>
    static void AttachLabels(List<Dot> allDots, Transform labelTransform, GameObject textPrefab, GameObject linePrefab, int option = 0)
    {
        for (int i = 0; i < allDots.Count; i++)
        {
            AttachLabel(allDots[i], allDots[(i + 1) % allDots.Count], labelTransform, textPrefab, linePrefab, option);
        }
    }
    /// <summary>
    /// 두 점을 따라 선분을 그리고 라벨 수치를 표시하는 함수.
    /// </summary>
    /// <param name="dot1">첫번째 점</param>
    /// <param name="dot2">두번째 점</param>
    /// <param name="labelTransform">라벨 오브젝트를 넣을 부모 오브젝트</param>
    /// <param name="textPrefab">라벨 프리팹</param>
    /// <param name="linePrefab">수치선 프리팹</param>
    /// <param name="option">라벨표시 방법 옵션</param>
    public static void AttachLabel(Dot dot1, Dot dot2, Transform labelTransform, GameObject textPrefab, GameObject linePrefab, int option = 0)
    {
        // 옵션에 따른 세부 값
        float[] offset = { 0.37f, 0.05f, -0.2f };
        bool[] line = { true, false, true };
        float[] linePadding = { -0.02f, 0, 0f };
        float[] linePos = { 0.15f, 0, 0 };
        bool[] colorB = { true, false, true };

        float length = GetGridValue(Vector3.Distance(dot1.position, dot2.position));
        int axis = (Mathf.Abs(dot2.x - dot1.x) > Mathf.Abs(dot2.y - dot1.y)) ? 0 : 2;

        // 프리팹으로 부터 라벨 생성 및 수치값 반영
        GameObject g = new GameObject("label_" + axis);
        g.transform.SetParent(labelTransform);
        Transform text = GameObject.Instantiate(textPrefab, g.transform).transform;
        text.GetComponentInChildren<Text>().text = GetGridLabel(length) + "m";
        text.eulerAngles = Vector3.zero;
        text.localPosition = Vector3.back * 0.1f;
        text.GetComponentInChildren<Text>().color = colorB[option] ? Color.black : Color.white;
        text.GetComponentInChildren<Outline>().effectColor = colorB[option] ? Color.white : Color.black;

        // 방향에 따른 회전과 패딩(위치offset)
        Vector3 padding = Vector3.zero;
        float angle = 0;
        if (dot2.x - dot1.x > 0) { angle = 0; padding = new Vector3(0, 0, 1); }
        if (dot2.x - dot1.x < 0) { angle = 180; padding = new Vector3(0, 0, -1); }
        if (dot2.y - dot1.y > 0) { angle = 270; padding = new Vector3(-1, 0, 0); }
        if (dot2.y - dot1.y < 0) { angle = 90; padding = new Vector3(1, 0, 0); }
        g.transform.position = (dot1.position + dot2.position) * 0.5f + padding * offset[option] + Vector3.up * 5f;
        g.transform.Rotate(new Vector3(90, angle, 0));
        
        // 수치선 표시
        if (line[option])
        {
            GameObject l = GameObject.Instantiate(linePrefab, g.transform);
            l.transform.parent = g.transform;
            l.transform.localScale = new Vector3(length + linePadding[option], 1, 1);
            l.transform.localPosition = Vector3.down * linePos[option];
        }
    }


    #endregion
    #region Border

    /// <summary>
    /// 공간설정에서 벽을 나타낼 border 생성.
    /// 1개의 바닥 주변에 벽 두께(편의상 2배+바닥 아래에 위치하여 나머지 부분은 가림) * 벽 길이 크기의 quad를 4개 생성한다.
    /// 공간설정 중 매번 바뀌는 바닥 모양을 토대로 한 실제 벽을 생성하지 않고 시각적으로 벽의 존재를 표시하기 위해 사용.
    /// </summary>
    /// <param name="floor">테두리를 만들 대상 바닥</param>
    /// <param name="borderTransform">모든 테두리 오브젝트를 담을 부모 오브젝트</param>
    /// <param name="material">적용할 재질</param>
    /// <returns>해당 바닥의 테두리 오브젝트를 담은 부모 오브젝트</returns>
    public static GameObject MakeBorder(Transform floor, Transform borderTransform, Material material)
    {
        if (floor == null) return null;
        // 좌표 편의상 2배 두께로 만들어서 나머지 반절은 바닥으로 가림
        float thic = 0.1f * 2;
        float height = 0.1f;

        // 바닥에 속하는 테두리들을 담을 부모 오브젝트
        Transform bordersParent = new GameObject().transform;
        bordersParent.name = "BordersAnchor";
        bordersParent.SetParent(borderTransform);

        // 스케일과 위치 정보는 bordersParent가 갖는다.         
        // 그리고 각각의 테두리 오브젝트는 local 위치와 회전값만을 갖는다.
        if (floor.transform.position.y == 2) bordersParent.position = new Vector3(floor.position.x, 1, floor.position.z) + Vector3.down * height;
        else bordersParent.position = floor.transform.position + Vector3.down * height;
        bordersParent.localScale = floor.localScale;
        
        Transform border = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        border.name = "Border_Up";
        border.localScale = new Vector3(floor.localScale.x + thic, thic, 1);
        border.position = new Vector3(floor.position.x, bordersParent.position.y, floor.position.z + floor.localScale.z * 0.5f);
        border.Rotate(Vector3.right * 90);
        border.parent = bordersParent;

        border = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        border.name = "Border_Down";
        border.localScale = new Vector3(floor.localScale.x + thic, thic, 1);
        border.position = new Vector3(floor.position.x, bordersParent.position.y, floor.position.z - floor.localScale.z * 0.5f);
        border.Rotate(Vector3.right * 90);
        border.parent = bordersParent;

        border = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        border.name = "Border_Right";
        border.localScale = new Vector3(floor.localScale.z + thic, thic, 1);
        border.position = new Vector3(floor.position.x + floor.localScale.x * 0.5f, bordersParent.position.y, floor.position.z);
        border.Rotate(Vector3.right * 90 + Vector3.up * 90);
        border.parent = bordersParent;

        border = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        border.name = "Border_Left";
        border.localScale = new Vector3(floor.localScale.z + thic, thic, 1);
        border.position = new Vector3(floor.position.x - floor.localScale.x * 0.5f, bordersParent.position.y, floor.position.z);
        border.Rotate(Vector3.right * 90 + Vector3.up * 90);
        border.parent = bordersParent;

        //재질 적용
        foreach (var renderer in bordersParent.GetComponentsInChildren<Renderer>())
        {
            renderer.material = material;
            renderer.material.color = Color.black;
        }

        return bordersParent.gameObject;
    }
    #endregion
    #region 시각화 함수 for Debug 
    /// <summary>
    /// 점 리스트를 시각화하는 디버그용 함수. 해당 위치에 구체를 생성하여 좌표값을 확인하기 위해 작성.
    /// </summary>
    /// <param name="points">시각화할 점 리스트</param>
    /// <param name="visualizeTransfrom">시각화 오브젝트가 담길 부모 오브젝트</param>
    /// <param name="name">지정할 이름</param>
    /// <param name="color">지정할 컬러</param>
    /// <param name="text">이름을 라벨로 표시할 지 여부</param>
    public static void Visualize(List<Dot> points, Transform visualizeTransfrom, string name="", Color? color= null, bool text = false)
    {
        int i = 0;
        foreach (var point in points)
        {
            GameObject t = Visualize(point.position, name + (i++) + point.diretion + point.attribute, color ?? Color.white, text);
            t.transform.parent = visualizeTransfrom;
        }
    }

    /// <summary>
    /// 좌표를 시각화하기 위한 디버그용 함수.
    /// </summary>
    /// <param name="point">시각화할 좌표</param>
    /// <param name="name">지정할 이름</param>
    /// <param name="color">지정할 색</param>
    /// <param name="isText">이름을 라벨로 표시할 지 여부</param>
    /// <returns>시각화 오브젝트</returns>
    public static GameObject Visualize(Vector3 point, string name="", Color? color = null, bool isText=false)
    {
        GameObject t = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        t.transform.localScale = Vector3.one * 0.2f;
        t.transform.position = point;
        t.name = name;
        t.GetComponent<Renderer>().material.color = color ?? Color.white;

        if (!isText) return t;
        GameObject g = new GameObject();        
        g.transform.SetParent(t.transform);
        g.transform.localPosition = Vector3.zero;
        g.transform.localEulerAngles = Vector3.right * 90f;
        g.AddComponent<TextMesh>();
        TextMesh text = g.GetComponent<TextMesh>();
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.1f;
        text.fontSize = 30;
        text.color = Color.black;
        text.text = name;
        return t;
    }
    #endregion

}
