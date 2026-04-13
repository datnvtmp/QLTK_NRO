using SkiaSharp;
using System;
using System.Collections;
using Assets.src.e;

public class mGraphics
{
    public static int HCENTER = 1;
    public static int VCENTER = 2;
    public static int LEFT = 4;
    public static int RIGHT = 8;
    public static int TOP = 16;
    public static int BOTTOM = 32;

    private float _r, _g, _b, _a = 1f;

    public int clipX, clipY, clipW, clipH;
    private bool isClip;
    private bool isTranslate;

    // translateX/Y luôn là pixel thật (đã nhân zoom)
    public int translateX, translateY;
    private float translateXf, translateYf;

    public static int zoomLevel = 1;

    public const int BASELINE = 64;
    public const int SOLID = 0;
    public const int DOTTED = 1;
    public const int TRANS_MIRROR = 2;
    public const int TRANS_MIRROR_ROT180 = 1;
    public const int TRANS_MIRROR_ROT270 = 4;
    public const int TRANS_MIRROR_ROT90 = 7;
    public const int TRANS_NONE = 0;
    public const int TRANS_ROT180 = 3;
    public const int TRANS_ROT270 = 6;
    public const int TRANS_ROT90 = 5;

    // (cachedTextures removed — was dead code, never used)
    public static int addYWhenOpenKeyBoard;

    private int clipTX, clipTY;
    private int currentBGColor;

    private Vector2 pos = new(0f, 0f);
    private Rect rect;
    private Vector2 pivot;
    public Vector2 size = new(128f, 128f);
    public Vector2 relativePosition = new(0f, 0f);
    public Color clTrans;
    public static Color transParentColor = new Color(1f, 1f, 1f, 0f);

    // SKCanvas được set mỗi frame từ Win32Window.OnPaint
    public static SKCanvas canvas;

    private readonly SKPaint _paint = new SKPaint { IsAntialias = false };
    private readonly SKPaint _textPaint = new SKPaint { IsAntialias = true };

    private SKColor CurrentSKColor => new SKColor(
        (byte)(_r * 255), (byte)(_g * 255), (byte)(_b * 255), (byte)(_a * 255));

    private SKRect GetClipSKRect() =>
        new SKRect(clipX + clipTX, clipY + clipTY,
                   clipX + clipTX + clipW, clipY + clipTY + clipH);

    // ─────────────────────────────────────────────
    // TRANSLATE
    // ─────────────────────────────────────────────
    public void translate(int tx, int ty)
    {
        translateX += tx * zoomLevel;
        translateY += ty * zoomLevel;
        isTranslate = translateX != 0 || translateY != 0;
    }



    public int getTranslateX() => translateX / zoomLevel;
    public int getTranslateY() => translateY / zoomLevel + addYWhenOpenKeyBoard;

    // ─────────────────────────────────────────────
    // CLIP
    // ─────────────────────────────────────────────
    public void setClip(int x, int y, int w, int h)
    {
        clipTX = translateX;
        clipTY = translateY;
        clipX = x * zoomLevel;
        clipY = y * zoomLevel;
        clipW = w * zoomLevel;
        clipH = h * zoomLevel;
        isClip = true;
    }

    public int getClipX() => GameScr.cmx;
    public int getClipY() => GameScr.cmy;
    public int getClipWidth() => GameScr.gW;
    public int getClipHeight() => GameScr.gH;

    // ─────────────────────────────────────────────
    // COLOR
    // ─────────────────────────────────────────────
    public void setColor(int rgb)
    {
        _b = (rgb & 0xFF) / 256f;
        _g = ((rgb >> 8) & 0xFF) / 256f;
        _r = ((rgb >> 16) & 0xFF) / 256f;
        _a = 1f;
    }


    public void setColor(int rgb, float alpha)
    {
        setColor(rgb);
        _a = alpha;
    }



    // ─────────────────────────────────────────────
    // PRIMITIVES
    // ─────────────────────────────────────────────
    public void fillRect(int x, int y, int w, int h, int color, int alpha)
    {
        setColor(color, alpha / 255f);
        fillRect(x, y, w, h);
    }

    public void fillRect(int x, int y, int w, int h)
    {
        if (canvas == null) return;
        int px = x * zoomLevel + translateX;
        int py = y * zoomLevel + translateY;
        int pw = w * zoomLevel;
        int ph = h * zoomLevel;
        if (pw <= 0 || ph <= 0) return;
        _paint.Color = CurrentSKColor;
        _paint.Style = SKPaintStyle.Fill;
        if (isClip) { canvas.Save(); canvas.ClipRect(GetClipSKRect()); }
        canvas.DrawRect(px, py, pw, ph, _paint);
        if (isClip) canvas.Restore();
    }

