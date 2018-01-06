namespace SaboteurFoundation.Turn
{
    public class NewTurnResult
    {
        public Player NextPlayer { get; }

        public NewTurnResult(Player next)
        {
            NextPlayer = next;
        }
    }
}
