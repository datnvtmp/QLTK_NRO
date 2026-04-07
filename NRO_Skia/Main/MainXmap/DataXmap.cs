using System.Collections.Generic;

public static class DataXmap
{
    // ═══════════════════════════════════════════
    // DATA STORES
    // ═══════════════════════════════════════════
    public static readonly Dictionary<int, List<NextMap>> linkMaps = new();
    public static readonly Dictionary<string, int[]> planetDictionary = new();
    public static readonly HashSet<int> khiGasMapSet = new();
    public static readonly HashSet<int> manhVoBTMapSet = new();
    public static readonly HashSet<int> futureMapSet = new();

    // ═══════════════════════════════════════════
    // MAP ID ARRAYS
    // ═══════════════════════════════════════════
    public static readonly int[] idMapsNamek     = { 43, 22, 7, 8, 9, 11, 12, 13, 10, 31, 32, 33, 34, 43, 25 };
    public static readonly int[] idMapsXayda     = { 44, 23, 14, 15, 16, 17, 18, 20, 19, 35, 36, 37, 38, 52, 44, 26, 84, 113, 127, 129 };
    public static readonly int[] idMapsTraiDat   = { 42, 21, 0, 1, 2, 3, 4, 5, 6, 27, 28, 29, 30, 47, 42, 24, 53, 58, 59, 60, 61, 62, 55, 56, 54, 57 };
    public static readonly int[] idMapsTuongLai  = { 102, 92, 93, 94, 96, 97, 98, 99, 100, 103 };
    public static readonly int[] idMapsCold      = { 109, 108, 107, 110, 106, 105 };
    public static readonly int[] idMapsNappa     = { 68, 69, 70, 71, 72, 64, 65, 63, 66, 67, 73, 74, 75, 76, 77, 81, 82, 83, 79, 80, 131, 132, 133 };
    public static readonly int[] idMapsThapleo   = { 46, 45, 48, 50, 154, 155, 166 };
    public static readonly int[] idMapsManhVoBT  = { 153, 156, 157, 158, 159 };
    public static readonly int[] idMapsKhiGas    = { 149, 147, 152, 151, 148 };
    public static readonly int[] idMapsKhac      = { 181, 139, 140, 126 };

    // ═══════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════
    public const long POWER_REQ_40B = 40_000_000_000L;
    public const long POWER_REQ_60B = 60_000_000_000L;
    public const int CLAN_MAP_START = 53;
    public const int CLAN_MAP_END = 62;
    public const int NRD_MAP_START = 85;
    public const int NRD_MAP_END = 91;

    // ═══════════════════════════════════════════
    // STATIC CONSTRUCTOR
    // ═══════════════════════════════════════════
    static DataXmap()
    {
        InitializeHashSets();
        LoadLinkMaps();
        LoadNPCLinkMaps();
        AddPlanetXmap();
    }

    // ═══════════════════════════════════════════
    // INIT HASH SETS
    // ═══════════════════════════════════════════
    private static void InitializeHashSets()
    {
        foreach (int id in idMapsKhiGas) khiGasMapSet.Add(id);
        foreach (int id in idMapsManhVoBT) manhVoBTMapSet.Add(id);
        foreach (int id in idMapsTuongLai) futureMapSet.Add(id);
    }

