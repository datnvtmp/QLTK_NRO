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

    public static Hashtable cachedTextures = new Hashtable();
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

    public void translate(float x, float y)
    {
        translateXf += x;
        translateYf += y;
        isTranslate = translateXf != 0f || translateYf != 0f;
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

    public void setColor(Color color)
    {
        _r = color.r; _g = color.g; _b = color.b; _a = color.a;
    }

    public void setColor(int rgb, float alpha)
    {
        setColor(rgb);
        _a = alpha;
    }

    public void setBgColor(int rgb)
    {
        currentBGColor = rgb;
        setColor(rgb);
    }

    public Color setColorMiniMap(int rgb) =>
        new Color(((rgb >> 16) & 0xFF) / 256f,
                  ((rgb >> 8) & 0xFF) / 256f,
                  (rgb & 0xFF) / 256f);

    public float[] getRGB(Color cl) =>
        new float[] { cl.r * 256f, cl.g * 256f, cl.b * 256f };

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
    public void drawString(string s, int x, int y, GUIStyle style)
    {
        if (canvas == null) return;
        _textPaint.Color = style.normal.textColor.ToSKColor();
        _textPaint.TextSize = 12 * zoomLevel;
        if (isClip) { canvas.Save(); canvas.ClipRect(GetClipSKRect()); }
        canvas.DrawText(s, x, y + _textPaint.TextSize, _textPaint);
        if (isClip) canvas.Restore();
    }

    public void drawString(string s, int x, int y, GUIStyle style, int w) =>
        drawString(s, x, y, style);

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
    // drawRegion2 (color blend)
    // ─────────────────────────────────────────────
    public void drawRegion2(Image image, float x0, float y0, int w, int h,
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

        var src = new SKRect(
            x0 * zoomLevel,
            y0 * zoomLevel,
            (x0 + w) * zoomLevel,
            (y0 + h) * zoomLevel);

        var dst = new SKRect(dx, dy, dx + dw, dy + dh);

        var blend = image.colorBlend;
        if (blend.r != 1f || blend.g != 1f || blend.b != 1f)
        {
            float[] m = {
                blend.r, 0, 0, 0, 0,
                0, blend.g, 0, 0, 0,
                0, 0, blend.b, 0, 0,
                0, 0, 0, blend.a, 0
            };
            _paint.ColorFilter = SKColorFilter.CreateColorMatrix(m);
        }

        canvas.Save();
        if (isClip) canvas.ClipRect(GetClipSKRect());
        if (transform != 0) ApplyTransform(transform, dx + dw / 2f, dy + dh / 2f);
        canvas.DrawImage(skImage, src, dst, SKSamplingOptions.Default, _paint);
        canvas.Restore();
        _paint.ColorFilter = null;
    }

    public void drawRegionGui(Image image, float x0, float y0, int w, int h,
                              int transform, float x, float y, int anchor)
    { }

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

    public void drawImageFog(Image image, int x, int y, int anchor)
    {
        if (image != null)
            drawRegion(image, 0, 0, image.w / zoomLevel, image.h / zoomLevel, 0, x, y, anchor);
    }

    public void drawImagaByDrawTexture(Image image, float x, float y)
    {
        if (canvas == null || image == null) return;
        var skImage = image.GetSkImage();
        if (skImage == null) return;
        float px = x * zoomLevel + translateX;
        float py = y * zoomLevel + translateY;
        canvas.DrawImage(skImage, px, py, _paint);
    }

    public void drawImageScale(Image image, int x, int y, int w, int h, int transform)
    {
        if (canvas == null || image?.bitmap == null) return;
        int px = x * zoomLevel + translateX;
        int py = y * zoomLevel + translateY;
        int pw = w * zoomLevel;
        int ph = h * zoomLevel;
        var src = new SKRect(0, 0, image.bitmap.Width, image.bitmap.Height);
        var dst = new SKRect(px, py, px + pw, py + ph);
        canvas.Save();
        if (transform != 0) canvas.Scale(-1, 1, px + pw / 2f, py + ph / 2f);
        canvas.DrawImage(image.GetSkImage(), src, dst, SKSamplingOptions.Default, _paint);
        canvas.Restore();
    }

    public void drawImageSimple(Image image, int x, int y)
    {
        if (canvas == null || image?.bitmap == null) return;
        canvas.DrawImage(image.GetSkImage(), x * zoomLevel, y * zoomLevel, _paint);
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
    // GL LINES / FILL TRANS
    // ─────────────────────────────────────────────
    public void fillTrans(Image imgTrans, int x, int y, int w, int h)
    {
        setColor(0, 0.5f);
        fillRect(x, y, w, h);
    }

    public void fillArg(int i, int j, int k, int l, int m, int n) =>
        fillRect(i, j, k, l);

    public void drawlineGL(MyVector totalLine)
    {
        if (canvas == null) return;
        _paint.Style = SKPaintStyle.Stroke;
        _paint.StrokeWidth = 1;
        for (int i = 0; i < totalLine.size(); i++)
        {
            mLine line = (mLine)totalLine.elementAt(i);
            _paint.Color = new SKColor(
                (byte)(line.r * 255), (byte)(line.g * 255),
                (byte)(line.b * 255), (byte)(line.a * 255));
            int x1 = line.x1 * zoomLevel + (isTranslate ? translateX : 0);
            int y1 = line.y1 * zoomLevel + (isTranslate ? translateY : 0);
            int x2 = line.x2 * zoomLevel + (isTranslate ? translateX : 0);
            int y2 = line.y2 * zoomLevel + (isTranslate ? translateY : 0);
            canvas.DrawLine(x1, y1, x2, y2, _paint);
        }
        totalLine.removeAllElements();
    }

    public void drawLine(mGraphics g, int x, int y, int xTo, int yTo, int nLine, int color)
    {
        setColor(color);
        _paint.StrokeWidth = nLine;
        drawLine(x, y, xTo, yTo);
    }

    // ─────────────────────────────────────────────
    // STATIC HELPERS
    // ─────────────────────────────────────────────
    public static int getImageWidth(Image image) => image.getWidth();
    public static int getImageHeight(Image image) => image.getHeight();
    public static int getRealImageWidth(Image img) => img.w;
    public static int getRealImageHeight(Image img) => img.h;

    public static bool isNotTranColor(Color color)
    {
        if (color.a == 0) return false;
        return !(color.r == transParentColor.r && color.g == transParentColor.g &&
                 color.b == transParentColor.b && color.a == transParentColor.a);
    }

    public static Color setColorObj(int rgb) =>
        new Color(((rgb >> 16) & 0xFF) / 256f,
                  ((rgb >> 8) & 0xFF) / 256f,
                  (rgb & 0xFF) / 256f);

    public static int getIntByColor(Color cl) =>
        (((int)(cl.r * 255) & 0xFF) << 16) |
        (((int)(cl.g * 255) & 0xFF) << 8) |
        ((int)(cl.b * 255) & 0xFF);

    public static int blendColor(float level, int color, int colorBlend)
    {
        Color c1 = setColorObj(colorBlend);
        Color c2 = setColorObj(color);
        float nr = MathF.Max(0, MathF.Min(255, (c1.r * 255f + c2.r) * level + c2.r));
        float ng = MathF.Max(0, MathF.Min(255, (c1.g * 255f + c2.g) * level + c2.g));
        float nb = MathF.Max(0, MathF.Min(255, (c1.b * 255f + c2.b) * level + c2.b));
        return ((int)nr << 16) | ((int)ng << 8) | (int)nb;
    }

    public static Image blend(Image img0, float level, int rgb)
    {
        float tr = ((rgb >> 16) & 0xFF) / 256f;
        float tg = ((rgb >> 8) & 0xFF) / 256f;
        float tb = (rgb & 0xFF) / 256f;
        int w = img0.w, h = img0.h;
        var result = Image.createImage(w, h);
        for (int py = 0; py < h; py++)
            for (int px = 0; px < w; px++)
            {
                var c = img0.bitmap.GetPixel(px, py);
                if (c.Alpha > 0)
                {
                    float nr = MathF.Max(0, MathF.Min(1, (tr - c.Red / 255f) * level + c.Red / 255f));
                    float ng = MathF.Max(0, MathF.Min(1, (tg - c.Green / 255f) * level + c.Green / 255f));
                    float nb = MathF.Max(0, MathF.Min(1, (tb - c.Blue / 255f) * level + c.Blue / 255f));
                    result.bitmap.SetPixel(px, py, new SKColor(
                        (byte)(nr * 255), (byte)(ng * 255), (byte)(nb * 255), c.Alpha));
                }
            }
        Cout.LogError2("BLEND");
        return result;
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

    public Rect intersectRect(Rect r1, Rect r2)
    {
        float x = MathF.Max(r1.x, r2.x);
        float y = MathF.Max(r1.y, r2.y);
        float xMax = MathF.Min(r1.xMax, r2.xMax);
        float yMax = MathF.Min(r1.yMax, r2.yMax);
        return new Rect(x, y, xMax - x, yMax - y);
    }

    public void CreateLineMaterial() { }

    private void UpdatePos(int anchor) { }

    internal void drawRegion(Small img, int p1, int p2, int p3, int p4,
                             int transform, int x, int y, int anchor)
    {
        throw new NotImplementedException();
    }
}