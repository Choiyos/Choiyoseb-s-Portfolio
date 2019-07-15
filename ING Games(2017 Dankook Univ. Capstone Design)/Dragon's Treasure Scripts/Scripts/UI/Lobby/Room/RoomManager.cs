using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;
using System.Net.Sockets;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

namespace RealTimeClient
{
    public class RoomManager : MonoBehaviour
    {
        private Transform roomContent;
        private GameObject roomItem;

        Socket hClntSock;
        GameObject failMessage;

        #region Selectable Setting
        EventSystem system;
        #endregion

        #region SetActiveSetting
        GameObject createRoomManger;
        GameObject clientBox;
        GameObject roomList;
        #endregion

        void Start()
        {
            system = EventSystem.current;
            roomContent = GameObject.Find("RoomListContent").transform;
            roomItem = Resources.Load("Prefab/UI/RoomListItem", typeof(GameObject)) as GameObject;
            hClntSock = RealTimeConnect.instance.GetSocket();
            failMessage = GameObject.Find("Error").transform.GetChild(0).gameObject;
            LoadButton();
        }

        private void LoadButton()
        {
            clientBox = GameObject.Find("ClientBox");
            roomList = GameObject.Find("RoomList");
            createRoomManger = GameObject.Find("CreateRoomManger");

            Text RoomNameField = createRoomManger.transform.GetChild(4).GetComponentInChildren<Text>();
            Button CreateRoomButton = createRoomManger.transform.GetChild(2).GetComponent<Button>();
            Button MakeRoomButton = createRoomManger.transform.GetChild(0).GetComponent<Button>();
            Button BackRoomButton = createRoomManger.transform.GetChild(3).GetComponent<Button>();

            CreateRoomButton.onClick.AddListener(delegate { CreateCurrentRoom(RoomNameField.text); });

            MakeRoomButton.onClick.AddListener(delegate { MakeRoomSetActive(true); });
            BackRoomButton.onClick.AddListener(delegate { MakeRoomSetActive(false); });
        }

        private void MakeRoomSetActive(bool _flag)
        {
            if (_flag)
            {
                Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                if (next != null)
                {

                    InputField inputfield = next.GetComponent<InputField>();
                    if (inputfield != null)
                        inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

                    system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                }
            }
            for (int i = 1; i < 5; i++)
            {
                createRoomManger.transform.GetChild(i).gameObject.SetActive(_flag);
            }

            //createRoomManger.transform.GetChild(0).gameObject.SetActive(_flag);
            //clientBox.SetActive(!_flag);
            //roomList.SetActive(!_flag);
        }

        private void JoinCurrentRoomRequest(string _roomName)
        {
            StringBuilder sb = new StringBuilder();
             
            sb.AppendFormat("{0}{1}{2}", (Char)HeaderConstValue.RoomDat, (Char)HeaderConstValue.RoomJoinDat, _roomName);
            sb.Insert(0, (char)Encoding.UTF8.GetByteCount(sb.ToString()));

            byte[] joinRoom_buf = new byte[sb.Length];
            joinRoom_buf = Encoding.UTF8.GetBytes(sb.ToString());
            hClntSock.Send(joinRoom_buf);
        }

        private void CreateCurrentRoom(string _roomName)
        {
            //Debug.Log("방생성 요청");
            _roomName = Regex.Replace(_roomName," ", "", RegexOptions.Singleline);
            if (String.IsNullOrEmpty(_roomName))
            {
                failMessage.SetActive(true);
                failMessage.GetComponentInChildren<Text>().text = "방 이름은 공백으로 생성할 수 없습니다.";
                return;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}{1}{2}", (Char)HeaderConstValue.RoomDat, (Char)HeaderConstValue.RoomCreateDat, _roomName);
            sb.Insert(0, (char)Encoding.UTF8.GetByteCount(sb.ToString()));

            byte[] createRoom_buf = new byte[sb.Length];
            createRoom_buf = Encoding.UTF8.GetBytes(sb.ToString());
            hClntSock.Send(createRoom_buf);
        }

        

        public void ClearRoomList()
        {
            for (int i = 0; i < roomContent.childCount; i++)
            {
                Destroy(roomContent.GetChild(i).gameObject);
            }
        }

        public void RefreshRoomList(byte[] _roomList)
        {
            string roomName = null;
            int clientCount = 0;

            int offset = 2;
            int roomNameLengthStart = 0;
            int roomNameLength = 0;
            int roomNum = Convert.ToInt32(_roomList[1]);
            for (int i = 0; i < roomNum; i++, offset++)
            {
                roomNameLength = _roomList[offset];
                roomNameLengthStart = ++offset;
                //룸네임 길이만큼 바이트 생성
                byte[] room = new byte[roomNameLength];
                //생성한 바이트에 룸네임 스타트부터 엔드까지 길이 복사
                Array.Copy(_roomList, roomNameLengthStart, room, 0, roomNameLength);
                roomName = Encoding.UTF8.GetString(room);
                offset += roomNameLength;
                clientCount = Convert.ToInt32(_roomList[offset]);

                GameObject currentRoom = Instantiate(roomItem, roomContent);

                Text RoomNameContent = currentRoom.GetComponentInChildren<Text>();
                RoomNameContent.text = roomName;
                Text clientCountContent = currentRoom.transform.GetChild(1).GetComponent<Text>();
                clientCountContent.text = " (" + clientCount.ToString() + "/4)";

                Button btnCtrl = currentRoom.GetComponent<Button>();
                btnCtrl.onClick.AddListener(() => JoinCurrentRoomRequest(roomName));
            }
        }

    }
}
