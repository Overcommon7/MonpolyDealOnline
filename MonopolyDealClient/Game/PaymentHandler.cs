using ImGuiNET;
using System;
using System.Collections.Generic;
using Windows.Media.PlayTo;

namespace MonopolyDeal
{
    public static class PaymentHandler
    {
        struct PaymentInfo
        {
            public bool mPlayedSayNo;
            public int mPlayerNumber;
            public List<Card> mAsMoney;
            public List<Card> mNotMoney;
        }
        static List<PaymentInfo> mPayments = new();
        public static bool PaymentInProcess { get; private set; } = false;
        public static bool IsBeingPaid { get; private set; } = false;
        public static bool AllPlayersPaid { get; private set; } = false;
        public static void BeginPaymentProcess(bool gettingPaid)
        {
            AllPlayersPaid = false;
            PaymentInProcess = true;
            IsBeingPaid = gettingPaid;
            mPayments.Clear();
        }

        public static void OnPlayerPaid(PlayerManager playerManager, int playerNumber, byte[] data)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            var player = playerManager.GetOnlinePlayer(playerNumber);


            var strs = Format.ToString(data).Split('#', StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length < 2)
                throw new ArgumentException();

            var notMoneyCards = Serializer.GetCardsFromString<Card>(strs[0]);
            var asMoneyCards = Serializer.GetCardsFromString<Card>(strs[1]);
            PaymentInfo info = new PaymentInfo();
            info.mPlayerNumber = player.Number;
            info.mPlayedSayNo = false;
            info.mAsMoney = new List<Card>();
            info.mNotMoney = new List<Card>();

            int buildingTypesIndex = 0;
            List<SetType> buildingTypes = new List<SetType>();

            foreach (var setType in strs[3].Split(','))
                buildingTypes.Add(Enum.Parse<SetType>(setType));

            foreach (var card in notMoneyCards)
            {
                if (card is BuildingCard building)
                {
                    building.CurrentSetType = buildingTypes[buildingTypesIndex++];
                    player.PlayedCards.RemoveBuildingCard(building);
                }                   
                else if (card is PropertyCard property)
                {
                    player.PlayedCards.RemovePropertyCard(property);
                }                    
                info.mNotMoney.Add(card);
            }

            foreach (var card in asMoneyCards)
            {
                player.PlayedCards.RemoveMoneyCard(card);
                info.mNotMoney.Add(card);
            }

            mPayments.Add(info);
        }

        public static void OnPlayerSaidNo(GettingPaidWindow gettingPaidWindow, PlayerManager playerManager, int playerNumber)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            if (IsBeingPaid)
                gettingPaidWindow.PlayerSaidNo(playerNumber);

            PaymentInfo info = new PaymentInfo();
            info.mPlayerNumber = playerNumber;
            info.mPlayedSayNo = true;

            mPayments.Add(info);
        }

        public static void OnAllPlayersPaid()
        {
            AllPlayersPaid = true;
        }

        public static void PaymentComplete()
        {

        }

        public static void ImGuiDraw()
        {
            var playerManager = App.GetState<Gameplay>().PlayerManager;

            foreach (var payment in mPayments)
            {
                if (playerManager.LocalPlayer.Number == payment.mPlayerNumber)
                    continue;

                var player = playerManager.GetOnlinePlayer(payment.mPlayerNumber);

                if (!ImGui.TreeNode($"{player.Name}##{player.ID}PH")) 
                    continue;

                if (payment.mPlayedSayNo)
                {
                    ImGui.Text("Played Just Say No");
                    ImGui.TreePop();
                    continue;
                }

                ImGui.SeparatorText("Properties");

                int value = 0;
                foreach (var card in payment.mNotMoney)
                {
                    ImGui.Text(card.DisplayName());
                    value += card.Value;
                }

                ImGui.Spacing();
                ImGui.SeparatorText("Money");

                foreach (var card in payment.mAsMoney)
                {
                    ImGui.Text(card.DisplayName());
                    value += card.Value;
                }

                ImGui.Spacing();
                ImGui.Text($"Total Value: {value}");

                ImGui.TreePop();
            }
        }
    }
}
