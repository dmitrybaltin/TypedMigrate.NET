using System;
using MessagePack;

namespace TypedMigrate.Example.GameStates
{
    [Serializable]
    [MessagePackObject]
    public class Resource
    {
        [Key(0)] public string id;
        [Key(1)] public int amount;
    }
}