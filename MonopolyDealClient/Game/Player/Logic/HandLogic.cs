using ImGuiNET;

namespace MonopolyDeal
{
    public partial class LocalPlayer
    {
        void OnTurn_HandLogic(Card card)
        {
            ImGui.SameLine();
            if (!ImGui.Button("Play"))
                return;

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
                        mGameplay.GetWindow<ChargeRentPopup>().Open(card);
                        break;


                    case ActionType.JustSayNo:
                    case ActionType.Hotel:
                    case ActionType.House:
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

            }

            if (card is PropertyCard)
            {
                if (card is WildCard)
                {
                    mGameplay.GetWindow<PlayWildCardPopup>().Open(card);
                }
            }
        }
        void RespondToAction_HandLogic(Card card)
        {

        }
        void NotTurn_HandLogic(Card card)
        {

        }
    }
}