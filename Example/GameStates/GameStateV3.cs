using MessagePack;
using TypedMigrate.Core;

namespace TypedMigrate.Example.GameStates
{
    [MessagePackObject]
    public class GameStateV3 : GameStateBase
    {
        public GameStateV3() : base(3) {}

        [Key(1)] public PlayerProfile playerProfile;
        [Key(2)] public int lastReachedLevel;
    }
}