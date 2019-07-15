using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class KeyComponent : NetworkBehaviour
{
    // 열쇠 아이템의 기능을 위한 스크립트.
    public int numOfKey;
    public AudioSource keySound;
    public AudioClip keySoundClip;

    void Start()
    {
        numOfKey = 0;
    }

    public void GetOfKey(int value)
    {
        keySound.clip = keySoundClip;
        keySound.Play();
        CmdGetOfKey(value);
    }

    [Command]
    void CmdGetOfKey(int value)
    {
        RpcGetOfKey(value);
    }

    [ClientRpc]
    void RpcGetOfKey(int value)
    {
        numOfKey += value;
        numOfKey = Mathf.Clamp(numOfKey, 0, 3);
    }
}
