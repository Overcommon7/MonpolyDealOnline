using ImGuiNET;
using System;
using System.Collections.Generic;
using Windows.Media.PlayTo;
using Windows.System;

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
        public static string PlayerNameBeingPaid { get; private set; } = string.Empty;
        public static int PlayerNumberBeingPaid { get; private set; } = -1;
        public static int AmountDue { get; private set; } = 0;
        public static bool PaymentInProcess { get; private set; } = false;
        public static bool IsBeingPaid { get; private set; } = false;
        public static bool AllPlayersPaid { get; private set; } = false;
        public static void BeginPaymentProcess(int playerNumberBeingPaid, int amountDue)
        {
            var playerManager = App.GetState<Gameplay>().PlayerManager;

            AllPlayersPaid = false;
            PaymentInProcess = true;
            PlayerNumberBeingPaid = playerNumberBeingPaid;
            PlayerNameBeingPaid = playerManager.LocalPlayer.Name;
            IsBeingPaid = PlayerNumberBeingPaid == playerManager.LocalPlayer.Number;
            AmountDue = amountDue;
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

            if (strs.Length > 2)
            {
                foreach (var setType in strs[3].Split(','))
                {
                    buildingTypes.Add(Enum.Parse<SetType>(setType));
                }                    
            }
            

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
                info.mAsMoney.Add(card);
            }

            mPayments.Add(info);
        }

        public static void OnPlayerSaidNo(GettingPaidWindow gettingPaidWindow, PlayerManager playerManager, int playerNumber)
        {
            if (playerNumber == playerManager.LocalPlayer.Number)
                return;

            PaymentInfo info = new PaymentInfo();
            info.mPlayerNumber = playerNumber;
            info.mPlayedSayNo = true;

            mPayments.Add(info);
        }

        public static void RejectedNo(PlayerManager playerManager, int playerNumber)
        {
            if (IsBeingPaid)
                return;

            if (playerManager.LocalPlayer.Number != playerNumber)
            {
                int index = mPayments.FindIndex(payment => payment.mPlayerNumber == playerNumber);
                if (index >= 0)
                    mPayments.RemoveAt(index);
            }
            else
            {
                var player = playerManager.GetOnlinePlayer(PlayerNumberBeingPaid);
                App.GetState<Gameplay>().GetWindow<PayPopup>().Open(playerManager.LocalPlayer, [$"Player {player.Name} Has Rejected Your No"]);
            }

        }

        public static void OnAllPlayersPaid()
        {
            AllPlayersPaid = true;
        }

        public static void PaymentComplete(LocalPlayer player, int playerNumber)
        {
            if (playerNumber != player.Number)
                return;

            foreach (var payment in mPayments)
            {
                foreach (var card in payment.mAsMoney)
                    player.PlayedCards.AddMoneyCard(card);

                foreach (var card in payment.mNotMoney)
                {
                    if (card is BuildingCard building)
                    {
                        building.SetAsMoney(true);
                        foreach (var setType in Constants.SET_TYPES)
                        {
                            if (player.PlayedCards.HasFullSetOfType(setType))
                            {
                                if (building.IsHotel && !player.PlayedCards.HasHotel(setType) ||
                                   building.IsHouse && !player.PlayedCards.HasHouse(setType))
                                {
                                    building.SetAsMoney(false);
                                    player.PlayedCards.AddBuildingCard(building, setType);
                                    break;
                                }
                            } 
                        }

                        if (building.AsMoney)
                            player.PlayedCards.AddMoneyCard(card);
                    }
                    else if (card is WildPropertyCard wildProperty)
                    {
                        if (!player.PlayedCards.HasFullSetOfType(wildProperty.SetType1))
                            wildProperty.SetCurrentType(wildProperty.SetType1);

                        if (!player.PlayedCards.HasFullSetOfType(wildProperty.SetType2))
                            wildProperty.SetCurrentType(wildProperty.SetType2);

                        player.PlayedCards.AddPropertyCard(wildProperty);
                    }
                    else if (card is WildCard wild)
                    {
                        wild.SetCurrentType(SetType.None);
                        foreach (var setType in Constants.SET_TYPES)
                        {
                            if (!player.PlayedCards.HasFullSetOfType(setType))
                            {
                                wild.SetCurrentType(setType);
                                break;
                            }
                        }

                        player.PlayedCards.AddPropertyCard(wild);
                    }
                    else if (card is PropertyCard property)
                    {
                        player.PlayedCards.AddPropertyCard(property);
                    }
                    else
                    {
                        player.PlayedCards.AddMoneyCard(card);
                    }                    
                }
            }
        }

        public static void ImGuiDraw(PlayerManager playerManager, Action<int>? sayNoLogic)
        {

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
                    sayNoLogic?.Invoke(payment.mPlayerNumber);
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
