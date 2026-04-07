using System;
using System.Collections.Generic;
using System.Text;

public class XmapPathfinder
{
    private static XmapPathfinder _instance;
    public static XmapPathfinder GetInstance() => _instance ??= new XmapPathfinder();

    // ═══════════════════════════════════════════
    // FIND PATH — BFS
    // ═══════════════════════════════════════════
    // FIX: Bỏ visited set toàn cục, thay bằng prune theo độ dài path.
    // Lý do: visited set cũ khiến node bị đánh dấu qua path INVALID,
    // dẫn đến path VALID đi qua cùng node đó bị bỏ qua hoàn toàn.
    // Graph map game nhỏ (~200 node) nên prune theo length là đủ hiệu quả.
    public int[]? FindPath(int targetMapID, int currentMapID, long cPower, bool hasTask30)
    {
        if (currentMapID == targetMapID) return new[] { currentMapID };

        var queue = new Queue<int[]>();
        queue.Enqueue(new[] { currentMapID });

        int[]? shortest = null;

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();

            // Prune: không cần đi tiếp nếu path hiện tại đã dài hơn/bằng kết quả tốt nhất
            if (shortest != null && path.Length >= shortest.Length) continue;

            int currentMap = path[^1];
            if (!DataXmap.linkMaps.TryGetValue(currentMap, out var neighbors)) continue;

            foreach (var nextMap in neighbors)
            {
                int nextID = nextMap.MapID;

                // Tránh cycle trong path hiện tại
                if (ContainsMap(path, nextID)) continue;
                if (!CanMoveToMap(currentMap, nextID, hasTask30)) continue;

                var newPath = AppendPath(path, nextID);

                if (nextID == targetMapID)
                {
                    if (IsValidPath(newPath, cPower, hasTask30))
                        shortest = newPath;
                    // Không enqueue đích
                    continue;
                }

                if (shortest == null || newPath.Length < shortest.Length)
                    queue.Enqueue(newPath);
            }
        }

        return shortest;
    }

    // ═══════════════════════════════════════════
    // FIND NEXT MAP TO GO
    // ═══════════════════════════════════════════
    public NextMap? FindNextMapToGo(int currentMapID, int nextMapID)
    {
        if (!DataXmap.linkMaps.TryGetValue(currentMapID, out var maps)) return null;

        NextMap? fallback = null;
        foreach (var nm in maps)
        {
            if (nm.MapID != nextMapID) continue;

            // Ưu tiên NPC hoặc walk action
            if (nm.NpcID != -1 || nm.walk) return nm;

            // Fallback: waypoint thuần (không có NameIndex)
            if (string.IsNullOrEmpty(nm.NameIndex1)) fallback = nm;
        }
        return fallback;
    }

    // ═══════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════
    private bool CanMoveToMap(int from, int to, bool hasTask30)
    {
        if (!hasTask30 && to >= 105 && to <= 110) return false;
        return true;
    }

    private bool IsValidPath(int[] path, long cPower, bool hasTask30)
    {
        if (HasFutureMapLoop(path)) return false;
        if (!hasTask30 && HasColdMap(path)) return false;

        foreach (int mapID in path)
        {
            if (!CheckPowerReq(mapID, cPower)) return false;
            if (DataXmap.IsFutureMap(mapID) && Char.myCharz().taskMaint.taskId <= 24) return false;
        }
        return true;
    }

    private bool CheckPowerReq(int mapID, long power)
    {
        if ((mapID == 155 || mapID == 166) && power < DataXmap.POWER_REQ_60B) return false;
        if (mapID != 155 && mapID >= 153 && mapID <= 159 && power < DataXmap.POWER_REQ_40B) return false;
        return true;
    }

    private bool HasFutureMapLoop(int[] path)
    {
        for (int i = 1; i < path.Length - 1; i++)
            if (path[i] == 102 && path[i + 1] == 24 && DataXmap.IsFutureMap(path[i - 1]))
                return true;
        return false;
    }

    private bool HasColdMap(int[] path)
    {
        foreach (int id in path)
            if (id >= 105 && id <= 110) return true;
        return false;
    }

    // ═══════════════════════════════════════════
    // ERROR MESSAGES
    // ═══════════════════════════════════════════
    public string GetPathErrorMessage(int targetID, int currentID, long power, bool hasTask30)
    {
        return CheckPowerError(targetID, power)
            ?? CheckTaskError(targetID)
            ?? CheckClanError(targetID)
            ?? (targetID == 160 && !Lib.ExistItemBag(992) ? "Không có Nhẫn thời không!" : null)
            ?? $"Không thể tìm đường từ map {currentID} → map {targetID}.";
    }

    private string? CheckPowerError(int mapID, long power)
    {
        if ((mapID == 155 || mapID == 166) && power < DataXmap.POWER_REQ_60B)
            return $"Yêu cầu sức mạnh {FormatNumber(DataXmap.POWER_REQ_60B)} cho map {mapID}.";
        if (mapID != 155 && mapID >= 153 && mapID <= 159 && power < DataXmap.POWER_REQ_40B)
            return $"Yêu cầu sức mạnh {FormatNumber(DataXmap.POWER_REQ_40B)} cho map {mapID}.";
        return null;
    }

    private string? CheckTaskError(int mapID)
    {
        if (DataXmap.IsFutureMap(mapID) && Char.myCharz().taskMaint.taskId <= 24)
            return $"Hoàn thành nhiệm vụ để vào map {mapID}.";
        return null;
    }

    private string? CheckClanError(int mapID)
    {
        if (Char.myCharz().clan != null) return null;
        if (DataXmap.IsKhiGasMap(mapID) || DataXmap.IsManhVoBT(mapID) || DataXmap.IsClanMap(mapID))
            return $"Cần có pt để vào map {mapID}.";
        return null;
    }

    // ═══════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════
    private static bool ContainsMap(int[] path, int mapID)
    {
        foreach (int id in path)
            if (id == mapID) return true;
        return false;
    }

    private static int[] AppendPath(int[] path, int nextID)
    {
        var newPath = new int[path.Length + 1];
        Array.Copy(path, newPath, path.Length);
        newPath[^1] = nextID;
        return newPath;
    }

    private static string FormatNumber(long n)
    {
        var s = n.ToString();
        var sb = new StringBuilder();
        int len = s.Length;
        for (int i = 0; i < len; i++)
        {
            if (i > 0 && (len - i) % 3 == 0) sb.Append(',');
            sb.Append(s[i]);
        }
        return sb.ToString();
    }
}
