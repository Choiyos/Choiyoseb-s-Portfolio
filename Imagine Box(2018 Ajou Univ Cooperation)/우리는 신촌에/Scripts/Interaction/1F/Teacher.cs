using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teacher : MonoBehaviour, BehavioralObject
{
    [SerializeField]
    int countOfSavedChild; // 구해진 아이들의 수
    [SerializeField]
    SceneEventSystem sceneEventSystem;
    [SerializeField]
    [Header("아이구출시 이동속도 감소 비율")]
    [Range(0, 1)]
    float reductionRate;
    [SerializeField]
    CountOfLivedChildUI countOfLivedChildUI;

    public int CountOfSavedChild
    {
        get
        {
            return countOfSavedChild;
        }

        set
        {
            countOfSavedChild = value;
        }
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (player.GetComponent<PlayerProperty>().IsWithchild)
        {
            CountOfSavedChild++;
            if (countOfSavedChild == 7)
            {
                sceneEventSystem.SpeechBubbles(gameObject, "모든 아이들을 데려오셨군요! 이제 탈출합시다!", 1.5f);
            }
            else
            {
                sceneEventSystem.SpeechBubbles(gameObject, "아이를 데려왔군요! 부디 남은 아이들도 부탁해요...", 1.5f);
            }
            player.GetComponent<PlayerProperty>().IsWithchild = false;
            sceneEventSystem.PlayerSpeedRestore(player.GetComponent<PlayerInput>());
            countOfLivedChildUI.UIupdate();
            if (sceneEventSystem.ToBeSavedKid == "LibraryKid")
            {
                player.GetComponent<Inventory>().AddItem(new Item("FoldedPaper", "접힌 종이", "무언가 적혀있는 종이다.", Item.ItemType.View));
                sceneEventSystem.PushSystemMessage("학생 : 선생님 이거.. 2층에서 주웠어요.", 1);
                sceneEventSystem.PushSystemMessage("접힌 종이를 얻었다.", 1);
                sceneEventSystem.ToBeSavedKid = string.Empty;
            }
        }
        else
            sceneEventSystem.SpeechBubbles(gameObject, "학교에 남은 아이들을 꼭 데려와 주세요...", 1.5f);
    }
}