    public void drawRect(int x, int y, int w, int h)
    {
        fillRect(x, y, w, 1);
        fillRect(x, y, 1, h);
        fillRect(x + w, y, 1, h + 1);
        fillRect(x, y + h, w + 1, 1);
    }

    public void drawLine(int x1, int y1, int x2, int y2)
    {
        if (canvas == null) return;
        int px1 = x1 * zoomLevel + translateX;
        int py1 = y1 * zoomLevel + translateY;
        int px2 = x2 * zoomLevel + translateX;
        int py2 = y2 * zoomLevel + translateY;
        _paint.Color = CurrentSKColor;
        _paint.Style = SKPaintStyle.Stroke;
        _paint.StrokeWidth = 1;
        if (isClip) { canvas.Save(); canvas.ClipRect(GetClipSKRect()); }
        canvas.DrawLine(px1, py1, px2, py2, _paint);
        if (isClip) canvas.Restore();
    }

    public void drawRoundRect(int x, int y, int w, int h, int arcW, int arcH) =>
        drawRect(x, y, w, h);

    public void fillRoundRect(int x, int y, int w, int h, int arcW, int arcH) =>
        fillRect(x, y, w, h);

    // ─────────────────────────────────────────────
    // DRAW STRING
    // x,y là pixel thật (mFont tự tính trước khi gọi)
    // ─────────────────────────────────────────────


    // ─────────────────────────────────────────────
    // DRAW REGION
    // Nhận tọa độ LOGICAL, KHÔNG nhân zoom ở đây
    // ─────────────────────────────────────────────
    public void drawRegion(Image arg0, int x0, int y0, int w0, int h0,
                           int arg5, int x, int y, int arg8)
    {
        if (arg0 == null) return;
        _drawRegion(arg0, x0, y0, w0, h0, arg5, x, y, arg8);
    }

    public void drawRegion(Image arg0, int x0, int y0, int w0, int h0,
                           int arg5, float x, float y, int arg8)
    {
        if (arg0 == null) return;
        __drawRegion(arg0, x0, y0, w0, h0, arg5, x, y, arg8);
    }

    public void drawRegion(Image arg0, int x0, int y0, int w0, int h0,
                           int arg5, int x, int y, int arg8, bool isClip)
    {
        drawRegion(arg0, x0, y0, w0, h0, arg5, x, y, arg8);
    }

    // ─────────────────────────────────────────────
    // _drawRegion (int x,y)
    //
    // Quy tắc zoom:
    //   w,h   = logical (16 với ảnh res/x2 32px)
    //   src   = bitmap pixel = logical * zoomLevel → (0,0,32,32)
    //   dst   = màn hình pixel = logical * zoomLevel → (dx,dy,dx+32,dy+32)
    //   DrawBitmap 32→32: 1:1, không scale, nét hoàn hảo
    // ─────────────────────────────────────────────
    public void _drawRegion(Image image, float x0, float y0, int w, int h,
                            int transform, int x, int y, int anchor)
    {
        if (canvas == null || image == null) return;
        var skImage = image.GetSkImage();
        if (skImage == null) return;

        int dx = x * zoomLevel + translateX;
        int dy = y * zoomLevel + translateY;
        int dw = w * zoomLevel;
        int dh = h * zoomLevel;

        if ((anchor & HCENTER) == HCENTER) dx -= dw / 2;
        if ((anchor & VCENTER) == VCENTER) dy -= dh / 2;
        if ((anchor & RIGHT) == RIGHT) dx -= dw;
        if ((anchor & BOTTOM) == BOTTOM) dy -= dh;

        // src = vùng pixel trong bitmap (logical * zoom)
        var src = new SKRect(
            x0 * zoomLevel,
            y0 * zoomLevel,
            (x0 + w) * zoomLevel,
            (y0 + h) * zoomLevel);

        var dst = new SKRect(dx, dy, dx + dw, dy + dh);

        canvas.Save();
        if (isClip) canvas.ClipRect(GetClipSKRect());
        if (transform != 0) ApplyTransform(transform, dx + dw / 2f, dy + dh / 2f);
        _paint.Style = SKPaintStyle.Fill;
        canvas.DrawImage(skImage, src, dst, SKSamplingOptions.Default, _paint);
        canvas.Restore();
    }

