using System;

namespace NRO_Skia.Main.Until
{
    // Giả định các class nro, main được định nghĩa trong cùng Project hoặc đã được tham chiếu
    public class AutoPoint : IActionListener, IChatable
    {
        private static AutoPoint instance;

        public static byte typePotential;
        public static bool isAutoPoint;
        public static int damageToAuto;
        public static int hpToAuto;
        public static int mpToAuto;

        private static readonly string[] inputDamageAuto = new string[] { "Nhập Sức Đánh Mà Bạn Muốn Auto", "Sức Đánh" };
        private static readonly string[] inputHPAuto = new string[] { "Nhập HP Mà Bạn Muốn Auto", "HP" };
        private static readonly string[] inputMPAuto = new string[] { "Nhập MP Mà Bạn Muốn Auto", "MP" };

        // Potential Types
        public const byte HP = 0;
        public const byte MP = 1;
        public const byte DAMAGE = 2;
        public const byte DEFENSE = 3;
        public const byte CRITICAL = 4;

        public static AutoPoint GetInstance()
        {
            if (instance == null)
            {
                instance = new AutoPoint();
            }
            return instance;
        }

        public static void Update()
        {
            if (isAutoPoint)
            {
                DoIt();
            }
        }

        public void OnChatFromMe(string text, string to)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(ChatTextField.gI().tfChat.getText()))
            {
                Service.gI().chat(text);
                ResetChatTextField();
                return;
            }

            string chatStr = ChatTextField.gI().strChat;

            if (chatStr.Equals(inputDamageAuto[0]))
            {
                HandleAutoTarget(0, "Auto Cộng Sức Đánh Tới: ", "Sức Đánh Không Hợp Lệ, Vui Lòng Nhập Lại!");
            }
            else if (chatStr.Equals(inputHPAuto[0]))
            {
                HandleAutoTarget(1, "Auto Cộng HP Tới: ", "HP Không Hợp Lệ, Vui Lòng Nhập Lại!");
            }
            else if (chatStr.Equals(inputMPAuto[0]))
            {
                HandleAutoTarget(2, "Auto Cộng MP Tới: ", "MP Không Hợp Lệ, Vui Lòng Nhập Lại!");
            }
            else
            {
                Service.gI().chat(text);
            }

