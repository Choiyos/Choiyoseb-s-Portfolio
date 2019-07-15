using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace RealTimeClient
{
    public class ClientBoxManager : MonoBehaviour
    {


        int clientProfileImageIndex = 0;
        int clientRankImageIndex = 0;
        private GameObject clientBox;
        private Sprite[] clientCharacterImageArray;
        private Sprite[] clientRankImageArray;
        GameObject CreatRoomButton;
        Socket hClntSock;

        #region ButtonClick
        private Button[] playerButton;
        private Button playerProfileButton;
        int avatarIndex = 0;
        #endregion

        void Start()
        {
            hClntSock = RealTimeConnect.instance.GetSocket();
            LoadCharacterSprites();
            LoadRankSprites();
            LoadButtons();
            CreatRoomButton = GameObject.Find("CreateRoom");
        }

        private void LoadButtons()
        {
            clientBox = GameObject.Find("ClientBox");
            playerProfileButton = clientBox.transform.GetChild(2).GetComponent<Button>();
            playerProfileButton.onClick.AddListener(() => ClienBoxSetActive(false));
            playerButton = clientBox.transform.GetChild(3).GetComponentsInChildren<Button>();
            foreach (Button btn in playerButton)
            {
                btn.onClick.AddListener(delegate { AvatarPicker(btn.name); });
            }
        }

        private void ClienBoxSetActive(bool _flag)
        {
            //for (int i = 0; i < 3; i++)
            //{
            //    clientBox.transform.GetChild(i).gameObject.SetActive(_flag);
            //}
            clientBox.transform.GetChild(3).gameObject.SetActive(!_flag);
            CreatRoomButton.SetActive(_flag);
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

        private void LoadCharacterSprites()
        {
            object[] loadedIcons = Resources.LoadAll("Image/Character", typeof(Sprite));
            clientCharacterImageArray = new Sprite[loadedIcons.Length];
            for (int x = 0; x < loadedIcons.Length; x++)
            {
                clientCharacterImageArray[x] = (Sprite)loadedIcons[x];
            }
        }

        public void SettingClientBox(byte[] _message)
        {
            clientProfileImageIndex = Convert.ToInt32(_message[1]);
            if (clientProfileImageIndex == 0) clientProfileImageIndex = 1;
            clientRankImageIndex = Convert.ToInt32(_message[2]);

            byte[] client = new byte[_message.Length - 3];
            Array.Copy(_message, 3, client, 0, _message.Length - 3);
            string clientName = Encoding.UTF8.GetString(client);
            clientBox.transform.GetChild(1).GetComponent<Text>().text = clientName;

            Image characterRankImage = clientBox.transform.GetChild(0).GetComponent<Image>();
            characterRankImage.sprite = clientRankImageArray[clientRankImageIndex - 1];

            Image characterImage = clientBox.transform.GetChild(2).GetComponent<Image>();
            characterImage.sprite = clientCharacterImageArray[clientProfileImageIndex - 1];
        }

        private void ChangeProfileCharacter()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", (Char)HeaderConstValue.ClientBoxDat, (Char)(clientProfileImageIndex + 1));
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] clientCharacterChange_buf = new byte[sb.Length];
            clientCharacterChange_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientCharacterChange_buf);

            Image characterImage = clientBox.transform.GetChild(2).GetComponent<Image>();
            characterImage.sprite = clientCharacterImageArray[clientProfileImageIndex];
        }

        private void AvatarPicker(string buttonName)
        {
            switch (buttonName)
            {
                case "ArcherImage":
                    avatarIndex = 0;
                    break;
                case "BruteImage":
                    avatarIndex = 1;
                    break;
                case "PaladinImage":
                    avatarIndex = 2;
                    break;
                case "SwatImage":
                    avatarIndex = 3;
                    break;
                case "MagicianImage":
                    avatarIndex = 4;
                    break;
            }
            clientProfileImageIndex = avatarIndex;
            ClienBoxSetActive(true);
            ChangeProfileCharacter();


        }

    }
}