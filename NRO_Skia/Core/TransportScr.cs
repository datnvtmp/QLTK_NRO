public class TransportScr : mScreen, IActionListener
{
    public static TransportScr instance;

    public static Image ship;

    public static Image taungam;

    public sbyte type;

    public int speed = 5;

    public int[] posX;

    public int[] posY;

    public int[] posX2;

    public int[] posY2;

    private int cmx;

    private int n = 20;

    public short time;

    public short maxTime;

    public long last;

    public long curr;

    private bool isSpeed;

    private bool transNow;

    private int currSpeed;

    public TransportScr()
    {
        posX = new int[n];
        posY = new int[n];
        for (int i = 0; i < n; i++)
        {
            posX[i] = Res.random(0, GameCanvas.w);
            posY[i] = i * (GameCanvas.h / n);
        }
        posX2 = new int[n];
        posY2 = new int[n];
        for (int j = 0; j < n; j++)
        {
            posX2[j] = Res.random(0, GameCanvas.w);
            posY2[j] = j * (GameCanvas.h / n);
        }
    }

    public static TransportScr gI()
    {
        if (instance == null)
        {
            instance = new TransportScr();
        }
        return instance;
    }

    public override void switchToMe()
    {
        if (ship == null)
        {
            ship = GameCanvas.loadImage("/mainImage/myTexture2dfutherShip.png");
        }
        if (taungam == null)
        {
            taungam = GameCanvas.loadImage("/mainImage/taungam.png");
        }
        isSpeed = false;
        transNow = false;
        if (Char.myCharz().checkLuong() > 0 && type == 0)
        {
            center = new Command(mResources.faster, this, 1, null);
        }
        else
        {
            center = null;
        }
        currSpeed = 0;
        base.switchToMe();
    }

    public override void paint(mGraphics g)
    {
        // Nền đen
        g.setColor(0x000000);
        g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);

        // Logo giống màn login
        g.drawImage(LoginScr.imgTitle, GameCanvas.w / 2, GameCanvas.h / 2 - 24, StaticObj.BOTTOM_HCENTER);

        // Spinner shuriken
        GameCanvas.paintShukiren(GameCanvas.hw, GameCanvas.h / 2 + 24, g);

        // Chữ "Đang chuyển..."
        mFont.tahoma_7b_white.drawString(g, "Đang load map...", GameCanvas.w / 2, GameCanvas.h / 2, 2);

        base.paint(g);
    }
    public override void update()
    {
        Controller.isStopReadMessage = false;

        curr = mSystem.currentTimeMillis();
        if (curr - last >= 1000)
        {
            time++;
            last = curr;
        }

        // Giữ logic transport bắn gói tin
        if (!transNow)
        {
            currSpeed++;
            if (currSpeed >= 30)
            {
                transNow = true;
                Service.gI().transportNow();
            }
        }

        base.update();
    }

    public override void updateKey()
    {
        base.updateKey();
    }

    public void perform(int idAction, object p)
    {
        if (idAction == 1)
        {
            GameCanvas.startYesNoDlg(mResources.fasterQuestion, new Command(mResources.YES, this, 2, null), new Command(mResources.NO, this, 3, null));
        }
        if (idAction == 2 && Char.myCharz().checkLuong() > 0)
        {
            isSpeed = true;
            GameCanvas.endDlg();
            center = null;
        }
        if (idAction == 3)
        {
            GameCanvas.endDlg();
        }
    }
}
