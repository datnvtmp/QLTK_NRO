// ═══════════════════════════════════════════════════════
// AutoTrashItem — vứt item từng cái, mỗi 5 giây
// ═══════════════════════════════════════════════════════
namespace Mod
{
    public static class AutoTrashItem
    {
        public static List<int> TrashIds { get; } = new();
        public static bool IsRunning { get; private set; }

        private static long _nextDropTime = 0;
        private const int DELAY = 5_000; // 5 giây

        private static int _currentIndex = 0;

        public static void Start()
        {
            IsRunning = true;
            _currentIndex = 0;
        }

        public static void Stop()
        {
            IsRunning = false;
            _currentIndex = 0;
        }

        public static void SetIds(string csv)
        {
            TrashIds.Clear();
            foreach (var s in csv.Split(','))
                if (int.TryParse(s.Trim(), out int id))
                    TrashIds.Add(id);
        }

        // Gọi mỗi frame
        public static void Tick()
        {
            if (!IsRunning) return;
            if (Lib.TimeNow() < _nextDropTime) return;

            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null || TrashIds.Count == 0) return;

            for (int i = _currentIndex; i < bag.Length; i++)
            {
                var item = bag[i];
                if (item?.template == null) continue;
                if (!TrashIds.Contains(item.template.id)) continue;

                // vứt 1 item
                Service.gI().useItem(2, 1, (sbyte)i, -1);

                // cập nhật trạng thái
                _currentIndex = i + 1;
                _nextDropTime = Lib.TimeNow() + DELAY;
                return;
            }

            // nếu duyệt hết túi mà không còn item cần vứt
            _currentIndex = 0;
            _nextDropTime = Lib.TimeNow() + DELAY;
        }
    }
}