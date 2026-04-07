public class EffecMn
{
    public static MyVector vEff = new MyVector();

    // Giữ add/remove vì server gọi, nhưng không update/paint nữa
    public static void addEff(Effect me)
    {
        // Bỏ qua — không thêm vào vector nữa để tiết kiệm RAM
    }

    public static void removeEff(int id)
    {
        // Không cần xóa vì không thêm nữa
    }

    public static Effect getEffById(int id)
    {
        return null;
    }

    // ── Tất cả paint ──
    public static void paintBackGroundUnderLayer(mGraphics g, int x, int y, int layer) { }
    public static void paintLayer1(mGraphics g) { }
    public static void paintLayer2(mGraphics g) { }
    public static void paintLayer3(mGraphics g) { }
    public static void paintLayer4(mGraphics g) { }

    // ── Update ──
    public static void update() { }
}