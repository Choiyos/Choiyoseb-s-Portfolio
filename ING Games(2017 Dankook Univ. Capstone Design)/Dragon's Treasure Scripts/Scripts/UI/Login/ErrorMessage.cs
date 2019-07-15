using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ErrorMessage : MonoBehaviour
{

    void OnEnable()
    {
        StartCoroutine(Error());
    }

    IEnumerator Error()
    {
        yield return new WaitForSeconds(3.0f);
        gameObject.SetActive(false);
    }
}
