using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SoldierSkillCmd : NetworkBehaviour {
    // Soldier 캐릭터의 특수 기술인 수류탄 투척 기능을 하는 스크립트
    public GameObject burst;
    private Vector3 burst_pos;

    public void GranadeBurst(Vector3 pos)
    {
        burst_pos = pos;
        if (isServer)
        {
            CmdBurst();
        }
    }

    [Command]
    void CmdBurst()
    {
        RpcBurst();
    }

    [ClientRpc]
    void RpcBurst()
    {
        GameObject _burst = Instantiate(burst, burst_pos, transform.rotation);
        _burst.GetComponent<BurstDamage>()._identify_player = gameObject.name;
        NetworkServer.Spawn(_burst);
    }
}
