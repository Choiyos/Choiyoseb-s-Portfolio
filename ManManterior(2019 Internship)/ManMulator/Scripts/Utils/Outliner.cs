using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Direction
{
    Right, Up, Down, Left
}
/// <summary>
/// 방 바닥 외곽선 추출 클래스
/// </summary>
public class Outliner
{
    List<Dot> dots;
    List<Floor> floors;
    Dictionary<Vector3, List<Floor>> overlapPoint;
    /// <summary>
    /// 방 1개의 외곽선 추출.
    /// 방 안쪽에 공간이 비어있을 때를 위해 다중 리스트로 예외처리.
    /// </summary>
    /// <param name="subFloors"></param>
    /// <returns></returns>
    public List<List<Dot>> FindAllOutline(List<Transform> subFloors, bool isDebug =false)
    {
        //for debug
        Transform debugParent = null;        
        if (isDebug)
        {
            if (GameObject.Find("DebugOutline") == null)
            {
                debugParent = new GameObject("DebugOutline").transform;
            }
            else
            {
                debugParent = GameObject.Find("DebugOutline").transform;
                for (int i = 0; i < debugParent.childCount; i++)
                {
                    GameObject.Destroy(debugParent.GetChild(i).gameObject);
                }
            }
        }
        

        floors = new List<Floor>();
        List<List<Dot>> allDots = new List<List<Dot>>();
        dots = new List<Dot>();
        overlapPoint = new Dictionary<Vector3, List<Floor>>();

        for (int i = 0; i < subFloors.Count; i++)
        {
            floors.Add(new Floor(
                subFloors[i].position.x - subFloors[i].localScale.x / 2
                , subFloors[i].position.z - subFloors[i].localScale.z / 2
                , Mathf.Abs(subFloors[i].localScale.x)
                , Mathf.Abs(subFloors[i].localScale.z)
                ));
        }
        
        DisplayCrossing();
        //if(isDebug)Util.Visualize(dots, debugParent, "cross");
        DisplayCorner();
        //if(isDebug)Util.Visualize(dots, debugParent,"dot1_");
        RemoveInsideDots();
        //if(isDebug)Util.Visualize(dots,debugParent, "dot2_");
        RemoveOutsideDots();
        //if(isDebug) Util.Visualize(dots, debugParent, "dot3_");

        // 중복 점 + 내부 점 추가 제거 (DY 2019 / 04 / 16)
        RemoveAdditional(ref dots, floors);
        //if (isDebug) Util.Visualize(dots, debugParent, "dot4_");

        // 공간의 바깥 외곽선 탐지용.
        List<Dot> orderdDots = FollowOutLine(dots, dots[0]);
        allDots.Add(orderdDots);
        List<Dot> findedDots = orderdDots.ToList();
        List<Dot> remainDots = dots.FindAll(x => !findedDots.Contains(x)).ToList();        
        int count = 0;
        while (remainDots.Count >0)
        {
            if (count++ > 99) break;
            List<Dot> orderedDots2 = FollowOutLine(remainDots, remainDots[0]);
            //Util.Visualize(orderedDots2, count + "_");
            allDots.Add(orderedDots2);
            findedDots.AddRange(orderedDots2.ToArray());
            remainDots = dots.FindAll(x => !findedDots.Contains(x)).ToList();
        }
        //if(isDebug) Util.Visualize(dots, "d");
        return allDots;
    }

    /// <summary>
    /// 예외처리 되지 않은 방의 아웃라인을 받고 싶을 때.
    /// 현재는 스냅에서만 쓰이고 있음.
    /// </summary>
    /// <param name="subFloors"></param>
    /// <returns></returns>
    public List<Dot> FindOutline(List<Transform> subFloors)
    {
        List<List<Dot>> alldots = FindAllOutline(subFloors);
        return alldots.OrderBy(x => -1 * x.Count()).First();
    }

