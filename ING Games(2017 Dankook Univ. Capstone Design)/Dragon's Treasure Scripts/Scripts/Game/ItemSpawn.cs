using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemSpawn : NetworkBehaviour
{
    // 동적으로 아이템을 Spawn 해주는 기능의 스크립트.
    public GameObject[] ItemObject;
    public Transform Spawner;
    public static ItemSpawn m_Instance;
    public float spawnTime = 20.0f;
    int itemNumber = 0;

    void Start()
    {
        if (!isServer)
        {
            GetComponent<ItemSpawn>().enabled = false;
        }
        Spawner = GameObject.Find("ItemSpawnPoint").transform;
        if (m_Instance != null)
        {
            Debug.LogError("More than one GameManager in scene");
        }
        else
        {
            m_Instance = this;
        }

        if (isServer)
        {
            for (int i = 0; i < Spawner.childCount; i++)
            {
                StartCoroutine(Spawn(Spawner.GetChild(i).transform.position, Random.Range(0, ItemObject.Length)));
            }
        }
    }

    void Update()
    {
    }

    public static ItemSpawn Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new ItemSpawn();
            }
            return m_Instance;
        }
    }

    public IEnumerator Spawn(Vector3 pos, int objNum)
    {
        yield return new WaitForSeconds(spawnTime);
        RpcSpawning(pos, objNum);
    }

    [ClientRpc]
    void RpcSpawning(Vector3 pos, int objNum)
    {
        GameObject obj = Instantiate(ItemObject[objNum], pos, transform.rotation);
        obj.GetComponent<ItemInformation>().number = itemNumber++;
        NetworkServer.Spawn(obj);
    }
}
