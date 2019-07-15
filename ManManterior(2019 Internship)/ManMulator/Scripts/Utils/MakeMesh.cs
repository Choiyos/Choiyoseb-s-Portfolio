using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProBuilder2;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;
using System.Linq;

/// <summary>
/// <b>사용 위치</b> : SpacePresenter.MakeWallAndFloors(), DecoratePresenter.BindItem(), Util.MakeWalls(), Util.MakeOutlineWalls()<br></br>
/// <b>사용 시점</b> : 병합바닥 생성, 벽/외벽 생성, 문없음 주변 벽/바닥 생성<br></br>
/// <b>사용 플러그인</b> : ProBuilder2<br></br>
/// mesh를 동적으로 생성하는 클래스. 
/// 레퍼런스 오브젝트의 정보를 받아 필요한 점을 추출하고, 
/// 점들을 4개씩 사각형 단위로 묶어서 ProBuilder를 사용하여 새로운 mesh를 가지는 Pb_Object를 생성한다.
/// Probuilder로 새롭게 생성된 mesh의 경우 uv의 크기와 위치가 
/// 월드좌표에 공통적으로 반영되어 벽지나 바닥재 등 패턴이 있는 재질을 적용하기 편해서 사용함.
/// </summary>
public static class MakeMesh
{
    static bool debugMode = false;
    /// <summary>    
    /// <b>호출</b> : SpacePresenter.MakeWallAndFloors()<br></br>
    /// <b>참조</b> : GetVertices(), FindFace(), pb_Object.CreateInstanceWithVerticesFaces()<br></br>
    /// 바닥 생성 함수.
    /// 하나의 방을 이루는 바닥들의 위치와 크기를 참조하여 필요한 점을 수집하고, 
    /// 그 점들이 mesh를 이루기 위한 인덱스 배열을 찾아서 ProBuilder에 넘겨주어 새로운 mesh를 생성한다.
    /// 바닥 평면은 월드좌표에서 xz평면에서 생성되며, x를 주 좌표(메인 인덱스, index), z를 보조 좌표(sub 인덱스, index2)로 사용한다.
    /// </summary>
    /// <param name="floors">참조할 floor 정보(위치, 크기) 리스트</param>
    /// <param name="area">병합된 바닥의 넓이 값 - 견적계산에서 사용.</param>
    /// <param name="_debugMode">디버그를 위한 옵션</param>
    /// <returns>새롭게 생성된 mesh 오브젝트</returns>
    public static GameObject MakeRoomFloor(List<Transform> floors, out float area, bool _debugMode = false)
    {
        debugMode = _debugMode;
        area = 0;

        // mesh를 만드는데 필요한 점 수집.
        List<Vector3> vertices = GetVertices(floors);

        //if (debugMode) GameObject.DontDestroyOnLoad(Util.Visualize(vertices, "v", Color.white));

        // probuilder로 병합된 바닥 mesh 오브젝트 생성
        pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(vertices.ToArray(), FindFace(vertices, floors, 0, 2, 1, out area));
        return pb.gameObject;
    }

