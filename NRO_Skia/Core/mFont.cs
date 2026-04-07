using System;
using System.Collections;
using SkiaSharp;

public class mFont
{
    public static int LEFT = 0;
    public static int RIGHT = 1;
    public static int CENTER = 2;

    public static int RED = 0;
    public static int YELLOW = 1;
    public static int GREEN = 2;
    public static int FATAL = 3;
    public static int MISS = 4;
    public static int ORANGE = 5;
    public static int ADDMONEY = 6;
    public static int MISS_ME = 7;
    public static int FATAL_ME = 8;
    public static int HP = 9;
    public static int MP = 10;

    private int space;
    private Image imgFont;
    private string strFont;
    private int[][] fImages;
    public static int yAddFont;

    public static int[] colorJava = new int[31]
    {
        0, 16711680, 6520319, 16777215, 16755200, 5449989, 21285, 52224, 7386228, 16771788,
        0, 65535, 21285, 16776960, 5592405, 16742263, 33023, 8701737, 15723503, 7999781,
        16768815, 14961237, 4124899, 4671303, 16096312, 16711680, 16755200, 52224, 16777215, 6520319,
        16096312
    };

    public static mFont gI;
    public static mFont tahoma_7b_red;
    public static mFont tahoma_7b_blue;
    public static mFont tahoma_7b_white;
    public static mFont tahoma_7b_yellow;
    public static mFont tahoma_7b_yellowSmall;
    public static mFont tahoma_7b_dark;
    public static mFont tahoma_7b_green2;
    public static mFont tahoma_7b_green;
    public static mFont tahoma_7b_focus;
    public static mFont tahoma_7b_unfocus;
    public static mFont tahoma_7;
    public static mFont tahoma_7_blue1;
    public static mFont tahoma_7_blue1Small;
    public static mFont tahoma_7_green2;
    public static mFont tahoma_7_yellow;
    public static mFont tahoma_7_grey;
    public static mFont tahoma_7_red;
    public static mFont tahoma_7_blue;
    public static mFont tahoma_7_green;
    public static mFont tahoma_7_white;
    public static mFont tahoma_8b;
    public static mFont number_yellow;
    public static mFont number_red;
    public static mFont number_green;
    public static mFont number_gray;
    public static mFont number_orange;
    public static mFont bigNumber_red;
    public static mFont bigNumber_While;
    public static mFont bigNumber_yellow;
    public static mFont bigNumber_green;
    public static mFont bigNumber_orange;
    public static mFont bigNumber_blue;
    public static mFont bigNumber_black;
    public static mFont nameFontRed;
    public static mFont nameFontYellow;
    public static mFont nameFontGreen;
    public static mFont tahoma_7_greySmall;
    public static mFont tahoma_7b_yellowSmall2;
    public static mFont tahoma_7b_green2Small;
    public static mFont tahoma_7_whiteSmall;
    public static mFont tahoma_7b_greenSmall;

    // Font Unity (chỉ dùng khi zoom=1, để tương thích)
    public Font myFont;

    private int height;
    private int wO;

    public Color color1 = Color.white;
    public Color color2 = Color.gray;
    public sbyte id;
    public int fstyle;

    public string st1 = "áàảãạăắằẳẵặâấầẩẫậéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵđÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬÉÈẺẼẸÊẾỀỂỄỆÍÌỈĨỊÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÚÙỦŨỤƯỨỪỬỮỰÝỲỶỸỴĐ";
    public string st2 = "\u00b8µ¶·¹\u00a8¾»¼½Æ©ÊÇÈÉËÐÌÎÏÑªÕÒÓÔÖÝ×ØÜÞãßáâä«èåæçé¬íêëìîóïñòô\u00adøõö÷ùýúûüþ®\u00b8µ¶·¹¡¾»¼½Æ¢ÊÇÈÉËÐÌÎÏÑ£ÕÒÓÔÖÝ×ØÜÞãßáâä¤èåæçé¥íêëìîóïñòô¦øõö÷ùýúûüþ§";
    public const string str = " 0123456789+-*='_?.,<>/[]{}!@#$%^&*():aáàảãạâấầẩẫậăắằẳẵặbcdđeéèẻẽẹêếềểễệfghiíìỉĩịjklmnoóòỏõọôốồổỗộơớờởỡợpqrstuúùủũụưứừửữựvxyýỳỷỹỵzwAÁÀẢÃẠĂẰẮẲẴẶÂẤẦẨẪẬBCDĐEÉÈẺẼẸÊẾỀỂỄỆFGHIÍÌỈĨỊJKLMNOÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢPQRSTUÚÙỦŨỤƯỨỪỬỮỰVXYÝỲỶỸỴZW";

