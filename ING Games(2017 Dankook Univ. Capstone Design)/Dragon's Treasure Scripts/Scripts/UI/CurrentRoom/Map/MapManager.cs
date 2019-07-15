using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RealTimeClient
{
    public class MapManager : MonoBehaviour
    {


        private Sprite[] mapImageArray;
        int mapOffset = 0;
        Button leftButton, rightButton;
        Image mapImage;
        GameObject mapSelectButton;
        Socket hClntSock;


        void Start()
        {
            hClntSock = RealTimeConnect.instance.GetSocket();
            mapSelectButton = GameObject.Find("MapSelectButton");
            LoadButtons();
            LoadSprites();
            
            mapImage = GameObject.Find("MapImage").GetComponent<Image>();
        }

        /// <summary>
        /// Load all sprites in "Image / Character"
        /// </summary>
        private void LoadSprites()
        {
            object[] loadedIcons = Resources.LoadAll("Image/Map", typeof(Sprite));
            mapImageArray = new Sprite[loadedIcons.Length];
            for (int x = 0; x < loadedIcons.Length; x++)
            {
                mapImageArray[x] = (Sprite)loadedIcons[x];
            }
        }

        /// <summary>
        /// Loads a playable list from the children of the "SelectCharacter" in the canvas.
        /// </summary>
        private void LoadButtons()
        {
            leftButton = mapSelectButton.transform.GetChild(0).GetComponent<Button>();
            rightButton = mapSelectButton.transform.GetChild(1).GetComponent<Button>();

            leftButton.onClick.AddListener(() => ChangeMapRequest(1));
            rightButton.onClick.AddListener(() => ChangeMapRequest(2));
        }

        internal void CheckClientHost(int host)
        {
            if (host == 1)
            {
                mapSelectButton.SetActive(true);
            }
            else if (host == 0)
            {
                mapSelectButton.SetActive(false);
            }         
        }

        void ChangeMapRequest(int _index)
        {
            Debug.Log("맵 변경 요청");

            //LeftButton Click.
            if (_index == 1)
            {
                mapOffset--;
                if (mapOffset < 0)
                {
                    mapOffset = mapImageArray.Length - 1;
                }
            }
            //RightButton Click.
            else if (_index == 2)
            {
                mapOffset++;
                if (mapOffset == mapImageArray.Length)
                {
                    mapOffset = 0;
                }
            }

            Debug.Log("mapOffset : " + mapOffset);

            //서버에 맵 인덱스 전송S
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", (Char)HeaderConstValue.MapDat, (Char)mapOffset);
            sb.Insert(0, (char)Encoding.ASCII.GetByteCount(sb.ToString()));

            byte[] mapChange_buf = new byte[sb.Length];
            mapChange_buf = Encoding.ASCII.GetBytes(sb.ToString());

            hClntSock.Send(mapChange_buf);
        }

        internal void ChangeMap(int mapindex)
        {
            mapImage.sprite = mapImageArray[mapindex];
           GameObject.Find("NetworkManager").GetComponent<NetworkManager_Custom>().mapIndex = mapindex;
        }
    }
}