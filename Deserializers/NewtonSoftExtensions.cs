using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TypedMigrate.Core;

namespace TypedMigrate.Deserializers
{
    public static class NewtonSoftExtensions
    {
        /// <summary>
        /// This serializer extracts the version from the TStateTo object  
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="TStateTo"></typeparam>
        /// <returns></returns>
        public static TStateTo TryDeserializeNewtonsoft<TStateTo>(this byte[] data)
            where TStateTo : GameStateBase, new()
        {
            var json = Encoding.UTF8.GetString(data);
            
            var baseState = JsonConvert.DeserializeObject<GameStateBase>(json);

            var stateTo = new TStateTo();

            if (baseState.version != stateTo.version) 
                return null;
            
            JsonConvert.PopulateObject(json, stateTo);
            return stateTo;
        }
        
        public static TStateTo TryDeserializeJObject<TStateTo>(this JObject jObject, int version)
            where TStateTo : GameStateBase =>
            jObject.ToObject<GameStateBase>().version == version 
                ? jObject.ToObject<TStateTo>() 
                : null;

        public static TStateTo TryDeserializeJObject<TStateTo>(this JObject jObject)
            where TStateTo : GameStateBase, new()
        {
            var baseState = jObject.ToObject<GameStateBase>();

            var stateTo = new TStateTo();
            
            return baseState.version == stateTo.version 
                ? jObject.ToObject<TStateTo>()
                : null;
        }

        public static byte[] SerializeNewtonsoft<TState>(this TState state)
        {
            var jsonString = JsonConvert.SerializeObject(state);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            return bytes;
        }
    }
}