public class TransportScr : mScreen, IActionListener
{
    public static TransportScr instance;

    public sbyte type;
    public short time;
    public short maxTime;

    public long last;
    public long curr;
    private bool isSpeed;
    private bool transNow;
    private int frameCount; // Dùng để đếm frame thay cho việc tính quãng đường

    public static TransportScr gI()
    {
        if (instance == null)
            instance = new TransportScr();
        return instance;
    }

    public override void switchToMe()
    {
        // Reset toàn bộ thông số
        isSpeed = false;
        transNow = false;
        frameCount = 0;
        time = 0;
        last = mSystem.currentTimeMillis();

        // Hiện nút Tăng Tốc nếu đi Tàu Vũ Trụ (type == 0) và có tốn thời gian
        if (Char.myCharz().checkLuong() > 0 && type == 0 && maxTime > 0)
        {
            center = new Command(mResources.faster, this, 1, null);
        }
        else
        {
            center = null;
        }

        base.switchToMe();
    }

    public override void paint(mGraphics g)
    {
        // 1. Nền đen hoàn toàn (Cực nhẹ cho GPU)
        g.setColor(0x000000);
        g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);

        // 2. Logo (Nếu có) và vòng xoay nhỏ để biết game không bị đơ
        if (LoginScr.imgTitle != null)
            g.drawImage(LoginScr.imgTitle, GameCanvas.w / 2, GameCanvas.h / 2 - 24, StaticObj.BOTTOM_HCENTER);
        GameCanvas.paintShukiren(GameCanvas.hw, GameCanvas.h / 2 + 24, g);

        // 3. Vẽ text thông báo
        if (type == 0 && maxTime > 0 && !isSpeed)
        {
            int remain = maxTime - time;
            if (remain < 0) remain = 0;
            mFont.tahoma_7b_white.drawString(g, $"Đang di chuyển... {remain}s", GameCanvas.w / 2, GameCanvas.h / 2 + 45, 2);
        }
        else
        {
            mFont.tahoma_7b_white.drawString(g, "Đang load map...", GameCanvas.w / 2, GameCanvas.h / 2 + 45, 2);
        }

        base.paint(g);
    }

    public override void update()
    {
        Controller.isStopReadMessage = false;

        // Bộ đếm thời gian thực (Giây)
        curr = mSystem.currentTimeMillis();
        if (curr - last >= 1000)
        {
            time++;
            last = curr;
        }

        if (!transNow)
        {
            if (type == 0) // LOGIC TÀU VŨ TRỤ
            {
                if (isSpeed)
                {
                    // Nếu bấm skip: cho chạy 30 frame (~0.5s) rồi nhảy map luôn
                    frameCount++;
                    if (frameCount > 30) DoTransport();
                }
                else
                {
                    // Nếu đợi thường: Chờ đủ thời gian maxTime mới nhảy map
                    if (time >= maxTime)
                    {
                        frameCount++;
                        if (frameCount > 10) DoTransport();
                    }
                }
            }
            else // LOGIC TÀU NGẦM (Type == 1)
            {
                // Tàu ngầm bản gốc đi rất lẹ, ta cho đợi khoảng 40 frame (~0.7s)
                frameCount++;
                if (frameCount > 40) DoTransport();
            }
        }

        base.update();
    }

    private void DoTransport()
    {
        transNow = true;
        Service.gI().transportNow();
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
        else if (idAction == 2 && Char.myCharz().checkLuong() > 0)
        {
            isSpeed = true;
            GameCanvas.endDlg();
            center = null;
        }
        else if (idAction == 3)
        {
            GameCanvas.endDlg();
        }
    }
}