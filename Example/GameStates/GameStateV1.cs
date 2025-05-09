using System;
using TypedMigrate.Core;

namespace TypedMigrate.Example.GameStates
{
    [Serializable]
    public class GameStateV1 : GameStateBase
    {
        public GameStateV1() : base(1) {}

        public string playerName;
        public int playerLevel;
        public int lastReachedLevel;
        public int coinsAmount;
    }
}