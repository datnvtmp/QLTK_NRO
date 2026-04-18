using Assets.src.e;
using Assets.src.g;
using SkiaSharp;
using System;

public class GameCanvas : IActionListener
{
    public static long timeNow = 0L;

    public static bool open3Hour;

    public static bool lowGraphic = true;

    public static bool serverchat = false;

    public static bool isMoveNumberPad = true;

    public static bool isLoading;

    public static bool isTouch = false;

    public static bool isTouchControl;

    public static bool isTouchControlSmallScreen;

    public static bool isTouchControlLargeScreen;

    public static bool isConnectFail;

    public static GameCanvas instance;

    public static bool bRun;

    public static bool[] keyPressed = new bool[30];

    public static bool[] keyReleased = new bool[30];

    public static bool[] keyHold = new bool[30];

    public static bool isPointerDown;

    public static bool isPointerClick;

    public static bool isPointerJustRelease;

    public static bool isPointerSelect;

    public static bool isPointerMove;

    public static int px;

    public static int py;

    public static int pxFirst;

    public static int pyFirst;

    public static int pxLast;

    public static int pyLast;

    public static int pxMouse;

    public static int pyMouse;

    public static Position[] arrPos = new Position[4];

    public static int gameTick;

    public static int taskTick;

    public static bool isEff1;

    public static bool isEff2;

    public static long timeTickEff1;

    public static long timeTickEff2;

    public static int w;

    public static int h;

    public static int hw;

    public static int hh;

    public static int wd3;

    public static int hd3;

    public static int w2d3;

    public static int h2d3;

    public static int w3d4;

    public static int h3d4;

    public static int wd6;

    public static int hd6;

    public static mScreen currentScreen;

    public static Menu menu = new Menu();

    public static Panel panel;

    public static Panel panel2;

    public static ChooseCharScr chooseCharScr;

    public static LoginScr loginScr;

    public static RegisterScreen registerScr;

    public static Dialog currentDialog;

    public static MsgDlg msgdlg;

    public static InputDlg inputDlg;

    public static MyVector currentPopup = new MyVector();

    public static int requestLoseCount;

    public static MyVector listPoint;

    public static Paint paintz;

    public static bool isGetResFromServer;

    public static Image[] imgBG;

    public static int skyColor;

    public static int curPos = 0;

    public static int[] bgW;

    public static int[] bgH;

    public static int planet = 0;

    private mGraphics g = new mGraphics();

    public static Image img18;

    public static Image[] imgBlue = new Image[7];

    public static Image[] imgViolet = new Image[7];

    public static MyHashTable danhHieu = new MyHashTable();

    public static MyVector messageServer = new MyVector(string.Empty);

    public static bool isPlaySound = true;

    private static int clearOldData;

    public static int timeOpenKeyBoard;

    public static bool isFocusPanel2;

    public static int fps = 0;

    public static int max;

    public static int up;

    public static int upmax;

    private long timefps = mSystem.currentTimeMillis() + 1000;

    private long timeup = mSystem.currentTimeMillis() + 1000;

    public static int isRequestMapID = -1;

    public static long waitingTimeChangeMap;

    private static int dir_ = -1;

    private int tickWaitThongBao;

    public bool isPaintCarret;

    public static MyVector debugUpdate;

    public static MyVector debugPaint;

    public static MyVector debugSession;

    private static bool isShowErrorForm = false;

    public static bool paintBG;

    public static int gsskyHeight;

    public static int gsgreenField1Y;

    public static int gsgreenField2Y;

    public static int gshouseY;

    public static int gsmountainY;

    public static int bgLayer0y;

    public static int bgLayer1y;

    public static Image imgCloud;

    public static Image imgSun;

    public static Image imgSun2;

    public static Image imgClear;

    public static Image[] imgBorder = new Image[3];

    public static Image[] imgSunSpec = new Image[3];

    public static int borderConnerW;

    public static int borderConnerH;

    public static int borderCenterW;

    public static int borderCenterH;

    public static int[] cloudX;

    public static int[] cloudY;

    public static int sunX;

    public static int sunY;

    public static int sunX2;

    public static int sunY2;

    public static int[] layerSpeed;

    public static int[] moveX;

    public static int[] moveXSpeed;

    public static bool isBoltEff;

    public static bool boltActive;

    public static int tBolt;

    public static Image imgBgIOS;

    public static int typeBg = -1;

    public static int transY;

    public static int[] yb = new int[5];

    public static int[] colorTop;

    public static int[] colorBotton;

    public static int yb1;

    public static int yb2;

    public static int yb3;

    public static int nBg = 0;

    public static int lastBg = -1;

    public static int[] bgRain = new int[3] { 1, 4, 11 };

    public static int[] bgRainFont = new int[1] { -1 };

    public static Image imgCaycot;

    public static Image tam;

    public static int typeBackGround = -1;

    public static int saveIDBg = -10;

    public static bool isLoadBGok;

    private static long lastTimePress = 0L;

    public static int keyAsciiPress;

    public static int pXYScrollMouse;

    private static Image imgSignal;

    public static MyVector flyTexts = new MyVector();

    public int longTime;

    public static long timeBreakLoading;

    private static string thongBaoTest;

    public static int xThongBaoTranslate = w - 60;

    public static bool isPointerJustDown = false;

    private int count = 1;

    public static bool csWait;

    public static MyRandom r = new MyRandom();

    public static bool isBlackScreen;

    public static int[] bgSpeed;

    public static int cmdBarX;

    public static int cmdBarY;

    public static int cmdBarW;

    public static int cmdBarH;

    public static int cmdBarLeftW;

    public static int cmdBarRightW;

    public static int cmdBarCenterW;

    public static int hpBarX;

    public static int hpBarY;

    public static int hpBarW;

    public static int expBarW;

    public static int lvPosX;

    public static int moneyPosX;

    public static int hpBarH;

    public static int girlHPBarY;

    public int timeOut;

    public int[] dustX;

    public int[] dustY;

    public int[] dustState;

    public static int[] wsX;

    public static int[] wsY;

    public static int[] wsState;

    public static int[] wsF;

    public static Image[] imgWS;

    public static Image imgShuriken;

    public static Image[][] imgDust;

    public static bool isResume;

    public static ServerListScreen serverScreen;

    public static ServerScr serverScr;

    public static SelectCharScr _SelectCharScr;

    public bool resetToLoginScr;

    public static long TIMEOUT;

    public static int timeLoading = 15;

    private static readonly Dictionary<string, Image> _imageCache = new();

    public GameCanvas()
    {
        switch (Rms.loadRMSInt("languageVersion"))
        {
            case -1:
                Rms.saveRMSInt("languageVersion", 2);
                break;
            default:
                Main.doClearRMS();
                Rms.saveRMSInt("languageVersion", 2);
                break;
            case 2:
                break;
        }
        clearOldData = Rms.loadRMSInt(GameMidlet.VERSION);
        if (clearOldData != 1)
        {
            Main.doClearRMS();
            Rms.saveRMSInt(GameMidlet.VERSION, 1);
        }
        initGame();
    }

    public static string getPlatformName()
    {
        return "Pc platform xxx";
    }

