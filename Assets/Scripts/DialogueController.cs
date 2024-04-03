using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    private List<string> messages;
    [SerializeField] private Text text;
    [SerializeField] private int blinkRate;
    private float framesElapsed;
    private int messageInd;
    private static DialogueController instance;
    private string activeMessage => messages[messageInd];
    private Action callback;

    private void Awake()
    {
        instance = this;
        transform.parent.gameObject.SetActive(false);
    }
    private void FixedUpdate()
    {
        // text crawl
        if (framesElapsed < activeMessage.Length)
        {
            text.text = activeMessage.Substring(0, (int)framesElapsed) + "<color=#00000000>" + activeMessage.Substring((int)framesElapsed, activeMessage.Length - (int)framesElapsed) + "</color>";
            framesElapsed += char.IsPunctuation(activeMessage[(int)framesElapsed]) ? .125f : 1;
        }
        // cursor blink
        else
        {
            text.text = activeMessage + (((int)framesElapsed - activeMessage.Length) % blinkRate < blinkRate / 2 ? " ↪" : "");
            ++framesElapsed;
        }
    }

    public static void Show(List<string> messages, Action callback = null)
    {
        // begin message sequence
        instance.callback = callback;
        instance.messages = messages;
        instance.transform.parent.gameObject.SetActive(true);
        instance.text.text = "";
        instance.messageInd = 0;
        instance.framesElapsed = 0;
        Player.DisableControl();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // complete the current message
            framesElapsed = activeMessage.Length * (activeMessage.Length / ((int)framesElapsed + 1));

            // begin the next message
            if (framesElapsed == 0 && ++messageInd >= messages.Count)
            {
                instance.transform.parent.gameObject.SetActive(false);
                Player.EnableControl();
                callback?.Invoke();
            }
        }
    }
}