    /// <summary>
    /// <b>호출</b> : Util.MakeWalls(), Util.MakeOutlineWalls()<br></br>
    /// <b>참조</b> : GetVertices(), FindFace(), GetIntersect(), Util.Index(), pb_Object.CreateInstanceWithVerticesFaces() <br></br>
    /// 벽 생성 함수. 
    /// 문/창문이 있을 경우, 그 부분에 구멍이 뚫린 벽 모델을 얻기 위해
    /// Probuilder로 기존 벽과 오브젝트의 transfrom 정보를 참조하여 모든 점과 교점을 수집한 뒤,
    /// 부분적으로 사각형 단위로 submesh를 생성하고 통합하여 mesh를 생성한다.<br></br>
    /// 현재 월드좌표에서 y축을 높이 축으로 사용 중이며,
    /// 따라서 벽 생성이 xy 평면 또는 zy평면에서 생성되므로 
    /// 각 벽의 방향에 따라 주 좌표 축(메인 인덱스, index)로 삼고, y축을 sub 좌표 축(index2)로 사용한다.
    /// 또 mesh 생성시 normal(앞/뒷면)구분을 위해 벽의 회전 각도에 따라 주 좌표 축의 방향(dir)을 음수나 양수로 지정하였다.
    /// </summary>
    /// <param name="wall">생성시 참조할 기본벽</param>
    /// <param name="elements">문/창문 리스트</param>
    /// <param name="material">생성된 벽에 적용할 재질</param>
    /// <param name="area">생성된 벽의 면적</param>
    /// <param name="isDebug">디버그 옵션</param>
    /// <returns>
    /// probuilder로 새로 생성한 벽 mesh 오브젝트
    /// </returns>
    public static GameObject MakeWalls(Transform wall, List<Transform> elements, Material material, out float area, bool isDebug = false)
    {
        debugMode = isDebug;
        List<Vector3> vertices = new List<Vector3>();
        List<Transform> elementsInThis = new List<Transform>();
        area = 0;

        // 벽의 회전 각도로 메인 축 인덱스 찾기 : x = 0, z = 2
        int index = Util.Index(wall);

        // 벽의 회전 각도로 메인 축의 방향 찾기 : 정방향 = 1, 역방향 = -1
        float dir = (wall.eulerAngles.y == 90 || wall.eulerAngles.y == 180) ? -1 : 1;

        // 기본 벽을 기준으로 모서리 점 추가
        vertices.AddRange(GetVertices(wall, wall, index));

        // 벽 안에 문/창문이 있을 경우 점 추가
        foreach (var element in elements)
        {
            if (GetIntersect(wall, element, index, dir))
            {
                Vector3[] verts = GetVertices(element, wall, index);
                vertices.AddRange(verts);
                elementsInThis.Add(element);
            }
        }

        // 초기 dot 생성 확인
        //if (debugMode) GameObject.DontDestroyOnLoad(Util.Visualize(vertices, "dots")); 

        // 면 만들기
        pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(vertices.ToArray(), FindFace(vertices, elementsInThis, index, 1, dir, out area));
        pb.GetComponent<MeshRenderer>().material = material;
        pb.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        //pb.name = "Wall"+index;
        return pb.gameObject;
    }

    /// <summary>
    /// <b>호출</b> : DecoratePresenter.BindItem()<br></br>
    /// <b>참조</b> : Util.GetGridValue(), pb_Object.CreateInstanceWithVerticesFaces() <br></br>
    /// 문 없음 객체에서 문 모델 오브젝트 대신 빈 벽을 땜빵할 mesh를 새롭게 생성하는 함수.
    /// 벽지/바닥재 적용 시 uv 스케일링과 월드좌표 반영을 위해서 참조 quad를 probuilder로 다시 생성한다.
    /// </summary>
    /// <param name="quad">다시 생성할 참조 quad</param>
    /// <param name="material">적용할 기본 재질</param>
    /// <param name="area">생성된 mesh의 면적 - 견적에서 사용됨.</param>
    /// <returns>probuilder로 생성된 mesh 오브젝트</returns>
    public static GameObject MakeQuad(GameObject quad, Material material, out float area)
    {
        // quad의 모서리 정보(x,y)를 월드 좌표로 수집. 
        Vector3[] vertices = new Vector3[4];
        vertices[0] = quad.transform.TransformPoint(Vector3.left * 0.5f + Vector3.down * 0.5f);
        vertices[1] = quad.transform.TransformPoint(Vector3.right * 0.5f + Vector3.down * 0.5f);
        vertices[2] = quad.transform.TransformPoint(Vector3.left * 0.5f + Vector3.up * 0.5f);
        vertices[3] = quad.transform.TransformPoint(Vector3.right * 0.5f + Vector3.up * 0.5f);

        pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(vertices, MakeBox(new int[] { 0, 1, 2, 3 }));
        pb.GetComponent<MeshRenderer>().material = material;        
        pb.name = quad.name;

        GameObject.Destroy(quad);
        area = Util.GetGridValue(quad.transform.localScale.x * quad.transform.localScale.y);

        return pb.gameObject;
    }
        
    #region 공용

    /// <summary>
    /// <b>호출</b> : MakeQuad(), FindFace()<br></br>
    /// 사각형 mesh 1개를 생성하는데 필효한 점 index 배열 정보 함수.
    /// ProBuilder에서 mesh를 생성할 때 기본단위가 삼각형이기 때문에 
    /// 편의상 사각형 단위로 맞춰주기 위해 사각형 모서리 4개의 배열을 삼각형 2개의 배열을 변환. 
    /// </summary>
    /// <param name="indices"> mesh 생성에 필요한 index 배열. 0=좌하단,1=우하단,2=좌상단,3=우상단 </param>
    /// <returns>Probuilder가 사각형 mesh 생성시 필요한 점 인덱스 배열정도를 담는 pb_Face 객체.</returns>
    private static pb_Face[] MakeBox(int[] indices)
    {
        return new pb_Face[] { new pb_Face(new int[] { indices[0], indices[1], indices[2] }), new pb_Face(new int[] { indices[1], indices[3], indices[2] }) };
    }