    private int yAdd;
    private string pathImage;

    public static sbyte cl__white;
    public static sbyte cl__yellow;
    public static sbyte cl__yellowSmall;
    public static sbyte cl_dark = 0;
    public static sbyte cl_green = 1;
    public static sbyte cl__blue = 2;
    public static sbyte cl_red = 3;

    // ─────────────────────────────────────────────
    // CONSTRUCTOR — bitmap font (zoom=1)
    // ─────────────────────────────────────────────
    public mFont(string strFont, string pathImage, string pathData, int space)
    {
        try
        {
            this.strFont = strFont;
            this.space = space;
            this.pathImage = pathImage;
            DataInputStream dataInputStream = null;
            reloadImage();
            try
            {
                dataInputStream = MyStream.readFile(pathData);
                fImages = new int[dataInputStream.readShort()][];
                for (int i = 0; i < fImages.Length; i++)
                {
                    fImages[i] = new int[4];
                    fImages[i][0] = dataInputStream.readShort();
                    fImages[i][1] = dataInputStream.readShort();
                    fImages[i][2] = dataInputStream.readShort();
                    fImages[i][3] = dataInputStream.readShort();
                    setHeight(fImages[i][3]);
                }
                dataInputStream.close();
            }
            catch (Exception)
            {
                try { dataInputStream?.close(); } catch { }
            }
        }
        catch (Exception ex) { ex.StackTrace?.ToString(); }
    }

    // ─────────────────────────────────────────────
    // CONSTRUCTOR — SkiaSharp font (zoom>=2)
    // ─────────────────────────────────────────────
    public mFont(sbyte id)
    {
        string text = "chelthm";
        if ((id > 0 && id < 10) || id == 19)
        {
            yAdd = 1;
            text = "barmeneb";
        }
        else if (id >= 10 && id <= 18)
        {
            text = "chelthm";
            yAdd = 2;
        }
        else if (id > 24)
        {
            text = "staccato";
        }
        this.id = id;
        myFont = null; // không dùng Unity Font nữa

        if (id < 25)
        {
            color1 = setColorFont(id);
            color2 = setColorFont(id);
        }
        else
        {
            color1 = bigColor(id);
            color2 = bigColor(id);
        }
        // wO dùng để tính width xấp xỉ
        wO = getWidthExactOf("o");
    }

