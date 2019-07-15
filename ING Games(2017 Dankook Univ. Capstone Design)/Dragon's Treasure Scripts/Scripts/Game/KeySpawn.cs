using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class KeySpawn : NetworkBehaviour
{
    // 동적으로 열쇠 아이템을 Spawn해주는 스크립트.
    public GameObject KeyObject;
    Transform Spawner;
    public static KeySpawn m_Instance;
    [SyncVar]
    public float spawnTime = 20.0f;
    public float _elapsedTime = 0.0f;
    public List<Vector3> keySpawnPos = new List<Vector3>();
    int itemNumber = 100;

    void Awake()
    {
        Spawner = GameObject.Find("KeySpawnPoint").transform;
        if (m_Instance != null)
        {
            Debug.LogError("More than one GameManager in scene");
        }
        else
        {
            m_Instance = this;
        }
        for (int i = 0; i < Spawner.childCount; i++)
        {
            keySpawnPos.Add(Spawner.GetChild(i).position);
        }
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= 20.0f)
        {
            if (isServer)
                CmdKeySpawn();
            _elapsedTime = 0f;
        }
    }

    public static KeySpawn Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new KeySpawn();
            }
            return m_Instance;
        }
    }

    [Command]
    public void CmdKeySpawn()
    {
        RpcKeySpawning();
    }

    [ClientRpc]
    void RpcKeySpawning()
    {
        if (keySpawnPos.Count == 0) return;
        Vector3 pos = keySpawnPos[Random.Range(0, keySpawnPos.Count)];
        GameObject key = Instantiate(KeyObject, pos, transform.rotation);
        keySpawnPos.Remove(pos);
        key.GetComponent<ItemInformation>().number = itemNumber++;
        NetworkServer.Spawn(key);
    }

    [Command]
    public void CmdSpawn(Vector3 pos)
    {
        RpcSpawn(pos);
    }

    [ClientRpc]
    void RpcSpawn(Vector3 pos)
    {
        GameObject key = Instantiate(KeyObject, pos, transform.rotation);
        key.GetComponent<ItemInformation>().number = itemNumber++;
        NetworkServer.Spawn(key);
    }
}