    /// <summary>
    /// <b>호출</b> : FindFace()<br></br>
    /// 좌하단 점(vert0)을 기준으로 사각형이 생성될 수 있는지 탐색하고,
    /// 사각형이 생성될 수 있다면 각 모서리 점의 index 배열을 반환.<br></br>
    /// 2차원 (바닥의 경우 xz평면에서) 각 점의 이름과 위치 :<br></br> 
    /// vert0 : 좌하단(min x, min z) / vert1: 우하단(max x, min z) / vert2: 좌상단(min x, max z) / vert3: 우상단(max x, max z)
    /// </summary>
    /// <param name="vertices">index를 받아올 참조 점 배열</param>
    /// <param name="vert0index">기준 점</param>
    /// <param name="index">첫번째 축 인덱스(바닥생성시=x=0)</param>
    /// <param name="index2">두번째 축 인덱스(바닥생성시=z=2)</param>
    /// <param name="dir">첫번째 축 인덱스의 방향(바닥생성시=1)</param>
    /// <param name="subArea">생성될 사각형의 면적</param>
    /// <returns>
    /// 사각형이 생성되면 각 모서리 index 배열 반환 /
    /// 사각형이 생성되지 못하면 null 반환
    /// </returns>
    private static int[] FindBox(List<Vector3> vertices, int vert0index, int index, int index2, float dir, out float subArea)
    {
        Vector3 vert0 = vertices[vert0index];
        Vector3[] verts;
        int[] indices = new int[4];
        subArea = 0;
        indices[0] = vert0index;

        // 1. vert2 탐색 : y = 0 & 0 번에서 x축으로 가장 가까운 점 찾기
        verts = vertices.FindAll(x =>
                Util.IsEquals(x[index2], vert0[index2])
                && (dir > 0 ? Util.IsBig(x[index], vert0[index]) : Util.IsBig(vert0[index], x[index]))
                ).OrderBy(x => x[index] * dir).ToArray();

        if (verts.Count() == 0) return null;
        Vector3 vert2 = verts[0];
        indices[2] = vertices.FindIndex(vert2.Equals);

        // 2. vert1 탐색 : x = 0 & vert3이 존재하는 리스트 찾기
        verts = vertices.FindAll(x =>
                Util.IsEquals(x[index], vert0[index])
                && Util.IsBig(x[index2], vert0[index2])
                && vertices.FindAll(y => Util.IsEquals(y[index], vert2[index]) && Util.IsEquals(y[index2], x[index2])).Count > 0
                ).OrderBy(x => x[index2]).ToArray();
        if (verts.Count() == 0) return null;
        Vector3 vert1 = verts[0];
        indices[1] = vertices.FindIndex(vert1.Equals);

        // 3. vert3 탐색: y = vert1.y & x = vert2.x
        verts = vertices.FindAll(x => Util.IsEquals(x[index], vert2[index]) && Util.IsEquals(x[index2], vert1[index2])).ToArray();
        if (verts.Count() == 0) return null;
        indices[3] = vertices.FindIndex(verts[0].Equals);

        subArea = Mathf.Abs(vert0[index] - vert2[index]) * Mathf.Abs(vert0[index2] - vert1[index2]);
        return indices;
    }

