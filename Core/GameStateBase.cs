using System;
using MessagePack;

namespace TypedMigrate.Core
{
    [Serializable]
    [MessagePackObject]
    public class GameStateBase
    {
        [Key(0)] public int version;

        public GameStateBase(int version) => this.version = version;
    }
}