    // ─────────────────────────────────────────────
    // __drawRegion (float x,y)
    // ─────────────────────────────────────────────
    public void __drawRegion(Image image, int x0, int y0, int w, int h,
                             int transform, float x, float y, int anchor)
    {
        if (canvas == null || image == null) return;
        var skImage = image.GetSkImage();
        if (skImage == null) return;

        float dx = x * zoomLevel + translateX;
        float dy = y * zoomLevel + translateY;
        float dw = w * zoomLevel;
        float dh = h * zoomLevel;

        if ((anchor & HCENTER) == HCENTER) dx -= dw / 2f;
        if ((anchor & VCENTER) == VCENTER) dy -= dh / 2f;
        if ((anchor & RIGHT) == RIGHT) dx -= dw;
        if ((anchor & BOTTOM) == BOTTOM) dy -= dh;

        // src = vùng pixel trong bitmap (logical * zoom)
        var src = new SKRect(
            x0 * zoomLevel,
            y0 * zoomLevel,
            (x0 + w) * zoomLevel,
            (y0 + h) * zoomLevel);

        var dst = new SKRect(dx, dy, dx + dw, dy + dh);

        canvas.Save();
        if (isClip) canvas.ClipRect(GetClipSKRect());
        if (transform != 0) ApplyTransform(transform, dx + dw / 2f, dy + dh / 2f);
        _paint.Style = SKPaintStyle.Fill;
        canvas.DrawImage(skImage, src, dst, SKSamplingOptions.Default, _paint);
        canvas.Restore();
    }

    // ─────────────────────────────────────────────
    // DRAW IMAGE helpers
    // ─────────────────────────────────────────────
    public void drawImage(Image image, int x, int y, int anchor)
    {
        if (image != null)
            drawRegion(image, 0, 0, getImageWidth(image), getImageHeight(image), 0, x, y, anchor);
    }

    public void drawImage(Image image, int x, int y)
    {
        if (image != null)
            drawRegion(image, 0, 0, getImageWidth(image), getImageHeight(image), 0, x, y, TOP | LEFT);
    }

    public void drawImage(Image image, float x, float y, int anchor)
    {
        if (image != null)
            drawRegion(image, 0, 0, getImageWidth(image), getImageHeight(image), 0, x, y, anchor);
    }

    // ─────────────────────────────────────────────
    // TRANSFORM
    // ─────────────────────────────────────────────
    private void ApplyTransform(int transform, float cx, float cy)
    {
        switch (transform)
        {
            case 2: canvas.Scale(-1, 1, cx, cy); break;
            case 1: canvas.Scale(1, -1, cx, cy); break;
            case 3: canvas.Scale(-1, -1, cx, cy); break;
            case 5: canvas.RotateDegrees(90, cx, cy); break;
            case 6: canvas.RotateDegrees(270, cx, cy); break;
            case 4: canvas.RotateDegrees(270, cx, cy); canvas.Scale(-1, 1, cx, cy); break;
            case 7: canvas.RotateDegrees(90, cx, cy); canvas.Scale(-1, 1, cx, cy); break;
        }
    }

    // ─────────────────────────────────────────────
    // STATIC HELPERS
    // ─────────────────────────────────────────────
    public static int getImageWidth(Image image) => image.getWidth();
    public static int getImageHeight(Image image) => image.getHeight();

    public static Color setColorObj(int rgb) =>
        new Color(((rgb >> 16) & 0xFF) / 256f,
                  ((rgb >> 8) & 0xFF) / 256f,
                  (rgb & 0xFF) / 256f);

    public static int blendColor(float level, int color, int colorBlend)
    {
        Color c1 = setColorObj(colorBlend);
        Color c2 = setColorObj(color);
        float nr = MathF.Max(0, MathF.Min(255, (c1.r * 255f + c2.r) * level + c2.r));
        float ng = MathF.Max(0, MathF.Min(255, (c1.g * 255f + c2.g) * level + c2.g));
        float nb = MathF.Max(0, MathF.Min(255, (c1.b * 255f + c2.b) * level + c2.b));
        return ((int)nr << 16) | ((int)ng << 8) | (int)nb;
    }

    
    // ─────────────────────────────────────────────
    // RESET
    // ─────────────────────────────────────────────
    public void reset()
    {
        isClip = false;
        isTranslate = false;
        translateX = 0;
        translateY = 0;
        translateXf = 0;
        translateYf = 0;
    }


    internal void drawRegion(Small img, int p1, int p2, int p3, int p4,
                             int transform, int x, int y, int anchor)
    {
        throw new NotImplementedException();
    }
}