    /// <summary>
    /// <b>호출</b> : MakeRoomFloor(), MakeWalls()<br></br>
    /// <b>참조</b> : FindBox(),  CheckIsFloorInside() MakeBox(), Util.IsBig()<br></br>
    /// probuilder로 생성할 mesh의 구성할 vertice의 index 배열을 생성.
    /// 기준점 vert0를 아래(min Y)에서 위(max u)로, 왼쪽(min index1)에서 오른쪽(max index1)방향으로 탐색하면서
    /// 사각형단위로 mesh를 생성할 수 있으면 index 배열을 수집한다.
    /// 또 사각형 mesh가 생성가능 하더라도 참조 오브젝트의 범위를 벗어나거나, 
    /// 참조 오브젝트가 가리는(문/창문) 위치에 있는지 검사하여 mesh 생성 여부를 결정한다.
    /// </summary>
    /// <param name="vertices">index를 받아올 대상 배열</param>
    /// <param name="elements">mesh 생성시 참조할 오브젝트 정보(바닥/문/창문)</param>
    /// <param name="index">mesh 생성할 첫번째 인덱스(바닥생성시=x=0)</param>
    /// <param name="index2">mesh 생성할 두번째 인덱스(바닥생성시=z=2)</param>
    /// <param name="dir">mesh 생성시 첫번째 인덱스의 방향(바닥생성시=1)</param>
    /// <param name="area">생성된 mesh의 면적</param>
    /// <returns>probuilder로 생성할 mesh의 face를 구성할 vertice의 index 배열</returns>
    private static pb_Face[] FindFace(List<Vector3> vertices, List<Transform> elements, int index, int index2, float dir, out float area)
    {
        List<pb_Face> pb_Faces = new List<pb_Face>();
        Vector3[] findVerts;
        area = 0;

        // 첫번째 vert0(기준점) 탐색 : 모든 점 중에 가장 좌측 하단 점.
        Vector3 vert0 = vertices.OrderBy(x => x[index2]).OrderBy(x => x[index] * dir).First();
        float lastIndexValue = vert0[index];
        float lastIndex2Value = vert0[index2];

        int count = 0;
        do
        {
            // 임시 무한루프 방지
            if (count++ > vertices.Count()) break;

            // vert0(사각형 생성 기준점)이 잡히는지 확인
            //if (debugMode) GameObject.DontDestroyOnLoad(Util.Visualize(vert0, "0_", Color.red)); 

            // 0: 가장 좌하단 점       
            int[] indices = FindBox(vertices, vertices.FindIndex(vert0.Equals), index, index2, dir, out float subArea);
            if (indices != null)
            {
                // 점 4개를 성공적으로 찾았을 경우 사각형 생성!!
                if (index2 != 1)
                {
                    // 바닥의 경우
                    if (CheckIsFloorInside(vertices, elements, indices))
                    {
                        area += subArea;
                        pb_Faces.AddRange(MakeBox(indices));
                    }
                }
                else
                {
                    // 벽인 경우
                    if (elements == null || CheckVertices(vertices, elements, indices, index))
                    {
                        //if (debugMode) GameObject.DontDestroyOnLoad(Util.Visualize(vert0, "0v", Color.yellow));
                        area += subArea;
                        pb_Faces.AddRange(MakeBox(indices));
                    }
                }

                // 0점이 정상적으로 탐색되는지 검사
                //if (debugMode) GameObject.DontDestroyOnLoad(Util.Visualize(vert0, "v0", Color.red));

                lastIndexValue = vertices[indices[2]][index];
                lastIndex2Value = vertices[indices[1]][index2];

                // 1-1. next 0번 탐색 : 0번과 같은 x & 마지막 box의 y값 이상 & 자기보다 y값이 큰 점이 1개 이상 
                findVerts = vertices.FindAll(x => !(Util.IsEquals(x[index], vert0[index]) && Util.IsEquals(x[index2], vert0[index2]))
                                                    && Util.IsEquals(x[index], vert0[index])
                                                    && (Util.IsEquals(x[index2], lastIndex2Value) || Util.IsBig(x[index2], lastIndex2Value))
                                                    && vertices.FindAll(y => Util.IsEquals(y[index], x[index]) && Util.IsBig(y[index2], x[index2])).Count() > 0
                                                    ).OrderBy(x => x[index2]).OrderBy(x => x[index] * dir).ToArray();
            }
            else
            {
                // 0점이 정상적으로 탐색되는지 검사
                //if (debugMode) GameObject.DontDestroyOnLoad(Util.Visualize(vert0, "not0_", Color.red));

                // 1-2. next 0번 탐색 : 0번과 같은 x & 마지막 box의 y값 이상 & 자기보다 y값이 큰 점이 1개 이상 
                findVerts = vertices.FindAll(x => Util.IsEquals(x[index], vert0[index]) && Util.IsBig(x[index2], vert0[index2])
                                                    && vertices.FindAll(y => Util.IsEquals(y[index], x[index]) && Util.IsBig(y[index2], x[index2])).Count() > 0
                                                    ).OrderBy(x => x[index2]).OrderBy(x => x[index] * dir).ToArray();
            }

            if (findVerts.Count() > 0) vert0 = findVerts[0];
            else
            {
                // 2. next 0번 탐색 : 0번 다음으로 가까운 x 중 가장 y가 작은 점
                findVerts = vertices.FindAll(x => (dir > 0 ? Util.IsBig(x[index], vert0[index]) : Util.IsBig(vert0[index], x[index]))
                                                ).OrderBy(x => x[index2]).OrderBy(x => x[index] * dir).ToArray();
                if (findVerts.Count() > 0) vert0 = findVerts[0];
            }
        }
        while (findVerts.Count() > 0);

        return pb_Faces.ToArray();
    }