    /// <summary>
    /// 단순 비교만으로는 삭제되지 않는 점들을 삭제하기 위한 함수.
    /// </summary>
    /// <param name="dots">삭제될 점이 있는 리스트</param>
    /// <param name="floors"></param>
    private void RemoveAdditional(ref List<Dot> dots, List<Floor> floors)
    {
        List<Dot> removeDots = new List<Dot>();
        foreach (var d in dots)
        {

            // 겹치는 점 존재시 삭제
            if (dots.FindAll(x => x != d && Util.IsEquals(d.position.x, x.position.x) && Util.IsEquals(d.position.z, x.position.z)
                   && (d.attribute.CompareTo("Corner") == 0 || d.attribute.CompareTo(x.attribute) == 0) ).Count > 0                
                && removeDots.FindAll(x => x != d && Util.IsEquals(d.position.x, x.position.x) && Util.IsEquals(d.position.z, x.position.z)).Count == 0)
            {
                removeDots.Add(d);
            }

            // 변 사이에 있는 corner 삭제
            if (floors.FindAll(x=>Util.GetSide(x, d.position)).Count() > 0 && d.attribute.CompareTo("Corner")==0
                && removeDots.FindAll(x => x != d && Util.IsEquals(d.position.x, x.position.x) && Util.IsEquals(d.position.z, x.position.z)).Count == 0)
            {
                removeDots.Add(d);
            }
        }

        foreach (var r in removeDots)
        {
            if (dots.FindAll(x => Util.IsEquals(r.position.x, x.position.x) && Util.IsEquals(r.position.z, x.position.z)).Count > 0) dots.Remove(r);
        }

        removeDots = new List<Dot>();
        foreach (var dot in dots)
        {
            if (Util.CheckIsDotOutside(floors, dot.position)) removeDots.Add(dot);
        }

        foreach (var dot in removeDots)
        {
            if (dots.FindAll(x => Util.IsEquals(dot.position.x, x.position.x) && Util.IsEquals(dot.position.z, x.position.z)).Count > 0) dots.Remove(dot);
        }
    }

    /// <summary>
    /// 바닥과 바닥의 모서리끼리 교차하는 부분을 점으로 나타내기 위한 함수.
    /// </summary>
    private void DisplayCrossing()
    {
        // 모든 바닥들에 대한 O(n^2) 검사 실시.
        // 바닥끼리 교차점을 표시.
        for (int i = 0; i < floors.Count; i++)
        {
            for (int j = i + 1; j < floors.Count; j++)
            {
                if (i != j)
                    GetIntersect(floors[i], floors[j]);
            }
        }
    }

