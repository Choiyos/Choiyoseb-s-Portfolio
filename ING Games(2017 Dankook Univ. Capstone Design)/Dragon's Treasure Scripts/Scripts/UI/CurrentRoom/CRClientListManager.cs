using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace RealTimeClient
{
    public class CRClientListManager : MonoBehaviour
    {
        private Transform clientListContent;
        private GameObject clientItem;

        private Sprite[] clientCharacterImageArray;
        private Sprite[] clientRankImageArray;


        void Start()
        {
            clientListContent = GameObject.Find("ClientList").transform;
            clientItem = Resources.Load("Prefab/UI/ClientItem", typeof(GameObject)) as GameObject;
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
            for (int i = 0; i < clientCount; i++, offset++)
            {     
                clientNameLength = Convert.ToInt32(_message[offset]);
                clientNameStart = ++offset;

                byte[] client = new byte[clientNameLength];
                Array.Copy(_message, clientNameStart, client, 0, clientNameLength);
                string clientName = Encoding.ASCII.GetString(client);

                offset += clientNameLength;
                clientRankIndex = Convert.ToInt32(_message[offset]);

                GameObject currentClient = Instantiate(clientItem, clientListContent);
                Text clientNameContent = currentClient.transform.GetChild(2).GetComponent<Text>();
                clientNameContent.text = clientName;

                Image clientRankImage = currentClient.transform.GetChild(4).GetComponent<Image>();
                clientRankImage.sprite = clientRankImageArray[clientRankIndex-1];
            }
        }
    }
}