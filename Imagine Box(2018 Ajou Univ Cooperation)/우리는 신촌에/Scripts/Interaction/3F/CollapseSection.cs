using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapseSection : MonoBehaviour {



    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            GameObject.Find("UnstableBookshelf").GetComponent<UnstableBook>().IsZombieIn = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            GameObject.Find("UnstableBookshelf").GetComponent<UnstableBook>().IsZombieIn = false;
            GameObject.Find("GameManager").SendMessage("GameOver");
        }
    }


}
