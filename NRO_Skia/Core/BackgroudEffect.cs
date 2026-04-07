using System;
using Assets.src.e;

public class BackgroudEffect
{
    public static MyVector vBgEffect = new MyVector();

    private int[] x;
    private int[] y;
    private int[] vx;
    private int[] vy;
    public static int[] wP;
    private int num;
    private int xShip;
    private int yShip;
    private int way;
    private int trans;
    private int frameFire;
    private int tFire;
    private int tStart;
    private int speed;
    private bool isFly;

    public static Image imgSnow;
    public static Image imgHatMua;
    public static Image imgMua1;
    public static Image imgMua2;
    public static Image imgSao;
    private static Image imgLacay;
    private static Image imgShip;
    private static Image imgFire1;
    private static Image imgFire2;

    private int[] type;
    private int sum;
    public int typeEff;
    public int xx;
    public int waterY;
    private bool[] isRainEffect;
    private int[] frame;
    private int[] t;
    private bool[] activeEff;
    private int yWater;
    private int colorWater;

    public const int TYPE_MUA = 0;
    public const int TYPE_LATRAIDAT_1 = 1;
    public const int TYPE_LATRAIDAT_2 = 2;
    public const int TYPE_SAMSET = 3;
    public const int TYPE_SAO = 4;
    public const int TYPE_LANAMEK_1 = 5;
    public const int TYPE_LASAYAI_1 = 6;
    public const int TYPE_LANAMEK_2 = 7;
    public const int TYPE_SHIP_TRAIDAT = 8;
    public const int TYPE_HANHTINH = 9;
    public const int TYPE_WATER = 10;
    public const int TYPE_SNOW = 11;
    public const int TYPE_MUA_FRONT = 12;
    public const int TYPE_CLOUD = 13;
    public const int TYPE_FOG = 14;
    public const int TYPE_LUNAR_YEAR = 15;

    public static int PIXEL = 16;

    // ── Bỏ load ảnh water ngay lúc khởi động ──
    public static Image water1 = null;
    public static Image water2 = null;
    public static Image imgChamTron1;
    public static Image imgChamTron2;
    public static short id_water1;
    public static short id_water2;
    public static Image water3 = null;

    public static bool isFog;
    public static bool isPaintFar;
    public static int nCloud;
    public static Image imgCloud1;
    public static Image imgFog;
    public static int cloudw;
    public static int xfog;
    public static int yfog;
    public static int fogw;

    private int[] dem = new int[6] { 0, 1, 2, 1, 0, 0 };
    private int[] tick;

    public BackgroudEffect(int typeS)
    {
        typeEff = typeS;
        // Không khởi tạo gì cả — không cần load ảnh, không cần arrays
    }

    public static void clearImage()
    {
        TileMap.yWater = 0;
    }

    // Giữ lại isHaveRain vì GameScr.switchToMe() dùng để bật sound mưa

    public static void initCloud() { }      // rỗng
    public static void updateCloud2() { }   // rỗng
    public static void updateFog() { }      // rỗng
    public static void paintCloud2(mGraphics g) { }  // rỗng

    public static void paintFog(mGraphics g) { }     // rỗng

    private void reloadShip() { }

    // ── Tất cả paint instance ──
    public void paintWater(mGraphics g) { }
    public void paintFar(mGraphics g) { }
    public void paintBehindTile(mGraphics g) { }
    public void paintBack(mGraphics g) { }
    public void paintFront(mGraphics g) { }
    public void paintLacay1(mGraphics g, Image img) { }
    public void paintLacay2(mGraphics g, Image img) { }

    // ── Update instance ──
    public void update() { }

    // ── Static helpers (giữ addEffect/addWater vì server gửi lệnh này) ──
    public static void addEffect(int id)
    {
        // Không tạo object mới nữa — server gửi lệnh nhưng ta bỏ qua
    }

    public static void addWater(int color, int yWater)
    {
        // Bỏ qua
    }

    // ── Tất cả paint static ──
    public static void paintWaterAll(mGraphics g) { }
    public static void paintBehindTileAll(mGraphics g) { }
    public static void paintFrontAll(mGraphics g) { }
    public static void paintFarAll(mGraphics g) { }
    public static void paintBackAll(mGraphics g) { }

    // ── Update static ──
    public static void updateEff() { }
}