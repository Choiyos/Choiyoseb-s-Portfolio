using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealTimeClient
{
    public class RealTimeConnect : MonoBehaviour
    {

        #region SingleTone Setting
        private static RealTimeConnect _instance;

        public static RealTimeConnect instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = (RealTimeConnect)FindObjectOfType(typeof(RealTimeConnect));
                    if (!_instance)
                    {
                        Debug.LogError("액티브된 인스턴스가 없음.");
                    }
                }
                return _instance;
            }
        }
        public Socket GetSocket()
        {
            return hClntSock;
        }
        private Socket hClntSock;
        #endregion

        #region Connect Setting
        private string server_address = "172.25.235.57";
        private int port = 8183;
        private int Connect_Timeout = 2000;
        #endregion

        #region EnterGame Setting        
        private enum state { None, Login, Signin }
        private state State;
        private bool issuccess;
        private ManualResetEvent conDone = new ManualResetEvent(false);
        private ManualResetEvent logDone = new ManualResetEvent(false);
        private int category;

        string Client_ID;
        string Client_PW;
        #endregion

        #region ErrorSetting
        private GameObject failMessage;
        bool isError = false;
        string errorMessage = null;
        #endregion

        #region SingleTone
        void Start()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            //LoadButton();
            failMessage = GameObject.Find("Error").transform.GetChild(0).gameObject;
            DontDestroyOnLoad(gameObject);
        }

        public string GetID()
        {
            return Client_ID;
        }
        #endregion

        #region ButtonSetting
        private GameObject buttons;
        public void OnLoginClick()
        {
            State = state.Login;
            Connect();
            EnterGame();
            Login_ChangeToScene();
        }

        public void OnCreateIDClick()
        {
            State = state.Signin;
            Connect();
            EnterGame();
        }

        private void LoadButton()
        {
            buttons = GameObject.Find("Buttons");
            Button loginButton = buttons.transform.GetChild(0).GetComponent<Button>();
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(() => OnLoginClick());

            Button signInButton = buttons.transform.GetChild(1).GetComponent<Button>();
            signInButton.onClick.RemoveAllListeners();
            signInButton.onClick.AddListener(() => ButtonSetActive(false));

            Button createIDButton = buttons.transform.GetChild(2).GetComponent<Button>();
            createIDButton.onClick.RemoveAllListeners();
            createIDButton.onClick.AddListener(() => OnCreateIDClick());
            createIDButton.onClick.AddListener(() => ButtonSetActive(true));

            Button backButton = buttons.transform.GetChild(3).GetComponent<Button>();
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => ButtonSetActive(true));

            Button exitButton = buttons.transform.GetChild(4).GetComponent<Button>();
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => OnExitClick());
        }

        public void OnExitClick()
        {
#if     UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ButtonSetActive(bool _flag)
        {
            for (int i = 0; i < 2; i++)
            {
                buttons.transform.GetChild(i).gameObject.SetActive(_flag);
            }
            for (int i = 2; i < 4; i++)
            {
                buttons.transform.GetChild(i).gameObject.SetActive(!_flag);
            }
        }

        void OnLevelWasLoaded(int level)
        {
            if (level == 0)
            {
                if (_instance == this)
                {
                    //LoadButton();
                    failMessage = GameObject.Find("Error").transform.GetChild(0).gameObject;
                }
            }
        }
        #endregion

        private void Connect()
        {
            IPAddress address = IPAddress.Parse(server_address);
            IPEndPoint RemoteEndPoint = new IPEndPoint(address, port);

            hClntSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // server IP and port
            try
            {
                IAsyncResult Connect_timeout_handler = hClntSock.BeginConnect(RemoteEndPoint, new AsyncCallback(on_connect), hClntSock);
                if (Connect_timeout_handler.AsyncWaitHandle.WaitOne(Connect_Timeout, false))
                {
                    conDone.WaitOne();
                }
                else
                {
                    conDone.Set();
                    isError = true;
                    errorMessage = "Connection TimeOut!";
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                errorMessage = e.Message;
                issuccess = false;
                conDone.Set();
            }
        }

        private void EnterGame()
        {
            
            if (State == state.Login)
            {
                category = 0;
                Client_ID = GameObject.Find("ID field").GetComponent<InputField>().text;
                Client_PW = GameObject.Find("Password field").GetComponent<InputField>().text;
            }
            else if (State == state.Signin)
            {
                category = 1;
                Client_ID = GameObject.Find("ID field2").GetComponent<InputField>().text;
                Client_PW = GameObject.Find("Password field2").GetComponent<InputField>().text;
            }
            else
            {
                isError = true;
                errorMessage = "State Error!";
                return;
            }
            StringBuilder Info_string = new StringBuilder();
            Info_string.AppendFormat("{0}{1}{2}{3}{4}", (char)category, (char)Client_ID.Length, Client_ID, (char)Client_PW.Length, Client_PW);
            byte[] Login_Packet = new byte[Info_string.Length];
            Login_Packet = Encoding.ASCII.GetBytes(Info_string.ToString());
            try
            {
                IAsyncResult Send_timeout_handler = hClntSock.BeginSend(Login_Packet, 0, Login_Packet.Length, SocketFlags.None, new AsyncCallback(on_login_complete), hClntSock);
                if (Send_timeout_handler.AsyncWaitHandle.WaitOne(2000, false))
                {
                    // Debug.Log("Login Packet successfully sent to server");
                }
                else
                {
                    failMessage.SetActive(true);
                    failMessage.GetComponentInChildren<Text>().text = "Send-Timeout!";
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                errorMessage = e.Message;
                issuccess = false;
            }
        }

        public void Login_ChangeToScene()
        {

            logDone.WaitOne(500);

            if (issuccess == true)
            {
                //SceneManager.LoadScene(1);
                GameObject.Find("UICamera").GetComponent<MainSceneUI>().MoveToLobby();
                logDone.Reset();
            }
            else
            {
                Debug.Log("Login again");
                logDone.Reset();
            }
        }

        void Update()
        {
            if (isError)
            {
                failMessage.SetActive(true);
                failMessage.GetComponentInChildren<Text>().text = errorMessage;
                isError = false;
            }
        }

        private void on_connect(IAsyncResult ar)
        {
            conDone.Set();
            Socket hClntSock = (Socket)ar.AsyncState;
            hClntSock.EndConnect(ar);
        }

        private void on_login_complete(IAsyncResult ar)
        {
            Socket hClntSock = (Socket)ar.AsyncState;
            hClntSock.EndSend(ar);

            byte[] validation = new byte[100];

            try
            {
                int received = hClntSock.Receive(validation);
                if (received == 0) // received nothing
                {

                    isError = true;
                    errorMessage = "Disconnected with server. Login Failed";
                    hClntSock.Close();
                }
                else // receive complete
                {
                    int result = Convert.ToInt32(validation[0]);
                    int msg_received = Convert.ToInt32(validation[1]);
                    byte[] message = new byte[msg_received];
                    Array.Copy(validation, 2, message, 0, msg_received);
                    string Message = Encoding.ASCII.GetString(message);

                    if (result == 0) // Login or Sign-in failed
                    {
                        isError = true;
                        errorMessage = Message;
                        issuccess = false;
                    }
                    else // Login or Sign-in sucess
                    {
                        issuccess = true;
                    }
                }
                logDone.Set();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                errorMessage = e.Message;
                issuccess = false;
            }
        }

    }
}