            ResetChatTextField();
        }

        private void HandleAutoTarget(int type, string successMsg, string errorMsg)
        {
            try
            {
                int value = int.Parse(ChatTextField.gI().tfChat.getText());

                if (ChatTextField.gI().strChat.Equals(inputHPAuto[0]))
                {
                    if (value <= Char.myCharz().cHPGoc)
                    {
                        GameScr.info1.addInfo("HP Phải Lớn Hơn HP Hiện Tại (" + NinjaUtil.getMoneys((long)Char.myCharz().cHPGoc) + ")");
                        return;
                    }
                }
                else if (ChatTextField.gI().strChat.Equals(inputMPAuto[0]))
                {
                    // Logic MP nếu cần
                }
                else if (ChatTextField.gI().strChat.Equals(inputDamageAuto[0]))
                {
                    if (value <= Char.myCharz().cDamGoc)
                    {
                        GameScr.info1.addInfo("Sức Đánh Phải Lớn Hơn Sức Đánh Hiện Tại (" + NinjaUtil.getMoneys((long)Char.myCharz().cDamGoc) + ")");
                        return;
                    }
                }

                switch (type)
                {
                    case 0:
                        damageToAuto = value;
                        break;
                    case 1:
                        hpToAuto = value;
                        break;
                    case 2:
                        mpToAuto = value;
                        break;
                }

                isAutoPoint = true;
                GameScr.info1.addInfo(successMsg + NinjaUtil.getMoneys((long)value) + "\nAuto [STATUS: ON]");
            }
            catch (Exception)
            {
                GameScr.info1.addInfo(errorMsg);
            }
        }

        public void OnCancelChat() { }

        public void Perform(int idAction, object p)
        {
            switch (idAction)
            {
                case 3:
                    isAutoPoint = !isAutoPoint;
                    GameScr.info1.addInfo("Auto\n" + (isAutoPoint ? "[STATUS: ON]" : "[STATUS: OFF]"));
                    break;
                case 4:
                    OpenChatInput(inputDamageAuto);
                    break;
                case 5:
                    OpenChatInput(inputHPAuto);
                    break;
                case 6:
                    OpenChatInput(inputMPAuto);
                    break;
                case 7:
                    isAutoPoint = false;
                    GameScr.info1.addInfo("Đã Dừng Auto Cộng Điểm\n[STATUS: OFF]");
                    break;
            }
        }

        private void OpenChatInput(string[] inputConfig)
        {
            ChatTextField chatTF = ChatTextField.gI();
            chatTF.strChat = inputConfig[0];
           // chatTF.tfChat.field93 = inputConfig[1];

            // Sử dụng hàm startChat2 tương tự logic Java
            chatTF.startChat2(GetInstance(), "");
        }

        private static void ResetChatTextField()
        {
            ChatTextField.gI().strChat = "Chat";
            ChatTextField.gI().isShow = false;
        }

        public static void DoIt()
        {
            if (GameCanvas.gameTick % 20 != 0) return;

            if (Char.myCharz().cTiemNang < 100) return;

            bool damageCompleted = (damageToAuto == 0 || Char.myCharz().cDamGoc >= damageToAuto);
            bool hpCompleted = (hpToAuto == 0 || Char.myCharz().cHPGoc >= hpToAuto);
            bool mpCompleted = (mpToAuto == 0 || Char.myCharz().cMPGoc >= mpToAuto);

            if (damageCompleted && hpCompleted && mpCompleted)
            {
                isAutoPoint = false;
                GameScr.info1.addInfo("Auto Cộng Điểm Hoàn Thành!\n[STATUS: OFF]");
                return;
            }

            if (TryUpgradeStat(DAMAGE, Char.myCharz().cDamGoc, damageToAuto)) return;
            if (TryUpgradeStat(HP, Char.myCharz().cHPGoc, hpToAuto)) return;
            TryUpgradeStat(MP, Char.myCharz().cMPGoc, mpToAuto);
        }

        private static bool TryUpgradeStat(byte type, int currentValue, int targetValue)
        {
            if (targetValue == 0 || currentValue >= targetValue) return false;

            int remaining = targetValue - currentValue;
            bool isHPMP = (type == HP || type == MP);

            if (isHPMP) remaining /= 20;

            UpgradeCost costs = CalculateCosts(type, currentValue);

            if (remaining >= 100 && Char.myCharz().cTiemNang >= costs.Cost100)
            {
                Service.gI().upPotential(type, 100);
                return true;
            }
            if (remaining >= 10 && Char.myCharz().cTiemNang >= costs.Cost10)
            {
                Service.gI().upPotential(type, 10);
                return true;
            }
            if (remaining >= 1 && Char.myCharz().cTiemNang >= costs.Cost1)
            {
                Service.gI().upPotential(type, 1);
                return true;
            }

            return false;
        }

        private static UpgradeCost CalculateCosts(byte type, int currentValue)
        {
            long cost1 = 0, cost10 = 0, cost100 = 0;

            switch (type)
            {
                case DAMAGE:
                    int expForOneAdd = Char.myCharz().expForOneAdd;
                    cost1 = (long)currentValue * expForOneAdd;
                    cost10 = 10L * (2L * currentValue + 9L) / 2L * expForOneAdd;
                    cost100 = 100L * (2L * currentValue + 99L) / 2L * expForOneAdd;
                    break;

                case HP:
                case MP:
                    cost1 = (long)(currentValue + 1000);
                    cost10 = (long)(10 * (2 * (currentValue + 1000) + 180) / 2);
                    cost100 = (long)(100 * (2 * (currentValue + 1000) + 1980) / 2);
                    break;
            }

            return new UpgradeCost(cost1, cost10, cost100);
        }

        public void perform(int idAction, object p)
        {
            throw new NotImplementedException();
        }

        public void onChatFromMe(string text, string to)
        {
            throw new NotImplementedException();
        }

        public void onCancelChat()
        {
            throw new NotImplementedException();
        }

        private class UpgradeCost
        {
            public long Cost1 { get; set; }
            public long Cost10 { get; set; }
            public long Cost100 { get; set; }

            public UpgradeCost(long c1, long c10, long c100)
            {
                Cost1 = c1;
                Cost10 = c10;
                Cost100 = c100;
            }
        }
    }
}