    /// <summary>
    /// <b>호출</b> : FindFace() <br></br>
    /// <b>참조</b> : Util.GetIntersect(), Util.ConvertFloorToFloor()<br></br>
    /// 사각형 부분이 바닥 구멍이 뚫린 부분인지 검사하는 함수.
    /// </summary>
    /// <param name="vertices">참조 점 리스트</param>
    /// <param name="floors">검사할 바닥 리스트</param>
    /// <param name="indices">사각형을 구성하는 점 인덱스 배열</param>
    /// <returns>
    /// false : 바닥 구멍뚫린 부분 -> 사각형 생성 x /
    /// true : 바닥이 하나 이상 존재 -> 사각형 생성 
    /// </returns>
    private static bool CheckIsFloorInside(List<Vector3> vertices, List<Transform> floors, int[] indices)
    {
        Vector3 center = Vector3.zero;
        foreach (var i in indices)
        {
            center += vertices[i];
        }
        center /= 4;
        int check = 0;
        foreach (var f in floors)
        {
            if (Util.GetIntersect(Util.ConvertFloorToFloor(f), center)) check++;
        }
        return check > 0;
    }

    /// <summary>
    /// <b>호출</b> : FindFace() <br></br>
    /// <b>참조</b> : Util.GetIntersect()<br></br>
    /// 생성하려고 하는 사각형 위치에 문이나 창문 오브젝트가 존재하는지 검사하는 함수.
    /// </summary>
    /// <param name="vertices"> 참조 점 리스트</param>
    /// <param name="elements"> 참조 오브젝트</param>
    /// <param name="indices">사각형 구성 인덱스 배열 </param>
    /// <param name="index"> 첫번째 축(x=0)</param>
    /// <returns>
    /// false : 검사 실패 => 해당 사각형 생성 안함 /
    /// true : 검사 성공 => 해당 사각형 생성
    /// </returns>
    private static bool CheckVertices(List<Vector3> vertices, List<Transform> elements, int[] indices, int index)
    {
        Vector3 center = Vector3.zero;
        foreach (var i in indices)
        {
            center += vertices[i];
        }
        center /= 4;
        foreach (var e in elements)
        {
            // 사각형 중앙 점이 오브젝트와 겹치는 경우 사각형 생성 x
            if (Util.GetIntersect(e, center, index)) return false;
        }
        return true;
    }

    #endregion

    #region makeWall

    /// <summary>
    /// <b>호출</b> : MakeWalls() <br></br>
    /// <b>참조</b> : Util.IsBig() <br></br>
    /// 벽 안에 문 또는 창문 오브젝트가 포함되는지 검사하는 함수.
    /// </summary>
    /// <param name="wall">기준 벽</param>
    /// <param name="element">검사할 오브젝트</param>
    /// <param name="index">첫번째 축 인덱스</param>
    /// <param name="dir">첫번째 축 인덱스 방향</param>
    /// <returns> true - 포함됨 / false - 포함되지 않음 </returns>
    private static bool GetIntersect(Transform wall, Transform element, int index, float dir)
    {
        Vector3 indexVector = Vector3.right * 0.5f * dir;
        int index2 = index == 0 ? 2 : 0;

        if (dir > 0)
        {
            if (Util.IsBig(wall.TransformPoint(-indexVector)[index], (element.position[index] + element.localScale.x * 0.5f))) return false;
            if (Util.IsBig((element.position[index] - element.localScale.x * 0.5f), wall.TransformPoint(indexVector)[index])) return false;
        }
        else
        {
            if (Util.IsBig((element.position[index] - element.localScale.x * 0.5f), wall.TransformPoint(indexVector)[index])) return false;
            if (Util.IsBig(wall.TransformPoint(-indexVector)[index], (element.position[index] + element.localScale.x * 0.5f))) return false;
        }

        if (Util.IsBig(wall.position[index2], (element.position[index2] + element.localScale.z * 0.5f))) return false;
        if (Util.IsBig((element.position[index2] - element.localScale.z * 0.5f), wall.position[index2])) return false;

        if (Util.IsBig(wall.TransformPoint(Vector3.up * -0.5f).y, element.TransformPoint(Vector3.up * 0.5f).y)) return false;
        if (Util.IsBig(element.TransformPoint(Vector3.up * -0.5f).y, wall.TransformPoint(Vector3.up * 0.5f).y)) return false;

        return true;
    }

