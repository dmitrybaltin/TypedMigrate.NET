using TypedMigrate.Example.GameStates;
using TypedMigrate.Core;
using TypedMigrate.Deserializers;

namespace TypedMigrate.Example.Migration
{
    /// <summary>
    /// A step-by-step parser for game state data, providing a clear, versioned deserialization process.
    /// 
    /// The parser first attempts to deserialize the data using the most recent game state version.
    /// If deserialization fails, it tries previous versions, applying the necessary migrations to update the data.
    /// This process is done using delegates like <c>TryDeserializeMessagePack</c> and <c>TryDeserializeNewtonsoft</c> which handle the deserialization for different formats.
    /// 
    /// Note that different versions of the game state may use different deserialization mechanisms.
    /// For example, older versions might use Newtonsoft.Json, while newer ones may switch to MessagePack.
    /// 
    /// The methods accept a <c>byte[]</c> as input since it is a universal format for serialized data.
    public static class SimpleParser
    {
        /// <summary>
        /// Deserialize data and migrate it to GameStateLast if it is required. 
        /// </summary>
        /// <param name="data">The raw game state data in serialized form.</param>
        /// <returns>The parsed game state object, with migrations applied, or an exception if an unknown version is encountered.</returns>
        public static GameState Deserialize(this byte[] data) =>
            data.TryDeserializeMessagePack<GameState>()
                ?? data.DeserializeV3().ToLast();

        private static GameStateV3 DeserializeV3(this byte[] data) =>
            data.TryDeserializeMessagePack<GameStateV3>()
                ?? data.DeserializeV2().ToV3();

        private static GameStateV2 DeserializeV2(this byte[] data) =>
            data.TryDeserializeNewtonsoft<GameStateV2>()
                ?? data.DeserializeV1().ToV2();

        private static GameStateV1 DeserializeV1(this byte[] data) =>
            data.TryDeserializeNewtonsoft<GameStateV1>()
                ?? throw new System.Exception("Unknown game state version");
    }
}