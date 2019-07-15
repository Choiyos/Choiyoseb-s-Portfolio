using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    [SerializeField]
    GameObject target;
    Vector3 targetPos;
    [SerializeField]
    float sensitivity;
    Vector3 temp;
    IEnumerator routine;

    public float angle;

    void Start()
    {
        routine = update();
        StartCoroutine(routine);
    }

    public void StopCamMove()
    {
        StopCoroutine(routine);
    }

    public void StartCamMove()
    {
        routine = update();
        StartCoroutine(routine);
    }

    public void SetTarget(Vector3 targetPos)
    {
        target = null;
        this.targetPos = targetPos;
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;
    }

    public void SetTarget(Vector3 targetPos, float sensitivity_) // 카메라의 속도 조정이 필요할 경우 sensitivity를 설정.
    {
        target = null;
        this.targetPos = targetPos;
        sensitivity = sensitivity_;
    }

    public void SetTarget(GameObject target, float sensitivity_)
    {
        this.target = target;
        sensitivity = sensitivity_;
    }

    IEnumerator update()
    {
        while (true)
        {
            yield return null;
            if (target)
            {
                temp = target.transform.position + new Vector3(0, 1.7f * angle, -angle);
            }
            else
            {
                temp = targetPos + new Vector3(0, 1.7f * angle, -angle);
            }
            transform.position = Vector3.Lerp(transform.position, temp, Time.deltaTime * sensitivity);
        }
    }

    public void ShakeCamera()
    {
        StopCamMove();
        transform.DOShakePosition(1, 0.2f, 15, 70);
    }

    public void MoveCamera(GameObject target)
    {
        transform.position = target.transform.position + new Vector3(0, 1.7f * angle, -angle);
    }
}
