# AutoCSKB - Tài liệu luồng hoạt động

## Tổng quan

AutoCSKB là hệ thống tự động giao dịch Capsule Siêu Không Bão (CSKB) giữa 2 loại bot:
- **Bot Up (TypeUp = 0)**: Tích lũy CSKB, khi đủ 99 cái → giao cho Bot Nhận
- **Bot Nhận (TypeContain = 1)**: Nhận CSKB từ Bot Up → cất vào rương đồ

Hệ thống bao gồm 3 thành phần:
- `AutoCSKB.cs` (NRO_Skia) — Chạy trên mỗi game client
- `Form1.cs` (QLTK) — Server quản lý trung tâm, điều phối giao dịch
- `GameConstants.cs` — Config item ID và số lượng

---

## Kiến trúc tổng thể

```
┌──────────┐     CSKB_FULL      ┌──────────┐     CSKB_RECEIVER_ID     ┌──────────┐
│  Bot Up  │ ──────────────────► │   QLTK   │ ◄──────────────────────  │ Bot Nhận │
│  (x5)    │ ◄────────────────── │  (Queue) │ ──────────────────────►  │  (x∞)    │
└──────────┘  CSKB_RECEIVER_ID  └──────────┘   Mở acc / Kill process  └──────────┘
                                      │
                                      ├─ CSKB_RECEIVER_DONE → Ghép bot Up tiếp theo
                                      └─ CSKB_RECEIVER_FULL → Mark full, tìm acc nhận khác
```

---

## State Machine

```
                    ┌─────────────────────────────────────────────┐
                    │              GIAO DỊCH (cả 2 bot)           │
                    │                                             │
                    │  Idle → Inviting → AddingItem → Locking     │
                    │                                    ↓        │
                    │                              Accepting      │
                    └──────────────────────────┬──────────────────┘
                                               │
                               ┌───────────────┴───────────────┐
                               │ Bot Up          │ Bot Nhận      │
                               │ → Idle          │ → GoHome      │
                               │ (chờ lần sau)   │    ↓           │
                               │                 │ WaitHome (1.5s)│
                               │                 │    ↓           │
                               │                 │ OpeningNpc     │
                               │                 │    ↓           │
                               │                 │ Storing        │
                               │                 │    ↓           │
                               │                 │ DONE / FULL    │
                               └─────────────────┘────────────────┘
```

---

## Luồng chi tiết

### 1. Bot Up (TypeUp = 0)

```
Vào game
  ↓
ApplyConfig ← QLTK gửi config khi CharInfo
  ↓
Update() mỗi frame:
  ├─ FindCSKBInBag(99) — tìm item CSKB có quantity >= 99
  │   ├─ Không có → return (chờ tích lũy)
  │   └─ Có → IsRunning = true
  │         ├─ Chưa báo QLTK? → GameClient.SendNow(CSKB_FULL) [1 lần duy nhất]
  │         └─ Di chuyển đến CSKBMap + CSKBZone
  │               ├─ StartGoToMap (cooldown 3s)
  │               └─ requestChangeZone (cooldown 5s)
  ↓
Idle: Chờ CharID từ QLTK (CSKB_RECEIVER_ID)
  ↓
Có CharID → findCharInMap(CharID) → giaodich(0, ...) mời trade [cooldown 5s]
  ↓
Inviting: Chờ trade panel mở (panel.type == 13)
  ├─ Timeout 15s → Idle (thử lại)
  └─ Panel mở → AddingItem
  ↓
AddingItem: Thêm CSKB vào giao dịch
  ├─ Delay 1.5s
  ├─ quantity = Math.Min(quantity, 99) — tối đa 99 cái
  ├─ giaodich(2, ...) gửi item
  └─ vMyGD.size() > 0 → Locking
  ↓
Locking: Khóa giao dịch
  ├─ Delay 2s
  └─ giaodich(5, ...) → Accepting
  ↓
Accepting: Chờ cả 2 bên khóa → Chấp nhận
  ├─ Cả 2 isLock → delay 1.5s → giaodich(7, ...)
  └─ GIAO DỊCH HOÀN TẤT → Idle (chờ tích lũy lại 99)
```

### 2. Bot Nhận (TypeContain = 1)

```
Vào game
  ↓
Gửi charID về QLTK: GameClient.SendNow(CSKB_RECEIVER_ID) [1 lần]
  ↓
Pre-check: Inventory đã có CSKB chưa?
  ├─ CÓ → GoHome (cất đồ trước, không nhận trade)
  └─ KHÔNG → tiếp tục
  ↓
Di chuyển đến CSKBMap + CSKBZone
  ↓
Idle: Chờ popup mời giao dịch (idAction == 11114)
  └─ Có popup → giaodich(1, ...) chấp nhận → Inviting
  ↓
Inviting → AddingItem: Chờ Bot Up thêm item
  └─ vFriendGD.size() > 0 → Locking
  ↓
Locking → Accepting → GIAO DỊCH HOÀN TẤT
  ↓
╔═══════════════════════════════════════════╗
║         POST-TRADE FLOW (Bot Nhận)         ║
╠═══════════════════════════════════════════╣
║                                            ║
║  GoHome: Di chuyển về nhà (cgender + 21)   ║
║    ├─ StartGoToMap (cooldown 3s)           ║
║    └─ Đến nơi → WaitHome                  ║
║                                            ║
║  WaitHome: Delay 1.5s                      ║
║    └─ Hết delay → OpeningNpc               ║
║                                            ║
║  OpeningNpc: Mở rương đồ                   ║
║    ├─ openMenu(3) gửi lệnh mở             ║
║    ├─ Detect: panel.type == 2 → Storing    ║
║    └─ Timeout 5s → retry openMenu(3)       ║
║                                            ║
║  Storing: Cất CSKB vào rương               ║
║    ├─ Check rương có CSKB? (HasCSKBInBox)  ║
║    │   └─ CÓ → FULL, thoát                ║
║    ├─ Check inven có CSKB? (FindCSKBInBag) ║
║    │   └─ KHÔNG → DONE, thoát              ║
║    ├─ useItem(1, 0, indexUI, -1)           ║
║    ├─ Cooldown 2s giữa mỗi lần            ║
║    └─ Retry tối đa 5 lần                  ║
║       └─ 5 lần fail → FULL, thoát          ║
║                                            ║
╚═══════════════════════════════════════════╝
```

