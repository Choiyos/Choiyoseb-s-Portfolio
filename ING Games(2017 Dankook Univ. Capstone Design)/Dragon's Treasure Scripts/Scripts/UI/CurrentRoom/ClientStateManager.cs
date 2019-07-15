using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RealTimeClient
{
    public class ClientStateManager : MonoBehaviour
    {
        Socket hClntSock;
        private GameObject clientContent;

        #region ImageSetting
        private int clientProfileImageIndex = -1;
        private Sprite[] clientCharacterImageArray;
        #endregion

        #region ReadySetting
        int isReady = 0;
        bool[] clientReady = new bool[4] { false, false, false, false };
        bool isAllReady = true;
        GameObject leaveObject;
        GameObject crObject;
        Transform startMessageTr;
        #endregion

        #region ButtonClick
        private Button[] playerButton;
        private GameObject readyButtonObject;
        int avatarIndex = 0;
        #endregion


        void Start()
        {
            hClntSock = RealTimeConnect.instance.GetSocket();
            clientContent = GameObject.Find("ClientList");
            leaveObject = GameObject.Find("Leave");
            crObject = GameObject.Find("CurrentRoom");
            startMessageTr = crObject.transform.GetChild(crObject.transform.childCount - 1);
            LoadSprites();
            LoadButtons();
        }

        /// <summary>
        /// Loads a playable list from the children of the "SelectCharacter" in the canvas.
        /// </summary>
        private void LoadButtons()
        {
            readyButtonObject = GameObject.Find("ClientIsReady");
            //Debug.Log(readyButtonObject.gameObject.name);
            readyButtonObject.GetComponent<Button>().onClick.AddListener(() => ReadyButtonClick());
            playerButton = GameObject.Find("SelectCharacter").GetComponentsInChildren<Button>();
            foreach (Button btn in playerButton)
            {
                btn.onClick.AddListener(delegate { AvatarPicker2(btn.name); });
            }
        }

        /// <summary>
        /// Load all sprites in "Image / Character"
        /// </summary>
        private void LoadSprites()
        {
            object[] loadedIcons = Resources.LoadAll("Image/Character", typeof(Sprite));
            clientCharacterImageArray = new Sprite[loadedIcons.Length];
            for (int x = 0; x < loadedIcons.Length; x++)
            {
                clientCharacterImageArray[x] = (Sprite)loadedIcons[x];
            }
        }

        public void CheckClientReady(byte[] _message)
        {
            //Debug.Log("Ready Click");
            isAllReady = true;
            for (int i = 2; i < _message.Length; i++)
            {
                if (Convert.ToInt32(_message[i]) == 1)
                {
                    clientContent.transform.GetChild(i - 2).GetChild(5).gameObject.SetActive(true);
                    clientReady[i - 2] = true;
                }
                else if (Convert.ToInt32(_message[i]) == 0)
                {
                    clientContent.transform.GetChild(i - 2).GetChild(5).gameObject.SetActive(false);
                    clientReady[i - 2] = false;
                }
                else
                {
                    Debug.Log("Error");
                }
            }
            for (int i = 0; i < _message.Length-2; i++)
            {
                isAllReady &= clientReady[i];
            }
            //Debug.Log("플레이어 수 : " + (_message.Length - 2));
            //GameStart.
            if (isAllReady)
                StartCoroutine(StartGame());

        }

        public void CheckClientCharacter(byte[] _message)
        {
            for (int i = 2; i < _message.Length; i++)
            {
                if (Convert.ToInt32(_message[i]) == -1 || Convert.ToInt32(_message[i]) == 0)
                {
                    continue;
                }                
                clientProfileImageIndex = Convert.ToInt32(_message[i]);
                Image clientCharacterImage = clientContent.transform.GetChild(i - 2).GetChild(3).GetComponent<Image>();
                clientCharacterImage.sprite = clientCharacterImageArray[clientProfileImageIndex - 1];
            }
        }



        IEnumerator StartGame()
        {
            startMessageTr.gameObject.SetActive(true);
            for (int i = 5; i > 0; i--)
            {
                if (!isAllReady)
                {
                    startMessageTr.gameObject.SetActive(false);
                    if (i < 3)
                    {
                        leaveObject.SetActive(true);
                    }
                    yield break;
                }
                if (i == 3)
                {
                    leaveObject.SetActive(false);
                }
                startMessageTr.GetComponentInChildren<Text>().text = "게임시작 " + i + "초 전!";
                //Debug.Log("게임시작 " + i + "초 전!");
                yield return new WaitForSeconds(1.0f);
            }
            startMessageTr.gameObject.SetActive(false);
            GameStartSignal();
        }

        private void GameStartSignal()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", (Char)HeaderConstValue.SystemDat, (Char)HeaderConstValue.GameStartDat);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] gameStart_buf = new byte[sb.Length];
            gameStart_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(gameStart_buf);
            //클라이언트 신청 후 서버가 홀펀칭 해줄 때까지 딜레이가 필요할 수도 있음.
        }


        private void ReadyButtonClick()
        {
            if (isReady == 0)
            {
                isReady = 1;
                ChangeReadyState();
            }
            else if (isReady == 1)
            {
                isReady = 0;
                ChangeReadyState();
            }
        }

        private void ChangeReadyState()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}{2}", (Char)HeaderConstValue.ClientStateDat, (Char)HeaderConstValue.ClientReadyDat, (Char)isReady);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] clientReadyChange_buf = new byte[sb.Length];
            clientReadyChange_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientReadyChange_buf);
        }

        private void ChangeCharacterState()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}{2}", (Char)HeaderConstValue.ClientStateDat, (Char)HeaderConstValue.ClientCharacterDat, (Char)(clientProfileImageIndex + 1));
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] clientCharacterChange_buf = new byte[sb.Length];
            clientCharacterChange_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientCharacterChange_buf);
        }

        private void AvatarPicker2(string buttonName)
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
            ChangeCharacterState();
        }
    }
}