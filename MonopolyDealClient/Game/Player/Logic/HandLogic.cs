using ImGuiNET;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        bool OnTurn_HandLogic(Card card, int id)
        {
            ImGui.SameLine();
            if (!ImGui.Button($"Play##{id}"))
                return false;

            if (card is ActionCard action)
            {
                switch (action.ActionType)
                {
                    case ActionType.DealBreaker:
                        mGameplay.GetWindow<DealBreakerPopup>().Open(card);
                        break;
                    case ActionType.WildRent:
                        mGameplay.GetWindow<WildRentPopup>().Open(card);
                        break;
                    case ActionType.SlyDeal:
                        mGameplay.GetWindow<SlyDealPopup>().Open(card);
                        break;
                    case ActionType.ForcedDeal:
                        mGameplay.GetWindow<ForcedDealPopup>().Open(card);
                        break;
                    case ActionType.Rent:
                        mGameplay.GetWindow<ChargeRentPopup>().Open(this, card);
                        break;
                    case ActionType.Hotel:
                    case ActionType.House:
                        mGameplay.GetWindow<PlayBuildingCardPopup>().Open(card);                     
                        break;


                    case ActionType.JustSayNo:
                    case ActionType.PassGo:
                        mGameplay.GetWindow<PlayActionCardPopup>().Open(card, TargetType.None);
                        break;
                    case ActionType.DebtCollector:
                        mGameplay.GetWindow<PlayActionCardPopup>().Open(card, TargetType.One);
                        break;
                    case ActionType.ItsMyBirthday:
                        mGameplay.GetWindow<PlayActionCardPopup>().Open(card, TargetType.All);
                        break;
                }

                return false;
            }

            if (card is PropertyCard property)
            {
                if (property is WildCard)
                {
                    mGameplay.GetWindow<PlayWildCardPopup>().Open(this, card);
                    return false;
                }

                Hand.RemoveCard(card);
                PlayedCards.AddPropertyCard(property);
                Client.SendData(ClientSendMessages.PlayPropertyCard, card.ID.ToString(), Number);
                return true;
            }

            if (card is MoneyCard money)
            {
                Hand.RemoveCard(card);
                PlayedCards.AddMoneyCard(money);
                Client.SendData(ClientSendMessages.PlayMoneyCard, card.ID.ToString(), Number);
                return true;
            }

            return false;
              
        }
        bool RespondToAction_HandLogic(Card card, int id)
        {
            return false;
        }
        bool NotTurn_HandLogic(Card card, int id)
        {
            return false;
        }
    }
}