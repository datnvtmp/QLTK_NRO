using SkiaSharp;
using System;
using System.IO;

public class Image : IDisposable
{
	public SKBitmap bitmap;
 private SKImage skImage;
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
           skImage?.Dispose();
			skImage = null;
			_disposed = true;
		}
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

	public static void update() { }
	public SKImage GetSkImage()
	{
		if (_disposed || bitmap == null)
		{
			return null;
		}
		if (skImage == null)
		{
			skImage = SKImage.FromBitmap(bitmap);
		}
		return skImage;
	}

	public int getWidth() => w / mGraphics.zoomLevel;
	public int getHeight() => h / mGraphics.zoomLevel;

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
              skImage?.Dispose();
				bitmap = null;
              skImage = null;
				w = 0;
				h = 0;
				_disposed = true;
			}
		}
	}
}
