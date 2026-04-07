using SkiaSharp;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public struct Vector2
{
    public float x, y;
    public Vector2(float x, float y) { this.x = x; this.y = y; }
    public static Vector2 zero => new(0, 0);
    public static Vector2 one => new(1, 1);
    public float magnitude => (float)System.Math.Sqrt(x * x + y * y);
    public SKPoint ToSKPoint() => new SKPoint(x, y);
    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.x + b.x, a.y + b.y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.x - b.x, a.y - b.y);
    public static Vector2 operator *(Vector2 a, float f) => new(a.x * f, a.y * f);
    public override string ToString() => $"({x}, {y})";
}

public struct Vector3
{
    public float x, y, z;
    public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    public Vector2 ToVector2() => new Vector2(x, y);
}

public struct Rect
{
    public float x, y, width, height;
    public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; }
    public float xMin => x;
    public float yMin => y;
    public float xMax => x + width;
    public float yMax => y + height;
    public bool Contains(Vector2 p) => p.x >= x && p.x <= x + width && p.y >= y && p.y <= y + height;
    public SKRect ToSKRect() => new SKRect(x, y, x + width, y + height);
}

public struct Color
{
    public float r, g, b, a;
    public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
    public static Color white => new(1, 1, 1);
    public static Color black => new(0, 0, 0);
    public static Color red => new(1, 0, 0);
    public static Color green => new(0, 1, 0);
    public static Color blue => new(0, 0, 1);
    public static Color yellow => new(1, 1, 0);
    public static Color clear => new(0, 0, 0, 0);
    public static Color gray => new(0.5f, 0.5f, 0.5f);
    public SKColor ToSKColor() => new SKColor(
        (byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
}

public static class Debug
{
    public static void Log(object msg) => Console.WriteLine($"[LOG] {msg}");
    public static void LogError(object msg) => Console.WriteLine($"[ERROR] {msg}");
    public static void LogWarning(object msg) => Console.WriteLine($"[WARN] {msg}");
}


public class GameObject
{
    public string name = "";
    public bool activeSelf = true;
    public void SetActive(bool v) => activeSelf = v;
    public T GetComponent<T>() where T : new() => new T();
}

public class ScriptableObject { }

public static class PlayerPrefs
{
    private static Dictionary<string, string> _data = new();
    public static void SetInt(string k, int v) => _data[k] = v.ToString();
    public static int GetInt(string k, int def = 0) => _data.TryGetValue(k, out var v) ? int.Parse(v) : def;
    public static void SetString(string k, string v) => _data[k] = v;
    public static string GetString(string k, string def = "") => _data.TryGetValue(k, out var v) ? v : def;
    public static void Save() { }
    public static void DeleteKey(string k) { _data.Remove(k); }
    public static bool HasKey(string k) => _data.ContainsKey(k);
}

public static class Time
{
    public static float deltaTime => 0.016f;
    public static float time => Environment.TickCount / 1000f;
}

public static class Screen
{
    public static int width => 800;
    public static int height => 600;
    public static int orientation { get; set; }
}

public static class Input
{
    public static bool GetKeyDown(KeyCode key) => false;
    public static bool GetKey(KeyCode key) => false;
    public static Vector2 mousePosition => new(0, 0);
    public static bool GetMouseButtonDown(int btn) => false;
    public static bool GetMouseButton(int btn) => false;
}

public enum KeyCode
{
    None, Space, Return, Escape, UpArrow, DownArrow, LeftArrow, RightArrow,
    A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    Equals, Minus, Backspace, Period, At, Tab, Delete,
    Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9
}

public static class Application
{
    public static int targetFrameRate = 60;
    public static bool runInBackground = true;
    public static string persistentDataPath => Path.Combine(AppContext.BaseDirectory, "Data");
    public static void Quit() => System.Environment.Exit(0);
    public static void OpenURL(string url) {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
}

public static class Resources
{
    public static T Load<T>(string path) where T : class => null;
    public static object Load(string path, System.Type t) => null;
    public static void UnloadUnusedAssets() { }
}

public class WWW
{
    private static readonly HttpClient _client = new HttpClient();
    private System.Threading.Tasks.Task<string> _task;
    private string _result;
    private string _error;

    public bool isDone { get; private set; }
    public string text => _result ?? string.Empty;
    public string error => _error;

    public WWW(string url)
    {
        _task = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                return await _client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                return string.Empty;
            }
        });
        _task.ContinueWith(t =>
        {
            _result = t.Result;
            isDone = true;
        });
    }
}

public static class ScreenOrientation { 
    public static int LandscapeLeft = 0, Portrait = 1, AutoRotation = 2, LandscapeRight = 3, PortraitUpsideDown = 4; 
}

public static class SystemInfo { public static string deviceModel = "PC"; }

public class Font { }

public class GUIContent
{
    public string text;
    public GUIContent(string t) { text = t; }
}

public class GUIStyleState { public Color textColor; }

public class GUIStyle
{
    public Font font;
    public TextAnchor alignment;
    public GUIStyleState normal = new GUIStyleState();
    public GUIStyle() { }
    public GUIStyle(GUIStyle other) { }
    public (float x, float y) CalcSize(GUIContent c) => (c.text.Length * 7f, 14f);
}

public enum TextAnchor { UpperLeft, UpperRight, UpperCenter, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }

public class GUISkin { public GUIStyle label = new GUIStyle(); }

public static class GUI { public static GUISkin skin = new GUISkin(); }

public enum EventType { KeyDown, KeyUp, MouseDown, MouseUp, Repaint }
public class Event { 
    public static Event current = new Event();
    public EventType type;
    public KeyCode keyCode;
    public char character;
}

// Thêm KeyCode còn thi?u
public static class KeyCodeExtra {
    public const KeyCode LeftShift = (KeyCode)160;
    public const KeyCode RightShift = (KeyCode)161;
    public const KeyCode Minus = (KeyCode)45;
    public const KeyCode F1 = (KeyCode)112;
    public const KeyCode F2 = (KeyCode)113;
    public const KeyCode F3 = (KeyCode)114;
    public const KeyCode Backspace = (KeyCode)8;
    public const KeyCode Period = (KeyCode)46;
    public const KeyCode At = (KeyCode)64;
    public const KeyCode Tab = (KeyCode)9;
}
