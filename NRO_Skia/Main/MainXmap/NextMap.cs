using System;
using System.Collections.Generic;

public class NextMap
{
    // ═══════════════════════════════════════════
    // DATA
    // ═══════════════════════════════════════════
    public int MapID;
    public int NpcID;
    public string NameIndex1 = "";
    public string NameIndex2 = "";
    public string NameIndex3 = "";
    public string NameIndex1Phu = "";
    public string NameIndex2Phu = "";
    public string NameIndex3Phu = "";
    public string NameIndex1Phu2 = "";
    public string NameIndex2Phu2 = "";
    public string NameIndex3Phu2 = "";
    public int indexNpc = -1;
    public int indexNpc2 = -1;
    public int indexNpc3 = -1;
    public bool walk;
    public int x = -1;
    public int y = -1;
    public int waypointIndex = -1;

    // ── Enter state (per-instance) ────────────
    private bool _isEntering;
    private bool _hasTeleported;
    private long _enterDelayStart;
    private long _teleportTime;

    // ── Static ────────────────────────────────
    private static readonly HashSet<string> _npcDaTuChoiKeo = new();
    public static bool nextSuKienHalloween;

    // ═══════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════
    public NextMap(int mapID, int npcID, string name)
        : this(mapID, npcID, name, "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1) { }

    public NextMap(int mapID, int npcID, string n1, string n2, string n3)
        : this(mapID, npcID, n1, n2, n3, false, -1, -1, "", "", "", "", "", "", -1, -1, -1) { }

    public NextMap(int mapID, int npcID,
        string n1, string n2, string n3,
        bool walk, int x, int y,
        string p1, string p2, string p3,
        string p1v2, string p2v2, string p3v2,
        int idx1, int idx2, int idx3)
    {
        MapID = mapID; NpcID = npcID;
        NameIndex1 = n1; NameIndex2 = n2; NameIndex3 = n3;
        this.walk = walk; this.x = x; this.y = y;
        NameIndex1Phu = p1; NameIndex2Phu = p2; NameIndex3Phu = p3;
        NameIndex1Phu2 = p1v2; NameIndex2Phu2 = p2v2; NameIndex3Phu2 = p3v2;
        indexNpc = idx1; indexNpc2 = idx2; indexNpc3 = idx3;
    }

    // ═══════════════════════════════════════════
    // GOTO MAP
    // ═══════════════════════════════════════════
    public void GotoMap()
    {
        if (walk)
        {
            if (x != -1 && y != -1)
                Char.myCharz().currentMovePoint = new MovePoint(x, y);
            return;
        }

        if (NpcID == -1)
        {
            var wp = GetWayPoint();
            if (wp != null) Enter(wp);
            return;
        }

        HandleNpcInteraction();
    }

    // ═══════════════════════════════════════════
    // NPC INTERACTION
    // ═══════════════════════════════════════════
    private void HandleNpcInteraction()
    {
        var npc = GetNPC(NpcID);
        if (npc == null || ModPro.confirming) return;

        // FIX: Dùng MainXmap.TeleportTo — đã xóa bản duplicate ở đây
        MainXmap.TeleportTo(npc.cx, npc.cy - 3);

        if (nextSuKienHalloween && indexNpc != -1)
        {
            string key = $"{MapID}-{NpcID}";
            if (!_npcDaTuChoiKeo.Contains(key))
            {
                ModPro.StartConfirmNpc((short)NpcID);
                _npcDaTuChoiKeo.Add(key);
                return;
            }
        }

        if (indexNpc != -1)
        {
            Service.gI().openMenu(NpcID);
            Service.gI().confirmMenu((short)NpcID, (sbyte)indexNpc);
            if (indexNpc2 != -1)
            {
                Service.gI().confirmMenu((short)NpcID, (sbyte)indexNpc2);
                if (indexNpc3 != -1)
                    Service.gI().confirmMenu((short)NpcID, (sbyte)indexNpc3);
            }
        }
        else if (!string.IsNullOrEmpty(NameIndex1))
        {
            ModPro.StartConfirmNpc((short)NpcID,
                NameIndex1, NameIndex2, NameIndex3,
                NameIndex1Phu, NameIndex2Phu, NameIndex3Phu,
                NameIndex1Phu2, NameIndex2Phu2, NameIndex3Phu2);
        }
    }

    // ═══════════════════════════════════════════
    // WAYPOINT
    // ═══════════════════════════════════════════
    public Waypoint? GetWayPoint()
    {
        // Ưu tiên index trực tiếp nếu đã biết
        if (waypointIndex >= 0 && waypointIndex < TileMap.vGo.size())
            return (Waypoint)TileMap.vGo.elementAt(waypointIndex);

        string targetName = GetMapName();
        for (int i = 0; i < TileMap.vGo.size(); i++)
        {
            var wp = (Waypoint)TileMap.vGo.elementAt(i);
            if (targetName == GetMapName(wp.popup))
                return wp;
        }
        return null;
    }

    public string GetMapName() => TileMap.mapNames[MapID];
    public string GetMapName(PopUp popup) => string.Join(" ", popup.says).Trim();

    // ═══════════════════════════════════════════
    // ENTER WAYPOINT
    //
    // Flow cũ: init tick → teleport lệch offset → walk vào → request  (3-N tick)
    // Flow mới:
    //   - teleDirect : teleport thẳng vào wp, request ngay          (1 tick)
    //   - bình thường: teleport vào wp, gửi charMove, request ngay  (1 tick)
    //   - edge wp (cạnh màn hình): cần walk vật lý vào vùng trigger,
    //     teleport vào rồi đợi server xác nhận qua _hasTeleported    (2 tick)
    //
    // Server chỉ cần nhận được charMove đúng tọa độ trong vùng wp
    // trước requestChangeMap — TeleportTo đã lo việc đó.
    // ═══════════════════════════════════════════
    public void Enter(Waypoint wp)
    {
        // Đặc biệt: map 166 → 155 đi trái (không có waypoint, dùng LoadMapLeft)
        if (TileMap.mapID == 166 && MapID == 155)
        {
            MainXmap.LoadMapLeft();
            return;
        }

        int targetX = CalculateTargetX(wp);
        int targetY = wp.maxY;
        if (targetX == -1 || targetY == -1) return;

        long now = mSystem.currentTimeMillis();
        bool isEdgeWp = wp.maxX < 60 || wp.minX > TileMap.pxw - 60;

        if (MainXmap.teleDirect)
        {
            // teleDirect: không delay gì cả
            MainXmap.TeleportTo(targetX, targetY);
            RequestMapChange(wp);
            return;
        }

        if (!_hasTeleported)
        {
            MainXmap.TeleportTo(targetX, targetY);
            Char.myCharz().cdir = Char.myCharz().cx < targetX ? 1 : -1;
            _hasTeleported = true;
            _teleportTime = now;

            if (!isEdgeWp)
            {
                // Wp giữa: charMove đã gửi trong TeleportTo → request luôn
                // Timing giữa các map do MainXmap.customMapDelay kiểm soát
                RequestMapChange(wp);
                ResetEnterState();
            }
            return;
        }

        // Edge wp: chờ tối thiểu 50ms để server nhận charMove trước
        if (now - _teleportTime >= 50)
        {
            RequestMapChange(wp);
            ResetEnterState();
        }
    }
    private int CalculateTargetX(Waypoint wp)
    {
        if (wp.maxX < 60) return 15;
        if (wp.minX > TileMap.pxw - 60) return TileMap.pxw - 15;
        return (wp.minX + wp.maxX) / 2;
    }

    private void RequestMapChange(Waypoint wp)
    {
        if (wp.isOffline) Service.gI().getMapOffline();
        else Service.gI().requestChangeMap();
    }

    private void ResetEnterState()
    {
        _isEntering = false;
        _hasTeleported = false;
        _teleportTime = 0;
    }

    // ═══════════════════════════════════════════
    // STATIC HELPERS
    // ═══════════════════════════════════════════
    public static Npc? GetNPC(int npcId)
    {
        for (int i = 0; i < GameScr.vNpc.size(); i++)
        {
            var npc = (Npc)GameScr.vNpc.elementAt(i);
            if (npc.template.npcTemplateId == npcId) return npc;
        }
        return null;
    }
}