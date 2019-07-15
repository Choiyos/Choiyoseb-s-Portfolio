using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkTransform_custom : NetworkBehaviour
{
    [SyncVar]
    private Vector3 SyncPos;
    [SyncVar]
    private Quaternion SyncRot;
    [SerializeField]
    private float lerpRatePos = 3f;
    [SerializeField]
    private float lerpRateRot = 2.5f;
    [SerializeField]
    private Transform myTransform;

    private Vector3 lastPos;
    private float thresholdPos = 0.05f;
    private Quaternion lastRot;
    private float thresholdRot = 0.5f;

    void Update()
    {
        LerpPosition();
        LerpRotation();
    }

    void FixedUpdate()
    {
        TransmitPosition();
        TransmitRotation();
    }

    void LerpPosition()
    {
        myTransform.transform.position = Vector3.Lerp(myTransform.position, SyncPos, Time.deltaTime * lerpRatePos);
    }

    void LerpRotation()
    {
        myTransform.transform.rotation = Quaternion.Lerp(myTransform.rotation, SyncRot, Time.deltaTime * lerpRateRot);
    }

    [Command]
    void CmdProvideToServerPosition(Vector3 pos)
    {
        RpcProvidePositionToServer(pos);
    }

    [Command]
    void CmdProvideToServerRotation(Quaternion rot)
    {
        RpcProvideRotationToServer(rot);
    }

    [ClientRpc]
    void RpcProvidePositionToServer(Vector3 pos)
    {
        SyncPos = pos;
    }

    [ClientRpc]
    void RpcProvideRotationToServer(Quaternion rot)
    {
        SyncRot = rot;
    }

    [Client]
    void TransmitPosition()
    {
        if (isLocalPlayer && Vector3.Distance(myTransform.position, lastPos) > thresholdPos)
        {
            CmdProvideToServerPosition(myTransform.position);
            lastPos = myTransform.position;
        }
    }

    [Client]
    void TransmitRotation()
    {
        if (isLocalPlayer && Quaternion.Angle(myTransform.rotation, lastRot) > thresholdRot)
        {
            CmdProvideToServerRotation(myTransform.rotation);
            lastRot = myTransform.rotation;
        }
    }
}