    public void initGame()
    {
        try
        {
            MotherCanvas.instance.setChildCanvas(this);
            w = MotherCanvas.instance.getWidthz();
            h = MotherCanvas.instance.getHeightz();
            hw = w / 2;
            hh = h / 2;
            isTouch = true;
            if (w >= 240)
            {
                isTouchControl = true;
            }
            if (w < 320)
            {
                isTouchControlSmallScreen = true;
            }
            if (w >= 320)
            {
                isTouchControlLargeScreen = true;
            }
            msgdlg = new MsgDlg();
            if (h <= 160)
            {
                Paint.hTab = 15;
                mScreen.cmdH = 17;
            }
            GameScr.d = ((w <= h) ? h : w) + 20;
            instance = this;
            mFont.init();
            mScreen.ITEM_HEIGHT = mFont.tahoma_8b.getHeight() + 8;
            initPaint();
            panel = new Panel();
            imgShuriken = loadImage("/mainImage/myTexture2df.png");
            int num = Rms.loadRMSInt("clienttype");
            if (num != -1)
            {
                if (num > 7)
                {
                    Rms.saveRMSInt("clienttype", mSystem.clientType);
                }
                else
                {
                    mSystem.clientType = num;
                }
            }
            if (mSystem.clientType == 7 && (Rms.loadRMSString("fake") == null || Rms.loadRMSString("fake") == string.Empty))
            {
                imgShuriken = loadImage("/mainImage/wait.png");
            }
            imgClear = loadImage("/mainImage/myTexture2der.png");
            img18 = loadImage("/mainImage/18+.png");
            debugUpdate = new MyVector();
            debugPaint = new MyVector();
            debugSession = new MyVector();
            for (int i = 0; i < 3; i++)
            {
                imgBorder[i] = loadImage("/mainImage/myTexture2dbd" + i + ".png");
            }
            borderConnerW = mGraphics.getImageWidth(imgBorder[0]);
            borderConnerH = mGraphics.getImageHeight(imgBorder[0]);
            borderCenterW = mGraphics.getImageWidth(imgBorder[1]);
            borderCenterH = mGraphics.getImageHeight(imgBorder[1]);
            Panel.graphics = 1;
            lowGraphic = true;
            GameScr.isPaintChatVip = Rms.loadRMSInt("serverchat") != 1;
            Char.isPaintAura = Rms.loadRMSInt("isPaintAura") == 1;
            Char.isPaintAura2 = Rms.loadRMSInt("isPaintAura2") == 1;
            Res.init();
            SmallImage.loadBigImage();
            Panel.WIDTH_PANEL = 176;
            if (Panel.WIDTH_PANEL > w)
            {
                Panel.WIDTH_PANEL = w;
            }
            InfoMe.gI().loadCharId();
            Command.btn0left = loadImage("/mainImage/btn0left.png");
            Command.btn0mid = loadImage("/mainImage/btn0mid.png");
            Command.btn0right = loadImage("/mainImage/btn0right.png");
            Command.btn1left = loadImage("/mainImage/btn1left.png");
            Command.btn1mid = loadImage("/mainImage/btn1mid.png");
            Command.btn1right = loadImage("/mainImage/btn1right.png");
            serverScreen = new ServerListScreen();
            img18 = loadImage("/mainImage/18+.png");
            ServerListScreen.createDeleteRMS();
            serverScr = new ServerScr();
            loginScr = new LoginScr();
            _SelectCharScr = new SelectCharScr();
        }
        catch (Exception)
        {
            Debug.LogError("----------------->>>>>>>>>>errr");
        }
    }

    public static GameCanvas gI()
    {
        return instance;
    }

    public void initPaint()
    {
        paintz = new Paint();
    }

    public static void closeKeyBoard()
    {
        mGraphics.addYWhenOpenKeyBoard = 0;
        timeOpenKeyBoard = 0;
        Main.closeKeyBoard();
    }

    public void update()
    {
        if (currentScreen == _SelectCharScr)
        {
            if (gameTick % 2 == 0 && SmallImage.vt_images_watingDowload.size() > 0)
            {
                Small small = (Small)SmallImage.vt_images_watingDowload.elementAt(0);
                Service.gI().requestIcon(small.id);
                SmallImage.vt_images_watingDowload.removeElementAt(0);
            }
        }
        else if (isRequestMapID == 2 && waitingTimeChangeMap < mSystem.currentTimeMillis() && gameTick % 2 == 0 && currentScreen != null)
        {
            if (currentScreen == GameScr.gI())
            {
                if (Char.isLoadingMap)
                {
                    Char.isLoadingMap = false;
                }
                if (ServerListScreen.waitToLogin)
                {
                    ServerListScreen.waitToLogin = false;
                }
            }
            if (SmallImage.vt_images_watingDowload.size() > 0)
            {
                Small small2 = (Small)SmallImage.vt_images_watingDowload.elementAt(0);
                Service.gI().requestIcon(small2.id);
                SmallImage.vt_images_watingDowload.removeElementAt(0);
            }
            if (Effect.dowloadEff.size() <= 0)
            {
            }
        }
        if (mSystem.currentTimeMillis() > timefps)
        {
            timefps += 1000L;
            max = fps;
            fps = 0;
        }
        fps++;
        if (messageServer.size() > 0 && thongBaoTest == null)
        {
            startserverThongBao((string)messageServer.elementAt(0));
            messageServer.removeElementAt(0);
        }
        if (gameTick % 5 == 0)
        {
            timeNow = mSystem.currentTimeMillis();
        }
        Res.updateOnScreenDebug();
        try
        {
            if (TouchScreenKeyboard.visible)
            {
                timeOpenKeyBoard++;
                if (timeOpenKeyBoard > ((!Main.isWindowsPhone) ? 10 : 5))
                {
                    mGraphics.addYWhenOpenKeyBoard = 94;
                }
            }
            else
            {
                mGraphics.addYWhenOpenKeyBoard = 0;
                timeOpenKeyBoard = 0;
            }
            debugUpdate.removeAllElements();
            long num = mSystem.currentTimeMillis();
            if (num - timeTickEff1 >= 780 && !isEff1)
            {
                timeTickEff1 = num;
                isEff1 = true;
            }
            else
            {
                isEff1 = false;
            }
            if (num - timeTickEff2 >= 7800 && !isEff2)
            {
                timeTickEff2 = num;
                isEff2 = true;
            }
            else
            {
                isEff2 = false;
            }
            if (taskTick > 0)
            {
                taskTick--;
            }
            gameTick++;
            if (gameTick > 10000)
            {
                if (mSystem.currentTimeMillis() - lastTimePress > 20000 && currentScreen == loginScr)
                {
                    GameMidlet.instance.exit();
                }
                gameTick = 0;
            }
            if (currentScreen != null)
            {
                if (ChatPopup.serverChatPopUp != null)
                {
                    ChatPopup.serverChatPopUp.update();
                    ChatPopup.serverChatPopUp.updateKey();
                }
                else if (ChatPopup.currChatPopup != null)
                {
                    ChatPopup.currChatPopup.update();
                    ChatPopup.currChatPopup.updateKey();
                }
                else if (currentDialog != null)
                {
                    debug("B", 0);
                    currentDialog.update();
                }
                else if (menu.showMenu)
                {
                    debug("C", 0);
                    menu.updateMenu();
                    debug("D", 0);
                    menu.updateMenuKey();
                }
                else if (panel.isShow)
                {
                    panel.update();
                    if (isPointer(panel.X, panel.Y, panel.W, panel.H))
                    {
                        isFocusPanel2 = false;
                    }
                    if (panel2 != null && panel2.isShow)
                    {
                        panel2.update();
                        if (isPointer(panel2.X, panel2.Y, panel2.W, panel2.H))
                        {
                            isFocusPanel2 = true;
                        }
                    }
                    if (panel2 != null)
                    {
                        if (isFocusPanel2)
                        {
                            panel2.updateKey();
                        }
                        else
                        {
                            panel.updateKey();
                        }
                    }
                    else
                    {
                        panel.updateKey();
                    }
                    if (panel.chatTField != null && panel.chatTField.isShow)
                    {
                        panel.chatTFUpdateKey();
                    }
                    else if (panel2 != null && panel2.chatTField != null && panel2.chatTField.isShow)
                    {
                        panel2.chatTFUpdateKey();
                    }
                    else if ((isPointer(panel.X, panel.Y, panel.W, panel.H) && panel2 != null) || panel2 == null)
                    {
                        panel.updateKey();
                    }
                    else if (panel2 != null && panel2.isShow && isPointer(panel2.X, panel2.Y, panel2.W, panel2.H))
                    {
                        panel2.updateKey();
                    }
                    if (isPointer(panel.X + panel.W, panel.Y, w - panel.W * 2, panel.H) && isPointerJustRelease && panel.isDoneCombine)
                    {
                        panel.hide();
                    }
                }
                debug("E", 0);
                if (!isLoading)
                {
                    currentScreen.update();
                }
                debug("F", 0);
                if (!panel.isShow && ChatPopup.serverChatPopUp == null)
                {
                    currentScreen.updateKey();
                }
                Hint.update();
            }
            debug("Ix", 0);
            Timer.update();
            debug("Hx", 0);
            InfoDlg.update();
            debug("G", 0);
            if (resetToLoginScr)
            {
                resetToLoginScr = false;
                doResetToLoginScr(loginScr);
            }
            debug("Zzz", 0);
            if ((currentScreen != serverScr || !serverScr.isPaintNewUi) && Controller.isConnectOK)
            {
                if (Controller.isMain)
                {
                    ServerListScreen.testConnect = 2;
                    Service.gI().setClientType();
                    Service.gI().androidPack();
                }
                else
                {
                    Service.gI().setClientType2();
                    Service.gI().androidPack2();
                }
                Controller.isConnectOK = false;
            }
            if (Controller.isDisconnected)
            {
                if (!Controller.isMain)
                {
                    if (currentScreen == serverScreen && !Service.reciveFromMainSession)
                    {
                        serverScreen.cancel();
                    }
                    if (currentScreen == loginScr && !Service.reciveFromMainSession)
                    {
                        onDisconnected();
                    }
                }
                else
                {
                    onDisconnected();
                }
                Controller.isDisconnected = false;
            }
            if (Controller.isConnectionFail)
            {
                if (!Controller.isMain)
                {
                    if (currentScreen == serverScreen && ServerListScreen.isGetData && !Service.reciveFromMainSession)
                    {
                        ServerListScreen.testConnect = 0;
                        serverScreen.cancel();
                        Debug.Log("connect fail 1");
                    }
                    if (currentScreen == loginScr && !Service.reciveFromMainSession)
                    {
                        onConnectionFail();
                        Debug.Log("connect fail 2");
                    }
                }
                else
                {
                    if (Session_ME.gI().isCompareIPConnect())
                    {
                        onConnectionFail();
                    }
                    Debug.Log("connect fail 3");
                }
                Controller.isConnectionFail = false;
            }
            if (Main.isResume)
            {
                Main.isResume = false;
                if (currentDialog != null && currentDialog.left != null && currentDialog.left.actionListener != null)
                {
                    currentDialog.left.performAction();
                }
            }
            if (currentScreen != null && currentScreen is GameScr)
            {
                xThongBaoTranslate += dir_ * 2;
                if (xThongBaoTranslate - Panel.imgNew.getWidth() <= 60)
                {
                    dir_ = 0;
                    tickWaitThongBao++;
                    if (tickWaitThongBao > 150)
                    {
                        tickWaitThongBao = 0;
                        thongBaoTest = null;
                    }
                }
            }
            if (currentScreen != null && currentScreen.Equals(GameScr.gI()))
            {
                if (GameScr.info1 != null)
                {
                    GameScr.info1.update();
                }
                if (GameScr.info2 != null)
                {
                    GameScr.info2.update();
                }
            }
            isPointerSelect = false;
        }
        catch (Exception)
        {
        }

        Account.Update();
    }

