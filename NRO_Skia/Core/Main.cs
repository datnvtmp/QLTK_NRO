using System.Net.NetworkInformation;

public class Main
{
	public static mGraphics g;

	public static GameMidlet midlet;

	public static string res = "res";

	public static string mainThreadName = "Main";

	public static bool started;

	public static bool isIpod;

	public static bool isIphone4;

	public static bool isPC = true;

	public static bool isWindowsPhone;

	public static bool isIPhone;

	public static bool IphoneVersionApp;

	public static string IMEI;

	public static int versionIp;

	public static int numberQuit = 1;

	public static int typeClient = 4;

	public const sbyte PC_VERSION = 4;

	public const sbyte IP_APPSTORE = 5;

	public const sbyte WINDOWSPHONE = 6;

	public const sbyte IP_JB = 3;

	public static int waitTick;

	public static int f;

	public static bool isResume;

	public static bool isMiniApp = true;

	public static bool isQuitApp;

	public static int a = 1;

	public static bool isCompactDevice = true;

	public static void doClearRMS()
	{
		if (isPC)
		{
			int num = Rms.loadRMSInt("lastZoomlevel");
			if (num != mGraphics.zoomLevel)
			{
				Rms.clearAll();
				Rms.saveRMSInt("lastZoomlevel", mGraphics.zoomLevel);
			}
		}
	}

	public static string GetMacAddress()
	{
		string empty = string.Empty;
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		for (int i = 0; i < allNetworkInterfaces.Length; i++)
		{
			PhysicalAddress physicalAddress = allNetworkInterfaces[i].GetPhysicalAddress();
			if (physicalAddress.ToString() != string.Empty)
				return physicalAddress.ToString();
		}
		return string.Empty;
	}

	public static void closeKeyBoard() { }

	public static void setBackupIcloud(string path) { }

	public static void exit()
	{
		Application.Quit();
	}

	public static bool detectCompactDevice() => true;

	public static bool checkCanSendSMS() => false;
}

