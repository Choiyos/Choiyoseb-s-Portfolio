using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealTimeClient
{

    public class Logout : MonoBehaviour
    {
        Socket sk;

        void Start()
        {
            sk = RealTimeConnect.instance.GetSocket();
            Button logout = GameObject.Find("Logout").GetComponent<Button>();
            logout.onClick.AddListener(() => LogoutBtn());
        }

        private void LogoutBtn()
        {            
            sk.Close();
            SceneManager.LoadScene(0);
        }
    }
}