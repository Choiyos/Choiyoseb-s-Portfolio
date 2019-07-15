using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoDownstair : MonoBehaviour
{
    [SerializeField]
    List<GameObject> floors;
    GameObject player;
    SectionRendering sectionRendering;
    SceneEventSystem sceneEventSystem;
    PlayerRayCast playerRayCast;
    PlayerProperty playerProperty;
    GameObject floorEntrance;

    void Awake()
    {
        sectionRendering = GameObject.Find("IngameScene").GetComponent<SectionRendering>();
        sceneEventSystem = sectionRendering.GetComponent<SceneEventSystem>();
        player = sceneEventSystem.Player;
        playerRayCast = player.GetComponent<PlayerRayCast>();
        playerProperty = player.GetComponent<PlayerProperty>();
        floorEntrance = GameObject.Find("FloorEntrance");
    }

    public void BehaviorByInteraction()
    {
        StartCoroutine(BehaviorByInteraction_());
    }

    IEnumerator BehaviorByInteraction_()
    {
        int standingSection = int.Parse(playerProperty.StandingSection.transform.parent.name.Substring(0, 1));
        sceneEventSystem.TouchCanvas.SetActive(false);
        yield return StartCoroutine(sceneEventSystem.SceneFade(true));
        sceneEventSystem.TouchCanvas.SetActive(true);
        StartCoroutine(sceneEventSystem.SceneFade(false));

        playerRayCast.StopUpdate();
        playerProperty.UpAndDown = false;
        if (transform.parent.name == "LeftStairs")
        {
            player.transform.position = floorEntrance.transform.GetChild((standingSection - 2) * 2).transform.position;
        }
        else
        {
            player.transform.position = floorEntrance.transform.GetChild((standingSection - 2) * 2 + 1).transform.position;
        }
        Camera.main.GetComponent<CameraMove>().MoveCamera(player);
        
        floors[standingSection - 2].SetActive(true);
        floors[standingSection - 2].SendMessage("FloorInit");
        yield return new WaitForSeconds(0.1f);
        playerRayCast.StartUpdate();
        floors[standingSection - 1].SetActive(false);
        yield break;
    }
}
