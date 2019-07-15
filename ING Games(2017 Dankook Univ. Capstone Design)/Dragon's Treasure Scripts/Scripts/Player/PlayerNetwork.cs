using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetwork : NetworkBehaviour
{
    // 네트워킹을 위한 플레이어 단위의 초기화를 조정하는 스크립트.
    // 비활성화 시킬 컴포넌트 배열
    [SerializeField]
    Behaviour[] componentsToDisable;


    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    void Start()
    {
        if (!isLocalPlayer)
        {
            DisalbeComponents();
            AssignRemoteLayer();
        }
        GetComponent<Player>().Setup();
    }

    //클라이언트 시작할 때 자신의 이름을 Network ID로 바꿔줌
    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    // 로컬플레이어가 아니면 레이어를 "RemotePlayer"로 바꿔줌
    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    //배열에 있는 컴포넌트를 비활성화 시킴
    void DisalbeComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }
}
