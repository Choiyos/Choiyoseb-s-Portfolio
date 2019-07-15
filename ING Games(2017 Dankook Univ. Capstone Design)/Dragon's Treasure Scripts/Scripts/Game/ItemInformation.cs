using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemInformation : NetworkBehaviour {
    // 아이템의 애니메이션 기능을 하는 스크립트.
    public float rotSensitive;
    public float movSensitive;
    public string itemClassification;
    public int number;
    float velocity;
    float elapsTime;

    void Update()
    {
        elapsTime += Time.deltaTime;
        transform.localEulerAngles += new Vector3(0, rotSensitive, 0);
        transformMove();
    }

    void transformMove()
    {
        velocity = movSensitive * Mathf.Sin(3 * elapsTime);
        transform.Translate(0, velocity, 0);
    }
}
