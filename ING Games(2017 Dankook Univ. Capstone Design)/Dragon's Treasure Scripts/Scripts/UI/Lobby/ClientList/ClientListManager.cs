using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RealTimeClient
{
    public class ClientListManager : MonoBehaviour
    {

        private Transform clientListContent;
        private GameObject clientItem;
        private Sprite[] clientRankImageArray;

        Socket hClntSock;

        void Start()
        {            
            hClntSock = RealTimeConnect.instance.GetSocket();

            clientListContent = GameObject.Find("ClientListContent").transform;
            clientItem = Resources.Load("Prefab/UI/ClientListItem", typeof(GameObject)) as GameObject;
            LoadRankSprites();
        }

        private void LoadRankSprites()
        {
            object[] loadedIcons = Resources.LoadAll("Image/Rank", typeof(Sprite));
            clientRankImageArray = new Sprite[loadedIcons.Length];
            for (int x = 0; x < loadedIcons.Length; x++)
            {
                clientRankImageArray[x] = (Sprite)loadedIcons[x];
            }
        }

        public void ClearClientList()
        {
            for (int i = 0; i < clientListContent.childCount; i++)
            {
                Destroy(clientListContent.GetChild(i).gameObject);
            }
        }

        public void RefreshClientList(byte[] _message)
        {
            int clientCount = Convert.ToInt32(_message[1]);
            int offset = 2;
            int clientNameLength = 0;
            int clientNameStart = 0;
            int clientRankIndex = 0;
            //Debug.Log(clientCount);
            for (int i = 0; i < clientCount; i++, offset++)
            {
                clientNameLength = _message[offset];
                clientNameStart = ++offset;

                byte[] client = new byte[clientNameLength];
                Array.Copy(_message, clientNameStart, client, 0, clientNameLength);
                string clientName = Encoding.ASCII.GetString(client);
                offset += clientNameLength;
                clientRankIndex = _message[offset];

                GameObject currentClient = Instantiate(clientItem, clientListContent);
                Image characterRankImage = currentClient.transform.GetChild(1).GetComponent<Image>();
                characterRankImage.sprite = clientRankImageArray[clientRankIndex - 1];

                Text clientNameContent = currentClient.GetComponentInChildren<Text>();
                clientNameContent.text = clientName;
            }
        }
    }
}