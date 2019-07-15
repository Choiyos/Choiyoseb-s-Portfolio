using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputTab : MonoBehaviour
{
    EventSystem system;
    InputField idField;
    void Start()
    {
        system = EventSystem.current;// EventSystemManager.currentSystem;
        idField = GameObject.Find("ID field").GetComponent<InputField>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
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
        idField.text = Regex.Replace(idField.text, @"[^a-zA-Z0-9_]", "", RegexOptions.Singleline);
    }

}