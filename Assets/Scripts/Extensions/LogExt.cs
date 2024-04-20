using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LogExt : MonoBehaviour
{
    private static int numVisibleLines;
    private static Rect logRect = Rect.zero;
    private static StringBuilder logSB = new();
    private static string logString;
    private static GUIStyle logStyle = new() { richText = true, fontSize = 18 };
    private static GUIStyle logBGStyle = new() { normal = new GUIStyleState() { background = Texture2D.grayTexture } };

    private static (string, float)[] visibleLines;
    private static int curLineInd = 0;
    private static float lastRGBHue;
    private static int lastMessageNum = -1;
    private static List<float> prevUnscaledFrameTimes = new();

    private static string GetNextColorTag() => "<color=#" + ColorUtility.ToHtmlStringRGB(Color.HSVToRGB(lastRGBHue = (lastRGBHue + .1f) % 1f, 1f, .15f)) + ">";

    private void OnGUI()
    {
        // draw fps + log
        logSB.Clear();
        logSB.AppendLine("fps: " + prevUnscaledFrameTimes.Count);
        if (visibleLines[curLineInd].Item2 > Time.unscaledTime)
        {
            // traverse visibleLines as a ringbuffer to accumulate active log string
            for (int i = (curLineInd + 1) % numVisibleLines; i != curLineInd; i = (i + 1) % numVisibleLines)
            {
                if (visibleLines[i].Item2 > Time.unscaledTime)
                    logSB.AppendLine(visibleLines[i].Item1 + "</color>");
            }
            logSB.AppendLine(visibleLines[curLineInd].Item1 + "</color>");
        }
        logString = logSB.ToString();
        int lineCount = logString.Count(c => c == '\n');
        logRect.Set(0, 0, lineCount > 1 ? 450 : 80, 21 * lineCount);
        GUI.Box(logRect, GUIContent.none, logBGStyle);
        GUI.Label(logRect, logString, logStyle);
    }

    private void Update()
    {
        // update fps counter
        prevUnscaledFrameTimes.Add(Time.unscaledTime);
        while (Time.unscaledTime - prevUnscaledFrameTimes.First() > 1)
            prevUnscaledFrameTimes.RemoveAt(0);
    }

    // add message to screen display
    public static void Show(object message, float durationSeconds = 3)
        => visibleLines[curLineInd = (curLineInd + 1) % numVisibleLines] = (GetNextColorTag() + "[" + ++lastMessageNum + "]: " + message, Time.unscaledTime + durationSeconds);

    private void Awake()
    {
        // calculate number of visible lines
        numVisibleLines = Screen.height / 21 - 1;
        visibleLines = new(string, float)[numVisibleLines];

        DontDestroyOnLoad(gameObject);

        // disable logging outside of PIE
        enabled = Application.isEditor;
    }
    
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad() => new GameObject("LogExt").AddComponent<LogExt>();
}
