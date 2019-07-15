using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManager_Custom : NetworkManager
{

    #region ErrorSetting
    private GameObject failMessage;
    //bool isError = false;
    string errorMessage = null;
    #endregion

    #region CharactorSelection
    private Button[] playerButton;
    int avatarIndex = 0;
    #endregion

    #region PlayerSpawnSetting
    Vector3[] playerSpawnPosition;
    GameObject playerSpawnObject;
    public bool playerNameChanged = false;
    int playerSpawnCount = 0;
    public string playerName = null;
    #endregion

    #region MapSetting
    GameObject mapParent;
    public int mapIndex = 0;
    #endregion


    void Start()
    {
        failMessage = GameObject.Find("Error").transform.GetChild(0).gameObject;
        playerName = RealTimeClient.RealTimeConnect.instance.GetID();
        //LoadButtons();

    }

    private void OnLevelWasLoaded(int level)
    {
        if (level == 2)
        {
            LoadButtons();
        }
        if (level == 3)
        {
            //Debug.Log("메인게임 진입");
            mapParent = GameObject.Find("Map");
            //Debug.Log(mapParent.transform.childCount);
            switch (mapIndex)
            {
                case HeaderConstValue.Maze:
                    mapParent.transform.GetChild(HeaderConstValue.Maze).gameObject.SetActive(true);
                    break;
                case HeaderConstValue.Temple:
                    mapParent.transform.GetChild(HeaderConstValue.Temple).gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
            LoadSpawnPoint();
            //mapParent.transform.GetChild(mapIndex).gameObject.SetActive(true);
            // 메인 게임 스폰 위치 등도 고려하여 추가 작성.
        }
    }

    private void LoadButtons()
    {
        playerButton = GameObject.Find("SelectCharacter").GetComponentsInChildren<Button>();
        foreach (Button btn in playerButton)
        {
            string btnName = btn.name;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate { AvatarPicker(btn.name); });
        }
    }

    public void SetUpNetwork(string _hostIP)
    {
        if (_hostIP.Contains("host"))
        {
            StartupHost();
        }
        else
        {
            string[] tempIP = _hostIP.Split(':');
            JoinGame(tempIP[0]);
        }
    }

    private void StartupHost()
    {
        SetPort();
        HolePunching hp = new HolePunching();
        NetworkManager.singleton.StartHost();
    }


    private void JoinGame(string ip)
    {
        // Test the DNS resolution because Unity throws an error otherwise
        // https://msdn.microsoft.com/en-us/library/ms143998(v=vs.90)
        Debug.Log(ip);
        //try
        //{
        //    Dns.GetHostEntry(ip);
        //}
        //catch (SocketException e)
        //{
        //    Debug.Log(e.Message);
        //    errorMessage = e.Message;
        //    failMessage.SetActive(true);
        //    failMessage.GetComponentInChildren<Text>().text = errorMessage;               
        //}

        SetIPAddress(ip);

        SetPort();
        Debug.Log("StratClient");
        NetworkManager.singleton.StartClient();
    }


    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.Log("Could not connect to server: " + error);
        errorMessage = "Could not connect to server: " + error;
        failMessage.SetActive(true);
        failMessage.GetComponentInChildren<Text>().text = errorMessage;
        SceneManager.LoadScene(1);
    }

    void SetIPAddress(string ip)
    {
        NetworkManager.singleton.networkAddress =ip;
    }

    void SetPort()
    {
        NetworkManager.singleton.networkPort = 41426;
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
        playerPrefab = spawnPrefabs[avatarIndex];
        //Debug.Log("avatarIndex : " + avatarIndex);
    }


    bool flag = false;
    /// Copied from Unity's original NetworkManager script except where noted
    public override void OnClientConnect(NetworkConnection conn)
    {
        /// ***
        /// This is added:
        /// First, turn off the canvas...
        //characterSelectionCanvas.enabled = false;
        /// Can't directly send an int variable to 'addPlayer()' so you have to use a message service...

        /// ***
        Debug.Log("Try Connect");
        if (!flag && clientLoadedScene)
        {
            // Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
            flag = true;
            ClientScene.Ready(conn);
            //if (autoCreatePlayer)
            //{
            //    ///***
            //    /// This is changed - the original calls a differnet version of addPlayer
            //    /// this calls a version that allows a message to be sent
            //    ClientScene.AddPlayer(conn, 0, msg);
            //}
        }
    }
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        IntegerMessage msg = new IntegerMessage(avatarIndex);
        ClientScene.AddPlayer(conn, 0,msg);
        Debug.Log("Changed");
    }

    int pciOffset=1;
    /// Copied from Unity's original NetworkManager 'OnServerAddPlayerInternal' script except where noted
    /// Since OnServerAddPlayer calls OnServerAddPlayerInternal and needs to pass the message - just add it all into one.
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        /// *** additions
        /// I skipped all the debug messages...
        /// This is added to recieve the message from addPlayer()...
        /// 

    
        int pci = Convert.ToInt32(playerControllerId) + pciOffset;
        playerControllerId = (short)pci;
        pciOffset++;
        Debug.Log(playerControllerId);
        int id = 0;
        if (extraMessageReader != null)
        {
            IntegerMessage i = extraMessageReader.ReadMessage<IntegerMessage>();
            id = i.value;
            //Debug.Log(id);
        }
        /// using the sent message - pick the correct prefab
        GameObject playerPrefab = spawnPrefabs[id];
        //Debug.Log(playerPrefab.name);
        /// *** end of additions

        GameObject player;

        player = (GameObject)Instantiate(playerPrefab, playerSpawnPosition[UnityEngine.Random.Range(0, playerSpawnCount)], Quaternion.identity);

        //player name 변경
        //StartCoroutine(ChangeName(player));

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    private IEnumerator ChangeName(GameObject _player)
    {
        int timeHandler = 0;
        while (true)
        {
            if (playerNameChanged)
            {
                _player.name = playerName;
                playerNameChanged = false;
                yield return null;
            }
            else if (timeHandler >= 1000)
            {
                Debug.Log("시간안에 이름 못바꿈.");
                break;
            }
            else
            {
                yield return new WaitForSeconds(0.01f);
                timeHandler++;
            }
        }
    }

    private void LoadSpawnPoint()
    {
        if (mapIndex == 0)
        {
            playerSpawnObject = GameObject.Find("PlayerSpawnPoint");
        }
        else if (mapIndex == 1)
        {
            playerSpawnObject = GameObject.Find("TemplePlayerSpawnPoint");
        }
        playerSpawnCount = playerSpawnObject.transform.childCount;
        playerSpawnPosition = new Vector3[playerSpawnCount];
        for (int i = 0; i < playerSpawnCount; i++)
        {
            playerSpawnPosition[i] = playerSpawnObject.transform.GetChild(i).gameObject.transform.position;
        }
    }
}
