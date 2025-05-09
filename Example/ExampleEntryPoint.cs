using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TypedMigrate.Example.Migration;
using TypedMigrate.Example.GameStates;
using TypedMigrate.Deserializers;
using UnityEngine;

namespace TypedMigrate.Example
{
    public class ExampleEntryPoint : MonoBehaviour
    {
        private const string GameStateFileName = "gamestate.bin";

        private void Start()
        {
            TestParser($"{nameof(SimpleParser)} test", SimpleParser.Deserialize);
            TestParser($"{nameof(FluentParser)} test", FluentParser.Deserialize);
        }
        
        private void TestParser(string testName, Func<byte[], GameState> parse)
        {
            Debug.Log($"{testName} started");
            
            GenerateAndSaveInitialState();
                
            Debug.Log("Old state saved");
                
            var bytes = LoadGameState();

            var initialState = bytes.TryDeserializeNewtonsoft<GameStateV1>();
                
            PrintObject($"Old state loaded from the disk as '{initialState.GetType()}': ", initialState);
                
            var lastState = parse.Invoke(bytes);

            PrintObject($"The state successfully parsed to '{lastState.GetType()}': ", lastState);
            
            Debug.Log($"{testName} finished");
        }        
        
        private void GenerateAndSaveInitialState()
        {
            var state = new GameStateV1() {
                playerName = "Joker",
                playerLevel = 1,
                lastReachedLevel = 2,
                coinsAmount = 1000
            };
            var bytes = state.SerializeNewtonsoft();
            SaveGameState(bytes);
        }

        private static void SaveGameState(byte[] bytes)
        {
            var path = Path.Combine(Application.persistentDataPath, GameStateFileName);
            File.WriteAllBytes(path, bytes);
        }
        
        private static byte[] LoadGameState()
        {
            var path = Path.Combine(Application.persistentDataPath, GameStateFileName);
            return File.Exists(path) 
                ? File.ReadAllBytes(path) 
                : null;
        }

        private static void PrintObject(string title, object obj) => 
            Debug.Log($"{title}: {JsonConvert.SerializeObject(obj, Formatting.Indented)}");
    }
}