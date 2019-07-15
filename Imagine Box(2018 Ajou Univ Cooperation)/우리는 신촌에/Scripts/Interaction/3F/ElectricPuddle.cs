using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPuddle : MonoBehaviour,BehavioralObject {

    SceneEventSystem sceneEventSystem;


    private void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        sceneEventSystem.MonologueBubbles(player,"배전반에서 누전이 일어나고 있다. 가까이 다가가는 건 위험하다.", 1.5f);
    }
}