    /// <summary>
    /// <b>호출</b> : MakeWalls() <br></br>
    /// <b>참조</b> : Util.IsEquals() <br></br>
    /// 벽을 레퍼런스 삼아 문/창문 오브젝트 주위에 mesh를 생성할 수 있도록 점을 추가/생성 하는 함수.
    /// </summary>
    /// <param name="_element">점 생성시 참조 오브젝트</param>
    /// <param name="_wall">기준 벽</param>
    /// <param name="index">첫번째 축 인덱스</param>
    /// <returns>추가/생성된 점 리스트</returns>
    private static Vector3[] GetVertices(Transform _element, Transform _wall, int index)
    {
        List<Vector3> vertices = new List<Vector3>();
        Vector3 temp;

        // 축 방향 벡터
        Vector3 indexVector = (index == 0 ? Vector3.right : Vector3.forward) * 0.5f;

        // 영점 인덱스: 벽이 xy평면에 생성될 경우 영점인덱스 = z = 2.
        int zeroIndex = index == 0 ? 2 : 0;
        float zeroValue = _wall.position[zeroIndex];

        // 1. 좌측하단
        temp = _element.position - indexVector * _element.lossyScale.x + Vector3.down * _element.lossyScale.y * 0.5f;
        if (temp.y < _wall.TransformPoint(Vector3.down * 0.5f).y) temp.y = _wall.TransformPoint(Vector3.down * 0.5f).y;
        // index 축에서  범위 벗어나는 예외처리 필요
        //if (_wall !=_element && v[index] < _wall.TransformPoint(-moveVector)[index]) v[index] = _wall.TransformPoint(-moveVector)[index];
        temp[zeroIndex] = zeroValue;
        vertices.Add(temp);

        // 2. 좌측상단
        temp = _element.position - indexVector * _element.lossyScale.x + Vector3.up * _element.lossyScale.y * 0.5f;
        if (temp.y > _wall.TransformPoint(Vector3.up * 0.5f).y) temp.y = _wall.TransformPoint(Vector3.up * 0.5f).y;
        // index 축에서  범위 벗어나는 예외처리 필요
        //if (_wall != _element && v[index] < _wall.TransformPoint(-moveVector)[index]) v[index] = _wall.TransformPoint(-moveVector)[index];
        temp[zeroIndex] = zeroValue;
        vertices.Add(temp);

        // 3. 우측하단
        temp = _element.position + indexVector * _element.lossyScale.x + Vector3.down * _element.lossyScale.y * 0.5f;
        if (temp.y < _wall.TransformPoint(Vector3.down * 0.5f).y) temp.y = _wall.TransformPoint(Vector3.down * 0.5f).y;
        // index 축에서  범위 벗어나는 예외처리 필요
        //if (_wall != _element && v[index] > _wall.TransformPoint(moveVector)[index]) v[index] = _wall.TransformPoint(moveVector)[index];
        temp[zeroIndex] = zeroValue;
        vertices.Add(temp);

        // 4. 우측상단
        temp = _element.position + indexVector * _element.lossyScale.x + Vector3.up * _element.lossyScale.y * 0.5f;
        if (temp.y > _wall.TransformPoint(Vector3.up * 0.5f).y) temp.y = _wall.TransformPoint(Vector3.up * 0.5f).y;
        // index 축에서  범위 벗어나는 예외처리 필요
        //if (_wall != _element && v[index] > _wall.TransformPoint(moveVector)[index]) v[index] = _wall.TransformPoint(moveVector)[index];
        temp[zeroIndex] = zeroValue;
        vertices.Add(temp);

        // 참조 오브젝트가 문/창문일 경우 위아래에 점 추가
        if (_element != _wall)
        {
            // 오브젝트가 벽의 높이보다 낮을 경우 상단에 점 추가
            if (!Util.IsEquals(_element.TransformPoint(Vector3.up * 0.5f).y, _wall.TransformPoint(Vector3.up * 0.5f).y))
            {
                temp = _element.position - indexVector * _element.lossyScale.x + Vector3.up * _element.lossyScale.y;
                temp.y = _wall.TransformPoint(Vector3.up * 0.5f).y;
                temp[zeroIndex] = zeroValue;
                vertices.Add(temp);

                temp = _element.position + indexVector * _element.lossyScale.x + Vector3.up * _element.lossyScale.y;
                temp.y = _wall.TransformPoint(Vector3.up * 0.5f).y;
                temp[zeroIndex] = zeroValue;
                vertices.Add(temp);
            }
            // 오브젝트가 벽의 바닥보다 높을 경우 하단에 점 추가
            if (!Util.IsEquals(_element.TransformPoint(Vector3.down * 0.5f).y, _wall.TransformPoint(Vector3.down * 0.5f).y))
            {
                temp = _element.position - indexVector * _element.lossyScale.x + Vector3.down * _element.lossyScale.y;
                temp.y = _wall.TransformPoint(Vector3.down * 0.5f).y;
                temp[zeroIndex] = zeroValue;
                vertices.Add(temp);

                temp = _element.position + indexVector * _element.lossyScale.x + Vector3.down * _element.lossyScale.y;
                temp.y = _wall.TransformPoint(Vector3.down * 0.5f).y;
                temp[zeroIndex] = zeroValue;
                vertices.Add(temp);
            }
        }

        return vertices.ToArray();
    }