    // ─────────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────────
    public static void init()
    {
        if (mGraphics.zoomLevel == 1)
        {
            // Dùng bitmap font
            tahoma_7b_red = new mFont(str, "/myfont/tahoma_7b_red.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_blue = new mFont(str, "/myfont/tahoma_7b_blue.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_white = new mFont(str, "/myfont/tahoma_7b_white.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_yellow = new mFont(str, "/myfont/tahoma_7b_yellow.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_yellowSmall = new mFont(str, "/myfont/tahoma_7b_yellow.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_dark = new mFont(str, "/myfont/tahoma_7b_brown.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_green2 = new mFont(str, "/myfont/tahoma_7b_green2.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_green = new mFont(str, "/myfont/tahoma_7b_green.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_focus = new mFont(str, "/myfont/tahoma_7b_focus.png", "/myfont/tahoma_7b", 0);
            tahoma_7b_unfocus = new mFont(str, "/myfont/tahoma_7b_unfocus.png", "/myfont/tahoma_7b", 0);
            tahoma_7 = new mFont(str, "/myfont/tahoma_7.png", "/myfont/tahoma_7", 0);
            tahoma_7_blue1 = new mFont(str, "/myfont/tahoma_7_blue1.png", "/myfont/tahoma_7", 0);
            tahoma_7_green2 = new mFont(str, "/myfont/tahoma_7_green2.png", "/myfont/tahoma_7", 0);
            tahoma_7_yellow = new mFont(str, "/myfont/tahoma_7_yellow.png", "/myfont/tahoma_7", 0);
            tahoma_7_grey = new mFont(str, "/myfont/tahoma_7_grey.png", "/myfont/tahoma_7", 0);
            tahoma_7_red = new mFont(str, "/myfont/tahoma_7_red.png", "/myfont/tahoma_7", 0);
            tahoma_7_blue = new mFont(str, "/myfont/tahoma_7_blue.png", "/myfont/tahoma_7", 0);
            tahoma_7_green = new mFont(str, "/myfont/tahoma_7_green.png", "/myfont/tahoma_7", 0);
            tahoma_7_white = new mFont(str, "/myfont/tahoma_7_white.png", "/myfont/tahoma_7", 0);
            tahoma_8b = new mFont(str, "/myfont/tahoma_8b.png", "/myfont/tahoma_8b", -1);
            number_yellow = new mFont(" 0123456789+-", "/myfont/number_yellow.png", "/myfont/number", 0);
            number_red = new mFont(" 0123456789+-", "/myfont/number_red.png", "/myfont/number", 0);
            number_green = new mFont(" 0123456789+-", "/myfont/number_green.png", "/myfont/number", 0);
            number_gray = new mFont(" 0123456789+-", "/myfont/number_gray.png", "/myfont/number", 0);
            number_orange = new mFont(" 0123456789+-", "/myfont/number_orange.png", "/myfont/number", 0);
            bigNumber_red = number_red;
            bigNumber_While = tahoma_7b_white;
            bigNumber_yellow = number_yellow;
            bigNumber_green = number_green;
            bigNumber_orange = number_orange;
            bigNumber_blue = tahoma_7_blue1;
            nameFontRed = tahoma_7_red;
            nameFontYellow = tahoma_7_yellow;
            nameFontGreen = tahoma_7_green;
            tahoma_7_greySmall = tahoma_7_grey;
            tahoma_7b_yellowSmall2 = tahoma_7_yellow;
            tahoma_7b_green2Small = tahoma_7b_green2;
            tahoma_7_whiteSmall = tahoma_7_white;
            tahoma_7b_greenSmall = tahoma_7b_green;
            tahoma_7_blue1Small = tahoma_7_blue1;
            return;
        }

        // zoom >= 2: dùng SkiaSharp font
        gI = new mFont(0);
        tahoma_7b_red = new mFont(1);
        tahoma_7b_blue = new mFont(2);
        tahoma_7b_white = new mFont(3);
        tahoma_7b_yellow = new mFont(4);
        tahoma_7b_yellowSmall = new mFont(4);
        tahoma_7b_dark = new mFont(5);
        tahoma_7b_green2 = new mFont(6);
        tahoma_7b_green = new mFont(7);
        tahoma_7b_focus = new mFont(8);
        tahoma_7b_unfocus = new mFont(9);
        tahoma_7 = new mFont(10);
        tahoma_7_blue1 = new mFont(11);
        tahoma_7_blue1Small = tahoma_7_blue1;
        tahoma_7_green2 = new mFont(12);
        tahoma_7_yellow = new mFont(13);
        tahoma_7_grey = new mFont(14);
        tahoma_7_red = new mFont(15);
        tahoma_7_blue = new mFont(16);
        tahoma_7_green = new mFont(17);
        tahoma_7_white = new mFont(18);
        tahoma_8b = new mFont(19);
        number_yellow = new mFont(20);
        number_red = new mFont(21);
        number_green = new mFont(22);
        number_gray = new mFont(23);
        number_orange = new mFont(24);
        bigNumber_red = new mFont(25);
        bigNumber_yellow = new mFont(26);
        bigNumber_green = new mFont(27);
        bigNumber_While = new mFont(28);
        bigNumber_blue = new mFont(29);
        bigNumber_orange = new mFont(30);
        bigNumber_black = new mFont(31);
        nameFontRed = tahoma_7b_red;
        nameFontYellow = tahoma_7_yellow;
        nameFontGreen = tahoma_7_green;
        tahoma_7_greySmall = tahoma_7_grey;
        tahoma_7b_yellowSmall2 = tahoma_7_yellow;
        tahoma_7b_green2Small = tahoma_7b_green2;
        tahoma_7_whiteSmall = tahoma_7_white;
        tahoma_7b_greenSmall = tahoma_7b_green;
        yAddFont = 1;
        if (mGraphics.zoomLevel == 1) yAddFont = -3;
    }

    // ─────────────────────────────────────────────
    // DRAW STRING  — điểm vào chính
    // ─────────────────────────────────────────────
    public void drawString(mGraphics g, string st, int x, int y, int align)
    {
        if (mGraphics.zoomLevel == 1)
            DrawBitmapString(g, st, x, y, align);
        else
            setTypePaint(g, st, x, y, align, 0);
    }

    public void drawString(mGraphics g, string st, int x, int y, int align, mFont font)
    {
        if (mGraphics.zoomLevel == 1)
            DrawBitmapStringBorder(g, st, x, y, align, font);
        else
        {
            setTypePaint(g, st, x, y + 1, align, font?.id ?? 0);
            setTypePaint(g, st, x, y, align, 0);
        }
    }

    public void drawStringBorder(mGraphics g, string st, int x, int y, int align)
    {
        if (mGraphics.zoomLevel == 1)
            DrawBitmapString(g, st, x, y, align);
        else
            setTypePaint(g, st, x, y, align, 0);
    }

    public void drawStringBorder(mGraphics g, string st, int x, int y, int align, mFont font2)
    {
        if (mGraphics.zoomLevel == 1)
            DrawBitmapStringBorder(g, st, x, y, align, font2);
        else
            drawStringBd(g, st, x, y, align, font2);
    }

    public void drawStringBd(mGraphics g, string st, int x, int y, int align, mFont font)
    {
        setTypePaint(g, st, x - 1, y - 1, align, 0);
        setTypePaint(g, st, x - 1, y + 1, align, 0);
        setTypePaint(g, st, x + 1, y - 1, align, 0);
        setTypePaint(g, st, x + 1, y + 1, align, 0);
        setTypePaint(g, st, x, y - 1, align, 0);
        setTypePaint(g, st, x, y + 1, align, 0);
        setTypePaint(g, st, x + 1, y, align, 0);
        setTypePaint(g, st, x - 1, y, align, 0);
        setTypePaint(g, st, x, y, align, 0);
    }

    // ─────────────────────────────────────────────
    // BITMAP FONT (zoom=1)
    // ─────────────────────────────────────────────
    private void DrawBitmapString(mGraphics g, string st, int x, int y, int align)
    {
        int length = st.Length;
        int cur = align switch
        {
            0 => x,
            1 => x - getWidth(st),
            _ => x - (getWidth(st) >> 1),
        };
        for (int i = 0; i < length; i++)
        {
            int idx = strFont.IndexOf(st[i] + string.Empty);
            if (idx == -1) idx = 0;
            if (idx > -1)
            {
                int x2 = fImages[idx][0];
                int y2 = fImages[idx][1];
                int w = fImages[idx][2];
                int h = fImages[idx][3];
                if (y2 + h > imgFont.bitmap.Height)
                {
                    y2 -= imgFont.bitmap.Height;
                    x2 = imgFont.bitmap.Width / 2;
                }
                g.drawRegion(imgFont, x2, y2, w, h, 0, cur, y, 20);
            }
            cur += fImages[idx][2] + space;
        }
    }

    private void DrawBitmapStringBorder(mGraphics g, string st, int x, int y, int align, mFont border)
    {
        int length = st.Length;
        int cur = align switch
        {
            0 => x,
            1 => x - getWidth(st),
            _ => x - (getWidth(st) >> 1),
        };
        for (int i = 0; i < length; i++)
        {
            int idx = strFont.IndexOf(st[i]);
            if (idx == -1) idx = 0;
            if (idx > -1)
            {
                int x2 = fImages[idx][0];
                int y2 = fImages[idx][1];
                int w = fImages[idx][2];
                int h = fImages[idx][3];
                if (y2 + h > imgFont.bitmap.Height)
                {
                    y2 -= imgFont.bitmap.Height;
                    x2 = imgFont.bitmap.Width / 2;
                }
                g.drawRegion(imgFont, x2, y2, w, h, 0, cur, y, 20);
            }
            cur += fImages[idx][2] + space;
        }
    }

    // ─────────────────────────────────────────────
    // SKIA FONT (zoom>=2) — _drawString vẽ thật
    // ─────────────────────────────────────────────
    public void _drawString(mGraphics g, string st, int x0, int y0, int align)
    {
        if (mGraphics.canvas == null) return;
        y0 += yAddFont;

        // Tính pixel thật
        float textSize = GetFontSize();
        float px = x0 * mGraphics.zoomLevel + g.translateX;
        float py = y0 * mGraphics.zoomLevel + g.translateY;

        using var paint = new SKPaint
        {
            TextSize = textSize,
            Color = color1.ToSKColor(),
            IsAntialias = true,
        };

        float tw = paint.MeasureText(st);
        float drawX = align switch
        {
            0 => px,            // LEFT
            1 => px - tw,       // RIGHT
            _ => px - tw / 2f,  // CENTER
        };

        mGraphics.canvas.DrawText(st, drawX, py + textSize, paint);
    }

    public void setTypePaint(mGraphics g, string st, int x, int y, int align, sbyte idFont)
    {
        sbyte useId = id;
        if (idFont > 0) useId = idFont;
        x--;

        if (id > 24)
        {
            Color[] shadowColors = new Color[6]
            {
                setColor(6029312), setColor(7169025), setColor(7680),
                setColor(0), setColor(9264), setColor(6029312)
            };
            color1 = shadowColors[id - 25];
            color2 = shadowColors[id - 25];
            _drawString(g, st, x + 1, y, align);
            _drawString(g, st, x - 1, y, align);
            _drawString(g, st, x, y - 1, align);
            _drawString(g, st, x, y + 1, align);
            _drawString(g, st, x + 1, y + 1, align);
            _drawString(g, st, x + 1, y - 1, align);
            _drawString(g, st, x - 1, y - 1, align);
            _drawString(g, st, x - 1, y + 1, align);
            color1 = bigColor(id);
            color2 = bigColor(id);
        }
        else
        {
            setColorByID(useId);
        }
        _drawString(g, st, x, y - yAdd, align);
    }

    // ─────────────────────────────────────────────
    // WIDTH / HEIGHT
    // ─────────────────────────────────────────────
    public int getWidth(string s)
    {
        if (mGraphics.zoomLevel == 1)
        {
            int num = 0;
            for (int i = 0; i < s.Length; i++)
            {
                int idx = strFont?.IndexOf(s[i]) ?? 0;
                if (idx == -1) idx = 0;
                num += fImages[idx][2] + space;
            }
            return num;
        }
        return getWidthExactOf(s);
    }

    public int getWidthExactOf(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        try
        {
            using var paint = new SKPaint { TextSize = GetFontSize() };
            return (int)(paint.MeasureText(s) / mGraphics.zoomLevel);
        }
        catch
        {
            return getWidthNotExactOf(s);
        }
    }

    public int getWidthNotExactOf(string s) =>
        s.Length * wO / Math.Max(1, mGraphics.zoomLevel);

    public int getHeight()
    {
        if (mGraphics.zoomLevel == 1) return height;
        if (height > 0) return height / mGraphics.zoomLevel;
        height = (int)GetFontSize() + 2;
        return height / mGraphics.zoomLevel;
    }

    // font size tính theo zoom (base = 7px tương đương gốc J2ME)
    private float GetFontSize()
    {
        float baseSize = (id > 24) ? 10f : 7f; // bigNumber to hơn
        return baseSize * mGraphics.zoomLevel;
    }

    // ─────────────────────────────────────────────
    // COLOR HELPERS
    // ─────────────────────────────────────────────
    public void setHeight(int height) => this.height = height;

    public Color setColor(int rgb)
    {
        int num = rgb & 0xFF;
        int num2 = (rgb >> 8) & 0xFF;
        int num3 = (rgb >> 16) & 0xFF;
        return new Color(num3 / 256f, num2 / 256f, num / 256f);
    }

    public Color bigColor(int id)
    {
        Color[] array = new Color[7]
        {
            Color.red, Color.yellow, Color.green, Color.white,
            setColor(40404), Color.red, Color.black
        };
        return array[id - 25];
    }

    public void setColorByID(int ID)
    {
        color1 = setColor(colorJava[ID]);
        color2 = setColor(colorJava[ID]);
    }

    public Color setColorFont(sbyte id) => setColor(colorJava[id]);

    // ─────────────────────────────────────────────
    // SPLIT HELPERS (không đổi)
    // ─────────────────────────────────────────────
    public MyVector splitFontVector(string src, int lineWidth)
    {
        MyVector myVector = new MyVector();
        string text = string.Empty;
        for (int i = 0; i < src.Length; i++)
        {
            if (src[i] == '\n' || src[i] == '\b')
            {
                myVector.addElement(text);
                text = string.Empty;
                continue;
            }
            text += src[i];
            if (getWidth(text) > lineWidth)
            {
                int num = text.Length - 1;
                while (num >= 0 && text[num] != ' ') num--;
                if (num < 0) num = text.Length - 1;
                myVector.addElement(text.Substring(0, num));
                i = i - (text.Length - num) + 1;
                text = string.Empty;
            }
            if (i == src.Length - 1 && !text.Trim().Equals(string.Empty))
                myVector.addElement(text);
        }
        return myVector;
    }

    public string[] splitFontArray(string src, int lineWidth)
    {
        MyVector myVector = splitFontVector(src, lineWidth);
        string[] array = new string[myVector.size()];
        for (int i = 0; i < myVector.size(); i++)
            array[i] = (string)myVector.elementAt(i);
        return array;
    }

    public string[] splitStrInLine(string src, int lineWidth)
    {
        ArrayList list = splitStrInLineA(src, lineWidth);
        string[] array = new string[list.Count];
        for (int i = 0; i < list.Count; i++)
            array[i] = (string)list[i];
        return array;
    }

    public ArrayList splitStrInLineA(string src, int lineWidth)
    {
        ArrayList arrayList = new ArrayList();
        if (src.Length < 5) { arrayList.Add(src); return arrayList; }
        int i = 0, num = 0, length = src.Length;
        string text = string.Empty;
        try
        {
            while (true)
            {
                if (getWidthNotExactOf(text) < lineWidth)
                {
                    text += src[num]; num++;
                    if (src[num] != '\n' && num < length - 1) continue;
                    num = length - 1;
                }
                if (num != length - 1 && src[num + 1] != ' ')
                {
                    int num2 = num;
                    while (src[num + 1] != '\n' && (src[num + 1] != ' ' || src[num] == ' ') && num != i)
                        num--;
                    if (num == i) num = num2;
                }
                string text2 = src.Substring(i, num + 1 - i);
                if (text2[0] == '\n') text2 = text2.Substring(1);
                if (text2[text2.Length - 1] == '\n') text2 = text2.Substring(0, text2.Length - 1);
                arrayList.Add(text2);
                if (num == length - 1) break;
                for (i = num + 1; i != length - 1 && src[i] == ' '; i++) { }
                if (i == length - 1) break;
                num = i; text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            Cout.LogWarning("SPLIT ERR: " + ex.Message);
            arrayList.Add(src);
        }
        return arrayList;
    }

    public bool compare(string strSource, string str)
    {
        for (int i = 0; i < strSource.Length; i++)
            if ((string.Empty + strSource[i]).Equals(str)) return true;
        return false;
    }

    public static string[] splitStringSv(string _text, string _searchStr)
    {
        int num = 0, startIndex = 0;
        int length = _searchStr.Length;
        int num2 = _text.IndexOf(_searchStr, startIndex);
        while (num2 != -1) { startIndex = num2 + length; num2 = _text.IndexOf(_searchStr, startIndex); num++; }
        string[] array = new string[num + 1];
        int num3 = _text.IndexOf(_searchStr), num4 = 0, num5 = 0;
        while (num3 != -1)
        {
            array[num5++] = _text.Substring(num4, num3 - num4);
            num4 = num3 + length;
            num3 = _text.IndexOf(_searchStr, num4);
        }
        array[num5] = _text.Substring(num4);
        return array;
    }

    public void reloadImage()
    {
        if (mGraphics.zoomLevel == 1)
            imgFont = GameCanvas.loadImage(pathImage);
    }

    public void freeImage() { }

    public static mFont GetFont(sbyte colorCode) => colorCode switch
    {
        0 => tahoma_7b_dark,
        1 => tahoma_7b_green,
        2 => tahoma_7b_blue,
        3 => tahoma_7_red,
        4 => tahoma_7_green,
        5 => tahoma_7_blue,
        7 => tahoma_7b_red,
        _ => tahoma_7,
    };
}