using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollValue2 : MonoBehaviour
{


    int childNum = 0;
    int tempNum = 7;
    Vector3 setY = new Vector3(0, 30, 0);
    void Update()
    {
        childNum = gameObject.transform.childCount;
        if (childNum > tempNum)
        {
            GetComponent<RectTransform>().transform.position += setY;
            tempNum = childNum;
        }
    }
}
