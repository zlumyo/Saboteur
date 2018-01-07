﻿namespace SaboteurFoundation.Turn
{
    public class PlayBufAction : TurnAction
    {
        public Player PlayerToBuf { get; }

        public PlayBufAction(Card card, Player player) : base(card)
        {
            PlayerToBuf = player;
        }
    }
}