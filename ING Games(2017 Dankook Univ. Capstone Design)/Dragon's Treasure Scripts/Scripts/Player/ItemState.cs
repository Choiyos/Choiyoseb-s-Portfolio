using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemState : NetworkBehaviour
{
    // 맵에 자동으로 생성되는 아이템들의 기능을 위한 스크립트.
    public GameObject effectPrefeb;

    public void OnTriggerEnter(Collider col)
    {
        if (isLocalPlayer)
        {
            if (col.tag == "Item")
            {
                switch (col.GetComponent<ItemInformation>().itemClassification)
                {
                    case "Key": // 아이템이 열쇠인 경우
                        {
                            if (GetComponent<KeyComponent>().numOfKey != 3)
                                GetComponentInChildren<MessageUI>().PunchMessage("You've Got 1 Key !");
                            GetComponent<KeyComponent>().GetOfKey(1);
                            CmdKeyDestroy_(col.GetComponent<ItemInformation>().number);
                            break;
                        }
                    case "HealItem": // 아이템이 체력 회복 아이템인 경우
                        {
                            GetComponent<HpComponent>().CmdHpModification(GetComponent<HpComponent>().maxHealthPoint * 0.3f);
                            CmdHealEffect(transform.position, gameObject);
                            GetComponentInChildren<MessageUI>().
                                PunchMessage("Recovered " + GetComponent<HpComponent>().maxHealthPoint * 0.3f + "Helath point !");
                            CmdItemDestroy_(col.GetComponent<ItemInformation>().number);
                            break;
                        }
                    case "AttackUpItem": // 아이템이 공격력 증가 아이템인 경우
                        {
                            GetComponent<Player>().CmdAttackPowerIncrease(10.0f);
                            GetComponentInChildren<MessageUI>().PunchMessage("Increased 10 Attack power !");
                            CmdItemDestroy_(col.GetComponent<ItemInformation>().number);
                            break;
                        }
                    case "SpeedUpItem": // 아이템이 속도 증가 아이템인 경우
                        {
                            GetComponent<Player>().CmdSpeedIncrease(0.5f);
                            GetComponentInChildren<MessageUI>().PunchMessage("Increased 0.5 Moving speed !");
                            CmdItemDestroy_(col.GetComponent<ItemInformation>().number);
                            break;
                        }
                }
            }
        }
    }

    [Command]
    public void CmdHealEffect(Vector3 pos, GameObject parent) // 체력 회복 아이템의 파티클 이펙트 생성
    {
        RpcHealEffect(pos, parent);
    }

    [ClientRpc]
    public void RpcHealEffect(Vector3 pos, GameObject parent)
    {
        GameObject HealEffect = Instantiate(effectPrefeb, pos, parent.transform.rotation, parent.transform);
        NetworkServer.Spawn(HealEffect);
    }

    [Command]
    void CmdItemDestroy_(int id) // 아이템을 획득할 경우 사라지는 처리 함수
    {
        RpcItemDestroy_(id);
    }

    [ClientRpc]
    void RpcItemDestroy_(int id)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            if (item.GetComponent<ItemInformation>().number == id)
            {
                ItemSpawn Item_Manager = GameObject.Find("SpawnManager").GetComponent<ItemSpawn>();
                StartCoroutine(Item_Manager.Spawn(item.transform.position, UnityEngine.Random.Range(0, Item_Manager.ItemObject.Length)));
                Destroy(item);
                break;
            }
        }
    }

    [Command]
    void CmdKeyDestroy_(int id) // 열쇠를 획득할 경우 사라지는 처리 함수
    {
        RpcKeyDestroy_(id);
    }

    [ClientRpc]
    void RpcKeyDestroy_(int id)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            if (item.GetComponent<ItemInformation>().number == id)
            {
                KeySpawn Item_Manager = GameObject.Find("SpawnManager").GetComponent<KeySpawn>();
                Item_Manager.keySpawnPos.Add(item.transform.position);
                Destroy(item);
                break;
            }
        }
    }
}

