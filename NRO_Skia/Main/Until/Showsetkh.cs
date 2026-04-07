//using System;
//using System.Collections.Generic;
//using System.Text;

//public static class ShowSetKH
//{
//    // ═══════════════════════════════════════════
//    // SET DEFINITIONS — load từ gender
//    // ═══════════════════════════════════════════
//    private static readonly Dictionary<string, (string label, string prefix)[]> SetDefs = new()
//    {
//        ["TĐ"] = new[]
//        {
//            ("Sgk",   "Set Sôngôku"),
//            ("Kok",   "Set Thần Vũ Trụ"),
//            ("txh",   "Set Thê"),
//            ("Gohan", "Set Gohan"),
//            ("Kirin", "Set Kirin"),
//        },
//        ["XD"] = new[]
//        {
//            ("KKR",   "Set Kakarot"),
//            ("CD ",   "Set Ca Đíc"),
//            ("CDM",   "Set Cađic M"),
//            ("Gohan", "Set Gohan"),
//            ("Nap",   "Set Nappa"),
//        },
//        ["NK"] = new[]   // Namek — mọi gender còn lại
//        {
//            ("Picolo",   "Set Picolo"),
//            ("Daimao",   "Set Pikkoro Daimao"),
//            ("Ốc tiêu",  "Set Ốc tiêu"),
//            ("Gohan",    "Set Gohan"),
//            ("Nail",     "Set Nail chiến binh"),
//        },
//    };

//    // ═══════════════════════════════════════════
//    // RESULT
//    // ═══════════════════════════════════════════
//    public record SetResult(string[] Lines, bool[] IsFullSet);

//    // ═══════════════════════════════════════════
//    // CORE — đếm + build lines
//    // ═══════════════════════════════════════════
//    public static SetResult GetSetResult()
//    {
//        string gender = Char.myCharz().cgender;
//        var defs = SetDefs.TryGetValue(gender, out var d) ? d : SetDefs["NK"];
//        int nSets = defs.Length;

//        // counts[setIdx][slotType 0-4]
//        var counts = new int[nSets][];
//        for (int i = 0; i < nSets; i++) counts[i] = new int[5];

//        CountItems(Char.myCharz().arrItemBag, defs, counts);
//        CountItems(Char.myCharz().arrItemBox, defs, counts);
//        CountItems(Char.myCharz().arrItemBody, defs, counts);

//        var lines = new string[nSets];
//        var isFullSet = new bool[nSets];

//        for (int i = 0; i < nSets; i++)
//        {
//            int[] c = counts[i];
//            isFullSet[i] = c[0] >= 1 && c[1] >= 1 && c[2] >= 1 && c[3] >= 1 && c[4] >= 1;

//            bool hasAny = false;
//            foreach (int v in c) if (v != 0) { hasAny = true; break; }

//            lines[i] = hasAny
//                ? $"{defs[i].label} [{c[0]}]-[{c[1]}]-[{c[2]}]-[{c[3]}]-[{c[4]}]"
//                : "";
//        }

//        return new SetResult(lines, isFullSet);
//    }

//    // ═══════════════════════════════════════════
//    // PAINT VARIANTS
//    // ═══════════════════════════════════════════

//    // Vẽ tại x, y tùy chỉnh
//    public static void PaintDOKH(mGraphics g, int x, int y)
//    {
//        var result = GetSetResult();
//        int h = mFont.tahoma_7b_yellow.getHeight();
//        DrawLines(g, result, x, y, h);
//    }

//    // Vẽ trái giữa màn hình
//    public static void PaintDOKH(mGraphics g)
//    {
//        var result = GetSetResult();
//        int h = mFont.tahoma_7b_yellow.getHeight();
//        int totalH = result.Lines.Length * h;
//        int yStart = (GameCanvas.h - totalH) / 2;
//        DrawLines(g, result, 10, yStart, h);
//    }

//    // Vẽ phải với Y tùy chỉnh
//    public static void paintDOKHRight(mGraphics g, int yStart)
//    {
//        var result = GetSetResult();
//        int h = mFont.tahoma_7b_yellow.getHeight();

//        for (int i = 0; i < result.Lines.Length; i++)
//        {
//            string line = result.Lines[i];
//            if (string.IsNullOrEmpty(line)) continue;

//            int xPos = GameCanvas.w - mFont.tahoma_7.getWidth(line) - 10;
//            var font = result.IsFullSet[i] ? mFont.tahoma_7_yellow : mFont.tahoma_7_white;
//            font.drawString(g, line, xPos, yStart + i * h, 0);
//        }
//    }

//    // ═══════════════════════════════════════════
//    // PRIVATE HELPERS
//    // ═══════════════════════════════════════════
//    private static void DrawLines(mGraphics g, SetResult result, int x, int y, int h)
//    {
//        for (int i = 0; i < result.Lines.Length; i++)
//        {
//            string line = result.Lines[i];
//            if (string.IsNullOrEmpty(line)) continue;
//            var font = result.IsFullSet[i] ? mFont.tahoma_7_yellow : mFont.tahoma_7_white;
//            font.drawString(g, line, x, y + i * h, 0);
//        }
//    }

//    private static void CountItems(
//        Item[]? items,
//        (string label, string prefix)[] defs,
//        int[][] counts)
//    {
//        if (items == null) return;

//        foreach (var item in items)
//        {
//            try
//            {
//                if (item == null
//                    || item.template.type > 4
//                    || item.itemOption[1] == null
//                    || item.itemOption[2] == null) continue;

//                string opt1 = item.itemOption[1].optionTemplate.name;
//                string opt2 = item.itemOption[2].optionTemplate.name;
//                int slot = item.template.type;

//                for (int i = 0; i < defs.Length; i++)
//                {
//                    string prefix = defs[i].prefix;
//                    if (opt1.StartsWith(prefix) || opt2.StartsWith(prefix))
//                    {
//                        counts[i][slot]++;
//                        break; // 1 item chỉ thuộc 1 set
//                    }
//                }
//            }
//            catch { }
//        }
//    }
//}