    #endregion

    #region makeFloor

    /// <summary>
    /// <b>호출</b> : GetVertices()<br></br>
    /// <b>참조</b> : Util.GetDirVector()<br></br>
    /// 바닥 모서리 4개의 점 수집 함수.
    /// </summary>
    /// <param name="_floor"> 참조 바닥</param>
    /// <returns> 점 리스트 </returns>
    private static Vector3[] GetVertices(Transform _floor)
    {
        Vector3[] vertices = new Vector3[4];
        vertices[0] = _floor.TransformPoint(Util.GetDirVector(0) * -1 + Util.GetDirVector(2));
        vertices[1] = _floor.TransformPoint(Util.GetDirVector(0) * -1 + Util.GetDirVector(2) * -1);
        vertices[2] = _floor.TransformPoint(Util.GetDirVector(0) + Util.GetDirVector(2));
        vertices[3] = _floor.TransformPoint(Util.GetDirVector(0) + Util.GetDirVector(2) * -1);
        return vertices;
    }

    /// <summary>
    /// <b>호출</b> : MakeRoomFloor()<br></br>
    /// <b>참조</b> : GetVertices(),GetCrossPoints(), Util.IsEquals(), Util.ConvertFloorToFloor(), Util.IsCorner(), Util.GetSide(), Util.GetIntersect(), Util.GetGridPos()<br></br>
    /// 바닥 mesh 생성을 위해 필요한 점을 수집하는 함수.<br></br>
    /// 바닥은 한 개 이상의 사각형으로 이루어 지며, 여러 개의 바닥이 겹쳐져 있는 경우에는 
    /// 사각형 단위로 생성할 수 있도록 교점을 생성하고
    /// 중복점과 필요없는 점들을 모두 제거하여 바닥 생성에 필요한 점 리스트만 남긴다.
    /// Vector3 내부 xyz 값은 float로서 원치 않는 상황(ex: 1 -> 0.9999...) 발생을 막기위해 
    /// 마지막에 모든 값을 grid(0.05m)단위로 변환한다.
    /// </summary>
    /// <param name="floors">바닥 리스트</param>
    /// <returns>바닥 생성시 필요한 점 리스트</returns>
    static List<Vector3> GetVertices(List<Transform> floors)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> innerPoints = new List<Vector3>();
        List<Vector3> crossPoints = new List<Vector3>();
        List<Vector3> innerPointsNotDelete = new List<Vector3>();

