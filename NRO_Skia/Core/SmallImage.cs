using System;
using Assets.src.e;

public class SmallImage
{
	public static int[][] smallImg;

	public static SmallImage instance;

	public static Image[] imgbig;

	public static Small[] imgNew;

	public static MyVector vKeys = new MyVector();

	public static Image imgEmpty = null;

	public static sbyte[] newSmallVersion;

	public static MyVector vt_images_watingDowload = new MyVector();

	public static int smallCount;

	public static short maxSmall;

	public SmallImage()
	{
		readImage();
	}

	public static void loadBigRMS()
	{
		if (imgbig == null)
		{
			imgbig = new Image[5]
			{
				GameCanvas.loadImageRMS("/img/Big0.png"),
				GameCanvas.loadImageRMS("/img/Big1.png"),
				GameCanvas.loadImageRMS("/img/Big2.png"),
				GameCanvas.loadImageRMS("/img/Big3.png"),
				GameCanvas.loadImageRMS("/img/Big4.png")
			};
		}
	}

	public static void freeBig()
	{
		if (imgbig != null)
		{
			for (int i = 0; i < imgbig.Length; i++)
			{
				imgbig[i]?.Dispose();
				imgbig[i] = null;
			}
		}
		imgbig = null;
		mSystem.gcc();
	}

	public static void loadBigImage()
	{
		imgEmpty = Image.createRGBImage(new int[1], 1, 1, bl: true);
	}

	public static void init()
	{
		instance = null;
		instance = new SmallImage();
	}

	public void readData(byte[] data)
	{
	}

	public void readImage()
	{
		int num = 0;
		try
		{
			DataInputStream dataInputStream = new DataInputStream(Rms.loadRMS("NR_image"));
			short num2 = dataInputStream.readShort();
			smallImg = new int[num2][];
			for (int i = 0; i < smallImg.Length; i++)
			{
				smallImg[i] = new int[5];
			}
			for (int j = 0; j < num2; j++)
			{
				num++;
				smallImg[j][0] = dataInputStream.readUnsignedByte();
				smallImg[j][1] = dataInputStream.readShort();
				smallImg[j][2] = dataInputStream.readShort();
				smallImg[j][3] = dataInputStream.readShort();
				smallImg[j][4] = dataInputStream.readShort();
			}
		}
		catch (Exception ex)
		{
			Cout.LogError3("Loi readImage: " + ex.ToString() + "i= " + num);
		}
	}

	public static void clearHastable()
	{
	}

	public static void createImage(int id)
	{
		Res.outz("is request =" + id + " zoom=" + mGraphics.zoomLevel);

		// Dispose ảnh cũ nếu có
		var old = imgNew[id];
		if (old != null && old.img != null && old.img != imgEmpty)
			old.img.Dispose();

		// Bước 1: thử load file local trước
        Image image = GameCanvas.loadImage("/SmallImage/Small" + id + ".png");
        if (image != null)
        {
            imgNew[id] = new Small(image, id);
            return;
        }

        // Bước 2: thử load từ RMS
        string rmsKey = mGraphics.zoomLevel + "Small" + id;
        sbyte[] array = Rms.loadRMS(rmsKey);
        if (array != null)
        {
            bool invalid = newSmallVersion != null
                        && array.Length % 127 != newSmallVersion[id];
            if (!invalid)
            {
                Image imageRms = Image.createImage(array, 0, array.Length);
                if (imageRms != null)
                {
                    imgNew[id] = new Small(imageRms, id);
                    return;
                }
            }
        }

        // Bước 3: không có gì → dùng ảnh trống + request server
        imgNew[id] = new Small(imgEmpty, id);
        if (GameCanvas.currentScreen == GameCanvas._SelectCharScr)
            Service.gI().requestIcon(id);
        else
            vt_images_watingDowload.addElement(imgNew[id]);
    }

