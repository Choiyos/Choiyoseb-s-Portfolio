using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RealTimeClient
{

    public class ChatInputfield : MonoBehaviour
    {

        public ChatterManager chatManager;
        private InputField inputfield;

        private void Start()
        {
            inputfield = GetComponent<InputField>();
        }

        public void ValueChanged()
        {
            if (inputfield.text.Contains("\n"))
                chatManager.WriteMessage(inputfield);
        }
    }
}