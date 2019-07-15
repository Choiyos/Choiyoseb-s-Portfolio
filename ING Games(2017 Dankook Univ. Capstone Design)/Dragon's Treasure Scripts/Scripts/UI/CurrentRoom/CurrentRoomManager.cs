using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

namespace RealTimeClient
{
    public class CurrentRoomManager : MonoBehaviour
    {

        #region ErrorSetting
        private GameObject failMessage;
        bool isError = false;
        string errorMessage = null;
        #endregion

        #region PacketSetting
        byte[] packetLength = new byte[1];
        Socket hClntSock;
        int category = -1;
        #endregion

        #region RoomSetting
        bool roomDat = false;
        byte[] roomName;
        #endregion

        #region ChatSetting
        ChatterManager cm;
        bool chatDat = false;
        string msg = null;
        #endregion

        #region ClientListSetting
        CRClientListManager crClm;
        byte[] clientListData;
        bool clientListDat = false;
        #endregion

        #region ClientStateSetting
        ClientStateManager csm;
        byte[] clientStateData;
        byte[] clientReadyData;
        byte[] clientCharacterData;
        bool clientStateDat = false;
        bool clientReadyDat = false;
        bool clientCharacterDat = false;
        #endregion

        #region MapSetting
        MapManager mm;
        bool clientHostDat = false;
        bool clientMapIndexDat = false;
        int host;
        int mapindex;
        #endregion

        #region SystemSetting
        NetworkManager_Custom nmc;
        private Button leaveButton;
        bool isSuccessRoomMovement = false;
        bool isSuccessGameStart = false;
        string hostIP = null;
        #endregion

        void Start()
        {
            hClntSock = RealTimeConnect.instance.GetSocket();

            SceneMoveComplete();

            #region LoadManagers

            cm = GameObject.Find("ChatterManager").GetComponent<ChatterManager>();
            crClm = GameObject.Find("CRClientListManager").GetComponent<CRClientListManager>();
            csm = GameObject.Find("ClientStateManager").GetComponent<ClientStateManager>();
            nmc = GameObject.Find("NetworkManager").GetComponent<NetworkManager_Custom>();
            mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            failMessage = GameObject.Find("Error").transform.GetChild(0).gameObject;
            leaveButton = GameObject.Find("Leave").GetComponent<Button>();
            leaveButton.onClick.AddListener(() => ClientLeaveRequest());
            #endregion
        }


        void Update()
        {
            //Room Data
            if (roomDat)
            {
                GameObject.Find("RoomNameText").GetComponent<Text>().text = Encoding.UTF8.GetString(roomName);
                roomDat = false;
            }

            //Chatting Data
            if (chatDat)
            {
                cm.TransmitMessage(msg);
                chatDat = false;
            }

            //Client List Data
            if (clientListDat)
            {
                crClm.ClearClientList();
                crClm.RefreshClientList(clientListData);
                clientListDat = false;
            }

            //Client State Data
            if (clientStateDat && !clientListDat)
            {
                if (clientReadyDat)
                {
                    csm.CheckClientReady(clientReadyData);
                    clientReadyDat = false;
                }
                if (clientCharacterDat)
                {
                    Invoke("CC", 0.0f);
                    //csm.CheckClientCharacter(clientCharacterData);                    
                    clientCharacterDat = false;
                }
                clientStateDat = false;
            }

            //Map Data
            if (clientHostDat)
            {
                mm.CheckClientHost(host);
                clientHostDat = false;
            }
            if (clientMapIndexDat)
            {
                mm.ChangeMap(mapindex);
                clientMapIndexDat = false;
            }

            //Room Move Data
            if (isSuccessRoomMovement)
            {
                Debug.Log("1번");
                BackToLobby();
                isSuccessRoomMovement = false;
            }

            //Game Start Data
            if (isSuccessGameStart)
            {
                nmc.SetUpNetwork(hostIP);
                isSuccessGameStart = false;
            }

            //Error Data
            if (isError)
            {
                failMessage.SetActive(true);
                failMessage.GetComponentInChildren<Text>().text = errorMessage;
                isError = false;
            }
        }

        #region Funcs

        void CC()
        {
            csm.CheckClientCharacter(clientCharacterData);
        }

        private void SceneMoveComplete()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((char)HeaderConstValue.SceneMoveCompleteDat);

            byte[] packet = new byte[sb.Length];
            packet = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(packet);
            hClntSock.Receive(packetLength);
            if (Convert.ToInt32(packetLength[0]) == 6)
            {
                /// suceess
                hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                Invoke("ClientListRequest", 0f);
                Invoke("ClientStateRequest", 0f);
                Invoke("RoomNameRequest", 0f);
                Invoke("HostCheckRequest", 0f);
            }
            else
            {
                Debug.Log("fail");
            }
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

