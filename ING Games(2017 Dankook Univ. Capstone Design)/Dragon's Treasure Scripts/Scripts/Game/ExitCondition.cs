using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ExitCondition : NetworkBehaviour
{
    // 게임이 끝날 조건 (열쇠 3개를 모은 상태로 출구에 도달할 경우)을 판단하고, 플레이어에게 메세지를 던지는 기능을 하는 스크립트.
    int posIndex = 1;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 11 && GetComponent<KeyComponent>().numOfKey >= 3)
        {
            transform.Find("Canvas").Find("GameOverUI").GetComponent<GameOverUI>().PunchGameOver(name);
            CmdEnd(name);
        }
        else if (col.gameObject.layer == 11 && GetComponent<KeyComponent>().numOfKey < 3)
        {
            if (col)
                GetComponentInChildren<MessageUI>().
                                    PunchMessage("Three keys are required to take this dragon's treasure.");
        }
    }

    [Command]
    void CmdEnd(string name)
    {
        RpcEnd(name);
    }

    [ClientRpc]
    void RpcEnd(string name)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (GameObject.Find("Maze") && GameObject.Find("SpawnManager"))
        {
            GameObject.Find("Maze").SetActive(false);
            GameObject.Find("SpawnManager").SetActive(false);
        }
        foreach (GameObject player in players)
        {
            if (player.name == name)
            {
                player.transform.position = new Vector3(0, 0, 0);
                continue;
            }
            else
            {
                player.transform.position = new Vector3(1.2f, 0.5f, (posIndex++) * 1.5f);
                print(player.transform.name + "의 position = " + player.transform.position);
                player.transform.Find("Canvas").Find("GameOverUI").GetComponent<GameOverUI>().PunchGameOver(name);
            }
        }
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            print(item.name);
            item.SetActive(false);
        }

        GameObject.Find("Plane").GetComponent<AudioSource>().Stop();
    }
}
