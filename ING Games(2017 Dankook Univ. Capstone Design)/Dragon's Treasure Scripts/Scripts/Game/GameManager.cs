using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    // 게임이 시작되고 게임매니져를 동적으로 생성하고 플레이어들의 정보를 입력해주는 스크립트.
    #region   Instance 사용 준비

    private GameObject gameObject;

    public static GameManager m_Instance;

    public MatchSettings matchSettings;

    void Awake()
    {
        if (m_Instance != null)
        {
            Debug.LogError("More than one GameManager in scene");
        }
        else
        {
            m_Instance = this;
        }
    }

    public static GameManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new GameManager();
                m_Instance.gameObject = new GameObject("_gameManager");
                m_Instance.gameObject.AddComponent<InputController>();
            }
            return m_Instance;
        }
    }

    private InputController m_InputController;
    public InputController InputController
    {
        get
        {
            if (m_InputController == null)
                m_InputController = gameObject.GetComponent<InputController>();
            return m_InputController;
        }
    }
    #endregion

    #region Player tracking

    private const string PLAYER_ID_PREFIX = "Player ";

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    public static void RegisterPlayer(string _netID, Player _player)
    {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
    }

    public static void UnRegisterPlayer(string _playerID)
    {
        players.Remove(_playerID);
    }

    public static Player GetPlayer(string _playerID)
    {
        return players[_playerID];
    }

    #endregion

}