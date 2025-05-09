using System.Collections.Generic;
using TypedMigrate.Example.GameStates;

namespace TypedMigrate.Example.Migration
{
    public static class Migrations
    {
        public static GameState ToLast(this GameStateV3 fromState) =>
            new()
            {
                LastReachedLevel = fromState.lastReachedLevel,
                LastAvailableLevel = fromState.lastReachedLevel + 10,
                PlayerProfile = fromState.playerProfile,
            };

        public static GameStateV3 ToV3(this GameStateV2 fromState) =>
            new()
            {
                lastReachedLevel = fromState.lastReachedLevel,
                playerProfile = new PlayerProfile
                {
                    playerName = fromState.playerName,
                    playerLevel = fromState.playerLevel,
                    coins = fromState.coins,
                    availableSkins = fromState.availableSkins,
                    equippedSkinId = fromState.availableSkins[0]
                }
            };

        public static GameStateV2 ToV2(this GameStateV1 fromState) =>
            new()
            {
                playerName = fromState.playerName,
                playerLevel = fromState.playerLevel,
                lastReachedLevel = fromState.lastReachedLevel,
                coins = new List<Resource>()
                {
                    new()
                    {
                        amount = fromState.coinsAmount,
                        id = "Gold"
                    }
                },
                availableSkins = new List<string>()
                {
                    "Default"
                }
            };
    }
}