    public void onDisconnected()
    {
        if (Controller.isConnectionFail)
        {
            Controller.isConnectionFail = false;
        }
        isResume = true;
        Session_ME.gI().clearSendingMessage();
        Session_ME2.gI().clearSendingMessage();
        Session_ME.gI().close();
        Session_ME2.gI().close();
        if (Controller.isLoadingData)
        {
            startOK(mResources.pls_restart_game_error, 8885, null);
            Controller.isDisconnected = false;
            return;
        }
        Debug.LogError(">>>>onDisconnected");
        if (currentScreen != serverScreen)
        {
            serverScreen.switchToMe();
            startOK(mResources.maychutathoacmatsong + " [4]", 8884, null);
        }
        else
        {
            endDlg();
        }
        Char.isLoadingMap = false;
        if (Controller.isMain)
        {
            ServerListScreen.testConnect = 0;
        }
        mSystem.endKey();
    }

    public void onConnectionFail()
    {
        if (currentScreen.Equals(SplashScr.instance))
        {
            startOK(mResources.maychutathoacmatsong + " [1]", 8884, null);
            return;
        }
        Session_ME.gI().clearSendingMessage();
        Session_ME2.gI().clearSendingMessage();
        ServerListScreen.isWait = false;
        if (Controller.isLoadingData)
        {
            startOK(mResources.maychutathoacmatsong + " [2]", 8884, null);
            Controller.isConnectionFail = false;
            return;
        }
        isResume = true;
        LoginScr.isContinueToLogin = false;
        LoginScr.serverName = ServerListScreen.nameServer[ServerListScreen.ipSelect];
        if (currentScreen != serverScreen)
        {
            ServerListScreen.countDieConnect = 0;
        }
        else
        {
            endDlg();
            ServerListScreen.loadScreen = true;
            serverScreen.switchToMe();
        }
        Char.isLoadingMap = false;
        if (Controller.isMain)
        {
            ServerListScreen.testConnect = 0;
        }
        mSystem.endKey();
    }

    public static bool isWaiting()
    {
        if (InfoDlg.isShow || (msgdlg != null && msgdlg.info.Equals(mResources.PLEASEWAIT)) || Char.isLoadingMap || LoginScr.isContinueToLogin)
        {
            return true;
        }
        return false;
    }

    public static void connect()
    {
        if (!Session_ME.gI().isConnected())
        {
            Session_ME.gI().connect(GameMidlet.IP, GameMidlet.PORT);
        }
    }

    public static void connect2()
    {
        if (!Session_ME2.gI().isConnected())
        {
            Res.outz("IP2= " + GameMidlet.IP2 + " PORT 2= " + GameMidlet.PORT2);
            Session_ME2.gI().connect(GameMidlet.IP2, GameMidlet.PORT2);
        }
    }

    public static void resetTrans(mGraphics g)
    {
        g.translate(-g.getTranslateX(), -g.getTranslateY());
        g.setClip(0, 0, w, h);
    }

    public static void resetTransGameScr(mGraphics g)
    {
        g.translate(-g.getTranslateX(), -g.getTranslateY());
        g.translate(0, 0);
        g.setClip(0, 0, w, h);
        g.translate(-GameScr.cmx, -GameScr.cmy);
    }

    public void initGameCanvas()
    {
        debug("SP2i1", 0);
        w = MotherCanvas.instance.getWidthz();
        h = MotherCanvas.instance.getHeightz();
        debug("SP2i2", 0);
        hw = w / 2;
        hh = h / 2;
        wd3 = w / 3;
        hd3 = h / 3;
        w2d3 = 2 * w / 3;
        h2d3 = 2 * h / 3;
        w3d4 = 3 * w / 4;
        h3d4 = 3 * h / 4;
        wd6 = w / 6;
        hd6 = h / 6;
        debug("SP2i3", 0);
        mScreen.initPos();
        debug("SP2i4", 0);
        debug("SP2i5", 0);
        inputDlg = new InputDlg();
        debug("SP2i6", 0);
        listPoint = new MyVector();
        debug("SP2i7", 0);
    }

