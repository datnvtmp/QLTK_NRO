using System.Threading;

public class Sound
{
	private const int INTERVAL = 5;

	private const int MAXTIME = 100;

	public static int status;

	public static int postem;

	public static int timestart;

	private static string filenametemp;

	private static float volumetem;

	public static bool isSound = true;

	public static bool isNotPlay;

	public static bool stopAll;


	public static GameObject[] player;

	public static sbyte MLogin;

	public static sbyte MBClick = 1;

	public static sbyte MTone = 2;

	public static sbyte MSanzu = 3;

	public static sbyte MChakumi = 4;

	public static sbyte MChai = 5;

	public static sbyte MOshin = 6;

	public static sbyte MEchigo = 7;

	public static sbyte MKojin = 8;

	public static sbyte MHaruna = 9;

	public static sbyte MHirosaki = 10;

	public static sbyte MOokaza = 11;

	public static sbyte MGiotuyet = 12;

	public static sbyte MHangdong = 13;

	public static sbyte MDeKeu = 14;

	public static sbyte MChimKeu = 15;

	public static sbyte MBuocChan = 16;

	public static sbyte MNuocChay = 17;

	public static sbyte MBomMau = 18;

	public static sbyte MKiemGo = 19;

	public static sbyte MKiem = 20;

	public static sbyte MTieu = 21;

	public static sbyte MKunai = 22;

	public static sbyte MCung = 23;

	public static sbyte MDao = 24;

	public static sbyte MQuat = 25;

	public static sbyte MCung2 = 26;

	public static sbyte MTieu2 = 27;

	public static sbyte MTieu3 = 28;

	public static sbyte MKiem2 = 29;

	public static sbyte MKiem3 = 30;

	public static sbyte MDao2 = 31;

	public static sbyte MDao3 = 32;

	public static sbyte MCung3 = 33;

	public static int l1;

	public static void setActivity(SoundMn.AssetManager ac)
	{
	}

	

	

	public static void init()
	{
		
	}

	public static void init(int[] musicID, int[] sID)
	{
		
	}

	public static void playSound(int id, float volume)
	{
	}

	public static void playSound1(int id, float volume)
	{
	}

	public static void getAssetSoundFile(string fileName, int pos)
	{
		
	}

	public static void stopAllz()
	{
		
	}

	public static void stopAllBg()
	{
	}

	public static void update()
	{
	}

	public static void stopMusic(int x)
	{
		
	}

	public static void play(int id, float volume)
	{
		
	}

	public static void playSoundRun(int id, float volume)
	{
		
	}

	public static void sTopSoundRun()
	{
	}

	public static bool isPlayingSound()
	{
		return false;
	}

	public static void playSoundNatural(int id, float volume, bool isLoop)
	{
		
	}

	public static void stopSoundNatural(int id)
	{
	}

	

	public static void playMus(int type, float vl, bool loop)
	{
		
	}

	public static void playSoundBGLoop(int id, float volume)
	{
		
	}

	public static void sTopSoundBG(int id)
	{
	}

	public static bool isPlayingSoundBG(int id)
	{
		return true;
	}

	public static void load(string filename, int pos)
	{
	}

	private static void _load(string filename, int pos)
	{
		
	}

	private static void __load(string filename, int pos)
	{
		
	}

	public static void start(float volume, int pos)
	{
		
	}

	public static void _start(float volume, int pos)
	{
		
	}

	public static void __start(float volume, int pos)
	{
		
	}

	public static void stop(int pos)
	{
		
	}

	public static void _stop(int pos)
	{
		
	}

	public static void __stop(int pos)
	{
		
	}
}