---

## QLTK Queue System

```
Bot Up #1 báo CSKB_FULL ─┐
Bot Up #2 báo CSKB_FULL ─┤     ┌──────────────┐
Bot Up #3 báo CSKB_FULL ─┼───► │ _cskbUpQueue │
Bot Up #4 báo CSKB_FULL ─┤     │  [#1,#2,#3]  │
Bot Up #5 báo CSKB_FULL ─┘     └──────┬───────┘
                                       │
                              ProcessCSKBQueue()
                                       │
                    ┌──────────────────┴──────────────────┐
                    │  Bot service chạy rồi?               │
                    │  ├─ KHÔNG → Log cảnh báo, giữ queue  │
                    │  └─ CÓ → Tìm acc nhận offline,       │
                    │          chưa full                    │
                    │     ├─ Không có → Log, giữ queue      │
                    │     └─ Có → Ghép! Dequeue sender      │
                    │           receiver.IsSelected = true   │
                    └─────────────────────────────────────────┘

Khi acc nhận DONE:  → reset active → ProcessCSKBQueue() → ghép tiếp
Khi acc nhận FULL:  → mark full → re-queue sender → ProcessCSKBQueue() → tìm acc nhận khác
Khi bot Up offline: → xóa khỏi queue / hủy active trade
```

---

## 4 Trường hợp khi cất đồ (HandleStoring)

| # | Rương | Inventory | Hành động | Message |
|---|:-----:|:---------:|-----------|---------|
| 1 | ✗ | ✗ | Thoát game | `CSKB_RECEIVER_DONE` |
| 2 | ✓ | ✗ | Báo đầy + thoát | `CSKB_RECEIVER_FULL` |
| 3 | ✗ | ✓ | Cất vào rương (max 5 lần) | `DONE` hoặc `FULL` |
| 4 | ✓ | ✓ | Báo đầy + thoát ngay | `CSKB_RECEIVER_FULL` |

---

## Anti-Spam Cooldowns

| Request | Cooldown | Guard |
|---------|----------|-------|
| `StartGoToMap(CSKBMap)` | 3s | `_lastAction` |
| `requestChangeZone` | 5s | `_lastAction` + `LAST_TIME_FINISH_XMAP` |
| `StartGoToMap(homeMap)` | 3s | `_lastAction` + `!isXmaping` |
| `giaodich(0, ...)` mời trade | 5s | `_lastInvite` |
| `giaodich(2, ...)` thêm item | 1 lần | `_sentItem` flag |
| `giaodich(5, ...)` lock | 2s | `_lastAction` |
| `giaodich(7, ...)` accept | 1.5s | `_lastAction` |
| `openMenu(3)` | 1 lần + 5s retry | `_npcMenuOpened` flag |
| `useItem(...)` cất đồ | 2s | `_lastAction` |
| `CSKB_FULL` | 1 lần | `_isFullNotified` flag |
| `CSKB_RECEIVER_ID` | 1 lần | `_idSent` flag |

---

## Config & Constants

| Biến | File | Mô tả |
|------|------|-------|
| `ID_ITEM_CSKB` | `GameConstants.cs` | ID item CSKB (mặc định: 380) |
| `MAX_STORE_RETRY` | `AutoCSKB.cs` | Số lần thử cất tối đa (5) |
| `CSKBMap` | `CSKBConfig` | Map giao dịch |
| `CSKBZone` | `CSKBConfig` | Zone giao dịch (-1 = bất kỳ) |
| `CSKBType` | `CSKBConfig` | 0 = Bot Up, 1 = Bot Nhận |

---

## Message Types (QLTK ↔ Bot)

| Message | Hướng | Mô tả |
|---------|-------|-------|
| `CSKB_FULL` | Bot Up → QLTK | Đã đủ 99 CSKB, yêu cầu mở acc nhận |
| `CSKB_RECEIVER_ID` | Bot Nhận → QLTK → Bot Up | Gửi charID để Bot Up tìm nhân vật |
| `CSKB_RECEIVER_DONE` | Bot Nhận → QLTK | Cất đồ xong, tắt acc nhận |
| `CSKB_RECEIVER_FULL` | Bot Nhận → QLTK | Rương đầy, tắt acc nhận + mark full |
