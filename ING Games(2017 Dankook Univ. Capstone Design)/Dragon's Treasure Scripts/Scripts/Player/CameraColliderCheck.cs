using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraColliderCheck : MonoBehaviour
{
    // 3인칭 시점에서 캐릭터가 벽면에 가려지는 문제를 해결하기 위해, 적정거리에 따라 캐릭터 카메라가 확대되는 기능을 하는 스크립트
    public GameObject CamPos;
    public Camera mainCam;
    int layerMask = 0;

    void LateUpdate()
    {
        layerMask = 1 << 10;
        Vector3 dir = CamPos.transform.position - transform.position;
        RaycastHit hitinfo;
        if (Physics.Raycast(transform.position, dir, out hitinfo, 3f,layerMask)) // (벽면, 플레이어) 벽에 레이캐스트가 걸리는 경우
        {
            float newYpos = -Vector3.Distance(transform.position, hitinfo.point) * 0.5f + 1.9f;
            {
                mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, hitinfo.point + new Vector3(0, newYpos, 0), 7f * Time.deltaTime);
            }
        }
        else // (벽면, 플레이어) 벽에 레이캐스트가 걸리지 않는 경우
            mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, CamPos.transform.position, 7f * Time.deltaTime);
    }
}