        void BackToLobby()
        {
            SceneManager.LoadScene(1);
        }

        private void ClientLeaveRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", (Char)HeaderConstValue.SystemDat, (Char)HeaderConstValue.ClientLeaveDat);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));
            byte[] clientLeave_buf = new byte[sb.Length];
            clientLeave_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientLeave_buf);
        }

        private void ClientStateRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", (Char)HeaderConstValue.ClientStateDat, (char)2);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));
            byte[] clientStateRequest_buf = new byte[sb.Length];
            clientStateRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientStateRequest_buf);
        }

        private void ClientListRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", (Char)HeaderConstValue.ClientListDat, (char)2);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));
            byte[] clientStateRequest_buf = new byte[sb.Length];
            clientStateRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientStateRequest_buf);
        }

        private void RoomNameRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", (Char)HeaderConstValue.RoomDat);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));
            byte[] currentRoomRequest_buf = new byte[sb.Length];
            currentRoomRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(currentRoomRequest_buf);
        }

        private void HostCheckRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", (Char)HeaderConstValue.HostCheck);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));
            byte[] currentRoomRequest_buf = new byte[sb.Length];
            currentRoomRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(currentRoomRequest_buf);
        }
        #endregion

        private void on_receive(IAsyncResult ar)
        {
            Socket hClntSock = (Socket)ar.AsyncState;
            hClntSock.EndReceive(ar);
            #region Changed Packet Logic
            byte[] message = new byte[Convert.ToInt32(packetLength[0])];
            int received = hClntSock.Receive(message, message.Length, SocketFlags.None);
            #endregion

            category = Convert.ToInt32(message[0]);
            //Debug.Log(category);
            switch (category)
            {
                case HeaderConstValue.RoomDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        roomName = new byte[received - 1];
                        Array.Copy(message, 1, roomName, 0, received - 1);
                        roomDat = true;
                        break;
                    }
                case HeaderConstValue.ChatDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        byte[] chat_message = new byte[received - 1];
                        Array.Copy(message, 1, chat_message, 0, received - 1);
                        msg = Encoding.UTF8.GetString(chat_message);
                        chatDat = true;
                        break;
                    }
                case HeaderConstValue.ClientListDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        clientListData = new byte[received];
                        Array.Copy(message, 0, clientListData, 0, received);
                        clientListDat = true;
                        break;
                    }
                case HeaderConstValue.ClientStateDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        clientStateData = new byte[received];
                        Array.Copy(message, 0, clientStateData, 0, received);
                        clientStateDat = true;
                        if (Convert.ToInt32(clientStateData[1]) == 0)
                        {
                            clientReadyData = clientStateData;
                            clientReadyDat = true;
                        }
                        if (Convert.ToInt32(clientStateData[1]) == 1)
                        {
                            clientCharacterData = clientStateData;
                            clientCharacterDat = true;
                        }
                        break;
                    }
                case HeaderConstValue.SystemDat:
                    {
                        if (Convert.ToInt32(message[1]) == 0)
                        {
                            int result = Convert.ToInt32(message[2]);

                            if (result == 1)
                            {
                                isSuccessRoomMovement = true;
                            }
                            else
                            {
                                hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                                byte[] from_server = new byte[received - 3];
                                Array.Copy(message, 3, from_server, 0, received - 3);
                                errorMessage = Encoding.UTF8.GetString(from_server);
                                Debug.Log(errorMessage);
                                isError = true;
                            }
                        }
                        if (Convert.ToInt32(message[1]) == 1)
                        {
                            int result = Convert.ToInt32(message[2]);
                            Debug.Log(result);
                            if (result == 1)
                            {
                                byte[] networkData = new byte[received - 3];
                                Array.Copy(message, 3, networkData, 0, received - 3);
                                hostIP = Encoding.UTF8.GetString(networkData);
                                isSuccessGameStart = true;
                                return;
                            }
                            else
                            {
                                hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                                byte[] from_server = new byte[received - 3];
                                Array.Copy(message, 3, from_server, 0, received - 3);
                                errorMessage = Encoding.UTF8.GetString(from_server);
                                Debug.Log(errorMessage);
                                isError = true;
                            }
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
                case HeaderConstValue.HostCheck:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        host = Convert.ToInt32(message[1]);
                        //Debug.Log(host);
                        clientHostDat = true;
                        break;
                    }
                case HeaderConstValue.MapDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, on_receive, hClntSock);
                        mapindex = Convert.ToInt32(message[1]);
                        //Debug.Log("MapIndex : " + mapindex);
                        clientMapIndexDat = true;
                        break;
                    }

            }
        }
    }
}