    public void start()
    {
    }

    public int getWidth()
    {
        return (int)ScaleGUI.WIDTH;
    }

    public int getHeight()
    {
        return (int)ScaleGUI.HEIGHT;
    }

    public static void debug(string s, int type)
    {
    }

    public void doResetToLoginScr(mScreen screen)
    {
        try
        {
            SoundMn.gI().stopAll();
            LoginScr.isContinueToLogin = false;
            TileMap.lastType = (TileMap.bgType = 0);
            Char.clearMyChar();
            GameScr.clearGameScr();
            GameScr.resetAllvector();
            InfoDlg.hide();
            GameScr.info1.hide();
            GameScr.info2.hide();
            GameScr.info2.cmdChat = null;
            Hint.isShow = false;
            ChatPopup.currChatPopup = null;
            Controller.isStopReadMessage = false;
            GameScr.loadCamera(fullmScreen: true, -1, -1);
            GameScr.cmx = 100;
            panel.currentTabIndex = 0;
            panel.selected = (isTouch ? (-1) : 0);
            panel.init();
            panel2 = null;
            GameScr.isPaint = true;
            ClanMessage.vMessage.removeAllElements();
            GameScr.textTime.removeAllElements();
            GameScr.vClan.removeAllElements();
            GameScr.vFriend.removeAllElements();
            GameScr.vEnemies.removeAllElements();
            TileMap.vCurrItem.removeAllElements();
            BackgroudEffect.vBgEffect.removeAllElements();
            EffecMn.vEff.removeAllElements();
            Effect.newEff.removeAllElements();
            menu.showMenu = false;
            panel.vItemCombine.removeAllElements();
            panel.isShow = false;
            if (panel.tabIcon != null)
            {
                panel.tabIcon.isShow = false;
            }
            if (mGraphics.zoomLevel == 1)
            {
                SmallImage.clearHastable();
            }
            Session_ME.gI().close();
            Session_ME2.gI().close();
        }
        catch (Exception ex)
        {
            Cout.println("Loi tai doResetToLoginScr " + ex.ToString());
        }
        ServerListScreen.isAutoConect = true;
        ServerListScreen.countDieConnect = 0;
        ServerListScreen.testConnect = -1;
        ServerListScreen.loadScreen = true;
        if (ServerListScreen.ipSelect == -1)
        {
            serverScr.switchToMe();
            return;
        }
        if (serverScreen == null)
        {
            serverScreen = new ServerListScreen();
        }
        serverScreen.switchToMe();
    }

    public static void showErrorForm(int type, string moreInfo)
    {
    }

    public static void paintCloud(mGraphics g)
    {
    }

    public static void updateBG()
    {
    }

    public static void fillRect(mGraphics g, int color, int x, int y, int w, int h, int detalY)
    {
        g.setColor(color);
        int cmy = GameScr.cmy;
        if (cmy > GameCanvas.h)
        {
            cmy = GameCanvas.h;
        }
        g.fillRect(x, y - ((detalY != 0) ? (cmy >> detalY) : 0), w, h + ((detalY != 0) ? (cmy >> detalY) : 0));
    }

    public static void paintBGGameScr(mGraphics g)
    {
        g.setColor(0x2A2A2A); // màu xám — đổi số này nếu muốn xám đậm/nhạt hơn
        g.fillRect(0, 0, w, h);
    }

    public static void resetBg()
    {
    }

    public static void getYBackground(int typeBg)
    {
        try
        {
            int gH = GameScr.gH23;
            switch (typeBg)
            {
                case 0:
                    yb[0] = gH - bgH[0] + 70;
                    yb[1] = yb[0] - bgH[1] + 20;
                    yb[2] = yb[1] - bgH[2] + 30;
                    yb[3] = yb[2] - bgH[3] + 50;
                    break;
                case 1:
                    yb[0] = gH - bgH[0] + 120;
                    yb[1] = yb[0] - bgH[1] + 40;
                    yb[2] = yb[1] - 90;
                    yb[3] = yb[2] - 25;
                    break;
                case 2:
                    yb[0] = gH - bgH[0] + 150;
                    yb[1] = yb[0] - bgH[1] - 60;
                    yb[2] = yb[1] - bgH[2] - 40;
                    yb[3] = yb[2] - bgH[3] - 10;
                    yb[4] = yb[3] - bgH[4];
                    break;
                case 3:
                    yb[0] = gH - bgH[0] + 10;
                    yb[1] = yb[0] + 80;
                    yb[2] = yb[1] - bgH[2] - 10;
                    break;
                case 4:
                    yb[0] = gH - bgH[0] + 130;
                    yb[1] = yb[0] - bgH[1];
                    yb[2] = yb[1] - bgH[2] - 20;
                    yb[3] = yb[1] - bgH[2] - 80;
                    break;
                case 5:
                    yb[0] = gH - bgH[0] + 40;
                    yb[1] = yb[0] - bgH[1] + 10;
                    yb[2] = yb[1] - bgH[2] + 15;
                    yb[3] = yb[2] - bgH[3] + 50;
                    break;
                case 6:
                    yb[0] = gH - bgH[0] + 100;
                    yb[1] = yb[0] - bgH[1] - 30;
                    yb[2] = yb[1] - bgH[2] + 10;
                    yb[3] = yb[2] - bgH[3] + 15;
                    yb[4] = yb[3] - bgH[4] + 15;
                    break;
                case 7:
                    yb[0] = gH - bgH[0] + 20;
                    yb[1] = yb[0] - bgH[1] + 15;
                    yb[2] = yb[1] - bgH[2] + 20;
                    yb[3] = yb[1] - bgH[2] - 10;
                    break;
                case 8:
                    yb[0] = gH - 103 + 150;
                    if (TileMap.mapID == 103)
                    {
                        yb[0] -= 100;
                    }
                    yb[1] = yb[0] - bgH[1] - 10;
                    yb[2] = yb[1] - bgH[2] + 40;
                    yb[3] = yb[2] - bgH[3] + 10;
                    break;
                case 9:
                    yb[0] = gH - bgH[0] + 100;
                    yb[1] = yb[0] - bgH[1] + 22;
                    yb[2] = yb[1] - bgH[2] + 50;
                    yb[3] = yb[2] - bgH[3];
                    break;
                case 10:
                    yb[0] = gH - bgH[0] - 45;
                    yb[1] = yb[0] - bgH[1] - 10;
                    break;
                case 11:
                    yb[0] = gH - bgH[0] + 60;
                    yb[1] = yb[0] - bgH[1] + 5;
                    yb[2] = yb[1] - bgH[2] - 15;
                    break;
                case 12:
                    yb[0] = gH + 40;
                    yb[1] = yb[0] - 40;
                    yb[2] = yb[1] - 40;
                    break;
                case 13:
                    yb[0] = gH - 80;
                    yb[1] = yb[0];
                    break;
                case 15:
                    yb[0] = gH - 20;
                    yb[1] = yb[0] - 80;
                    break;
                case 16:
                    yb[0] = gH - bgH[0] + 75;
                    yb[1] = yb[0] - bgH[1] + 50;
                    yb[2] = yb[1] - bgH[2] + 50;
                    yb[3] = yb[2] - bgH[3] + 90;
                    break;
                case 19:
                    yb[0] = gH - bgH[0] + 150;
                    yb[1] = yb[0] - bgH[1] - 60;
                    yb[2] = yb[1] - bgH[2] - 40;
                    yb[3] = yb[2] - bgH[3] - 10;
                    yb[4] = yb[3] - bgH[4];
                    break;
                default:
                    yb[0] = gH - bgH[0] + 75;
                    yb[1] = yb[0] - bgH[1] + 50;
                    yb[2] = yb[1] - bgH[2] + 50;
                    yb[3] = yb[2] - bgH[3] + 90;
                    break;
            }
        }
        catch (Exception)
        {
            int gH2 = GameScr.gH23;
            for (int i = 0; i < yb.Length; i++)
            {
                yb[i] = 1;
            }
        }
    }

