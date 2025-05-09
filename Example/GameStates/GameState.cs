using MessagePack;
using TypedMigrate.Core;

namespace TypedMigrate.Example.GameStates
{
    [MessagePackObject]
    public class GameState : GameStateBase
    {
        public GameState() : base(4) {}

        [Key(1)] public PlayerProfile PlayerProfile;
        [Key(2)] public int LastReachedLevel;
        [Key(3)] public int LastAvailableLevel;
    }
}