        // 1. 바닥 모서리 점 수집
        foreach (var floor in floors)
        {
            Vector3[] floorVertices = GetVertices(floor);
            foreach (var v in floorVertices)
            {
                // 중복 점은 수집하지 않음.
                if (vertices.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, v.z)).Count == 0) vertices.Add(v);
            }
        }

        // 2. 교점 추가 생성        
        // 2-1. 바닥 안에 포함되는 점(innderPoint) 탐색
        foreach (var floor in floors)
        {
            Floor f = Util.ConvertFloorToFloor(floor);
            foreach (var v in vertices)
            {
                if (!Util.IsCorner(f, v) && Util.GetIntersect(f, v)
                    && innerPoints.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, v.z)).Count == 0)
                {
                    innerPoints.Add(v);
                    // 해당 점이 바닥의 변에 위치하면 삭제하지 않는다.
                    if (Util.GetSide(f, v)) innerPointsNotDelete.Add(v);
                }
            }
        }

        // 2-2. cross point 생성
        // 2-2-1. innderPoint를 기준으로 cross point 생성
        crossPoints.AddRange(GetCrossPoints(vertices, floors, innerPoints, crossPoints));

        // 2-2-2. cross point 중에서도 inner point인 애들은 cross check 후 crosspoint 추가
        int index = 0;
        List<Vector3> cp = GetCrossPoints(vertices, floors, crossPoints, crossPoints);
        // 2-2-3. crosspoint가 더 발견되지 않을때까지 탐색
        while (cp.Count > 0)
        {
            crossPoints.AddRange(cp.ToArray());
            cp = GetCrossPoints(vertices, floors, crossPoints, crossPoints);
            if (index++ > 9999) break;
        }

        // 2-4. 모서리 점들 중 바닥 내부에 포함되는 점 삭제 (vertice - innerPoints)
        foreach (var v in innerPoints)
        {
            if (vertices.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, v.z)).Count() > 0) { vertices.Remove(v); }
        }
        vertices.AddRange(crossPoints.ToArray());

        // 2-5. 삭제되면 안되는 내부 점 다시 추가 (vertice + innerPointsNotDelete)
        foreach (var v in innerPointsNotDelete)
        {
            if (vertices.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, v.z)).Count() == 0) vertices.Add(v);
        }

        // 3. 좌표값을 grid(0.05m) 단위로 변환
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = Util.GetGridPos(vertices[i]);
        }

        return vertices;
    }

    /// <summary>
    /// <b>호출</b> : GetVertices()<br></br>
    /// <b>참조</b> : Util.IsCorner(), Util.ConvertFloorToFloor(), Util.GetIntersect(), Util.GetDirVector(), Util.IsEquals()<br></br>
    /// 바닥에 포함되는 내부 점이 있을 경우, 점을 기준으로 상하좌우 교점을 생성하는 함수.
    /// </summary>
    /// <param name="vertice">참조할 모든 점</param>
    /// <param name="floors">바닥 정보</param>
    /// <param name="innerPoints">바닥 범위 내부에 속하는 내부 점</param>
    /// <param name="beforePoints">이전에 찾은 교점</param>
    /// <returns>생성된 교점 리스트</returns>
    private static List<Vector3> GetCrossPoints(List<Vector3> vertice, List<Transform> floors, List<Vector3> innerPoints, List<Vector3> beforePoints)
    {
        List<Vector3> crossPoints = new List<Vector3>();
        Vector3 crossPoint;
        foreach (var floor in floors)
        {
            foreach (var v in innerPoints)
            {
                if (!Util.IsCorner(Util.ConvertFloorToFloor(floor), v) && Util.GetIntersect(Util.ConvertFloorToFloor(floor), v))
                {
                    crossPoint = new Vector3(v.x, 0, floor.TransformPoint(Util.GetDirVector(2)).z);
                    if (crossPoints.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, crossPoint.z)).Count == 0
                        && vertice.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, crossPoint.z)).Count == 0
                        && beforePoints.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, crossPoint.z)).Count == 0
                        )
                    {
                        crossPoints.Add(crossPoint);
                    }

                    crossPoint = new Vector3(v.x, 0, floor.TransformPoint(Util.GetDirVector(2) * -1).z);
                    if (crossPoints.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, crossPoint.z)).Count == 0
                        && vertice.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, crossPoint.z)).Count == 0
                        && beforePoints.FindAll(x => Util.IsEquals(x.x, v.x) && Util.IsEquals(x.z, crossPoint.z)).Count == 0
                        )
                    {
                        crossPoints.Add(crossPoint);
                    }

                    crossPoint = new Vector3(floor.TransformPoint(Util.GetDirVector(0)).x, 0, v.z);
                    if (crossPoints.FindAll(x => Util.IsEquals(x.z, v.z) && Util.IsEquals(x.x, crossPoint.x)).Count == 0
                        && vertice.FindAll(x => Util.IsEquals(x.z, v.z) && Util.IsEquals(x.x, crossPoint.x)).Count == 0
                        && beforePoints.FindAll(x => Util.IsEquals(x.z, v.z) && Util.IsEquals(x.x, crossPoint.x)).Count == 0
                        )
                    {
                        crossPoints.Add(crossPoint);
                    }

                    crossPoint = new Vector3(floor.TransformPoint(Util.GetDirVector(0) * -1).x, 0, v.z);
                    if (crossPoints.FindAll(x => Util.IsEquals(x.z, v.z) && Util.IsEquals(x.x, crossPoint.x)).Count == 0
                        && vertice.FindAll(x => Util.IsEquals(x.z, v.z) && Util.IsEquals(x.x, crossPoint.x)).Count == 0
                        && beforePoints.FindAll(x => Util.IsEquals(x.z, v.z) && Util.IsEquals(x.x, crossPoint.x)).Count == 0
                        )
                    {
                        crossPoints.Add(crossPoint);
                    }
                }

            }
        }
        return crossPoints;
    }

    #endregion
}