    public static void loadBG(int typeBG)
    {
        try
        {
            isLoadBGok = true;

            if (typeBg == 12)
            {
                BackgroudEffect.yfog = TileMap.pxh - 100;
            }
            else
            {
                BackgroudEffect.yfog = TileMap.pxh - 160;
            }
            BackgroudEffect.clearImage();

            if ((TileMap.lastBgID == typeBG && TileMap.lastType == TileMap.bgType) || typeBG == -1)
            {
                return;
            }

            transY = 12;
            TileMap.lastBgID = (sbyte)typeBG;
            TileMap.lastType = (sbyte)TileMap.bgType;
            typeBg = typeBG;
            isBoltEff = false;

            // =========================================================
            // 4. BẮT ĐẦU TẠO "DỮ LIỆU GIẢ" VÀ CHẶN NGANG TẠI ĐÂY
            // =========================================================

            layerSpeed = new int[5] { 0, 0, 0, 0, 0 }; // Cho tốc độ mây bằng 0 hết
            moveX = new int[5];
            moveXSpeed = new int[5];

            // Khởi tạo các mảng quan trọng để chống lỗi NullReferenceException
            bgW = new int[5];
            bgH = new int[5];
            imgBG = new Image[0]; // Mảng ảnh RỖNG HOÀN TOÀN, KHÔNG NẠP TÝ RAM NÀO!

            imgCloud = null;
            imgSun = null;
            imgSun2 = null;
            imgSunSpec = null;

            return;
        }
        catch (Exception)
        {
            isLoadBGok = false;
        }
    }

    public void keyPressedz(int keyCode)
    {
        lastTimePress = mSystem.currentTimeMillis();
        if ((keyCode >= 48 && keyCode <= 57) || (keyCode >= 65 && keyCode <= 122) || keyCode == 10 || keyCode == 8 || keyCode == 13 || keyCode == 32 || keyCode == 31)
        {
            keyAsciiPress = keyCode;
        }
        mapKeyPress(keyCode);
    }