    public static void drawSmallImage(mGraphics g, int id, int x, int y, int transform, int anchor)
	{
		if (imgbig == null)
		{
			Small small = imgNew[id];
			if (small == null)
			{
				createImage(id);
			}
			else
			{
				g.drawRegion(small, 0, 0, mGraphics.getImageWidth(small.img), mGraphics.getImageHeight(small.img), transform, x, y, anchor);
			}
		}
		else if (smallImg != null)
		{
			if (id >= smallImg.Length || smallImg[id][1] >= 256 || smallImg[id][3] >= 256 || smallImg[id][2] >= 256 || smallImg[id][4] >= 256)
			{
				Small small2 = imgNew[id];
				if (small2 == null)
				{
					createImage(id);
				}
				else
				{
					small2.paint(g, transform, x, y, anchor);
				}
			}
			else if (imgbig[smallImg[id][0]] != null)
			{
				g.drawRegion(imgbig[smallImg[id][0]], smallImg[id][1], smallImg[id][2], smallImg[id][3], smallImg[id][4], transform, x, y, anchor);
			}
		}
		else if (GameCanvas.currentScreen != GameScr.gI())
		{
			Small small3 = imgNew[id];
			if (small3 == null)
			{
				createImage(id);
			}
			else
			{
				small3.paint(g, transform, x, y, anchor);
			}
		}
	}

	public static void drawSmallImage(mGraphics g, int id, int f, int x, int y, int w, int h, int transform, int anchor)
	{
		if (imgbig == null)
		{
			Small small = imgNew[id];
			if (small == null)
			{
				createImage(id);
			}
			else
			{
				g.drawRegion(small.img, 0, f * w, w, h, transform, x, y, anchor);
			}
		}
		else if (smallImg != null)
		{
			if (id >= smallImg.Length || smallImg[id] == null || smallImg[id][1] >= 256 || smallImg[id][3] >= 256 || smallImg[id][2] >= 256 || smallImg[id][4] >= 256)
			{
				Small small2 = imgNew[id];
				if (small2 == null)
				{
					createImage(id);
				}
				else
				{
					small2.paint(g, transform, f, x, y, w, h, anchor);
				}
			}
			else if (smallImg[id][0] != 4 && imgbig[smallImg[id][0]] != null)
			{
				g.drawRegion(imgbig[smallImg[id][0]], 0, f * w, w, h, transform, x, y, anchor);
			}
			else
			{
				Small small3 = imgNew[id];
				if (small3 == null)
				{
					createImage(id);
				}
				else
				{
					small3.paint(g, transform, f, x, y, w, h, anchor);
				}
			}
		}
		else if (GameCanvas.currentScreen != GameScr.gI())
		{
			Small small4 = imgNew[id];
			if (small4 == null)
			{
				createImage(id);
			}
			else
			{
				small4.paint(g, transform, f, x, y, w, h, anchor);
			}
		}
	}

	public static void update()
	{
		// Chạy mỗi 500 tick thay vì 1000 — dọn rác thường xuyên hơn
		if (GameCanvas.gameTick % 500 != 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < imgNew.Length; i++)
		{
			if (imgNew[i] != null)
			{
				num++;
				imgNew[i].update();
				smallCount++;
			}
		}
		// Nếu quá nhiều ảnh, giải phóng riêng từng ảnh không dùng
		// thay vì nuke toàn bộ mảng (tránh lag spike khi load lại)
		if (num > 150)
		{
			for (int j = 0; j < imgNew.Length; j++)
			{
				if (imgNew[j] != null
					&& imgNew[j].timeUpdate - imgNew[j].timePaint > 1
					&& !Char.myCharz().isCharBodyImageID(j))
				{
					if (imgNew[j].img != null && imgNew[j].img != imgEmpty)
						imgNew[j].img.Dispose();
					imgNew[j] = null;
				}
			}
			mSystem.gcc();
		}
	}
}
