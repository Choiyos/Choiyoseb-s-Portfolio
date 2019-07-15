using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCallback : MonoBehaviour
{
    // 각 씬에 AddComponent해서 씬의 초기화를 도울 수 있도록 하는 스크립트.

    SceneInitializer sceneInitializer;

    void Awake()
    {
        sceneInitializer = GameObject.Find("GameManager").GetComponent<SceneInitializer>();
        sceneInitializer.SceneChanged();
    }
}