    public void mapKeyPress(int keyCode)
    {
        if (currentDialog != null)
        {
            currentDialog.keyPress(keyCode);
            keyAsciiPress = 0;
            return;
        }
        currentScreen.keyPress(keyCode);
        switch (keyCode)
        {
            case -38:
            case -1:
                if ((currentScreen is GameScr || currentScreen is CrackBallScr) && Char.myCharz().isAttack)
                {
                    clearKeyHold();
                    clearKeyPressed();
                }
                else
                {
                    keyHold[21] = true;
                    keyPressed[21] = true;
                }
                break;
            case -39:
            case -2:
                if ((currentScreen is GameScr || currentScreen is CrackBallScr) && Char.myCharz().isAttack)
                {
                    clearKeyHold();
                    clearKeyPressed();
                }
                else
                {
                    keyHold[22] = true;
                    keyPressed[22] = true;
                }
                break;
            case -3:
                if ((currentScreen is GameScr || currentScreen is CrackBallScr) && Char.myCharz().isAttack)
                {
                    clearKeyHold();
                    clearKeyPressed();
                }
                else
                {
                    keyHold[23] = true;
                    keyPressed[23] = true;
                }
                break;
            case -4:
                if ((currentScreen is GameScr || currentScreen is CrackBallScr) && Char.myCharz().isAttack)
                {
                    clearKeyHold();
                    clearKeyPressed();
                }
                else
                {
                    keyHold[24] = true;
                    keyPressed[24] = true;
                }
                break;
            case -5:
            case 10:
                if ((currentScreen is GameScr || currentScreen is CrackBallScr) && Char.myCharz().isAttack)
                {
                    clearKeyHold();
                    clearKeyPressed();
                    break;
                }
                keyHold[25] = true;
                keyPressed[25] = true;
                keyHold[15] = true;
                keyPressed[15] = true;
                break;
            case 48:
                keyHold[0] = true;
                keyPressed[0] = true;
                break;
            case 49:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[1] = true;
                    keyPressed[1] = true;
                }
                break;
            case 51:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[3] = true;
                    keyPressed[3] = true;
                }
                break;
            case 55:
                keyHold[7] = true;
                keyPressed[7] = true;
                break;
            case 57:
                keyHold[9] = true;
                keyPressed[9] = true;
                break;
            case 42:
                keyHold[10] = true;
                keyPressed[10] = true;
                break;
            case 35:
                keyHold[11] = true;
                keyPressed[11] = true;
                break;
            case -21:
            case -6:
                keyHold[12] = true;
                keyPressed[12] = true;
                break;
            case -22:
            case -7:
                keyHold[13] = true;
                keyPressed[13] = true;
                break;
            case 50:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[2] = true;
                    keyPressed[2] = true;
                }
                break;
            case 52:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[4] = true;
                    keyPressed[4] = true;
                }
                break;
            case 54:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[6] = true;
                    keyPressed[6] = true;
                }
                break;
            case 56:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[8] = true;
                    keyPressed[8] = true;
                }
                break;
            case 53:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[5] = true;
                    keyPressed[5] = true;
                }
                break;
            case -8:
                keyHold[14] = true;
                keyPressed[14] = true;
                break;
            case -26:
                keyHold[16] = true;
                keyPressed[16] = true;
                break;
            case 113:
                keyHold[17] = true;
                keyPressed[17] = true;
                break;
        }
    }

    public void keyReleasedz(int keyCode)
    {
        keyAsciiPress = 0;
        mapKeyRelease(keyCode);
    }

    public void mapKeyRelease(int keyCode)
    {
        switch (keyCode)
        {
            case -38:
            case -1:
                keyHold[21] = false;
                break;
            case -39:
            case -2:
                keyHold[22] = false;
                break;
            case -3:
                keyHold[23] = false;
                break;
            case -4:
                keyHold[24] = false;
                break;
            case -5:
            case 10:
                keyHold[25] = false;
                keyReleased[25] = true;
                keyHold[15] = true;
                keyPressed[15] = true;
                break;
            case 48:
                keyHold[0] = false;
                keyReleased[0] = true;
                break;
            case 49:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[1] = false;
                    keyReleased[1] = true;
                }
                break;
            case 51:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[3] = false;
                    keyReleased[3] = true;
                }
                break;
            case 55:
                keyHold[7] = false;
                keyReleased[7] = true;
                break;
            case 57:
                keyHold[9] = false;
                keyReleased[9] = true;
                break;
            case 42:
                keyHold[10] = false;
                keyReleased[10] = true;
                break;
            case 35:
                keyHold[11] = false;
                keyReleased[11] = true;
                break;
            case -21:
            case -6:
                keyHold[12] = false;
                keyReleased[12] = true;
                break;
            case -22:
            case -7:
                keyHold[13] = false;
                keyReleased[13] = true;
                break;
            case 50:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[2] = false;
                    keyReleased[2] = true;
                }
                break;
            case 52:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[4] = false;
                    keyReleased[4] = true;
                }
                break;
            case 54:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[6] = false;
                    keyReleased[6] = true;
                }
                break;
            case 56:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[8] = false;
                    keyReleased[8] = true;
                }
                break;
            case 53:
                if (currentScreen == CrackBallScr.instance || (currentScreen == GameScr.instance && isMoveNumberPad && !ChatTextField.gI().isShow))
                {
                    keyHold[5] = false;
                    keyReleased[5] = true;
                }
                break;
            case -8:
                keyHold[14] = false;
                break;
            case -26:
                keyHold[16] = false;
                break;
            case 113:
                keyHold[17] = false;
                keyReleased[17] = true;
                break;
        }
    }

    public void pointerMouse(int x, int y)
    {
        pxMouse = x;
        pyMouse = y;
    }

    public void scrollMouse(int a)
    {
        pXYScrollMouse = a;
        if (panel != null && panel.isShow)
        {
            panel.updateScroolMouse(a);
        }
    }

    public void pointerDragged(int x, int y)
    {
        isPointerSelect = false;
        if (Res.abs(x - pxLast) >= 10 || Res.abs(y - pyLast) >= 10)
        {
            isPointerClick = false;
            isPointerDown = true;
            isPointerMove = true;
        }
        px = x;
        py = y;
        curPos++;
        if (curPos > 3)
        {
            curPos = 0;
        }
        arrPos[curPos] = new Position(x, y);
    }

    public static bool isHoldPress()
    {
        if (mSystem.currentTimeMillis() - lastTimePress >= 800)
        {
            return true;
        }
        return false;
    }

    public void pointerPressed(int x, int y)
    {
        isPointerSelect = false;
        isPointerJustRelease = false;
        isPointerJustDown = true;
        isPointerDown = true;
        isPointerClick = false;
        isPointerMove = false;
        lastTimePress = mSystem.currentTimeMillis();
        pxFirst = x;
        pyFirst = y;
        pxLast = x;
        pyLast = y;
        px = x;
        py = y;
    }

    public void pointerReleased(int x, int y)
    {
        if (!isPointerMove)
        {
            isPointerSelect = true;
        }
        isPointerDown = false;
        isPointerMove = false;
        isPointerJustRelease = true;
        isPointerClick = true;
        mScreen.keyTouch = -1;
        px = x;
        py = y;
    }

    public static bool isPointerHoldIn(int x, int y, int w, int h)
    {
        if (!isPointerDown && !isPointerJustRelease)
        {
            return false;
        }
        if (px >= x && px <= x + w && py >= y && py <= y + h)
        {
            return true;
        }
        return false;
    }

    public static bool isPointSelect(int x, int y, int w, int h)
    {
        if (!isPointerSelect)
        {
            return false;
        }
        if (px >= x && px <= x + w && py >= y && py <= y + h)
        {
            return true;
        }
        return false;
    }

    public static bool isMouseFocus(int x, int y, int w, int h)
    {
        if (pxMouse >= x && pxMouse <= x + w && pyMouse >= y && pyMouse <= y + h)
        {
            return true;
        }
        return false;
    }

    public static void clearKeyPressed()
    {
        for (int i = 0; i < keyPressed.Length; i++)
        {
            keyPressed[i] = false;
        }
        isPointerJustRelease = false;
    }

    public static void clearKeyHold()
    {
        for (int i = 0; i < keyHold.Length; i++)
        {
            keyHold[i] = false;
        }
    }

    public void paintChangeMap(mGraphics g)
    {
        string empty = string.Empty;
        resetTrans(g);
        g.setColor(0);
        g.fillRect(0, 0, w, h);
        g.drawImage(LoginScr.imgTitle, w / 2, h / 2 - 24, StaticObj.BOTTOM_HCENTER);
        paintShukiren(hw, h / 2 + 24, g);
        mFont.tahoma_7b_white.drawString(g, mResources.PLEASEWAIT + ((LoginScr.timeLogin <= 0) ? empty : (" " + LoginScr.timeLogin + "s")), w / 2, h / 2, 2);
        GameClient.SendNow(MsgType.DATA_INGAME, "Case -26: " + mResources.PLEASEWAIT + ((LoginScr.timeLogin <= 0) ? empty : (" [" + LoginScr.timeLogin + "s]")));
    }

    public void paint(mGraphics gx)
    {
        try
        {
            debugPaint.removeAllElements();
            debug("PA", 1);
            if (currentScreen != null)
            {
                currentScreen.paint(g);
            }
            debug("PB", 1);
            g.translate(-g.getTranslateX(), -g.getTranslateY());
            g.setClip(0, 0, w, h);
            if (panel.isShow)
            {
                panel.paint(g);
                if (panel2 != null && panel2.isShow)
                {
                    panel2.paint(g);
                }
                if (panel.chatTField != null && panel.chatTField.isShow)
                {
                    panel.chatTField.paint(g);
                }
                if (panel2 != null && panel2.chatTField != null && panel2.chatTField.isShow)
                {
                    panel2.chatTField.paint(g);
                }
            }
            Res.paintOnScreenDebug(g);
            InfoDlg.paint(g);
            if (currentDialog != null)
            {
                debug("PC", 1);
                currentDialog.paint(g);
            }
            else if (menu.showMenu)
            {
                debug("PD", 1);
                resetTrans(g);
                menu.paintMenu(g);
            }
            GameScr.info1.paint(g);
            GameScr.info2.paint(g);
            if (GameScr.gI().popUpYesNo != null)
            {
                GameScr.gI().popUpYesNo.paint(g);
            }
            if (ChatPopup.currChatPopup != null)
            {
                ChatPopup.currChatPopup.paint(g);
            }
            Hint.paint(g);
            if (ChatPopup.serverChatPopUp != null)
            {
                ChatPopup.serverChatPopUp.paint(g);
            }
            if (currentDialog != null)
            {
                currentDialog.paint(g);
            }
            if (isWait())
            {
                paintChangeMap(g);
                if (timeLoading > 0 && LoginScr.timeLogin <= 0 && mSystem.currentTimeMillis() - TIMEOUT >= 1000)
                {
                    timeLoading--;
                    if (timeLoading == 0)
                    {
                        timeLoading = 15;
                    }
                    TIMEOUT = mSystem.currentTimeMillis();
                }
            }
            debug("PE", 1);
            resetTrans(g);
            resetTrans(g);
            int num = h / 4;
            if (currentScreen != null && currentScreen is GameScr && thongBaoTest != null)
            {
                g.setClip(60, num, w - 120, mFont.tahoma_7_white.getHeight() + 2);
                mFont.tahoma_7_grey.drawString(g, thongBaoTest, xThongBaoTranslate, num + 1, 0);
                mFont.tahoma_7_yellow.drawString(g, thongBaoTest, xThongBaoTranslate, num, 0);
                g.setClip(0, 0, w, h);
            }
            AutoManager.PaintScr(g);
        }
        catch (Exception ex)
        {
            NRO_Skia.Program.LogCrash(ex);
        }
    }

    public static void endDlg()
    {
        if (inputDlg != null)
        {
            inputDlg.tfInput.setMaxTextLenght(500);
        }
        currentDialog = null;
        InfoDlg.hide();
    }

    public static void startOKDlg(string info)
    {
        closeKeyBoard();
        msgdlg.setInfo(info, null, new Command(mResources.OK, instance, 8882, null), null);
        currentDialog = msgdlg;
    }

    public static void startWaitDlg(string info)
    {
        closeKeyBoard();
        msgdlg.setInfo(info, null, new Command(mResources.CANCEL, instance, 8882, null), null);
        currentDialog = msgdlg;
        msgdlg.isWait = true;
    }

    public static void startOKDlg(string info, bool isError)
    {
        closeKeyBoard();
        msgdlg.setInfo(info, null, new Command(mResources.CANCEL, instance, 8882, null), null);
        currentDialog = msgdlg;
        msgdlg.isWait = true;
    }

    public static void startWaitDlg()
    {
        closeKeyBoard();
        Char.isLoadingMap = true;
    }

    public void openWeb(string strLeft, string strRight, string url, string str)
    {
        msgdlg.setInfo(str, new Command(strLeft, this, 8881, url), null, new Command(strRight, this, 8882, null));
        currentDialog = msgdlg;
    }

    public static void startOK(string info, int actionID, object p)
    {
        closeKeyBoard();
        msgdlg.setInfo(info, null, new Command(mResources.OK, instance, actionID, p), null);
        msgdlg.show();
    }

    public static void startYesNoDlg(string info, int iYes, object pYes, int iNo, object pNo)
    {
        closeKeyBoard();
        msgdlg.setInfo(info, new Command(mResources.YES, instance, iYes, pYes), new Command(string.Empty, instance, iYes, pYes), new Command(mResources.NO, instance, iNo, pNo));
        msgdlg.show();
    }

    public static void startYesNoDlg(string info, Command cmdYes, Command cmdNo)
    {
        closeKeyBoard();
        msgdlg.setInfo(info, cmdYes, null, cmdNo);
        msgdlg.show();
    }

    public static void startserverThongBao(string msgSv)
    {
        thongBaoTest = msgSv;
        xThongBaoTranslate = w - 60;
        dir_ = -1;
    }

    public static string getMoneys(int m)
    {
        string text = string.Empty;
        int num = m / 1000 + 1;
        for (int i = 0; i < num; i++)
        {
            if (m >= 1000)
            {
                int num2 = m % 1000;
                text = ((num2 != 0) ? ((num2 >= 10) ? ((num2 >= 100) ? ("." + num2 + text) : (".0" + num2 + text)) : (".00" + num2 + text)) : (".000" + text));
                m /= 1000;
                continue;
            }
            text = m + text;
            break;
        }
        return text;
    }

    public static int getX(int start, int w)
    {
        return (px - start) / w;
    }

    public static int getY(int start, int w)
    {
        return (py - start) / w;
    }

    protected void sizeChanged(int w, int h)
    {
    }

    public static bool isGetResourceFromServer()
    {
        return true;
    }

    public static Image loadImageRMS(string path)
    {
        string localPath = Main.res + "/x" + mGraphics.zoomLevel + path;
        // KHÔNG cutPng ở đây — giữ nguyên .png để đọc file
        string rmsKey = cutPng(localPath); // chỉ dùng cho RMS key

        Image result = null;
        try
        {
            result = Image.createImage(localPath); // dùng localPath có .png
        }
        catch (Exception ex)
        {
            try
            {
                string[] array = Res.split(rmsKey, "/", 0);
                string filename = "x" + mGraphics.zoomLevel + array[array.Length - 1];
                sbyte[] array2 = Rms.loadRMS(filename);
                if (array2 != null)
                {
                    result = Image.createImage(array2, 0, array2.Length);
                    array2 = null;
                }
            }
            catch (Exception)
            {
                Cout.LogError("Loi ham khong tim thay: " + ex.ToString());
            }
        }
        return result;
    }

    public static Image loadImage(string path)
    {
        path = Main.res + "/x" + mGraphics.zoomLevel + path;
        if (_imageCache.TryGetValue(path, out var cached)) return cached;

        Image result = null;

        // Font không dùng lowGraphic
        bool isFont = path.Contains("myfont") || path.Contains("MyFont");
        if (AutoManager.lowGraphic && !isFont)
        {
            var bmp = new SKBitmap(4, 4);
            var c = SKColor.FromHsl((path.GetHashCode() & 0xFF) / 255f * 360f, 80, 60);
            bmp.Erase(c);
            result = new Image { bitmap = bmp, w = 4, h = 4 };
        }
        else
        {
            try
            {
                result = Image.createImage(path);
            }
            catch (Exception) { }
        }

        if (result != null) _imageCache[path] = result;
        return result;
    }

    public static string cutPng(string str)
    {
        string result = str;
        if (str.Contains(".png"))
        {
            result = str.Replace(".png", string.Empty);
        }
        return result;
    }

    public static int random(int a, int b)
    {
        return a + r.nextInt(b - a);
    }

    public bool startDust(int dir, int x, int y)
    {
        return false;
    }


    public static bool isPaint(int x, int y)
    {
        if (x < GameScr.cmx)
        {
            return false;
        }
        if (x > GameScr.cmx + GameScr.gW)
        {
            return false;
        }
        if (y < GameScr.cmy)
        {
            return false;
        }
        if (y > GameScr.cmy + GameScr.gH + 30)
        {
            return false;
        }
        return true;
    }



    public static void paintShukiren(int x, int y, mGraphics g)
    {
        // Tạo một mảng chứa 4 trạng thái xoay từ mGraphics (Không xoay, Xoay 90, Xoay 180, Xoay 270)
        int[] transforms = new int[] { 0, 5, 3, 6 };

        // Tính toán góc xoay liên tục dựa vào thời gian của game
        // Chia 2 để giảm tốc độ xoay (thích xoay nhanh thì bỏ / 2 đi)
        int rotation = transforms[(GameCanvas.gameTick / 2) % 4];

        // Cắt góc (0,0) vì ta chỉ dùng 1 khung hình, và đưa biến rotation vào
        g.drawRegion(imgShuriken, 0, 0, 16, 16, rotation, x, y, mGraphics.HCENTER | mGraphics.VCENTER);
    }

    public void resetToLoginScrz()
    {
        resetToLoginScr = true;
    }

    public static bool isPointer(int x, int y, int w, int h)
    {
        if (!isPointerDown && !isPointerJustRelease)
        {
            return false;
        }
        if (px >= x && px <= x + w && py >= y && py <= y + h)
        {
            return true;
        }
        return false;
    }

    public void perform(int idAction, object p)
    {
        switch (idAction)
        {
            case 9000:
                endDlg();
                SplashScr.imgLogo?.Dispose();
                SplashScr.imgLogo = null;
                SmallImage.loadBigRMS();
                mSystem.gcc();
                ServerListScreen.bigOk = true;
                ServerListScreen.loadScreen = true;
                GameScr.gI().loadGameScr();
                if (currentScreen != loginScr)
                {
                    serverScreen.switchToMe2();
                }
                break;
            case 999:
                mSystem.closeBanner();
                endDlg();
                break;
            case 888396:
                endDlg();
                break;
            case 888397:
                {
                    string text4 = (string)p;
                    break;
                }
            case 9999:
                endDlg();
                connect();
                Service.gI().setClientType();
                if (loginScr == null)
                {
                    loginScr = new LoginScr();
                }
                loginScr.doLogin();
                break;
            case 8881:
                {
                    string url = (string)p;
                    try
                    {
                        GameMidlet.instance.platformRequest(url);
                    }
                    catch (Exception)
                    {
                    }
                    currentDialog = null;
                    break;
                }
            case 8882:
                InfoDlg.hide();
                currentDialog = null;
                ServerListScreen.isAutoConect = false;
                ServerListScreen.countDieConnect = 0;
                break;
            case 8884:
                endDlg();
                if (serverScr == null)
                {
                    serverScr = new ServerScr();
                }
                serverScr.switchToMe();
                break;
            case 8885:
                GameMidlet.instance.exit();
                break;
            case 8886:
                {
                    endDlg();
                    string name = (string)p;
                    Service.gI().addFriend(name);
                    break;
                }
            case 8887:
                {
                    endDlg();
                    int charId = (int)p;
                    Service.gI().addPartyAccept(charId);
                    break;
                }
            case 8888:
                {
                    int charId2 = (int)p;
                    Service.gI().addPartyCancel(charId2);
                    endDlg();
                    break;
                }
            case 8889:
                {
                    string str = (string)p;
                    endDlg();
                    Service.gI().acceptPleaseParty(str);
                    break;
                }
            case 88810:
                {
                    int playerMapId = (int)p;
                    endDlg();
                    Service.gI().acceptInviteTrade(playerMapId);
                    break;
                }
            case 88811:
                endDlg();
                Service.gI().cancelInviteTrade();
                break;
            case 88814:
                {
                    Item[] items = (Item[])p;
                    endDlg();
                    Service.gI().crystalCollectLock(items);
                    break;
                }
            case 88817:
                ChatPopup.addChatPopup(string.Empty, 1, Char.myCharz().npcFocus);
                Service.gI().menu(Char.myCharz().npcFocus.template.npcTemplateId, menu.menuSelectedItem, 0);
                break;
            case 88818:
                {
                    short menuId2 = (short)p;
                    Service.gI().textBoxId(menuId2, inputDlg.tfInput.getText());
                    endDlg();
                    break;
                }
            case 88819:
                {
                    short menuId = (short)p;
                    Service.gI().menuId(menuId);
                    break;
                }
            case 88820:
                {
                    string[] array = (string[])p;
                    if (Char.myCharz().npcFocus == null)
                    {
                        break;
                    }
                    int menuSelectedItem = menu.menuSelectedItem;
                    if (array.Length > 1)
                    {
                        MyVector myVector = new MyVector();
                        for (int i = 0; i < array.Length - 1; i++)
                        {
                            myVector.addElement(new Command(array[i + 1], instance, 88821, menuSelectedItem));
                        }
                        menu.startAt(myVector, 3);
                    }
                    else
                    {
                        ChatPopup.addChatPopup(string.Empty, 1, Char.myCharz().npcFocus);
                        Service.gI().menu(Char.myCharz().npcFocus.template.npcTemplateId, menuSelectedItem, 0);
                    }
                    break;
                }
            case 88821:
                {
                    int menuId3 = (int)p;
                    ChatPopup.addChatPopup(string.Empty, 1, Char.myCharz().npcFocus);
                    Service.gI().menu(Char.myCharz().npcFocus.template.npcTemplateId, menuId3, menu.menuSelectedItem);
                    break;
                }
            case 88822:
                ChatPopup.addChatPopup(string.Empty, 1, Char.myCharz().npcFocus);
                Service.gI().menu(Char.myCharz().npcFocus.template.npcTemplateId, menu.menuSelectedItem, 0);
                break;
            case 88823:
                startOKDlg(mResources.SENTMSG);
                break;
            case 88824:
                startOKDlg(mResources.NOSENDMSG);
                break;
            case 88825:
                startOKDlg(mResources.sendMsgSuccess, isError: false);
                break;
            case 88826:
                startOKDlg(mResources.cannotSendMsg, isError: false);
                break;
            case 88827:
                startOKDlg(mResources.sendGuessMsgSuccess);
                break;
            case 88828:
                startOKDlg(mResources.sendMsgFail);
                break;
            case 88829:
                {
                    string text5 = inputDlg.tfInput.getText();
                    if (!text5.Equals(string.Empty))
                    {
                        Service.gI().changeName(text5, (int)p);
                        InfoDlg.showWait();
                    }
                    break;
                }
            case 88836:
                inputDlg.tfInput.setMaxTextLenght(6);
                inputDlg.show(mResources.INPUT_PRIVATE_PASS, new Command(mResources.ACCEPT, instance, 888361, null), TField.INPUT_TYPE_NUMERIC);
                break;
            case 888361:
                {
                    string text3 = inputDlg.tfInput.getText();
                    endDlg();
                    if (text3.Length < 6 || text3.Equals(string.Empty))
                    {
                        startOKDlg(mResources.ALERT_PRIVATE_PASS_1);
                        break;
                    }
                    try
                    {
                        Service.gI().activeAccProtect(int.Parse(text3));
                        break;
                    }
                    catch (Exception ex3)
                    {
                        startOKDlg(mResources.ALERT_PRIVATE_PASS_2);
                        Cout.println("Loi tai 888361 Gamescavas " + ex3.ToString());
                        break;
                    }
                }
            case 88837:
                {
                    string text2 = inputDlg.tfInput.getText();
                    endDlg();
                    try
                    {
                        Service.gI().openLockAccProtect(int.Parse(text2.Trim()));
                        break;
                    }
                    catch (Exception ex2)
                    {
                        Cout.println("Loi tai 88837 " + ex2.ToString());
                        break;
                    }
                }
            case 88839:
                {
                    string text = inputDlg.tfInput.getText();
                    endDlg();
                    if (text.Length < 6 || text.Equals(string.Empty))
                    {
                        startOKDlg(mResources.ALERT_PRIVATE_PASS_1);
                        break;
                    }
                    try
                    {
                        startYesNoDlg(mResources.cancelAccountProtection, 888391, text, 8882, null);
                        break;
                    }
                    catch (Exception)
                    {
                        startOKDlg(mResources.ALERT_PRIVATE_PASS_2);
                        break;
                    }
                }
            case 888391:
                {
                    string s = (string)p;
                    endDlg();
                    Service.gI().clearAccProtect(int.Parse(s));
                    break;
                }
            case 888392:
                Service.gI().menu(4, menu.menuSelectedItem, 0);
                break;
            case 888393:
                if (loginScr == null)
                {
                    loginScr = new LoginScr();
                }
                loginScr.doLogin();
                Main.closeKeyBoard();
                break;
            case 888394:
                endDlg();
                break;
            case 888395:
                endDlg();
                break;
            case 101023:
                Main.numberQuit = 0;
                break;
            case 101024:
                Res.outz("output 101024");
                endDlg();
                break;
            case 101025:
                endDlg();
                if (ServerListScreen.loadScreen)
                {
                    serverScreen.switchToMe();
                }
                else
                {
                    serverScreen.show2();
                }
                break;
            case 101026:
                mSystem.onDisconnected();
                break;
            case 100001:
                Service.gI().getFlag(0, -1);
                InfoDlg.showWait();
                break;
            case 100002:
                if (loginScr == null)
                {
                    loginScr = new LoginScr();
                }
                loginScr.backToRegister();
                break;
            case 100005:
                if (Char.myCharz().statusMe == 14)
                {
                    startOKDlg(mResources.can_not_do_when_die);
                }
                else
                {
                    Service.gI().openUIZone();
                }
                break;
            case 100006:
                mSystem.onDisconnected();
                break;
            case 100016:
                ServerListScreen.SetIpSelect(17, issave: false);
                instance.doResetToLoginScr(serverScreen);
                ServerListScreen.waitToLogin = true;
                endDlg();
                break;
        }
    }

    public static void clearAllPointerEvent()
    {
        isPointerClick = false;
        isPointerDown = false;
        isPointerJustDown = false;
        isPointerJustRelease = false;
        isPointerSelect = false;
        GameScr.gI().lastSingleClick = 0L;
        GameScr.gI().isPointerDowning = false;
    }

    public static bool isWait()
    {
        return Char.isLoadingMap || LoginScr.isContinueToLogin || ServerListScreen.waitToLogin || ServerListScreen.isWait || SelectCharScr.isWait;
    }
    public static void clearImageCache()
    {
        foreach (var img in _imageCache.Values)
            img?.Dispose(); // gọi Dispose() từng Image → giải phóng SKBitmap khỏi RAM
        _imageCache.Clear(); // xóa hết key-value trong Dictionary
    }
}
