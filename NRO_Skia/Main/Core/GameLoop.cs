using SkiaSharp;
using System;

public class GameLoop
{
    private int _width, _height;

    private enum InitState { Waiting, Counting, Done, Failed }
    private InitState _initState = InitState.Waiting;
    private int _initCount = 0;
    private const int INIT_DELAY_FRAMES = 10;

    private readonly mGraphics _cachedGraphics = new();
    private volatile bool _sleeping = false;

    public GameLoop(int width, int height)
    {
        _width = width;
        _height = height;
    }

    // ═══════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════
    public void OnUpdate()
    {
        if (_initState == InitState.Done) { RunUpdate(); return; }
        if (_initState == InitState.Failed) return;

        if (_initState == InitState.Waiting)
            _initState = InitState.Counting;

        if (++_initCount < INIT_DELAY_FRAMES) return;

        DoInit();
    }

    private void RunUpdate()
    {
        try
        {
            Rms.update();
            ipKeyboard.update();
            Session_ME.update();
            Session_ME2.update();
            GameMidlet.gameCanvas?.update();
            Image.update();
            DataInputStream.update();
            SMS.update();
            Net.update();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Update ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════
    // RENDER
    // ═══════════════════════════════════════════
    public void OnRender(SKCanvas skCanvas)
    {
        if (_initState != InitState.Done || GameMidlet.gameCanvas == null) return;

        if (_sleeping)
        {
            using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true, TextAlign = SKTextAlign.Center };
            using var font1 = new SKFont(SKTypeface.FromFamilyName("Consolas"), 14);
            using var font2 = new SKFont(SKTypeface.FromFamilyName("Consolas"), 11);
            float cx = _width / 2f, cy = _height / 2f;
            skCanvas.DrawText("[ SLEEP ]", cx, cy - 10, font1, paint);
            paint.Color = SKColors.Gray;
            skCanvas.DrawText("Ấn [ \\ ] hoặc click để tiếp tục", cx, cy + 12, font2, paint);
            return;
        }

        try
        {
            var g = _cachedGraphics;
            g.reset();
            // KHÔNG scale canvas — mGraphics tự nhân zoomLevel rồi
            GameMidlet.gameCanvas.paint(g);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Paint ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════
    // RESIZE
    // ═══════════════════════════════════════════
    public void OnResize(int w, int h)
    {
        _width = w;
        _height = h;
        if (_initState == InitState.Done)
        {
            ScaleGUI.WIDTH = w;
            ScaleGUI.HEIGHT = h;
            // cập nhật logical size khi resize
            GameCanvas.w = w / mGraphics.zoomLevel;
            GameCanvas.h = h / mGraphics.zoomLevel;
        }
    }

    // ═══════════════════════════════════════════
    // INPUT
    // ═══════════════════════════════════════════
    public void OnKeyDown(int vk)
    {
        if (vk == 0xDC) { _sleeping = !_sleeping; return; }
        var kc = MapVK(vk);
        if (kc != KeyCode.None && MotherCanvas.instance != null)
            MotherCanvas.instance.onKeyPressed(MyKeyMap.map(kc));
    }

    public void OnKeyUp(int vk)
    {
        var kc = MapVK(vk);
        if (kc != KeyCode.None && MotherCanvas.instance != null)
            MotherCanvas.instance.onKeyReleased(MyKeyMap.map(kc));
    }

    public void OnMouseDown(int x, int y)
    {
        if (_sleeping) { _sleeping = false; return; }
        MotherCanvas.instance?.onPointerPressed(x, y);
    }
    public void OnMouseScroll(int delta)
    {
        MotherCanvas.instance?.scrollMouse(delta * 10);
    }
    public void OnMouseUp(int x, int y) =>
        MotherCanvas.instance?.onPointerReleased(x, y);

    public void OnMouseDrag(int x, int y) =>
        MotherCanvas.instance?.onPointerDragged(x, y);
    public void OnChar(int ch)
    {
        MotherCanvas.instance?.onKeyPressed(ch);
    }

    // ═══════════════════════════════════════════
    // INIT — thứ tự quan trọng!
    // ═══════════════════════════════════════════

    private void DoInit()
    {
        Main.isPC = true;  // ← THÊM DÒNG NÀY
        mSystem.isTest = true;
        try
        {
            // Bước 1: set WIDTH/HEIGHT để MotherCanvas tính zoom
            Step("ScaleGUI_Pre", InitScaleGUI_Pre);

            // Bước 2: MotherCanvas.checkZoomLevel tự set mGraphics.zoomLevel
            Step("MotherCanvas", () => MotherCanvas.instance = new MotherCanvas());

            // Bước 3: giờ zoomLevel đã đúng, tính logical size
            Step("ScaleGUI_Post", InitScaleGUI_Post);

            Step("Session_ME", () =>
            {
                Session_ME.gI().setHandler(Controller.gI());
                Session_ME2.gI().setHandler(Controller.gI());
                Session_ME2.isMainSession = false;
            });

            Step("GameCanvas", () =>
            {
                GameMidlet.instance = new GameMidlet_Inner();
                GameMidlet.gameCanvas = new GameCanvas();
                GameCanvas.instance = GameMidlet.gameCanvas;
                GameCanvas.bRun = true;
            });

            TryLoad("TileMap", () => TileMap.loadBg());
            TryLoad("Paint", () => global::Paint.loadbg());
            TryLoad("PopUp", () => PopUp.loadBg());
            TryLoad("GameScr", () => GameScr.loadBg());
            TryLoad("InfoMe", () => InfoMe.gI().loadCharId());
            TryLoad("Panel", () => Panel.loadBg());
            TryLoad("Menu", () => Menu.loadBg());
            TryLoad("KeyMap", () => Key.mapKeyPC());
            TryLoad("Sound", () => SoundMn.gI().loadSound(TileMap.mapID));

            Step("SplashScr", () =>
            {
                GameMidlet.gameCanvas!.start();
                SplashScr.loadImg();
                SplashScr.loadSplashScr();
                GameCanvas.currentScreen = new SplashScr();
            });

            mSystem.isTest = false;
            _initState = InitState.Done;
            Console.WriteLine($"[Init OK] zoom={mGraphics.zoomLevel}, logical={GameCanvas.w}x{GameCanvas.h}, window={_width}x{_height}");
        }
        catch (Exception ex)
        {
            _initState = InitState.Failed;
            Console.WriteLine($"[INIT FATAL] {ex}\n{ex.StackTrace}");
        }
    }

    // Bước 1: chỉ set WIDTH/HEIGHT, chưa tính GameCanvas.w/h
    private void InitScaleGUI_Pre()
    {
        ScaleGUI.WIDTH = _width;
        ScaleGUI.HEIGHT = _height;
        ScaleGUI.scaleScreen = false;
        if (mGraphics.zoomLevel == 0) mGraphics.zoomLevel = 1;
        Console.WriteLine($"[ScaleGUI_Pre] window={_width}x{_height}");
    }

    // Bước 3: sau khi MotherCanvas đã set zoomLevel đúng
    private void InitScaleGUI_Post()
    {
        if (mGraphics.zoomLevel < 1) mGraphics.zoomLevel = 1;
        if (mGraphics.zoomLevel > 2) mGraphics.zoomLevel = 2;

        // ✅ Giống Unity gốc — game tự adapt theo logical size
        GameCanvas.w = _width / mGraphics.zoomLevel;
        GameCanvas.h = _height / mGraphics.zoomLevel;

        ScaleGUI.WIDTH = _width;
        ScaleGUI.HEIGHT = _height;

        Console.WriteLine($"[Init] zoom={mGraphics.zoomLevel}, logical={GameCanvas.w}x{GameCanvas.h}, window={_width}x{_height}");
    }

    private static void Step(string name, Action action)
    {
        Console.WriteLine($"  >> {name}...");
        action();
        Console.WriteLine($"  ✓  {name}");
    }

    private static void TryLoad(string name, Action action)
    {
        try { action(); Console.WriteLine($"  ✓  {name}"); }
        catch (Exception ex) { Console.WriteLine($"  ✗  {name}: {ex.Message} (skipped)"); }
    }

    // ═══════════════════════════════════════════
    // VK → KeyCode
    // ═══════════════════════════════════════════
    private static KeyCode MapVK(int vk) => vk switch
    {
        0x41 => KeyCode.A,
        0x42 => KeyCode.B,
        0x43 => KeyCode.C,
        0x44 => KeyCode.D,
        0x45 => KeyCode.E,
        0x46 => KeyCode.F,
        0x47 => KeyCode.G,
        0x48 => KeyCode.H,
        0x49 => KeyCode.I,
        0x4A => KeyCode.J,
        0x4B => KeyCode.K,
        0x4C => KeyCode.L,
        0x4D => KeyCode.M,
        0x4E => KeyCode.N,
        0x4F => KeyCode.O,
        0x50 => KeyCode.P,
        0x51 => KeyCode.Q,
        0x52 => KeyCode.R,
        0x53 => KeyCode.S,
        0x54 => KeyCode.T,
        0x55 => KeyCode.U,
        0x56 => KeyCode.V,
        0x57 => KeyCode.W,
        0x58 => KeyCode.X,
        0x59 => KeyCode.Y,
        0x5A => KeyCode.Z,
        0x30 => KeyCode.Alpha0,
        0x31 => KeyCode.Alpha1,
        0x32 => KeyCode.Alpha2,
        0x33 => KeyCode.Alpha3,
        0x34 => KeyCode.Alpha4,
        0x35 => KeyCode.Alpha5,
        0x36 => KeyCode.Alpha6,
        0x37 => KeyCode.Alpha7,
        0x38 => KeyCode.Alpha8,
        0x39 => KeyCode.Alpha9,
        0x26 => KeyCode.UpArrow,
        0x28 => KeyCode.DownArrow,
        0x25 => KeyCode.LeftArrow,
        0x27 => KeyCode.RightArrow,
        0x20 => KeyCode.Space,
        0x0D => KeyCode.Return,
        0x1B => KeyCode.Escape,
        0x08 => KeyCode.Backspace,
        0x70 => KeyCode.F1,
        0x71 => KeyCode.F2,
        0x72 => KeyCode.F3,
        0xBD => KeyCode.Minus,
        0xBB => KeyCode.Equals,
        0xBE => KeyCode.Period,
        0x09 => KeyCode.Tab,
        _ => KeyCode.None
    };
}

internal class GameMidlet_Inner : GameMidlet
{
    public GameMidlet_Inner() : base(skipInit: true) { }
}