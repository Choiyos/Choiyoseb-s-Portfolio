using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabUI : MonoBehaviour
{
    // Tab 키를 눌렀을 때, 플레이어들 간의 리더보드(킬 스코어, 데스 스코어 등)를 띄워주는 기능의 스크립트.
    public GameObject tabUI;
    public GameObject nameText;
    public GameObject killText;
    public GameObject deathText;
    GameObject[] temp_allPlayer;
    List<GameObject> _temp_allPlayer = new List<GameObject>();
    public GameObject[] nameText_remote;
    public GameObject[] killText_remote;
    public GameObject[] deathText_remote;

    void Start()
    {
        nameText.GetComponent<Text>().text = gameObject.transform.root.name;
        temp_allPlayer = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < temp_allPlayer.Length; i++)
        {
            if (temp_allPlayer[i].layer == 9 && !_temp_allPlayer.Contains(temp_allPlayer[i])) // remotePlayer인 경우.
            {
                _temp_allPlayer.Add(temp_allPlayer[i]);
            }
        }
    }

    void Update()
    {
        killText.GetComponent<Text>().text = gameObject.transform.root.GetComponent<Player>().killScore.ToString();
        deathText.GetComponent<Text>().text = gameObject.transform.root.GetComponent<Player>().deathScore.ToString();
        #region (난입이 있을 경우)
        temp_allPlayer = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < temp_allPlayer.Length; i++)
        {
            if (temp_allPlayer[i].layer == 9 && !_temp_allPlayer.Contains(temp_allPlayer[i])) // remotePlayer인 경우.
            {
                _temp_allPlayer.Add(temp_allPlayer[i]);
            }
        }
        #endregion

        for (int i = 0; i < _temp_allPlayer.Count; i++)
        {
            if (_temp_allPlayer[i] == null)
            {
                nameText_remote[i].GetComponent<Text>().text = "OUT";
                killText_remote[i].GetComponent<Text>().text = "-";
                deathText_remote[i].GetComponent<Text>().text = "-";
            }
            else
            {
                nameText_remote[i].GetComponent<Text>().text = _temp_allPlayer[i].name;
                killText_remote[i].GetComponent<Text>().text = _temp_allPlayer[i].GetComponent<Player>().killScore.ToString();
                deathText_remote[i].GetComponent<Text>().text = _temp_allPlayer[i].GetComponent<Player>().deathScore.ToString();
            }
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            tabUI.gameObject.SetActive(true);
        }
        else
            tabUI.gameObject.SetActive(false);
    }
}
