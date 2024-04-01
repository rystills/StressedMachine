using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    private List<string> messages;
    [SerializeField] private Text text;
    [SerializeField] private int blinkRate;
    private float framesElapsed;
    private int activeMessage;
    private static DialogueController instance;

    private void Awake()
    {
        instance = this;
        transform.parent.gameObject.SetActive(false);
    }
    private void FixedUpdate()
    {
        // text crawl
        if (framesElapsed < messages[activeMessage].Length)
        {
            text.text = messages[activeMessage].Substring(0, (int)framesElapsed) + "<color=#00000000>" + messages[activeMessage].Substring((int)framesElapsed, messages[activeMessage].Length - (int)framesElapsed) + "</color>";
            framesElapsed += char.IsPunctuation(messages[activeMessage][(int)framesElapsed]) ? .125f : 1;
        }
        // cursor blink
        else
        {
            text.text = messages[activeMessage] + (((int)framesElapsed - messages[activeMessage].Length) % blinkRate < blinkRate / 2 ? " ↪" : "");
            ++framesElapsed;
        }
    }

    public static void Show(List<string> messages)
    {
        // begin message sequence
        instance.messages = messages;
        instance.transform.parent.gameObject.SetActive(true);
        instance.text.text = "";
        instance.activeMessage = 0;
        instance.framesElapsed = 0;
        Player.DisableControl();
    }

    private void Update()
    {
        // move on to the next message
        if (Input.GetMouseButtonDown(0) && framesElapsed >= messages[activeMessage].Length)
        {
            if (++activeMessage >= messages.Count)
            {
                instance.transform.parent.gameObject.SetActive(false);
                Player.EnableControl();
            }
            else framesElapsed = 0;
        }
    }
}