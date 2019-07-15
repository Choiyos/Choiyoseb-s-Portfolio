using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GranadeMove : NetworkBehaviour
{
    // Solider의 특수 기술인 수류탄 투척 기능을 하는 스크립트.
    public float rotX;
    public float Xspeed;
    public float Yspeed;
    public GameObject burst;
    public GameObject rotateModel;
    float elapsTime;

    void Start()
    {
        elapsTime = 0;
        StartCoroutine(update());
    }

    IEnumerator update()
    {
        yield return null;
        while (true)
        {
            elapsTime += Time.deltaTime;
            if (elapsTime >= 5)
            {
                GameObject.Find(GetComponent<RangedAttackTrigger>().identify_Player).
                    GetComponent<SoldierSkillCmd>().GranadeBurst(transform.position);
                Destroy(gameObject);
                yield break;
            }
            rotateModel.transform.Rotate(rotX * Time.deltaTime, 0, 0);
            transform.Translate(Vector3.forward * Time.deltaTime * Xspeed);
            transform.Translate(0, Yspeed * Time.deltaTime, 0);
            yield return new WaitForFixedUpdate();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" || col.tag == "Map")
            GameObject.Find(GetComponent<RangedAttackTrigger>().identify_Player).
                GetComponent<SoldierSkillCmd>().GranadeBurst(transform.position);
        Destroy(gameObject);
    }
}
