using System;
using System.Collections.Generic;
using System.Text;
using Mod;

// ═══════════════════════════════════════════════════════
// AutoItemRef — chạy trên game thread, không dùng Thread
// ═══════════════════════════════════════════════════════
public class AutoItemRef
{
    public string Name;
    public int Id;
    public int IdCon;
    public bool AutoFlag;

    private long _nextUseTime = 0;
    private bool _warnedEmpty = false;
    private long _verifyIconUntil = 0;
    private bool _iconMappingChecked = false;
    private bool _iconMappingValid = true;
    private long _nextDebugTime = 0;
    private bool _hadActiveBuff = false;

    public AutoItemRef(string name, int id, int idCon)
    {
        Name = name;
        Id = id;
        IdCon = idCon;
        AutoFlag = true;
    }

    // Gọi mỗi frame từ game thread
    public void Tick()
    {
        if (!AutoFlag) return;
        if (Char.myCharz()?.meDead == true) return;
        if (Lib.TimeNow() < _nextUseTime) return;

        long now = Lib.TimeNow();

        if (!Lib.ExistItemBag(Id))
        {
            if (!_warnedEmpty)
            {
                GameScr.info1?.addInfo($"|4|Đã hết {Name}, đang chờ...");
                Logger.Log("[AutoItem] Đã hết " + Name + ", đang chờ...");
                _warnedEmpty = true;
            }
            _nextUseTime = now + 10_000L;
            return;
        }
        _warnedEmpty = false;

        // Có IdCon → chờ buff hết mới dùng lại
        if (IdCon != -1)
        {
            int timeLeft = Lib.GetItemTimeInSeconds(IdCon);
            if (timeLeft > 0)
            {
                if (now >= _nextDebugTime)
                {
                    Logger.Log("[AutoItem] " + Name + ": idCon=" + IdCon + " hợp lệ, timeLeft=" + timeLeft + "s. Buff đang có: " + GetActiveBuffIconsDebug());
                    _nextDebugTime = now + 15_000L;
                }
                _iconMappingChecked = true;
                _iconMappingValid = true;
                _hadActiveBuff = true;
                _verifyIconUntil = 0;
                _nextUseTime = now + timeLeft * 1000L;
                return;
            }

            // Buff vừa hết → đợi cooldown trước khi dùng lại
            if (_hadActiveBuff)
            {
                _hadActiveBuff = false;
                long cooldown = IdCon == 379 ? 90_000L : 30_000L;
                _nextUseTime = now + cooldown;
                GameScr.info1?.addInfo($"|2|{Name}: buff hết, đợi {cooldown / 1000}s...");
                Logger.Log("[AutoItem] " + Name + ": buff hết, đợi " + (cooldown / 1000) + "s trước khi dùng lại");
                return;
            }

            if (_verifyIconUntil > 0)
            {
                if (now < _verifyIconUntil)
                {
                    _nextUseTime = now + 1000L;
                    return;
                }

                _verifyIconUntil = 0;
                _iconMappingChecked = true;
                _iconMappingValid = false;

                if (now >= _nextDebugTime)
                {
                    GameScr.info1?.addInfo($"|1|[AutoItem] {Name}: idCon={IdCon} có thể sai. Buff đang có: {GetActiveBuffIconsDebug()}");
                    Logger.Log("[AutoItem] " + Name + ": idCon=" + IdCon + " có thể sai. Buff đang có: " + GetActiveBuffIconsDebug());
                    _nextDebugTime = now + 15_000L;
                }

                _nextUseTime = now + 30_000L;
                return;
            }
        }

        Lib.UseItem(Id);
        Logger.Log("[AutoItem] Dùng item " + Name + " (itemId=" + Id + ", idCon=" + IdCon + ")");

        if (IdCon == -1)
        {
            _nextUseTime = now + 30_000L;
            return;
        }

        _verifyIconUntil = now + 5_000L;
        _nextUseTime = now + 1_000L;
    }

    private static string GetActiveBuffIconsDebug()
    {
        if (Char.vItemTime == null || Char.vItemTime.size() == 0) return "(none)";

        var sb = new StringBuilder();
        for (int i = 0; i < Char.vItemTime.size(); i++)
        {
            if (Char.vItemTime.elementAt(i) is not ItemTime it) continue;
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(it.idIcon).Append(":").Append(it.coutTime);
        }
        return sb.Length == 0 ? "(none)" : sb.ToString();
    }
}

// ═══════════════════════════════════════════════════════
// AutoItemManager
// ═══════════════════════════════════════════════════════
public class AutoItemManager
{
    private static AutoItemManager? _instance;
    public static AutoItemManager Instance => _instance ??= new AutoItemManager();
    private readonly Dictionary<int, AutoItemRef> _items = new();
    private AutoItemManager() { }

    // Gọi từ AutoTrain.Update() mỗi frame
    public void Tick()
    {
        foreach (var item in _items.Values)
            item.Tick();
    }

    public void StartAutoItem(string name, int itemId, int idCon)
    {
        if (_items.TryGetValue(itemId, out var existing) && existing.AutoFlag)
        {
            GameScr.info1?.addInfo($"|1|{name} đã đang Auto rồi!");
            return;
        }
        _items[itemId] = new AutoItemRef(name, itemId, idCon);
        GameScr.info1?.addInfo($"|2|Đã bật Auto {name} (idCon={idCon})!");
        Logger.Log("[AutoItem] StartAutoItem name=" + name + ", itemId=" + itemId + ", idCon=" + idCon);
    }

    public void StopAutoItem(int itemId)
    {
        if (!_items.TryGetValue(itemId, out var item))
        {
            GameScr.info1?.addInfo("|1|Item này không đang Auto!");
            return;
        }
        GameScr.info1?.addInfo($"|2|Đã dừng Auto {item.Name}!");
        _items.Remove(itemId);
    }

    public void StopAll()
    {
        _items.Clear();
        GameScr.info1?.addInfo("|2|Đã dừng tất cả Auto item!");
    }

    public void ApplyFromConfig(string csv)
    {
        var newIds = new HashSet<int>();
        foreach (var part in csv.Split(','))
            if (int.TryParse(part.Trim(), out int id))
                newIds.Add(id);

        // Stop item không còn trong config
        var toStop = new List<int>();
        foreach (var id in _items.Keys)
            if (!newIds.Contains(id)) toStop.Add(id);
        foreach (var id in toStop) _items.Remove(id);

        // Start item mới
        foreach (var id in newIds)
        {
            if (_items.ContainsKey(id)) continue;
            var item = Lib.GetItemByID(id);
            string name = item?.template?.name ?? $"Item_{id}";
            int idCon = item?.template?.iconID ?? -1;
            StartAutoItem(name, id, idCon);
        }
    }
}

