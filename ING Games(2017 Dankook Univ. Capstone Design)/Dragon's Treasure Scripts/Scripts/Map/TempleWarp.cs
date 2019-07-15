using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleWarp : MonoBehaviour {

    GameObject[] warpPosition;
    Vector3[] warpPositionv;
    string[] warpPositionName;
    void Start()
    {
        //warpPosition = GameObject.FindGameObjectsWithTag("Warp");
        warpPositionv = new Vector3[4];
        warpPositionv[0] = new Vector3(-0f, 0f, 14f);
        warpPositionv[1] = new Vector3(-14f, 0f, 0f);
        warpPositionv[2] = new Vector3(14f, 0f, 0f);
        warpPositionv[3] = new Vector3(-0f, 0f, -14f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Warp")
        {
            //Debug.Log("Player position : " + transform.position);
            //Debug.Log("other : " + other.name);
            //for (int i = 0; i < warpPositionv.Length-1; i++)
            //{
            //    //Debug.Log(warpPosition[i].name);
            //    if (warpPosition[i].name == other.name)
            //    {
            //        warpPosition[i] = warpPosition[warpPosition.Length - 1];
            //        warpPosition[warpPosition.Length - 1] = other.gameObject;
            //    }
            //}
            //GameObject warp = warpPositionv[Random.Range(0, warpPositionv.Length - 1)].transform.parent.GetChild(1).gameObject;

            //Vector3 v3 = warp.transform.position;
            transform.position = warpPositionv[Random.Range(0, warpPositionv.Length - 1)];
            //transform.rotation = warp.transform.rotation;
        }
    }
}
