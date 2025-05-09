using System;
using System.Collections.Generic;
using TypedMigrate.Core;

namespace TypedMigrate.Example.GameStates
{
    [Serializable]
    public class GameStateV2 : GameStateBase
    {
        public GameStateV2() : base(2) {}

        public string playerName;
        public int playerLevel;
        public int lastReachedLevel;
        public List<Resource> coins;
        public List<string> availableSkins;
    }
}