using System;
using System.Collections.Generic;
using System.Linq;

namespace TypedMigrate.Core
{
    public static class FluentApi
    {
        /// <summary>
        /// Performs a lazy migration from a sequence of old states to new states using both deserialization and transformation logic.
        /// The deserialization function is used to parse the original byte array,
        /// and the migration function is applied to each existing state.
        /// </summary>
        /// <param name="fromEnumerable">The sequence of previous state versions to be migrated.</param>
        /// <param name="deserializer">A function that attempts to deserialize the byte array into a <typeparamref name="TStateTo"/>.</param>
        /// <param name="migrateFunc">A function to convert each <typeparamref name="TStateFrom"/> into <typeparamref name="TStateTo"/>.</param>
        /// <typeparam name="TStateFrom">The original type of the old state.</typeparam>
        /// <typeparam name="TStateTo">The target type of the new state, derived from <see cref="GameStateBase"/>.</typeparam>
        /// <returns>
        /// An enumerable that yields the deserialized state first (if successful), followed by migrated versions of each input state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="migrateFunc"/> is <c>null</c>.</exception>
        public static IEnumerable<DataAndState<TStateTo>> DeserializeAndMigrate<TStateFrom, TStateTo>(
            this IEnumerable<DataAndState<TStateFrom>> fromEnumerable,
            Func<byte[], TStateTo> deserializer,
            Func<TStateFrom, TStateTo> migrateFunc)
            where TStateTo : GameStateBase
        {
            foreach (var from in fromEnumerable)
            {
                var deserializedState = deserializer(from.Data);

                if (deserializedState != null)
                    yield return new DataAndState<TStateTo>(from.Data, deserializedState);

                if (migrateFunc == null)
                    throw new ArgumentNullException(nameof(migrateFunc));

                yield return new DataAndState<TStateTo>(from.Data, migrateFunc(from.State));
            }
        }
        
        /// <summary>
        /// Converts the input byte array into an enumerable containing a single item,
        /// serving as the starting point of a fluent API chain.
        /// This method must always appear first in the chain,
        /// but its code executes last due to the lazy nature of iterator methods.
        /// </summary>
        /// <param name="data">The raw byte array input to deserialize.</param>
        /// <param name="deserializer">A function that attempts to deserialize the byte array into a <typeparamref name="TStateTo"/>.</param>
        /// <typeparam name="TStateTo">The resulting type derived from <see cref="GameStateBase"/>.</typeparam>
        /// <returns>An enumerable containing the deserialized object, or empty if deserialization fails.</returns>
        public static IEnumerable<DataAndState<TStateTo>> Deserialize<TStateTo> (
            this byte[] data,
            Func<byte[], TStateTo> deserializer)
            where TStateTo : GameStateBase
        {
            var deserializedState = deserializer(data);
            if (deserializedState != null)
                yield return new DataAndState<TStateTo>(data, deserializedState);
        }

        /// <summary>
        /// Extracts the final deserialized state object from a sequence of <see cref="DataAndState{TStateTo}"/>.
        /// Assumes that the sequence contains the only element.
        /// </summary>
        /// <typeparam name="TStateTo">The type of the resulting game state.</typeparam>
        /// <param name="enumerable">The sequence of intermediate migration results.</param>
        /// <returns>The final state object, or null if the sequence is empty.</returns>
        public static TStateTo Finish<TStateTo>(
            this IEnumerable<DataAndState<TStateTo>> enumerable)
            where TStateTo : GameStateBase
            => enumerable.SingleOrDefault()?.State;
    }

    /// <summary>
    /// Represents a pair of a deserialized state object and its serialized data.
    /// Used as an intermediate value in the fluent migration API,
    /// helping to keep the chain syntax concise and readable.
    /// </summary>
    /// <typeparam name="TState">The type of the deserialized state object.</typeparam>
    public class DataAndState<TState>
    {
        public readonly TState State;
        public readonly byte[] Data;

        public DataAndState(byte[] data, TState state)
        {
            Data = data;
            State = state;
        }
    }
}