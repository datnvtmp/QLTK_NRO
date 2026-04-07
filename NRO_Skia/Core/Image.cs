using SkiaSharp;
using System;
using System.IO;

public class Image : IDisposable
{
	public SKBitmap bitmap;
	private bool _disposed;
	private long _nativeSize;

	public void Dispose()
	{
		if (!_disposed)
		{
			if (_nativeSize > 0)
			{
				GC.RemoveMemoryPressure(_nativeSize);
				_nativeSize = 0;
			}
			bitmap?.Dispose();
			bitmap = null;
			_disposed = true;
		}
	}

	~Image()
	{
		Dispose();
	}

	private void TrackNativeMemory()
	{
		if (bitmap != null)
		{
			_nativeSize = (long)bitmap.ByteCount;
			if (_nativeSize > 0)
				GC.AddMemoryPressure(_nativeSize);
		}
	}

	private Image WithTracking()
	{
		TrackNativeMemory();
		return this;
	}

	public static Image imgTemp;
	public static string filenametemp;
	public static byte[] datatemp;
	public static Image imgSrcTemp;
	public static int xtemp;
	public static int ytemp;
	public static int wtemp;
	public static int htemp;
	public static int transformtemp;

	public int w;
	public int h;
	public static int status;
	public Color colorBlend = Color.black;

	public static Image createEmptyImage() => new Image();

	public static Image createImage(string filename)
	{
		try
		{
			var bmp = SKBitmap.Decode(filename);
			if (bmp == null) return null;
			var img = new Image { bitmap = bmp };
			img.w = bmp.Width;
			img.h = bmp.Height;
			img.TrackNativeMemory();
			return img;
		}
		catch (Exception e)
		{
			Cout.LogError("createImage(string) fail: " + filename + " " + e.Message);
			return null;
		}
	}

	public static Image createImage(byte[] imageData)
	{
		if (imageData == null || imageData.Length == 0)
		{
			Cout.LogError("Create Image from byte array fail");
			return null;
		}
		try
		{
			var bmp = SKBitmap.Decode(imageData);
			if (bmp == null) return null;
			var img = new Image { bitmap = bmp };
			img.w = bmp.Width;
			img.h = bmp.Height;
			img.TrackNativeMemory();
			return img;
		}
		catch (Exception e)
		{
			Cout.LogError("createImage(byte[]) fail: " + e.Message);
			return null;
		}
	}

	public static Image createImage(Image src, int x, int y, int w, int h, int transform)
	{
		if (src?.bitmap == null) return null;
		var newBmp = new SKBitmap(w, h);
		using (var canvas = new SKCanvas(newBmp))
		{
			if (transform == 2)
			{
				canvas.Translate(w, 0);
				canvas.Scale(-1, 1);
			}
			canvas.DrawBitmap(src.bitmap, new SKRectI(x, y, x + w, y + h), new SKRect(0, 0, w, h));
		}
		return new Image { bitmap = newBmp, w = w, h = h }.WithTracking();
	}

	public static Image createImage(int w, int h)
	{
		return new Image { bitmap = new SKBitmap(w, h), w = w, h = h }.WithTracking();
	}

	public static Image createImage(Image src)
	{
		if (src?.bitmap == null) return null;
		return new Image { bitmap = src.bitmap.Copy(), w = src.w, h = src.h }.WithTracking();
	}

	public static Image createImage(sbyte[] imageData, int offset, int lenght)
	{
		if (offset + lenght > imageData.Length) return null;
		byte[] array = new byte[lenght];
		for (int i = 0; i < lenght; i++)
			array[i] = convertSbyteToByte(imageData[i + offset]);
		return createImage(array);
	}

	public static byte convertSbyteToByte(sbyte var)
	{
		if (var > 0) return (byte)var;
		return (byte)(var + 256);
	}

	public static byte[] convertArrSbyteToArrByte(sbyte[] var)
	{
		byte[] array = new byte[var.Length];
		for (int i = 0; i < var.Length; i++)
			array[i] = var[i] > 0 ? (byte)var[i] : (byte)(var[i] + 256);
		return array;
	}

    public static Image createRGBImage(int[] rgb, int w, int h, bool bl)
    {
        var bmp = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
        var pixels = new SKColor[rgb.Length];
        for (int i = 0; i < rgb.Length && i < w * h; i++)
        {
            int argb = rgb[i];
            byte a = (byte)((argb >> 24) & 0xFF); if (a == 0) a = 255;
            byte r = (byte)((argb >> 16) & 0xFF);
            byte g = (byte)((argb >> 8) & 0xFF);
            byte b = (byte)(argb & 0xFF);
            pixels[i] = new SKColor(r, g, b, a);
        }
        bmp.Pixels = pixels;
        return new Image { bitmap = bmp, w = w, h = h }.WithTracking();
    }

    private static SKColor skColorFromRGB(int rgb)
	{
		byte b = (byte)(rgb & 0xFF);
		byte g = (byte)((rgb >> 8) & 0xFF);
		byte r = (byte)((rgb >> 16) & 0xFF);
		byte a = (byte)((rgb >> 24) & 0xFF);
		if (a == 0) a = 255;
		return new SKColor(r, g, b, a);
	}

	public static Color setColorFromRBG(int rgb)
	{
		int b = rgb & 0xFF;
		int g = (rgb >> 8) & 0xFF;
		int r = (rgb >> 16) & 0xFF;
		return new Color(r / 255f, g / 255f, b / 255f);
	}

	public static void update() { }

	public static byte[] loadData(string filename)
	{
		try
		{
			return File.ReadAllBytes(filename);
		}
		catch (Exception e)
		{
			throw new Exception("NULL POINTER EXCEPTION AT Image loadData " + filename, e);
		}
	}

	public static Image __createImage(int w, int h) => createImage(w, h);

	public static int getImageWidth(Image image) => image.getWidth();
	public static int getImageHeight(Image image) => image.getHeight();

	public int getWidth() => w / mGraphics.zoomLevel;
	public int getHeight() => h / mGraphics.zoomLevel;

	public Color[] getColor()
	{
		var pixels = new Color[w * h];
		for (int py = 0; py < h; py++)
			for (int px = 0; px < w; px++)
			{
				var c = bitmap.GetPixel(px, py);
				pixels[py * w + px] = new Color(c.Red / 255f, c.Green / 255f, c.Blue / 255f, c.Alpha / 255f);
			}
		return pixels;
	}

	public object texture
	{
		get => bitmap;
		set
		{
			if (value == null)
			{
				if (_nativeSize > 0)
				{
					GC.RemoveMemoryPressure(_nativeSize);
					_nativeSize = 0;
				}
				bitmap?.Dispose();
				bitmap = null;
				w = 0;
				h = 0;
				_disposed = true;
			}
		}
	}

	public int getRealImageWidth() => w;
	public int getRealImageHeight() => h;

	public void getRGB(ref int[] data, int x1, int x2, int x, int y, int w, int h)
	{
		for (int j = 0; j < h; j++)
			for (int i = 0; i < w; i++)
			{
				var c = bitmap.GetPixel(x + i, y + j);
				int idx = j * w + i;
				if (idx < data.Length)
					data[idx] = (c.Alpha << 24) | (c.Red << 16) | (c.Green << 8) | c.Blue;
			}
	}
}
