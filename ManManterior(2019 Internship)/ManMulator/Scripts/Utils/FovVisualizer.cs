using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <b>사용 위치</b> : DecorateSpace scene<br></br>
/// <b>사용 시점</b> : fps camera가 활성화 될 때<br></br>
/// 미니맵 화면에 띄울 fps camera 시야 시각화를 위한 클래스.<br></br>
/// fixedUpdate로 매 프레임마다 카메라 시야 위치를 계산하여 mesh를 업데이트.
/// </summary>
public class FovVisualizer : MonoBehaviour
{
    // 위치와 시야를 참조할 1인칭 카메라
    [SerializeField]
    Camera fpsCamera = default;

    [SerializeField]
    Camera minimapCamera = default;

    // 시야 범위를 나타낼 반투명 mesh
    [SerializeField]
    MeshFilter fovVisualizer = default;

    // 시야 가장자리 점선
    [SerializeField]
    LineRenderer lineL = default, lineR = default;

    Mesh mesh;

    // 미니맵 활성화 상태
    public bool isOn = false;

    private void Awake()
    {
        mesh = new Mesh();
    }

    /// <summary>
    /// 미니맵이 활성화 되어 있을 때
    /// fps 카메라의 위치와 양쪽 끝 시야 범위(viewport point) 좌표를 받아와서 시야를 나타내는 mesh를 새로 생성하여 업데이트.
    /// </summary>
    private void FixedUpdate()
    {
        if (!isOn) return;

        Vector3 position = fpsCamera.transform.position;
        this.transform.position = position;

        Vector3[] points = new Vector3[3];
        points[0] = Vector3.zero;
        points[1] = fpsCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, fpsCamera.farClipPlane));
        points[1].y = position.y;
        points[2] = fpsCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, fpsCamera.farClipPlane));
        points[2].y = position.y;

        mesh.vertices = new Vector3[] { points[0], points[1], points[2] };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.MarkDynamic();
        fovVisualizer.GetComponent<MeshFilter>().mesh = mesh;

        lineL.SetPosition(0, Vector3.zero);
        lineR.SetPosition(0, Vector3.zero);

        lineL.SetPosition(1, points[1]);
        lineR.SetPosition(1, points[2]);

        minimapCamera.transform.position = this.transform.position + Vector3.up * 10f;
    }
}
