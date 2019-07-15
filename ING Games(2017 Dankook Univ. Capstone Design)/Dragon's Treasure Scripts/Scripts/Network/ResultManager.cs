using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealTimeClient
{
    public class ResultManager : NetworkBehaviour
    {
        #region PacketSetting
        byte[] message = new byte[1000];
        byte[] packetLength = new byte[1];
        Socket hClntSock;
        int category = -1;
        #endregion

        #region GameSetting
        bool endGameDat = false;
        bool cursorLock = false;
        bool clientNameDat = false;
        string winnerName = null;
        string clientName = null;
        #endregion

        #region ErrorSetting
        private GameObject failMessage;
        string errorMessage = null;
        bool isError = false;
        private int posIndex=1;
        #endregion



        void Start()
        {
            hClntSock = RealTimeConnect.instance.GetSocket();
            failMessage = GameObject.Find("Error").transform.GetChild(0).gameObject;
            SceneMoveComplete();
        }

        private void SceneMoveComplete()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((char)HeaderConstValue.SceneMoveCompleteDat);

            byte[] packet = new byte[sb.Length];
            packet = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(packet);

            hClntSock.Receive(packetLength);
            if(Convert.ToInt32(packetLength[0]) == 6)
            {
                /// suceess
                //Debug.Log("on-game scene signal sucess");
                hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                ClientNameRequest();
               
            }
            else
            {
                // fail          
                hClntSock.Close();
                SceneManager.LoadScene(0);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;
            }
        }

        private void ClientNameRequest()
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}", (Char)HeaderConstValue.ClientNameDat);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] clientNameRequest_buf = new byte[sb.Length];
            clientNameRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientNameRequest_buf);
        }

        private void on_arrive(IAsyncResult ar)
        {
            try
            {
                Socket hClntSock = (Socket)ar.AsyncState;
                int received = hClntSock.EndReceive(ar);
                Debug.Log(received);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                cursorLock = !cursorLock;
                if (cursorLock)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            if (endGameDat)
            {
                SendStatus();
                endGameDat = false;
            }
            if (clientNameDat)
            {
                //ApplyClientName();
                clientNameDat = false;
            }
            if (isError)
            {
                failMessage.SetActive(true);
                failMessage.GetComponentInChildren<Text>().text = errorMessage;
                isError = false;
            }
        }


        private void on_receive(IAsyncResult ar)
        {
            Socket hClntSock = (Socket)ar.AsyncState;
            hClntSock.EndReceive(ar);


            //Debug.Log("Recieve 직전" + Convert.ToInt32(packetLength[0]));
            int received = hClntSock.Receive(message, Convert.ToInt32(packetLength[0]), SocketFlags.None);

            category = Convert.ToInt32(message[0]);

            Debug.Log("카테고리 : " + category);

            switch (category)
            {
                case HeaderConstValue.ClientNameDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        byte[] from_server = new byte[received - 1];
                        Array.Copy(message, 1, from_server, 0, received - 1);
                        clientName = Encoding.ASCII.GetString(from_server);
                        clientNameDat = true;
                        break;
                    }
                case HeaderConstValue.EndGameSuccess:
                    {

                        int result = message[1];
                        if (result == 1)
                        {
                            endGameDat = true;
                        }
                        else
                        {
                            hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                            byte[] from_server = new byte[received - 2];
                            Array.Copy(message, 2, from_server, 0, received - 2);
                            errorMessage = Encoding.UTF8.GetString(from_server);
                            Debug.Log(errorMessage);
                            isError = true;
                        }
                        break;
                    }                
                case HeaderConstValue.ControlDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        byte[] from_server = new byte[received - 1];
                        Array.Copy(message, 1, from_server, 0, received - 1);
                        errorMessage = Encoding.UTF8.GetString(from_server);
                        Debug.Log(errorMessage);
                        isError = true;
                        break;
                    }
            }
        }

        #region 게임 종료 처리
        public void EndGameRequest()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}", (Char)HeaderConstValue.EndGameDat);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] clientBoxRequest_buf = new byte[sb.Length];
            clientBoxRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientBoxRequest_buf);
        }

        public void WinnerName(string _name)
        {
            winnerName = _name;
        }
        /// <summary>
        /// Player오브젝트가 아니다보니 자신이 플레이어하고 있는 오브젝트가 누군지 판단하기 힘듬.
        /// Player에 직접 Send하는 함수를 만들거나 플레이어 순회해서 자기자신 찾는 과정 필요할듯.
        /// </summary>
        private void SendStatus()
        {
            int killScore = 0;
            int deathScore = 0;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.layer == 8)
                {
                    killScore = player.GetComponent<Player>().killScore;
                    deathScore = player.GetComponent<Player>().deathScore;
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}{2}", (Char)HeaderConstValue.StatusDat, (Char)killScore, (Char)deathScore);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));
            byte[] clientStatus_buf = new byte[sb.Length];
            clientStatus_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientStatus_buf);

            if (isServer)
            {
                RpcEnd();
            }
        }


        [ClientRpc]
        void RpcEnd()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            int mapIndex = GameObject.Find("NetworkManager").GetComponent<NetworkManager_Custom>().mapIndex;
            GameObject map = GameObject.Find("Map").transform.GetChild(mapIndex).gameObject;
            
            if (map && GameObject.Find("SpawnManager"))
            {
                GameObject.Find("Plane").GetComponentInChildren<AudioSource>().Stop();
                map.SetActive(false);
                GameObject.Find("SpawnManager").SetActive(false);
            }
            foreach (GameObject player in players)
            {
                if (player.name == winnerName)
                {
                    player.transform.position = new Vector3(0, 0, 0);
                    player.transform.Find("Canvas").Find("GameOverUI").GetComponent<GameOverUI>().PunchGameOver(winnerName);
                    continue;
                }
                else
                {
                    player.transform.position = new Vector3(1.2f, 0.5f, (posIndex++) * 1.5f);
                    print(player.transform.name + "의 position = " + player.transform.position);
                    player.transform.Find("Canvas").Find("GameOverUI").GetComponent<GameOverUI>().PunchGameOver(name);
                }
                //player.transform.Find("Canvas").Find("GameOverUI").GetComponent<GameOverUI>().PunchGameOver(winnerName);
            }

            GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
            foreach (GameObject item in items)
            {
                //print(item.name);
                item.SetActive(false);
            }            



        }
        #endregion
    }
}