    /// <summary>
    /// 두 바닥을 매개변수로 받아 서로 겹침여부 판단하고, 교차점 생성.
    /// </summary>
    /// <param name="r1">첫번 째 바닥</param>
    /// <param name="r2">두번 째 바닥</param>
    public void GetIntersect(Floor r1, Floor r2)
    {
        //바닥 두개가 겹쳤을 때 생기는 교차 영역
        Floor overlabedFloor = new Floor();

        // 사각형 둘이 겹치지 않을 때.
        if (Util.IsBig(r1.x , (r2.x + r2.w))) return;
        if (Util.IsBig(r2.x , (r1.x + r1.w))) return;
        if (Util.IsBig(r1.y , (r2.y + r2.h))) return;
        if (Util.IsBig(r2.y , (r1.y + r1.h))) return;

        // r1이 r2에 완전히 포함될때, 포함 된 상태에서 다른 바닥과 겹쳐있는 경우 제 기능을 못 할 수도 있음.
        if ((Util.IsBig(r1.x, r2.x) || Util.IsEquals(r1.x, r2.x))
            && (Util.IsBig((r2.x + r2.w), (r1.x + r1.w)) || Util.IsEquals((r2.x + r2.w), (r1.x + r1.w)))
            && (Util.IsBig((r2.y + r2.h), (r1.y + r1.h)) || Util.IsEquals((r2.y + r2.h), (r1.y + r1.h))) 
            && (Util.IsBig(r1.y, (r2.y + r2.h)) || Util.IsEquals(r1.y, (r2.y + r2.h))))
            return;

        // 사각형 둘이 겹칠 때.
        // 겹치는 교차점에 대한 좌표 생성.
        overlabedFloor.x = Mathf.Max(r1.x, r2.x);
        overlabedFloor.y = Mathf.Max(r1.y, r2.y);
        overlabedFloor.w = Mathf.Min(r1.x + r1.w, r2.x + r2.w) - overlabedFloor.x;
        overlabedFloor.h = Mathf.Min(r1.y + r1.h, r2.y + r2.h) - overlabedFloor.y;

        Vector3 overlapLeftUp = new Vector3(overlabedFloor.x, 0.1f, overlabedFloor.y + overlabedFloor.h);
        Vector3 overlapRightDown = new Vector3(overlabedFloor.x + overlabedFloor.w, 0.1f, overlabedFloor.y);

        // 두 사각형의 꼭짓점 끼리 닿아 있는 경우.
        if ((overlapLeftUp - overlapRightDown).magnitude < 0.001f)
        {
            return;
        }

        // 교차 영역 사각형이 선분 형태일 때.
        // 두 사각형이 평행으로 붙어있는 경우(r1이 오른쪽).
        if (Util.IsEquals(r1.x , (r2.x + r2.w)))
        {
            // r1 밑변이 r2보다 밑에있음.
            if (Util.IsBig(r2.y , r1.y))
            {
                //Left
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Left));
            }
            // r1 윗변이 r2보다 밑에 있음.
            if (Util.IsBig(r2.y + r2.h , (r1.y + r1.h)))
            {
                //Right
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y + overlabedFloor.h), Direction.Right));
            }
            if (Util.IsBig(r1.y + r1.h , (r2.y + r2.h)))
            {
                //Up
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y + overlabedFloor.h), Direction.Up));
            }
            if (Util.IsBig(r1.y , r2.y))
            {
                //Down
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Down));
            }
            return;
        }
        // 두 사각형이 평행으로 붙어있는 경우(r1이 왼쪽).
        if (Util.IsEquals((r1.x + r1.w) , r2.x))
        {
            if (Util.IsBig(r1.y , r2.y))
            {
                //Left
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Left));
            }
            if (Util.IsBig((r1.y + r1.h) , (r2.y + r2.h)))
            {
                //Right
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y + overlabedFloor.h), Direction.Right));
            }
            if (Util.IsBig((r2.y + r2.h) , (r1.y + r1.h)))
            {
                //Up
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y + overlabedFloor.h), Direction.Up));
            }
            if (Util.IsBig(r2.y , r1.y))
            {
                //Down
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Down));
            }
            return;
        }

        if (Util.IsEquals(r1.y , (r2.y + r2.h)))
        {
            if (Util.IsBig(r2.x , r1.x))
            {
                //Left
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Left));
            }
            if (Util.IsBig((r2.x + r2.w) , (r1.x + r1.w)))
            {
                //Rigwt
                dots.Add(new Dot(new Vector3(overlabedFloor.x + overlabedFloor.w, 0, overlabedFloor.y), Direction.Right));
            }
            if (Util.IsBig(r1.x , r2.x))
            {
                //Up
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Up));
            }
            if (Util.IsBig((r1.x + r1.w) , (r2.x + r2.w)))
            {
                //Down
                dots.Add(new Dot(new Vector3(overlabedFloor.x + overlabedFloor.w, 0, overlabedFloor.y), Direction.Down));
            }
            return;
        }

        if (Util.IsEquals((r1.y + r1.h) , r2.y))
        {
            if (Util.IsBig(r1.x , r2.x))
            {
                //Left
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Left));
            }
            if (Util.IsBig((r1.x + r1.w) , (r2.x + r2.w)))
            {
                //Rigwt
                dots.Add(new Dot(new Vector3(overlabedFloor.x + overlabedFloor.w, 0, overlabedFloor.y), Direction.Right));

            }
            if (Util.IsBig(r2.x , r1.x))
            {
                //Up
                dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Up));
            }
            if (Util.IsBig((r2.x + r2.w) , (r1.x + r1.w)))
            {
                //Down
                dots.Add(new Dot(new Vector3(overlabedFloor.x + overlabedFloor.w, 0, overlabedFloor.y), Direction.Down));
            }
            return;
        }


        // 교점이 교차부분 왼쪽 밑일 때.
        if (IsAcross(new Vector3(overlabedFloor.x, overlabedFloor.y, 0), r1, r2))
        {
            dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y), Direction.Left));
        }
        // 교점이 교차부분 오른쪽 밑일 때.
        if (IsAcross(new Vector3(overlabedFloor.x + overlabedFloor.w, overlabedFloor.y, 0), r1, r2))
        {
            dots.Add(new Dot(new Vector3(overlabedFloor.x + overlabedFloor.w, 0, overlabedFloor.y), Direction.Down));
        }
        // 교점이 교차부분 왼쪽 위일 때.
        if (IsAcross(new Vector3(overlabedFloor.x, overlabedFloor.y + overlabedFloor.h, 0), r1, r2))
        {
            dots.Add(new Dot(new Vector3(overlabedFloor.x, 0, overlabedFloor.y + overlabedFloor.h), Direction.Up));
        }
        // 교점이 교차부분 오른쪽 위일 때.
        if (IsAcross(new Vector3(overlabedFloor.x + overlabedFloor.w, overlabedFloor.y + overlabedFloor.h, 0), r1, r2))
        {
            dots.Add(new Dot(new Vector3(overlabedFloor.x + overlabedFloor.w, 0, overlabedFloor.y + overlabedFloor.h), Direction.Right));
        }
    }

    void DisplayCorner()
    {
        for (int i = 0; i < floors.Count; i++)
        {
            CreateCorner(floors[i].leftDown);
            CreateCorner(floors[i].leftUp);
            CreateCorner(floors[i].rightDown);
            CreateCorner(floors[i].rightUp);
        }
    }


    bool IsContainsPoint(Dictionary<Vector3,List<Floor>> dictionary, Vector3 point)
    {
        List<Vector3> keys = new List<Vector3>(dictionary.Keys);
        return keys.FindAll(x => Util.IsEquals(x.x, point.x) && Util.IsEquals(x.z, point.z)).Count > 0;
    }

    List<Floor> FindValue(Dictionary<Vector3, List<Floor>> dictionary, Vector3 point)
    {
        List<Vector3> keys = new List<Vector3>(dictionary.Keys);
        List<List<Floor>> values = new List<List<Floor>>(dictionary.Values);
        for (int i = 0; i < keys.Count; i++)
        {
            if( Util.IsEquals( keys[i].x, point.x) && Util.IsEquals(keys[i].z,point.z )) return values[i];
        }
        return null;
    }


    private void CreateCorner(Dot dot)
    {
        // 중복된 좌표가 있으면 기존 좌표를 삭제하고 새로 생성 하지 않음.
        // 짝수면 두 바닥이 평행으로 붙어있는 경우이고,
        // 홀수면 세 바닥이 직각을 이루는 형대로 꺾이는 것이기 때문에
        // 아래의 방식으로 예외처리 가능.
        Dot tempDot = dots.Find(x =>x.attribute == "Corner" && Util.IsEquals( x.position.x , dot.position.x) && Util.IsEquals(x.position.z, dot.position.z));
        
        if (tempDot == null && !IsContainsPoint(overlapPoint, dot.position))
        {            
            dots.Add(dot);            
        }
        else
        {
            try
            {
                // 이미 존재하는 점인지 검사.
                if (IsContainsPoint(overlapPoint, dot.position))
                {
                    // 해당 점의 바닥 정보를 찾아옴.
                    List<Floor> matchedFloors = FindValue(overlapPoint, dot.position);
                    matchedFloors.Add(dot.parentFloor);
                    // 바닥 3개가 ㄱ자로 붙어있을 때.
                    // 강제로 방향지정을 해주지 않으면 OutLiner가 고장나기 때문에 예외처리.
                    if (matchedFloors.Count == 3)
                    {
                        bool lu, ld, ru, rd;
                        lu = ld = ru = rd = false;

                        for (int i = 0; i < 3; i++)
                        {

                            if (Vector3.Equals(matchedFloors[i].leftUp.position, dot.position))
                            {
                                lu = !lu;
                            }
                            if (Vector3.Equals(matchedFloors[i].leftDown.position, dot.position))
                            {
                                ld = !ld;
                            }
                            if (Vector3.Equals(matchedFloors[i].rightUp.position, dot.position))
                            {
                                ru = !ru;
                            }
                            if (Vector3.Equals(matchedFloors[i].rightDown.position, dot.position))
                            {
                                rd = !rd;
                            }
                        }
                        List<Dot> removeDot = dots.FindAll(x => x.attribute == "Corner" && Util.IsEquals(x.position.x, dot.position.x) && Util.IsEquals(x.position.z, dot.position.z));                        
                        if (removeDot != null)
                        {
                            for (int i = 0; i < removeDot.Count; i++)
                            {
                                dots.Remove(removeDot[i]);
                            }
                        }
                        if (!lu)
                        {
                            //down
                            dots.Add(new Dot(dot.position, Direction.Down));
                        }
                        else if (!ld)
                        {
                            //right
                            dots.Add(new Dot(dot.position, Direction.Right));
                        }
                        else if (!ru)
                        {
                            //left
                            dots.Add(new Dot(dot.position, Direction.Left));
                        }
                        else if (!rd)
                        {
                            //up
                            dots.Add(new Dot(dot.position, Direction.Up));
                        }
                    }
                }
                else if (tempDot.attribute == "Corner" && !(tempDot.diretion == dot.diretion))
                {
                    overlapPoint.Add(dot.position, new List<Floor>() { dot.parentFloor });
                    FindValue(overlapPoint, dot.position).Add(tempDot.parentFloor);
                    dots.Remove(tempDot);                    
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

        }
    }

    /// <summary>
    /// 모든 점을 순회하면서 한 점이 다른 바닥의 안쪽에 포함되어있다면 무조건 삭제.
    /// </summary>
    private void RemoveInsideDots()
    {
        bool isRemoved = false;
        //모든 바닥들에 대한 O(n^2) 검사 실시.
        for (int i = 0; i < dots.Count; i++)
        {
            for (int j = 0; j < floors.Count; j++)
            {
                if (IsInside(dots[i], floors[j]))
                {
                    dots.Remove(dots[i]);                    
                    isRemoved = true;
                    break;
                }
            }
            if (isRemoved)
            {
                i--;
                isRemoved = false;
            }
        }
    }

    private void RemoveOutsideDots()
    {
        for (int i = 0; i < floors.Count; i++)
        {
            // 서로 평행으로 겹친 상태를 위한 예외처리.
            RemoveBetweenCorner(floors[i].leftUp, floors[i].rightUp);
            RemoveBetweenCorner(floors[i].rightDown, floors[i].rightUp);
            RemoveBetweenCorner(floors[i].leftDown, floors[i].rightDown);
            RemoveBetweenCorner(floors[i].leftDown, floors[i].leftUp);
        }
    }

    /// <summary>
    /// 평행선상에서 두 바닥의 모서리가 서로 포함되는 상황에 대한 예외처리 함수.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    private void RemoveBetweenCorner(Dot start, Dot end)
    {
        // start나 end와 평행선상의 점이 있다면 저장.
        Dot tempDot = dots.Find(
            dot => (Util.IsBig( dot.x , start.x) && Util.IsBig( end.x, dot.x))
            && (Util.IsEquals(dot.y , start.y)  && Util.IsEquals(dot.y , end.y))
            || (Util.IsEquals(dot.x , start.x)  && Util.IsEquals(dot.x , end.x) 
            && Util.IsBig( dot.y , start.y) && Util.IsBig( end.y, dot.y))
            );

        // 저장된 값이 있다면.
        if (tempDot != null && tempDot.parentFloor != null)
        {
            // 왼쪽이나 오른쪽 모서리에 붙어있는지 확인.
            if (Util.IsEquals(tempDot.x , start.x)  && Util.IsEquals(tempDot.x , end.x) )
            {
                // 해당 바닥이 아닌 평행선상의 다른 바닥에 있는지 여부 체크.
                // start와 end 사이에 있는 점의 바닥의 모서리에 start와 end중 어느것이라도 포함되어있다면.
                if (Util.IsBig( start.y , tempDot.parentFloor.leftDown.y) && Util.IsBig(tempDot.parentFloor.leftUp.y , start.y)
                    || Util.IsBig(start.y , tempDot.parentFloor.rightDown.y) && Util.IsBig(tempDot.parentFloor.rightUp.y , start.y))
                {
                    dots.Remove(start);
                    dots.Remove(tempDot);
                }
                else if (Util.IsBig(end.y , tempDot.parentFloor.leftDown.y) && Util.IsBig(tempDot.parentFloor.leftUp.y , end.y)
                    || Util.IsBig(end.y , tempDot.parentFloor.rightDown.y) && Util.IsBig(tempDot.parentFloor.rightUp.y , end.y))
                {
                    dots.Remove(end);
                    dots.Remove(tempDot);
                }
            }
            // 상하에 붙어있는 경우.
            else
            {
                if (Util.IsBig(start.x , tempDot.parentFloor.leftUp.x) && Util.IsBig(tempDot.parentFloor.rightUp.x , start.x)
                    || Util.IsBig(start.x , tempDot.parentFloor.leftDown.x) && Util.IsBig(tempDot.parentFloor.rightDown.x , start.x))
                {
                    dots.Remove(start);
                    dots.Remove(tempDot);

                }
                else if (Util.IsBig(end.x , tempDot.parentFloor.leftUp.x) && Util.IsBig(tempDot.parentFloor.rightUp.x , end.x)
                    || Util.IsBig(end.x , tempDot.parentFloor.leftDown.x) && Util.IsBig(tempDot.parentFloor.rightDown.x , end.x))
                {
                    dots.Remove(end);
                    dots.Remove(tempDot);
                }

            }
        }
        //dots.Find(dot => Util.IsBig(dot.x , start.x) && Util.IsBig(end.x , dot.x));
    }

    /// <summary>
    /// 모든 점들의 리스트를 받고, 점의 방향성대로 따라가며 외곽선을 순차적으로 만들어 반환하는 함수.
    /// </summary>
    /// <param name="_list">모든 점들이 무작위로 담겨있는 리스트</param>
    /// <param name="startPoint">시작점</param>
    /// <returns>정렬 완료된 리스트</returns>
    private List<Dot> FollowOutLine(List<Dot> _list, Dot startPoint)
    {
        Dot currentPoint = startPoint;
        List<Dot> orderedList = new List<Dot>();
        
        for (int i = 0; i < dots.Count && currentPoint != null; i++)
        {
            if (orderedList.Contains(currentPoint)) break;
            orderedList.Add(currentPoint);
            switch (currentPoint.diretion)
            {
                case Direction.Right:
                    currentPoint = _list.FindAll(x =>
                    Util.IsEquals(x.y, currentPoint.y) && Util.IsBig(x.x, currentPoint.x)
                    ).OrderBy(x => x.x).FirstOrDefault();
                    break;
                case Direction.Up:
                    currentPoint = _list.FindAll(x =>
                    Util.IsEquals(x.x, currentPoint.x) && Util.IsBig(x.y, currentPoint.y)
                    ).OrderBy(x => x.y).FirstOrDefault();
                    break;
                case Direction.Down:
                    currentPoint = _list.FindAll(x =>
                    Util.IsEquals(x.x, currentPoint.x) && Util.IsBig(currentPoint.y, x.y)
                    ).OrderByDescending(x => x.y).FirstOrDefault();
                    break;
                case Direction.Left:
                    currentPoint = _list.FindAll(x =>
                    Util.IsEquals(x.y, currentPoint.y) && Util.IsBig(currentPoint.x, x.x)
                    ).OrderByDescending(x => x.x).FirstOrDefault();
                    break;
                default:
                    break;
            }
            if (currentPoint == startPoint)
            {
                //순회 성공
                //Debug.Log(orderedList.Count);
                break;
            }

            if (currentPoint == null)
            {
                //Debug.Log("null" + orderedList.Count + " / " + _list.Count);
                break;
                //Util.Visualize(_list, "list");
            }
        }
        return orderedList;
    }

    /// <summary>
    /// 해당 점이 바닥의 안쪽에 있는지 검사.
    /// 점이 바닥의 모서리 라인에 있는 경우는 고려하지 않음.
    /// </summary>
    /// <param name="dot">검사할 점</param>
    /// <param name="r1">검사할 대상의 바닥</param>
    /// <returns></returns>
    bool IsInside(Dot dot, Floor r1)
    {
        float r1Width, r1Hight;
        r1Width = r1.x + r1.w;
        r1Hight = r1.y + r1.h;

        // 점이 바닥 바깥에 있을 때.
        if (Util.IsBig( dot.x , r1Width)) return false;
        if (Util.IsBig(r1.x , dot.x)) return false;
        if (Util.IsBig(dot.y , r1Hight)) return false;
        if (Util.IsBig(r1.y , dot.y)) return false;


        // 점이 바닥 안에 포함될 때.
        if (Util.IsBig(dot.x , r1.x)
            && Util.IsBig(r1Width , dot.x)
            && Util.IsBig(dot.y , r1.y)
            && Util.IsBig(r1Hight , dot.y))
            return true;
        return false;
    }

    /// <summary>
    /// 점이 두 바닥 사이에 겹쳐있는지 검사하는 함수.
    /// </summary>
    /// <param name="dot">검사할 점</param>
    /// <param name="r1">비교할 바닥 1</param>
    /// <param name="r2">비교할 바닥 2</param>
    /// <returns></returns>
    bool IsAcross(Vector3 dot, Floor r1, Floor r2)
    {

        float r1Width, r1Hight, r2Hight, r2Width;
        r1Width = r1.x + r1.w;
        r1Hight = r1.y + r1.h;
        r2Hight = r2.y + r2.h;
        r2Width = r2.x + r2.w;

        // 두 바닥의 좌표 중 하나의 축의 값이 같을 때.
        // (X축으로 겹쳐져 있거나 Y축으로 겹쳐져 있을 때)
        if (Util.IsEquals(dot.x , r1.x) && Util.IsEquals(dot.x , r2.x)
           || Util.IsEquals(dot.y , r1.y) && Util.IsEquals(dot.y , r2.y)
           || Util.IsEquals(dot.x , r1Width) && Util.IsEquals(dot.x , r2Width)
           || Util.IsEquals(dot.y , r1Hight) && Util.IsEquals(dot.y , r2Hight)) return false;


        // 교차점이 외곽선에 위치하는지 여부 검사.
        if (
            (Util.IsEquals(dot.x , r1.x) && Util.IsEquals(dot.y , r2Hight))
            || (Util.IsEquals(dot.x , r1Width) && Util.IsEquals(dot.y , r2Hight))
            || (Util.IsEquals(dot.y , r1.y) && Util.IsEquals(dot.x , r2.x))
            || (Util.IsEquals(dot.y , r1.y) && Util.IsEquals(dot.x , r2Width))
            || (Util.IsEquals(dot.x , r2.x) && Util.IsEquals(dot.y , r1Hight))
            || (Util.IsEquals(dot.x , r2Width) && Util.IsEquals(dot.y , r1Hight))
            || (Util.IsEquals(dot.y , r2.y) && Util.IsEquals(dot.x , r1.x))
            || (Util.IsEquals(dot.y , r2.y) && Util.IsEquals(dot.x , r1Width))
            )
            return true;

        return false;
    }

}
