using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Key
{
    public KeyCode keyCode;
    public bool pressed;
    public bool held;
    public bool released;
    private bool _newHeld;
    private float firstHeldTime = -1000;
    private float latestHeldTime = -1000;

    public Key(KeyCode code) => keyCode = code;

    public void ClearBuffers() => firstHeldTime = latestHeldTime = -1000;

    public void Update()
    {
        _newHeld = Input.GetKey(keyCode);
        pressed = _newHeld && !held;
        released = !_newHeld && held;
        held = _newHeld;
        if (pressed) firstHeldTime = Time.unscaledTime;
        if (held) latestHeldTime = Time.unscaledTime;
    }

    public bool Pressed(float timeBuffer) => pressed || (Time.unscaledTime - firstHeldTime <= timeBuffer);
    public bool Pressed(bool clearBuffersOnTrue)
    {
        if (clearBuffersOnTrue && pressed) ClearBuffers();
        return pressed;
    }
    public bool Pressed(int frameBuffer, bool clearBuffersOnTrue)
    {
        bool retVal = Pressed(frameBuffer);
        if (clearBuffersOnTrue && retVal) ClearBuffers();
        return retVal;
    }

    public bool Held(float timeBuffer) => held || (Time.unscaledTime - latestHeldTime <= timeBuffer);
    public bool Released(float timeBuffer) => released || (!held && Time.unscaledTime - latestHeldTime <= timeBuffer);
}

[DefaultExecutionOrder(-999)]
public class InputExt : MonoBehaviour
{

    public static Dictionary<string, Key> keys = new Dictionary<string, Key>();
    public static Vector3 mousePos;
    public static Vector2 stick1Pos;

    public static void ClearBuffers()
    {
        foreach (Key key in keys.Values) key.ClearBuffers();
        stick1Pos = Vector2.zero;
    }
    public static void Tick()
    {
        foreach (Key key in keys.Values) key.Update();
        mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        stick1Pos.x = Input.GetAxisRaw("Horizontal");
        stick1Pos.y = Input.GetAxisRaw("Vertical");
    }

    public static bool AnyKeyHeld() => keys.Values.Any(k => k.held);

    public static void RegisterKey(string action, KeyCode keyCode)
        => keys.Add(action, new(keyCode));

    private void Awake() => DontDestroyOnLoad(gameObject);

    private void Update() => Tick();

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad() => new GameObject("InputExt").AddComponent<InputExt>();
}
