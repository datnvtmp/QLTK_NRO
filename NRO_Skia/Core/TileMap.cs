using System;

public class TileMap
{
	public const int T_EMPTY = 0;

	public const int T_TOP = 2;

	public const int T_LEFT = 4;

	public const int T_RIGHT = 8;

	public const int T_TREE = 16;

	public const int T_WATERFALL = 32;

	public const int T_WATERFLOW = 64;

	public const int T_TOPFALL = 128;

	public const int T_OUTSIDE = 256;

	public const int T_DOWN1PIXEL = 512;

	public const int T_BRIDGE = 1024;

	public const int T_UNDERWATER = 2048;

	public const int T_SOLIDGROUND = 4096;

	public const int T_BOTTOM = 8192;

	public const int T_DIE = 16384;

	public const int T_HEBI = 32768;

	public const int T_BANG = 65536;

	public const int T_JUM8 = 131072;

	public const int T_NT0 = 262144;

	public const int T_NT1 = 524288;

	public const int T_CENTER = 1;

	public static int tmw;

	public static int tmh;

	public static int pxw;

	public static int pxh;

	public static int tileID;

	public static int lastTileID = -1;

	public static int[] maps;

	public static int[] types;

	public static Image[] imgTile;

	public static Image imgTileSmall;

	public static Image imgMiniMap;

	public static Image imgWaterfall;

	public static Image imgTopWaterfall;

	public static Image imgWaterflow;

	public static Image imgWaterlowN;

	public static Image imgWaterlowN2;

	public static Image imgWaterF;

	public static Image imgLeaf;

	public static sbyte size = 24;

	private static int bx;

	private static int dbx;

	private static int fx;

	private static int dfx;

	public static string[] instruction;

	public static int[] iX;

	public static int[] iY;

	public static int[] iW;

	public static int iCount;

	public static bool isMapDouble = false;

	public static string mapName = string.Empty;

	public static sbyte versionMap = 1;

	public static int mapID;

	public static int lastBgID = -1;

	public static int zoneID;

	public static int bgID;

	public static int bgType;

	public static int lastType = -1;

	public static int typeMap;

	public static sbyte planetID;

	public static sbyte lastPlanetId = -1;

	public static long timeTranMini;

	public static MyVector vGo = new MyVector();

	public static MyVector vItemBg = new MyVector();

	public static MyVector vCurrItem = new MyVector();

	public static string[] mapNames;

	public static sbyte MAP_NORMAL = 0;

	public static Image bong;

	public const int TRAIDAT_DOINUI = 0;

	public const int TRAIDAT_RUNG = 1;

	public const int TRAIDAT_DAORUA = 2;

	public const int TRAIDAT_DADO = 3;

	public const int NAMEK_THUNGLUNG = 5;

	public const int NAMEK_DOINUI = 4;

	public const int NAMEK_RUNG = 6;

	public const int NAMEK_DAO = 7;

	public const int SAYAI_DOINUI = 8;

	public const int SAYAI_RUNG = 9;

	public const int SAYAI_CITY = 10;

	public const int SAYAI_NIGHT = 11;

	public const int KAMISAMA = 12;

	public const int TIME_ROOM = 13;

	public const int HELL = 15;

	public const int BEERUS = 16;

	public const int THE_HELL = 19;

	public static Image[] bgItem = new Image[8];

	public static MyVector vObject = new MyVector();

	public static int[] offlineId = new int[6] { 21, 22, 23, 39, 40, 41 };

	public static int[] highterId = new int[6] { 21, 22, 23, 24, 25, 26 };

	public static int[] toOfflineId = new int[3] { 0, 7, 14 };

	public static int[][] tileType;

	public static int[][][] tileIndex;

	public static Image imgLight = GameCanvas.loadImage("/bg/light.png");

	public static int sizeMiniMap = 2;

	public static int gssx;

	public static int gssxe;

	public static int gssy;

	public static int gssye;

	public static int countx;

	public static int county;

	private static int[] colorMini = new int[2] { 5257738, 8807192 };

	public static int yWater = 0;

	public static void loadBg()
	{
		
	}

	public static bool isVoDaiMap()
	{
		if (mapID == 51 || mapID == 103 || mapID == 112 || mapID == 113 || mapID == 129 || mapID == 130)
		{
			return true;
		}
		return false;
	}

	public static bool isTrainingMap()
	{
		if (mapID == 39 || mapID == 40 || mapID == 41)
		{
			return true;
		}
		return false;
	}

	public static bool mapPhuBang()
	{
		if (GameScr.phuban_Info != null && mapID == GameScr.phuban_Info.idmapPaint)
		{
			return true;
		}
		return false;
	}

	public static BgItem getBIById(int id)
	{
		for (int i = 0; i < vItemBg.size(); i++)
		{
			BgItem bgItem = (BgItem)vItemBg.elementAt(i);
			if (bgItem.id == id)
			{
				return bgItem;
			}
		}
		return null;
	}

