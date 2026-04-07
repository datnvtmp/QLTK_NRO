public class Waypoint : IActionListener
{
    public short minX, minY, maxX, maxY;
    public bool isEnter, isOffline;
    public PopUp popup;

    public Waypoint(short minX, short minY, short maxX, short maxY, bool isEnter, bool isOffline, string name)
    {
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
        this.isEnter = isEnter;
        this.isOffline = isOffline;
        name = Res.changeString(name);

        // TỐI ƯU 1: Cache data, tránh gọi hàm get object nhiều lần
        var myChar = Char.myCharz();
        int mapId = TileMap.mapID;

        // TỐI ƯU 2: Tách logic chặn cửa (Đọc dễ hiểu hơn 100 lần)
        bool isBlockedBase = (mapId == 21 || mapId == 22 || mapId == 23) && minX >= 0 && minX <= 24;
        bool isBlockedHome = isOffline && (
            (mapId == 0 && myChar.cgender != 0) ||
            (mapId == 7 && myChar.cgender != 1) ||
            (mapId == 14 && myChar.cgender != 2)
        );

        if (isBlockedBase || isBlockedHome) return;

        // TỐI ƯU 3: Tính toán tọa độ tâm 1 lần duy nhất
        int centerX = minX + (maxX - minX) / 2;
        int actionId = 1; // Mặc định là action 1

        // Xử lý logic tạo PopUp
        if (TileMap.isInAirMap() || mapId == 47)
        {
            if (minY > 150 && TileMap.isInAirMap()) return; // Bỏ qua nếu ko thỏa mãn
            popup = new PopUp(name, centerX, maxY - (minX <= 100 ? 48 : 24));
        }
        else if (!isEnter && !isOffline)
        {
            popup = new PopUp(name, minX, minY - 24);
        }
        else
        {
            actionId = 2; // Đổi sang action chuyển map
            if (TileMap.isTrainingMap())
                popup = new PopUp(name, minX, minY - 16);
            else
                popup = new PopUp(name, centerX, minY - (minY == 0 ? -32 : 16));
        }

        // TỐI ƯU 4: Gom nhóm code lặp (DRY) xuống cuối cùng
        popup.command = new Command(null, this, actionId, this);
        popup.isWayPoint = true;
        popup.isPaint = false;
        PopUp.addPopUp(popup);
        TileMap.vGo.addElement(this);
    }

    public void perform(int idAction, object p)
    {
        switch (idAction)
        {
            case 1:
                {
                    int yEnd2 = (maxY > minY + 24) ? (minY + maxY) / 2 : maxY;
                    GameScr.gI().auto = 0;

                    // Cache lại myChar ở đây để code ngắn hơn
                    var myChar = Char.myCharz();
                    myChar.currentMovePoint = new MovePoint(minX + (maxX - minX) / 2, yEnd2);
                    myChar.cdir = (myChar.cx - myChar.currentMovePoint.xEnd <= 0) ? 1 : -1;

                    Service.gI().charMove();
                    break;
                }
            case 2:
                {
                    GameScr.gI().auto = 0;
                    var myChar = Char.myCharz();

                    if (myChar.isInEnterOfflinePoint() != null)
                    {
                        Service.gI().charMove();
                        // InfoDlg.showWait(); // Đã comment theo ý muốn của bạn
                        Service.gI().getMapOffline();
                        Char.ischangingMap = true;
                    }
                    else if (myChar.isInEnterOnlinePoint() != null)
                    {
                        Service.gI().charMove();
                        Service.gI().requestChangeMap();
                        Char.isLockKey = true;
                        Char.ischangingMap = true;
                        GameCanvas.clearKeyHold();
                        GameCanvas.clearKeyPressed();
                        // InfoDlg.showWait();
                    }
                    else
                    {
                        myChar.currentMovePoint = new MovePoint(minX + (maxX - minX) / 2, maxY);
                        myChar.cdir = (myChar.cx - myChar.currentMovePoint.xEnd <= 0) ? 1 : -1;
                        myChar.endMovePointCommand = new Command(null, this, 2, null);
                    }
                    break;
                }
        }
    }
}