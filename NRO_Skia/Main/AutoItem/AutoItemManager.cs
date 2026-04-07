using System;
using System.Collections.Generic;
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

        if (!Lib.ExistItemBag(Id))
        {
            if (!_warnedEmpty)
            {
                GameScr.info1?.addInfo($"|4|Đã hết {Name}, đang chờ...");
                _warnedEmpty = true;
            }
            _nextUseTime = Lib.TimeNow() + 10_000L;
            return;
        }
        _warnedEmpty = false;

        // Có IdCon → chờ buff hết mới dùng lại
        if (IdCon != -1)
        {
            int timeLeft = Lib.GetItemTimeInSeconds(IdCon);
            if (timeLeft > 0)
            {
                _nextUseTime = Lib.TimeNow() + timeLeft * 1000L;
                return;
            }
        }

        Lib.UseItem(Id);
        _nextUseTime = Lib.TimeNow() + (IdCon == -1 ? 30_000 : 3_000);
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
        GameScr.info1?.addInfo($"|2|Đã bật Auto {name}!");
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