    // ═══════════════════════════════════════════
    // LOAD LINK MAPS
    // ═══════════════════════════════════════════
    private static void LoadLinkMaps()
    {
        AddLinkMaps(new[] { 0, 21 });
        AddLinkMaps(new[] { 1, 47 });
        AddLinkMaps(new[] { 47, 111 });
        AddLinkMaps(new[] { 2, 24 });
        AddLinkMaps(new[] { 5, 29 });
        AddLinkMaps(new[] { 7, 22 });
        AddLinkMaps(new[] { 9, 25 });
        AddLinkMaps(new[] { 13, 33 });
        AddLinkMaps(new[] { 14, 23 });
        AddLinkMaps(new[] { 16, 26 });
        AddLinkMaps(new[] { 20, 37 });
        AddLinkMaps(new[] { 39, 21 });
        AddLinkMaps(new[] { 40, 22 });
        AddLinkMaps(new[] { 41, 23 });
        AddLinkMaps(new[] { 109, 105 });
        AddLinkMaps(new[] { 109, 106 });
        AddLinkMaps(new[] { 106, 107 });
        AddLinkMaps(new[] { 108, 105 });
        AddLinkMaps(new[] { 80, 105 });
        AddLinkMaps(new[] { 3, 27, 28, 29, 30 });
        AddLinkMaps(new[] { 11, 31, 32, 33, 34 });
        AddLinkMaps(new[] { 17, 35, 36, 37, 38 });
        AddLinkMaps(new[] { 109, 108, 107, 110, 106 });
        AddLinkMaps(new[] { 47, 46, 45, 48 });
        AddLinkMaps(new[] { 131, 132, 133 });
        AddLinkMaps(new[] { 42, 0, 1, 2, 3, 4, 5, 6 });
        AddLinkMaps(new[] { 43, 7, 8, 9, 11, 12, 13, 10 });
        AddLinkMaps(new[] { 52, 44, 14, 15, 16, 17, 18, 20, 19 });
        AddLinkMaps(new[] { 53, 58, 59, 60, 61, 62, 55, 56, 54, 57 });
        AddLinkMaps(new[] { 68, 69, 70, 71, 72, 64, 65, 63, 66, 67, 73, 74, 75, 76, 77, 81, 82, 83, 79, 80 });
        AddLinkMaps(new[] { 102, 92, 93, 94, 96, 97, 98, 99, 100, 103 });
        AddLinkMaps(new[] { 153, 156, 157, 158, 159 });
        AddLinkMaps(new[] { 46, 45, 48, 50, 154, 155, 166 });
        AddLinkMaps(new[] { 149, 147, 152, 151, 148 });
        AddLinkMaps(new[] { 139, 140 });
        AddLinkMaps(new[] { 160, 161, 162, 163 });
        AddLinkMaps(new[] { 84, 104 });
        AddLinkMaps(new[] { 123, 124 });
        AddLinkMaps(new[] { 122, 124 });

        // Waypoint đặc biệt map 124
        var nm = new NextMap(122, -1, "") { waypointIndex = 1 };
        EnsureKey(124);
        linkMaps[124].Add(nm);
    }

