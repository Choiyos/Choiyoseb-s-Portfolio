using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// -헤더- 
/// 0 : RoomBox
/// 1 : ChattingBox
/// 2 : ClientListBox
/// 3 : ClientBox
/// </summary>
/// 

namespace RealTimeClient
{
    public class PacketManager : MonoBehaviour
    {

        #region ErrorSetting
        private GameObject failMessage;
        string errorMessage = null;
        bool isError = false;
        #endregion

        #region PacketSetting
        Socket hClntSock;
        RoomManager rm;
        byte[] packetLength = new byte[1];
        int category = -1;
        #endregion

        #region ChatSetting
        ChatterManager cm;
        bool chatDat = false;
        bool noticeDat = false;
        string msg = null;
        #endregion

        #region RoomSetting
        byte[] roomData;
        bool roomDat = false;
        bool isSuccessRoomMovement = false;
        #endregion

        #region ClientListSetting
        ClientListManager clm;
        byte[] clientListData;
        bool clientListDat = false;
        #endregion

        #region ClientBoxSetting
        ClientBoxManager cbm;
        byte[] clientBoxData;
        bool clientBoxDat = false;
        #endregion


        void Start()
        {
            hClntSock = RealTimeConnect.instance.GetSocket();

            #region Load Managers
            rm = GameObject.Find("RoomManager").GetComponent<RoomManager>();
            cm = GameObject.Find("ChatterManager").GetComponent<ChatterManager>();
            clm = GameObject.Find("ClientListManager").GetComponent<ClientListManager>();
            cbm = GameObject.Find("ClientBoxManager").GetComponent<ClientBoxManager>();
            failMessage = GameObject.Find("Error").transform.GetChild(0).gameObject;
            #endregion
            SceneMoveComplete();
        }

        private void SceneMoveComplete()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((char)HeaderConstValue.SceneMoveCompleteDat);

            byte[] packet = new byte[sb.Length];
            packet = Encoding.ASCII.GetBytes(sb.ToString());


            hClntSock.Send(packet);
            IAsyncResult time_out_handler = hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_arrive), hClntSock);
            if (time_out_handler.AsyncWaitHandle.WaitOne(1000, false))
            {
                /// suceess                
                hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                ClientListRequest();
                ClientBoxRequest();
                RoomListRequest();
            }
            else
            {
                // fail          
                Debug.Log("여길??");
                hClntSock.Close();
                SceneManager.LoadScene(0);
            }
        }

        private void on_arrive(IAsyncResult ar)
        {
            try
            {
                Socket hClntSock = (Socket)ar.AsyncState;
                hClntSock.EndReceive(ar);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        void Update()
        {
            if (roomDat)
            {
                rm.ClearRoomList();
                if (Convert.ToInt32(roomData[1]) != 0)
                {
                    rm.RefreshRoomList(roomData);
                }
                roomDat = false;
            }
            if (isSuccessRoomMovement)
            {
                //Debug.Log("룸이동 허락");
                //rm.ChangeCurrentRoom();
                SceneManager.LoadScene(2);
                //Debug.Log("체인지룸 함수 끝");
                isSuccessRoomMovement = false;
                return;
            }
            if (chatDat)
            {
                cm.TransmitMessage(msg);
                chatDat = false;

            }
            if (clientListDat)
            {
                clm.ClearClientList();
                clm.RefreshClientList(clientListData);
                clientListDat = false;
            }
            if (clientBoxDat)
            {
                cbm.SettingClientBox(clientBoxData);
                clientBoxDat = false;
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

            #region Changed Packet Logic
            byte[] packet = new byte[Convert.ToInt32(packetLength[0])];
            //string str = Encoding.ASCII.GetString(packet);

            int received = hClntSock.Receive(packet, packet.Length, SocketFlags.None);
            //Debug.Log("received : " + received);
            //Debug.Log(" Convert.ToInt32(packetLength[0]) : " + Convert.ToInt32(packetLength[0]));

            //print(packet.Length);
            #endregion

            category = Convert.ToInt32(packet[0]);
            //Debug.Log("category : " + category);
            switch (category)
            {
                case HeaderConstValue.RoomDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        roomData = new byte[packet.Length];
                        Array.Copy(packet, 0, roomData, 0, received );
                        roomDat = true;
                        break;
                    }
                case HeaderConstValue.ChatDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        byte[] chat_message = new byte[received - 1];
                        Array.Copy(packet, 1, chat_message, 0, received - 1);
                        msg = Encoding.UTF8.GetString(chat_message);
                        chatDat = true;
                        break;
                    }
                case HeaderConstValue.ClientListDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        clientListData = new byte[received];
                        Array.Copy(packet, 0, clientListData, 0, received);
                        clientListDat = true;
                        break;
                    }
                case HeaderConstValue.ClientBoxDat:
                    {
                        hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                        clientBoxData = new byte[received];
                        Array.Copy(packet, 0, clientBoxData, 0, received);
                        clientBoxDat = true;
                        break;
                    }
                case HeaderConstValue.SystemDat:
                    {
                        if (Convert.ToInt32(packet[1]) == HeaderConstValue.RoomDat)
                        {
                            int result = Convert.ToInt32(packet[2]);
                            if (result == 1)
                            {
                                //Debug.Log("룸이동허락 패킷도착");
                                isSuccessRoomMovement = true;
                            }
                            else
                            {
                                Debug.Log("else");
                                hClntSock.BeginReceive(packetLength, 0, packetLength.Length, SocketFlags.None, new AsyncCallback(on_receive), hClntSock);
                                byte[] from_server = new byte[received - 3];
                                Array.Copy(packet, 3, from_server, 0, received - 3);
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
                        Array.Copy(packet, 1, from_server, 0, received - 1);
                        errorMessage = Encoding.UTF8.GetString(from_server);
                        Debug.Log(errorMessage);
                        isError = true;
                        break;
                    }              
            }
        }

        private void ClientBoxRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", (Char)HeaderConstValue.ClientBoxDat, (Char)0);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] clientBoxRequest_buf = new byte[sb.Length];
            clientBoxRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientBoxRequest_buf);
        }

        private void ClientListRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", (Char)HeaderConstValue.ClientListDat);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] clientListRequest_buf = new byte[sb.Length];
            clientListRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(clientListRequest_buf);
        }

        private void RoomListRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", (char)HeaderConstValue.RoomDat, (char)HeaderConstValue.RoomReqDat);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] roomListRequest_buf = new byte[sb.Length];
            roomListRequest_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(roomListRequest_buf);
        }
    }
}
