using UnityEngine;

public class ScrollValue : MonoBehaviour
{

    int childNum = 0;
    int tempNum = 9;
    Vector3 setY = new Vector3(0, 5, 0);
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
