using System.Net.Sockets;
using UnityEngine.UI;
using System.Text;
using UnityEngine;
using System.Threading;

namespace RealTimeClient
{
    public class ChatterManager : MonoBehaviour
    {
        Socket hClntSock;
        private Transform chatContent;
        private GameObject chatMessage;

        void Start()
        {
            hClntSock = RealTimeConnect.instance.GetSocket();
            chatContent = GameObject.Find("ChattingContent").transform;
            chatMessage = Resources.Load("Prefab/UI/ChattingItem", typeof(GameObject)) as GameObject;
        }

        public void WriteMessage(InputField sender)
        {
            if (!string.IsNullOrEmpty(sender.text) && sender.text.Trim().Length > 0)
            {
                sender.text = sender.text.Replace("/r", string.Empty).Replace("\n", string.Empty);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}{1}", (char)HeaderConstValue.ChatDat, sender.text);
                sb.Insert(0, (char)Encoding.UTF8.GetByteCount(sb.ToString()));

                byte[] message_buf = new byte[sb.Length];

                message_buf = Encoding.UTF8.GetBytes(sb.ToString());

                hClntSock.Send(message_buf);

                sender.text = string.Empty;
                sender.ActivateInputField();
            }
        }

        public void TransmitMessage(string _msg)
        {
            string message = _msg;

            GameObject newMessage = Instantiate(chatMessage, chatContent);
            Text content = newMessage.GetComponent<Text>();

            content.text = message;
        }
    }
}