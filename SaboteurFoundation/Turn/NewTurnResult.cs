namespace SaboteurFoundation.Turn
{
    public class NewTurnResult : TurnResult
    {
        public Player NextPlayer { get; }

        public NewTurnResult(Player next)
        {
            NextPlayer = next;
        }
    }
}
