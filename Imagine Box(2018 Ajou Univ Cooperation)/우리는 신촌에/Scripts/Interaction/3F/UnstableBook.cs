using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstableBook : MonoBehaviour,BehavioralObject{

    [SerializeField]
    GameObject zombie;
    SceneEventSystem sceneEventSystem;
    bool isZombieIn = false;
    bool isInteractWithBookshelf = false;

    public bool IsZombieIn
    {
        get
        {
            return isZombieIn;
        }

        set
        {
            isZombieIn = value;
        }
    }

    public bool IsInteractWithBookshelf
    {
        get
        {
            return isInteractWithBookshelf;
        }

        set
        {
            isInteractWithBookshelf = value;
        }
    }

    private void Awake()
    {
        sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
    }

    public void BehaviorByInteraction(GameObject player)
    {
        if (IsZombieIn)
        {
            StartCoroutine( BookshelfFalling());
        }
        else
        {
            sceneEventSystem.MonologueBubbles(sceneEventSystem.Player, "서가가 불안하게 서 있다. 살짝 밀면 넘어갈 것 같다.", 1.5f);
            IsInteractWithBookshelf = true;
        }
    }

    private IEnumerator BookshelfFalling()
    {
        yield return null;
        GetComponent<Animator>().SetBool("active", true);
        yield return new WaitForSeconds(0.7f);
        Destroy(zombie);
        GetComponent<BoxCollider>().enabled =false;
        GameObject.Find("LibraryKid").GetComponent<LibraryKid>().IsZombieDead = true;
        gameObject.layer = 0;
    }
    
}
