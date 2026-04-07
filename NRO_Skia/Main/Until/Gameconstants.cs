using System;
using System.IO;

public static class GameConstants
{
    // ═══════════════════════════════════════════
    // FILE PATHS
    // ═══════════════════════════════════════════
    public static readonly string LIB_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib");
    public static readonly string XMAP_FILE = Path.Combine(LIB_DIR, "xmap.txt");
    public static readonly string CONFIG_FILE = Path.Combine(LIB_DIR, "config.txt");
    public static readonly string CONFIG_FILE_LOCAL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
    // ═══════════════════════════════════════════
    // ITEM IDs
    // ═══════════════════════════════════════════
    public const int ID_ITEM_TDLT = 521;
    public const int ID_ITEM_TDLT_ICON = 4387;
    public const int ID_ITEM_TDLT3 = 1524;
    public const int ID_ITEM_TICKET_PRIVATE = 1825;
    public static readonly int ID_ITEM_CSKB = 380;
    public static readonly int[] ID_ITEM_NHO = { 212, 211 };
    public static readonly int[] ID_ITEM_PORATA = { 454, 921, 1884 };

    // ═══════════════════════════════════════════
    // MAP & NPC IDs
    // ═══════════════════════════════════════════
    public const int ID_MAP_AURU = 0;
    public const int ID_MAP_NAMEK = 7;
    public const int ID_MAP_KAKAROT = 14;
    public const int ID_NPC_BUMA = 7;
    public const int ID_NPC_DENDE = 8;
    public const int ID_NPC_APPULE = 9;

    // ═══════════════════════════════════════════
    // THỜI GIAN (milliseconds)
    // ═══════════════════════════════════════════
    public const long TIME_UPDATE_TICK = 50L;
    public const long TIME_CHANGE_ZONE = 6000L;
    public const long TIME_MOB_SEARCH = 5000L;
    public const long TIME_SKILL_COOLDOWN = 1000L;
    public const long TIME_BUY_DELAY = 1500L;
    public const long TIME_BUFF_HS = 50000L;

    // ═══════════════════════════════════════════
    // GAME TICKS
    // ═══════════════════════════════════════════
    public const int TICK_REVIVE = 180;
    public const int TICK_USE_ITEM = 180;

    // ═══════════════════════════════════════════
    // THRESHOLDS
    // ═══════════════════════════════════════════
    public const int MIN_STAMINA = 5;
    public const int MIN_TDLT_TIME = 5;

    // ═══════════════════════════════════════════
    // BUY STEPS
    // ═══════════════════════════════════════════
    public const int STEP_GO_TO_MAP = 0;
    public const int STEP_BUY_ITEM = 1;
    public const int STEP_USE_ITEM = 2;
    public const int STEP_RETURN = 3;
}