	public static bool isOfflineMap()
	{
		for (int i = 0; i < offlineId.Length; i++)
		{
			if (mapID == offlineId[i])
			{
				return true;
			}
		}
		return false;
	}

	public static bool isHighterMap()
	{
		for (int i = 0; i < offlineId.Length; i++)
		{
			if (mapID == highterId[i])
			{
				return true;
			}
		}
		return false;
	}

	public static bool isToOfflineMap()
	{
		for (int i = 0; i < toOfflineId.Length; i++)
		{
			if (mapID == toOfflineId[i])
			{
				return true;
			}
		}
		return false;
	}

	public static void freeTilemap()
	{
		if (imgTile != null)
		{
			for (int i = 0; i < imgTile.Length; i++)
			{
				imgTile[i]?.Dispose();
				imgTile[i] = null;
			}
		}
		imgTile = null;
		mSystem.gcc();
	}

	public static void loadTileCreatChar()
	{
	}

	public static bool isExistMoreOne(int id)
	{
		if (id == 156 || id == 330 || id == 345 || id == 334)
		{
			return false;
		}
		if (mapID == 54 || mapID == 55 || mapID == 56 || mapID == 57 || mapID == 58 || mapID == 59 || mapID == 103)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < vCurrItem.size(); i++)
		{
			BgItem bgItem = (BgItem)vCurrItem.elementAt(i);
			if (bgItem.id == id)
			{
				num++;
			}
		}
		if (num > 2)
		{
			return true;
		}
		return false;
	}

	public static void loadTileImage()
	{
		
	}

	public static void setTile(int index, int[] mapsArr, int type)
	{
		for (int i = 0; i < mapsArr.Length; i++)
		{
			if (maps[index] == mapsArr[i])
			{
				types[index] |= type;
				break;
			}
		}
	}

	public static void loadMap(int tileId)
	{
		pxh = tmh * size;
		pxw = tmw * size;
		Res.outz("load tile ID= " + tileID);
		int num = tileId - 1;
		try
		{
			for (int i = 0; i < tmw * tmh; i++)
			{
				for (int j = 0; j < tileType[num].Length; j++)
				{
					setTile(i, tileIndex[num][j], tileType[num][j]);
				}
			}
		}
		catch (Exception)
		{
			Cout.println("Error Load Map");
			GameMidlet.instance.exit();
		}
	}

	public static bool isInAirMap()
	{
		if (mapID == 45 || mapID == 46 || mapID == 48)
		{
			return true;
		}
		return false;
	}

	public static bool isDoubleMap()
	{
		if (isMapDouble || mapID == 45 || mapID == 46 || mapID == 48 || mapID == 51 || mapID == 52 || mapID == 103 || mapID == 112 || mapID == 113 || mapID == 115 || mapID == 117 || mapID == 118 || mapID == 119 || mapID == 120 || mapID == 121 || mapID == 125 || mapID == 129 || mapID == 130)
		{
			return true;
		}
		return false;
	}

	public static void getTile()
	{
        return;
    }

    public static void paintTile(mGraphics g, int frame, int indexX, int indexY)
    {
        // Thay vì vẽ ảnh, ta sẽ vẽ một ô vuông màu xám đậm (Mã màu 0x333333)
        g.setColor(0x333333);
        g.fillRect(indexX * size, indexY * size, size, size);

        // (Tùy chọn) Vẽ một đường viền mỏng màu đen để dễ phân biệt các bậc thang/khối đất
        g.setColor(0x000000);
        g.drawRect(indexX * size, indexY * size, size, size);
    }

    public static void paintTile(mGraphics g, int frame, int x, int y, int w, int h)
    {
        // Áp dụng tương tự cho các ô gạch có kích thước dị biệt
        g.setColor(0x333333);
        g.fillRect(x, y, w, h);

        g.setColor(0x000000);
        g.drawRect(x, y, w, h);
    }

    public static void paintTilemapLOW(mGraphics g)
	{
		for (int i = GameScr.gssx; i < GameScr.gssxe; i++)
		{
			for (int j = GameScr.gssy; j < GameScr.gssye; j++)
			{
				int num = maps[j * tmw + i] - 1;
				if (num != -1)
				{
					paintTile(g, num, i, j);
				}
				if ((tileTypeAt(i, j) & 0x20) == 32)
				{
					g.drawRegion(imgWaterfall, 0, 24 * (GameCanvas.gameTick % 4), 24, 24, 0, i * size, j * size, 0);
				}
				else if ((tileTypeAt(i, j) & 0x40) == 64)
				{
					if ((tileTypeAt(i, j - 1) & 0x20) == 32)
					{
						g.drawRegion(imgWaterfall, 0, 24 * (GameCanvas.gameTick % 4), 24, 24, 0, i * size, j * size, 0);
					}
					else if ((tileTypeAt(i, j - 1) & 0x1000) == 4096)
					{
						paintTile(g, 21, i, j);
					}
					Image image = null;
					image = ((tileID == 5) ? imgWaterlowN : ((tileID != 8) ? imgWaterflow : imgWaterlowN2));
					g.drawRegion(image, 0, (GameCanvas.gameTick % 8 >> 2) * 24, 24, 24, 0, i * size, j * size, 0);
				}
				if ((tileTypeAt(i, j) & 0x800) == 2048)
				{
					if ((tileTypeAt(i, j - 1) & 0x20) == 32)
					{
						g.drawRegion(imgWaterfall, 0, 24 * (GameCanvas.gameTick % 4), 24, 24, 0, i * size, j * size, 0);
					}
					else if ((tileTypeAt(i, j - 1) & 0x1000) == 4096)
					{
						paintTile(g, 21, i, j);
					}
					paintTile(g, maps[j * tmw + i] - 1, i, j);
				}
			}
		}
	}

    public static void paintTilemap(mGraphics g)
    {
        if (Char.isLoadingMap)
        {
            return;
        }

        // TẠO BỘ LỌC ĐẤT CỨNG (Bao gồm: Mặt trên, Trái, Phải, Đất lõi, Đáy, Cầu)
        // Các số này tương ứng: 2, 4, 8, 4096, 8192, 1024 trong mã nguồn của bạn
        int solidTypes = 2 | 4 | 8 | 4096 | 8192 | 1024;

        // 1. VÒNG LẶP VẼ KHỐI ĐẤT CHÍNH
        for (int j = GameScr.gssx; j < GameScr.gssxe; j++)
        {
            for (int k = GameScr.gssy; k < GameScr.gssye; k++)
            {
                if (j == 0 || j == tmw - 1) continue;

                int num = maps[k * tmw + j] - 1;
                int type = tileTypeAt(j, k); // Lấy thuộc tính vật lý của ô này

                // Bỏ qua vùng bên ngoài
                if ((type & 256) == 256) continue;

                // KIỂM TRA NGHIÊM NGẶT: Có gạch VÀ phải là "Đất cứng" mới được vẽ màu
                if (num != -1 && (type & solidTypes) != 0)
                {
                    g.setColor(0x333333);
                    g.fillRect(j * size, k * size, size, size);
                }
            }
        }

        // 2. XỬ LÝ VẼ BÙ 2 MÉP BẢN ĐỒ (Trái / Phải)
        if (GameScr.cmx < 24)
        {
            for (int l = GameScr.gssy; l < GameScr.gssye; l++)
            {
                int typeLeft = tileTypeAt(1, l);
                if (maps[l * tmw + 1] - 1 != -1 && (typeLeft & solidTypes) != 0)
                {
                    g.setColor(0x333333);
                    g.fillRect(0, l * size, size, size);
                }
            }
        }
        if (GameScr.cmx > GameScr.cmxLim)
        {
            int num3 = tmw - 2;
            for (int m = GameScr.gssy; m < GameScr.gssye; m++)
            {
                int typeRight = tileTypeAt(num3, m);
                if (maps[m * tmw + num3] - 1 != -1 && (typeRight & solidTypes) != 0)
                {
                    g.setColor(0x333333);
                    g.fillRect((num3 + 1) * size, m * size, size, size);
                }
            }
        }
    }


    public static void loadMapFromResource(int mapID)
	{
		DataInputStream dataInputStream = null;
		dataInputStream = MyStream.readFile("/mymap/" + mapID);
		tmw = (ushort)dataInputStream.read();
		tmh = (ushort)dataInputStream.read();
		maps = new int[dataInputStream.available()];
		for (int i = 0; i < tmw * tmh; i++)
		{
			maps[i] = (ushort)dataInputStream.read();
		}
		types = new int[maps.Length];
	}

	public static int tileAt(int x, int y)
	{
		try
		{
			return maps[y * tmw + x];
		}
		catch (Exception)
		{
			return 1000;
		}
	}

	public static int tileTypeAt(int x, int y)
	{
		try
		{
			return types[y * tmw + x];
		}
		catch (Exception)
		{
			return 1000;
		}
	}

	public static int tileTypeAtPixel(int px, int py)
	{
		try
		{
			return types[py / size * tmw + px / size];
		}
		catch (Exception)
		{
			return 1000;
		}
	}

	public static bool tileTypeAt(int px, int py, int t)
	{
		try
		{
			return (types[py / size * tmw + px / size] & t) == t;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static void setTileTypeAtPixel(int px, int py, int t)
	{
		types[py / size * tmw + px / size] |= t;
	}

	public static void setTileTypeAt(int x, int y, int t)
	{
		types[y * tmw + x] = t;
	}

	public static void killTileTypeAt(int px, int py, int t)
	{
		types[py / size * tmw + px / size] &= ~t;
	}

	public static int tileYofPixel(int py)
	{
		return py / size * size;
	}

	public static int tileXofPixel(int px)
	{
		return px / size * size;
	}

	public static void loadMainTile()
	{
		if (lastTileID != tileID)
		{
			getTile();
			lastTileID = tileID;
		}
	}
}
