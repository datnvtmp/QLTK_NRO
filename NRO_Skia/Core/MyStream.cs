using System;

public class MyStream
{
	public static DataInputStream readFile(string path)
	{
		path = Main.res + path;
		if (!File.Exists(path) && File.Exists(path + ".bin"))
		{
			path = path + ".bin";
		}
		try
		{
			return DataInputStream.getResourceAsStream(path);
		}
		catch (Exception)
		{
			return null;
		}
	}
}
