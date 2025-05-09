using TypedMigrate.Example.GameStates;
using TypedMigrate.Core;
using TypedMigrate.Deserializers;

namespace TypedMigrate.Example.Migration
{
    public static class FluentParser
    {
        /// <summary>
        /// A concise parser for game state data using a fluent API.
        /// Parsing always begins by attempting to deserialize the data using the latest game state version.
        /// If that fails, the parser falls back to previous versions and migrates them forward.
        /// This behavior is implemented using iterator methods like <c>ParseAndMigrate</c> and <c>Parse</c>,
        /// which defer execution—so the final methods in the chain are executed first.
        /// 
        /// This example also illustrates how different game state versions may use different deserialization mechanisms.
        /// For instance, older versions might use Newtonsoft.Json, while newer ones may switch to MessagePack.
        /// </summary>
        /// <param name="data">The raw game state data in serialized form.</param>
        /// <returns>The parsed and migrated game state object of the latest version, or <c>null</c> if parsing failed.</returns>
        public static GameState Deserialize(this byte[] data) => data
            .Deserialize(d => d.TryDeserializeNewtonsoft<GameStateV1>())
            .DeserializeAndMigrate(d => d.TryDeserializeNewtonsoft<GameStateV2>(), v1 => v1.ToV2()) 
            .DeserializeAndMigrate(d => d.TryDeserializeMessagePack<GameStateV3>(), v2 => v2.ToV3())
            .DeserializeAndMigrate(d => d.TryDeserializeMessagePack<GameState>(), v3 => v3.ToLast())
            .Finish();
    }
}