    // ═══════════════════════════════════════════
    // LOAD NPC LINK MAPS
    // ═══════════════════════════════════════════
    private static void LoadNPCLinkMaps()
    {
        AddNPCLink(19, 68, 12, "Đến Nappa", "", "", false, -1, -1, "Đồng ý", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(19, 109, 12, "Đến Cold", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);

        // Portal groups
        AddPortalGroup(24, new[] { 25, 26, 84 }, 10, new[] { 0, 1, 2 });
        AddPortalGroup(25, new[] { 24, 26, 84 }, 11, new[] { 0, 1, 2 });
        AddPortalGroup(26, new[] { 24, 25, 84 }, 12, new[] { 0, 1, 2 });
        AddPortalGroup(84, new[] { 24, 25, 26 }, 10, new[] { 0, 0, 0 });

        // Tương lai NPC (npc 38)
        AddNPCLink(27, 102, 38, "", "", "", false, -1, -1, "", "", "", "", "", "", 1, -1, -1);
        AddNPCLink(28, 102, 38, "", "", "", false, -1, -1, "", "", "", "", "", "", 1, -1, -1);
        AddNPCLink(29, 102, 38, "", "", "", false, -1, -1, "", "", "", "", "", "", 1, -1, -1);
        AddNPCLink(102, 27, 38, "", "", "", false, -1, -1, "", "", "", "", "", "", 1, -1, -1);

        // Clan
        AddNPCLink(27, 53, 25, "Vào (miễn phí)", "", "", false, -1, -1, "Tham Gia", "OK", "", "", "", "", -1, -1, -1);

        // Đại hội võ thuật
        AddNPCLink(52, 127, 44, "OK", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(52, 129, 23, "Đại Hội Võ Thuật Lần thứ 23", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(52, 113, 23, "Giải Siêu Hạng", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(113, 52, 22, "Về Đại Hội Võ Thuật", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(127, 52, 44, "Về Đại Hội Võ Thuật", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(129, 52, 23, "Về Đại Hội Võ Thuật", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);

        // Nappa portal
        AddNPCLink(68, 19, 12, "", "", "", false, -1, -1, "", "", "", "", "", "", 0, -1, -1);
        AddNPCLink(80, 131, 60, "", "", "", false, -1, -1, "", "", "", "", "", "", 0, -1, -1);
        AddNPCLink(131, 80, 60, "", "", "", false, -1, -1, "", "", "", "", "", "", 1, -1, -1);

        // Mảnh vỡ bảy tám
        AddNPCLink(5, 153, 13, "Nói chuyện", "Về khu vực bang", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(153, 5, 10, "Đảo Kame", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(153, 156, 47, "OK", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);

        // Tháp leo
        AddNPCLink(45, 48, 19, "", "", "", false, -1, -1, "", "", "", "", "", "", 3, -1, -1);
        AddNPCLink(48, 45, 20, "", "", "", false, -1, -1, "", "", "", "", "", "", 3, 0, -1);
        AddNPCLink(48, 50, 20, "", "", "", false, -1, -1, "", "", "", "", "", "", 3, 1, -1);
        AddNPCLink(50, 48, 44, "", "", "", false, -1, -1, "", "", "", "", "", "", 0, -1, -1);
        AddNPCLink(50, 154, 44, "", "", "", false, -1, -1, "", "", "", "", "", "", 1, -1, -1);
        AddNPCLink(154, 50, 55, "", "", "", false, -1, -1, "", "", "", "", "", "", 0, -1, -1);
        AddNPCLink(154, 155, 44, "", "", "", false, -1, -1, "", "", "", "", "", "", 1, -1, -1);
        AddNPCLink(155, 154, 44, "", "", "", false, -1, -1, "", "", "", "", "", "", 0, -1, -1);

        // Walk links
        AddNPCLink(155, 166, -1, "", "", "", true, 1400, 600, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(46, 47, -1, "", "", "", true, 80, 700, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(45, 46, -1, "", "", "", true, 80, 700, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(46, 45, -1, "", "", "", true, 380, 90, "", "", "", "", "", "", -1, -1, -1);

        // Khí Gas
        AddNPCLink(0, 149, 67, "OK", "", "", false, -1, -1, "Đồng ý", "", "", "", "", "", -1, -1, -1);

        // Map khác
        AddNPCLink(24, 139, 63, "", "", "", false, -1, -1, "", "", "", "", "", "", 0, -1, -1);
        AddNPCLink(139, 24, 63, "", "", "", false, -1, -1, "", "", "", "", "", "", 0, -1, -1);
        AddNPCLink(126, 19, 53, "OK", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(19, 126, 53, "OK", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);

        // Bình hút năng lượng
        AddNPCLink(52, 181, 44, "Bình hút năng lượng", "OK", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(181, 52, 44, "Về nhà", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);

        // Đảo rùa
        AddNPCLink(0, 123, 49, "Đồng ý", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
        AddNPCLink(122, 0, 49, "Về làng Aru", "", "", false, -1, -1, "", "", "", "", "", "", -1, -1, -1);
    }

    // ═══════════════════════════════════════════
    // PLANET DICTIONARY
    // ═══════════════════════════════════════════
    private static void AddPlanetXmap()
    {
        planetDictionary["Trái đất"] = idMapsTraiDat;
        planetDictionary["Namek"] = idMapsNamek;
        planetDictionary["Xayda"] = idMapsXayda;
        planetDictionary["Fide"] = idMapsNappa;
        planetDictionary["Tương lai"] = idMapsTuongLai;
        planetDictionary["Cold"] = idMapsCold;
        planetDictionary["Tháp leo"] = idMapsThapleo;
        planetDictionary["Khuc vực bang"] = idMapsManhVoBT;
        planetDictionary["Khi Gas"] = idMapsKhiGas;
        planetDictionary["Map Khác"] = idMapsKhac;
    }

    // ═══════════════════════════════════════════
    // PUBLIC HELPERS
    // ═══════════════════════════════════════════
    public static void AddLinkMaps(int[] link)
    {
        for (int i = 0; i < link.Length; i++)
        {
            EnsureKey(link[i]);
            if (i > 0) linkMaps[link[i]].Add(new NextMap(link[i - 1], -1, ""));
            if (i < link.Length - 1) linkMaps[link[i]].Add(new NextMap(link[i + 1], -1, ""));
        }
    }

    public static void AddNPCLink(int fromMap, int toMap, int npcID,
        string n1, string n2, string n3,
        bool walk, int x, int y,
        string p1, string p2, string p3,
        string p1v2, string p2v2, string p3v2,
        int idx1, int idx2, int idx3)
    {
        EnsureKey(fromMap);
        linkMaps[fromMap].Add(new NextMap(toMap, npcID, n1, n2, n3, walk, x, y,
            p1, p2, p3, p1v2, p2v2, p3v2, idx1, idx2, idx3));
    }

    private static void AddPortalGroup(int fromMap, int[] toMaps, int npcID, int[] indices)
    {
        for (int i = 0; i < toMaps.Length; i++)
            AddNPCLink(fromMap, toMaps[i], npcID, "", "", "", false, -1, -1,
                "", "", "", "", "", "", indices[i], -1, -1);
    }

    public static bool IsNRDMap(int mapID) => mapID >= NRD_MAP_START && mapID <= NRD_MAP_END;
    public static bool IsFutureMap(int mapID) => futureMapSet.Contains(mapID);
    public static bool IsKhiGasMap(int mapID) => khiGasMapSet.Contains(mapID);
    public static bool IsManhVoBT(int mapID) => manhVoBTMapSet.Contains(mapID);
    public static bool IsClanMap(int mapID) => mapID >= CLAN_MAP_START && mapID <= CLAN_MAP_END;

    // ═══════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════
    private static void EnsureKey(int mapId)
    {
        if (!linkMaps.ContainsKey(mapId))
            linkMaps[mapId] = new List<NextMap>();
    }
}