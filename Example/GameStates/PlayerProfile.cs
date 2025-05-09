using System;
using System.Collections.Generic;
using MessagePack;

namespace TypedMigrate.Example.GameStates
{
    [Serializable]
    [MessagePackObject]
    public class PlayerProfile
    {
        [Key(0)] public string playerName;
        [Key(1)] public int playerLevel;
        [Key(2)] public List<Resource> coins;
        [Key(3)] public List<string> availableSkins;
        [Key(4)] public string equippedSkinId;
    }
}