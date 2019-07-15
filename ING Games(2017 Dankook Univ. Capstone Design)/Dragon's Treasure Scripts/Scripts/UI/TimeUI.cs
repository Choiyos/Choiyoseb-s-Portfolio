using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour {
    // 인게임상에서 다음 키 생성까지의 시간을 표시해주는 스크립트.
    GameObject Spawn_gameobj;

    void Awake()
    {
        Spawn_gameobj = GameObject.Find("SpawnManager");
    }

    void Update()
    {
        GetComponent<Text>().text = "Up to Key Create : " + 
            (Spawn_gameobj.GetComponent<KeySpawn>().spawnTime - (float)(Spawn_gameobj.GetComponent<KeySpawn>()._elapsedTime)).ToString("N1");
    }
}
