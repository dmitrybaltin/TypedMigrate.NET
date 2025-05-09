using System;
using MessagePack;
using TypedMigrate.Core;

namespace TypedMigrate.Deserializers
{
    public static class MessagePackExtensions
    {
        /// <summary>
        /// Deserialization using MessagePack
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="TStateTo"></typeparam>
        /// <returns></returns>
        public static TStateTo TryDeserializeMessagePack<TStateTo>(this byte[] data)
            where TStateTo : GameStateBase, new()
        {
            try
            {
                var baseState = MessagePackSerializer.Deserialize<GameStateBase>(data);
                return baseState.version == new TStateTo().version 
                    ? MessagePackSerializer.Deserialize<TStateTo>(data)
                    : null;
            }
            catch (Exception ex)
            {
                //If the exception occurs we suppose that it is an incompatible version and return null result
                //Todo: Parse exceptions more carefully and throw out some of them  
                return null;
            }
        }
    }
}