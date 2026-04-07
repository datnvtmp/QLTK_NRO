using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class SplashScr : mScreen
{
	public static int splashScrStat;

	private bool isCheckConnect;

	private bool isSwitchToLogin;

	public static int nData = -1;

	public static int maxData = -1;

	public static SplashScr instance;

	public static Image imgLogo;

	private int timeLoading = 10;

	public long TIMEOUT;

	public SplashScr()
	{
		instance = this;
	}

	public static void loadSplashScr()
	{
		splashScrStat = 0;
        Rms.saveRMSString("acc", "Nguyễn Đạt");
        Rms.saveRMSString("pass", "check con cec");

        //Rms.saveRMSString("acc", "0989319946");
        //Rms.saveRMSString("pass", "dungcheckacc0");
    }

    public override void update()
    {
        splashScrStat++;

        if (splashScrStat == 1 && !isCheckConnect)
        {
            isCheckConnect = true;

            if (Rms.loadRMSInt("serverchat") != -1)
                GameScr.isPaintChatVip = Rms.loadRMSInt("serverchat") == 0;
            ServerListScreen.loadIP();
        }

        if (splashScrStat >= 5)
        {
            if (GameCanvas.serverScreen == null)
                GameCanvas.serverScreen = new ServerListScreen();

            if (Session_ME.gI().isConnected())
            {
                ServerListScreen.loadScreen = true;
                GameCanvas.serverScreen.switchToMe();
            }
            else if (splashScrStat >= 150)
            {
                mSystem.onDisconnected();
                GameCanvas.serverScreen.switchToMe();
            }
        }

        ServerListScreen.updateDeleteData();
    }

    public static void loadIP()
	{
		Res.err(">>>>>loadIP:  svselect == " + Rms.loadRMSInt(ServerListScreen.RMS_svselect));
		ServerListScreen.SetIpSelect(Rms.loadRMSInt(ServerListScreen.RMS_svselect), issave: false);
		if (ServerListScreen.ipSelect == -1)
		{
			Res.err(">>>loadIP:  svselect == -1");
			if (ServerListScreen.serverPriority == -1)
			{
				ServerListScreen.SetIpSelect(ServerListScreen.serverPriority, issave: true);
			}
			else
			{
				ServerListScreen.SetIpSelect(ServerListScreen.serverPriority, issave: true);
			}
		}
		ServerListScreen.ConnectIP();
	}

    public override void paint(mGraphics g)
    {
        // Đang tải data → ưu tiên hiện progress bar
        if (nData != -1)
        {
            g.setColor(0);
            g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
            g.drawImage(LoginScr.imgTitle, GameCanvas.w / 2, GameCanvas.h / 2 - 24, StaticObj.BOTTOM_HCENTER);
            GameCanvas.paintShukiren(GameCanvas.hw, GameCanvas.h / 2 + 24, g);
            mFont.tahoma_7b_white.drawString(g, mResources.downloading_data + nData * 100 / maxData + "%", GameCanvas.w / 2, GameCanvas.h / 2, 2);
            return;
        }

        // Frame 0-4: hiện logo
        if (splashScrStat < 5)
        {
            if (imgLogo != null)
            {
                g.setColor(16777215);
                g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
                g.drawImage(imgLogo, GameCanvas.w / 2, GameCanvas.h / 2, 3);
            }
            return;
        }

        // Frame 5+: đang chờ kết nối → loading spinner
        g.setColor(0);
        g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
        GameCanvas.paintShukiren(GameCanvas.hw, GameCanvas.hh, g);
        ServerListScreen.paintDeleteData(g);
    }


    public static void loadImg()
	{
		//imgLogo = GameCanvas.loadImage("/gamelogo.png");
	}
}
