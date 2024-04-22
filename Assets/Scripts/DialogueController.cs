using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    private List<string> messages;
    [SerializeField] private Text text;
    [SerializeField] private int blinkRate;
    [SerializeField] private float clickGraceFrames = 20;
    private float framesElapsed;
    private int messageInd;
    public static DialogueController instance;
    private string activeMessage => messages[messageInd];
    private List<Action> callbacks;

    [SerializeField] private AudioSource dialogueSnd;
    private List<char> characterOrder = new() { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                                                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                                                ',', '.', '?', '!', ' ' };

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
            if (framesElapsed % 1 == 0 && framesElapsed < activeMessage.Length)
                dialogueSnd.PlayAtPitch(Mathf.Sqrt((characterOrder.IndexOf(char.ToLower(activeMessage[(int)framesElapsed])) + 1f) / characterOrder.Count));
        }
        // cursor blink
        else
        {
            text.text = activeMessage + (messageInd == messages.Count - 1 ? " " : ((int)framesElapsed - activeMessage.Length) % blinkRate < blinkRate / 2 ? " ↪" : "");
            ++framesElapsed;
        }
    }

    public static void Show(List<string> messages, List<Action> callbacks = null)
    {
        // begin message sequence
        instance.callbacks = callbacks;
        instance.messages = messages;
        instance.transform.parent.gameObject.SetActive(true);
        instance.text.text = "";
        instance.messageInd = 0;
        instance.framesElapsed = 0;
        Player.DisableControl();
    }

    private void Update()
    {
        // apply a grace period to the first message to guard against accidental clicks
        if (framesElapsed >= clickGraceFrames || messageInd > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // complete the current message
                framesElapsed = activeMessage.Length * (activeMessage.Length / ((int)framesElapsed + 1));

                if (framesElapsed == 0 && ++messageInd >= messages.Count)
                {
                    instance.transform.parent.gameObject.SetActive(false);
                    callbacks?.ForEach(cb => cb.Invoke());
                    Player.EnableControl();
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                // return to the previous message
                framesElapsed = 0;
                messageInd = Mathf.Max(0, messageInd - 1);
            }
        }
    }
}