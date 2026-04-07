using System.Diagnostics;
using System.Runtime.InteropServices;
using SkiaSharp;

public static class Win32Window
{
    // ═══════════════════════════════════════════
    // WIN32 P/INVOKE
    // ═══════════════════════════════════════════
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [DllImport("imm32.dll")] static extern IntPtr ImmGetContext(IntPtr hwnd);
    [DllImport("imm32.dll")] static extern bool ImmReleaseContext(IntPtr hwnd, IntPtr himc);
    [DllImport("imm32.dll")] static extern bool ImmAssociateContextEx(IntPtr hwnd, IntPtr himc, uint dwFlags);

    [DllImport("user32.dll")] static extern IntPtr CreateWindowEx(uint exStyle, string className, string title, uint style, int x, int y, int w, int h, IntPtr parent, IntPtr menu, IntPtr hInstance, IntPtr param);
    [DllImport("user32.dll")] static extern bool RegisterClassEx(ref WNDCLASSEX wc);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hwnd, int cmd);
    [DllImport("user32.dll")] static extern bool UpdateWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool GetMessage(out MSG msg, IntPtr hwnd, uint min, uint max);
    [DllImport("user32.dll")] static extern bool TranslateMessage(ref MSG msg);
    [DllImport("user32.dll")] static extern IntPtr DispatchMessage(ref MSG msg);
    [DllImport("user32.dll")] static extern void PostQuitMessage(int code);
    [DllImport("user32.dll")] static extern IntPtr DefWindowProc(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
    [DllImport("user32.dll")] static extern bool InvalidateRect(IntPtr hwnd, IntPtr rect, bool erase);
    [DllImport("user32.dll")] static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT ps);
    [DllImport("user32.dll")] static extern bool EndPaint(IntPtr hwnd, ref PAINTSTRUCT ps);
    [DllImport("user32.dll")] static extern IntPtr LoadCursor(IntPtr hInstance, int cursor);
    [DllImport("user32.dll")] static extern bool AdjustWindowRect(ref RECT rect, uint style, bool menu);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hwnd);

    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")] static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO bmi, uint usage, out IntPtr bits, IntPtr section, uint offset);
    [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")] static extern bool BitBlt(IntPtr dst, int dx, int dy, int dw, int dh, IntPtr src, int sx, int sy, uint rop);

    [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string? name);
    [DllImport("winmm.dll")] static extern uint timeBeginPeriod(uint uPeriod);
    [DllImport("winmm.dll")] static extern uint timeEndPeriod(uint uPeriod);

    // ═══════════════════════════════════════════
    // STRUCTS
    // ═══════════════════════════════════════════
    [StructLayout(LayoutKind.Sequential)]
    struct WNDCLASSEX
    {
        public uint cbSize, style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra, cbWndExtra;
        public IntPtr hInstance, hIcon, hCursor, hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam, lParam; public uint time; public int ptX, ptY; }

    [StructLayout(LayoutKind.Sequential)]
    struct RECT { public int left, top, right, bottom; }

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFOHEADER
    {
        public uint biSize; public int biWidth, biHeight;
        public ushort biPlanes, biBitCount;
        public uint biCompression, biSizeImage;
        public int biXPelsPerMeter, biYPelsPerMeter;
        public uint biClrUsed, biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFO { public BITMAPINFOHEADER bmiHeader; public uint bmiColors; }

    [StructLayout(LayoutKind.Sequential)]
    struct PAINTSTRUCT
    {
        public IntPtr hdc; public bool fErase; public RECT rcPaint;
        public bool fRestore; public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
    }

    // ═══════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════
    const uint WS_FIXED_WINDOW = 0x00CA0000; // OVERLAPPED | CAPTION | SYSMENU | MINIMIZEBOX
    const uint WM_DESTROY = 0x0002;
    const uint WM_SIZE = 0x0005;
    const uint WM_PAINT = 0x000F;
    const uint WM_CLOSE = 0x0010;
    const uint WM_SHOWWINDOW = 0x0018;
    const uint WM_KEYDOWN = 0x0100;
    const uint WM_KEYUP = 0x0101;
    const uint WM_CHAR = 0x0102;
    const uint WM_MOUSEMOVE = 0x0200;
    const uint WM_LBUTTONDOWN = 0x0201;
    const uint WM_LBUTTONUP = 0x0202;
    const uint WM_MOUSEWHEEL = 0x020A;
    const uint SRCCOPY = 0x00CC0020;
    const int IDC_ARROW = 32512;

    // FPS khi visible và khi ẩn
    private static double FrameMsVisible => 1000.0 / AppConfig.FpsVisible;
    private static double FrameMsHidden => 1000.0 / AppConfig.FpsHidden;

    // ═══════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════
    private static IntPtr _hwnd;
    private static IntPtr _memDC, _hBitmap, _bits;
    private static SKSurface? _surface;
    private static GameLoop? _loop;

    private static volatile bool _isVisible = false;
    private static volatile bool _running = false;

    private static readonly object _renderLock = new();

    private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);
    private static WndProcDelegate? _wndProcDelegate;

    // ═══════════════════════════════════════════
    // PUBLIC
    // ═══════════════════════════════════════════
    public static void Run(int clientW, int clientH, string title, bool startHidden = false)
    {
        SetProcessDPIAware();

        Thread.CurrentThread.Name = Main.mainThreadName;

        var hInst = GetModuleHandle(null);
        _wndProcDelegate = WndProc;

        var wc = new WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
            style = 0x0003,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = hInst,
            hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW),
            lpszClassName = "NRO_Win32",
        };
        RegisterClassEx(ref wc);

        var rc = new RECT { right = clientW, bottom = clientH };
        AdjustWindowRect(ref rc, WS_FIXED_WINDOW, false);

        // ✅ MỚI:
        _hwnd = CreateWindowEx(0, "NRO_Win32", title, WS_FIXED_WINDOW,
            100, 100, rc.right - rc.left, rc.bottom - rc.top,
            IntPtr.Zero, IntPtr.Zero, hInst, IntPtr.Zero);

        // Tắt IME — UniKey gửi qua WM_CHAR trực tiếp
        ImmAssociateContextEx(_hwnd, IntPtr.Zero, 0);

        RebuildDIB(clientW, clientH);
        _loop = new GameLoop(clientW, clientH);

        timeBeginPeriod(1);
        _running = true;

        var gameThread = new Thread(GameLoopThread)
        {
            Name = "GameLoopThread",
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal,
        };
        gameThread.Start();

        ShowWindow(_hwnd, startHidden ? 0 : 5);
        _isVisible = !startHidden;
        UpdateWindow(_hwnd);

        while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

        _running = false;
        timeEndPeriod(1);
    }

    // ═══════════════════════════════════════════
    // GAME LOOP THREAD
    // Visible  → 60fps, update + render
    // Hidden   → 20fps, update only (không render → tiết kiệm CPU)
    // ═══════════════════════════════════════════
    private static void GameLoopThread()
    {
        var sw = Stopwatch.StartNew();

        while (_running)
        {
            bool visible = _isVisible;
            double frameMs = visible ? FrameMsVisible : FrameMsHidden;
            double frameStart = sw.Elapsed.TotalMilliseconds;

            // Update logic
            _loop?.OnUpdate();

            // Render chỉ khi visible
            if (visible)
                InvalidateRect(_hwnd, IntPtr.Zero, false);

            // Sleep chính xác đến frame kế tiếp — không SpinWait
            double elapsed = sw.Elapsed.TotalMilliseconds - frameStart;
            double sleepMs = frameMs - elapsed;

            if (sleepMs > 1.0)
                Thread.Sleep((int)sleepMs);
            else
                Thread.Sleep(1); // yield CPU, không busy-wait
        }
    }

    // ═══════════════════════════════════════════
    // WNDPROC
    // ═══════════════════════════════════════════
    private static IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp)
    {
        switch (msg)
        {
            case WM_PAINT:
                OnPaint(hwnd);
                return IntPtr.Zero;

            case WM_SHOWWINDOW:
                _isVisible = wp.ToInt64() != 0;
                if (!_isVisible)
                {
                    lock (_renderLock) { CleanupDIB(); }
                    GC.Collect(2, GCCollectionMode.Forced, true, true);
                    GC.WaitForPendingFinalizers();
                }
                else
                {
                    lock (_renderLock)
                    {
                        if (_surface == null)
                        {
                            GetClientRect(hwnd, out RECT rect);
                            RebuildDIB(rect.right, rect.bottom);
                        }
                    }
                }
                return DefWindowProc(hwnd, msg, wp, lp);

            case WM_SIZE:
                return IntPtr.Zero;

            case WM_KEYDOWN:
                int vk = (int)wp;
                // Chữ A-Z và số 0-9 để WM_CHAR xử lý (tránh duplicate với UniKey)
                bool isPrintable = (vk >= 0x30 && vk <= 0x39)
                                || (vk >= 0x41 && vk <= 0x5A);
                if (!isPrintable)
                    _loop?.OnKeyDown(vk);
                return IntPtr.Zero;

            case WM_KEYUP:
                _loop?.OnKeyUp((int)wp);
                return IntPtr.Zero;

            case WM_CHAR:
                int ch = (int)wp;
                if (ch >= 32) // bỏ control chars
                    _loop?.OnChar(ch);
                return IntPtr.Zero;

            case WM_LBUTTONDOWN:
                _loop?.OnMouseDown(LoWord(lp), HiWord(lp));
                return IntPtr.Zero;

            case WM_LBUTTONUP:
                _loop?.OnMouseUp(LoWord(lp), HiWord(lp));
                return IntPtr.Zero;

            case WM_MOUSEMOVE:
                if ((wp.ToInt64() & 0x0001) != 0)
                    _loop?.OnMouseDrag(LoWord(lp), HiWord(lp));
                return IntPtr.Zero;

            case WM_MOUSEWHEEL:
                int delta = (short)((wp.ToInt64() >> 16) & 0xFFFF);
                _loop?.OnMouseScroll(delta / 120);
                return IntPtr.Zero;

            case WM_CLOSE:
                ShowWindow(hwnd, 0);
                return IntPtr.Zero;

            case WM_DESTROY:
                _running = false;
                GameCanvas.bRun = false;
                Session_ME.gI().close();
                Session_ME2.gI().close();
                lock (_renderLock) { CleanupDIB(); }
                PostQuitMessage(0);
                return IntPtr.Zero;
        }
        return DefWindowProc(hwnd, msg, wp, lp);
    }

    // ═══════════════════════════════════════════
    // PAINT
    // ═══════════════════════════════════════════
    private static void OnPaint(IntPtr hwnd)
    {
        var hdc = BeginPaint(hwnd, out PAINTSTRUCT ps);
        lock (_renderLock)
        {
            if (_surface != null && _bits != IntPtr.Zero)
            {
                var canvas = _surface.Canvas;
                canvas.Clear(SKColors.Black);
                mGraphics.canvas = canvas;
                _loop?.OnRender(canvas);
                _surface.Flush();

                GetClientRect(hwnd, out RECT rect);
                BitBlt(hdc, 0, 0, rect.right, rect.bottom, _memDC, 0, 0, SRCCOPY);
            }
        }
        EndPaint(hwnd, ref ps);
    }

    // ═══════════════════════════════════════════
    // DIB
    // ═══════════════════════════════════════════
    private static void RebuildDIB(int w, int h)
    {
        CleanupDIB();
        var bmi = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                biWidth = w,
                biHeight = -h,
                biPlanes = 1,
                biBitCount = 32,
            }
        };
        var screenDC = GetDC(IntPtr.Zero);
        _memDC = CreateCompatibleDC(screenDC);
        _hBitmap = CreateDIBSection(screenDC, ref bmi, 0, out _bits, IntPtr.Zero, 0);
        ReleaseDC(IntPtr.Zero, screenDC);
        SelectObject(_memDC, _hBitmap);

        var info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
        _surface = SKSurface.Create(info, _bits, w * 4);
    }

    private static void CleanupDIB()
    {
        _surface?.Dispose(); _surface = null;
        if (_hBitmap != IntPtr.Zero) { DeleteObject(_hBitmap); _hBitmap = IntPtr.Zero; }
        if (_memDC != IntPtr.Zero) { DeleteDC(_memDC); _memDC = IntPtr.Zero; }
        _bits = IntPtr.Zero;
    }

    static int LoWord(IntPtr lp) => (short)(lp.ToInt64() & 0xFFFF);
    static int HiWord(IntPtr lp) => (short)((lp.ToInt64() >> 16) & 0xFFFF);

    public static void ShowGameWindow()
    {
        if (_hwnd == IntPtr.Zero) return;
        lock (_renderLock)
        {
            if (_surface == null)
            {
                GetClientRect(_hwnd, out RECT rect);
                RebuildDIB(rect.right, rect.bottom);
            }
        }
        ShowWindow(_hwnd, 9);
        ShowWindow(_hwnd, 5);
        SetForegroundWindow(_hwnd);
    }

    public static void HideGameWindow()
    {
        if (_hwnd != IntPtr.Zero)
            ShowWindow(_hwnd, 0);
    }
}