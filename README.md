# TypedMigrate.NET (C# & Unity)

A fully statically typed data migration system for C# and Unity.  

- ✅ **Truly statically typed code** — no explicit type casting or dynamic access at any stage.
- ✅ **Serializer-agnostic architecture** — compatible with any binary or text serializer and supports switching between serializers across versions.
- ✅ **Minimal boilerplate** — just declare your data and migration logic.
- ✅ **Concise, fluent-style syntax**.
- ✅ **High performance** with modern serializers like MessagePack or MemoryPack.

Usage Example:

```csharp
public static GameState Deserialize(this byte[] data) => data
    .Deserialize(d => d.TryDeserializeNewtonsoft<GameStateV1>())
    .DeserializeAndMigrate(d => d.TryDeserializeNewtonsoft<GameStateV2>(), v1 => v1.ToV2()) 
    .DeserializeAndMigrate(d => d.TryDeserializeMessagePack<GameStateV3>(), v2 => v2.ToV3())
    .DeserializeAndMigrate(d => d.TryDeserializeMessagePack<GameState>(), v3 => v3.ToLast())
    .Finish();
```

## The Problem and Existing Solutions

Player data — such as user profiles, inventories, in-game progress, unlocked content, and other personal assets — inevitably changes as the game evolves.
But unlike temporary data or app settings, this information is unique to each player and often represents months or even years of gameplay.

When the underlying data format changes (e.g. due to refactoring or new features), you can’t just discard or overwrite old data — that would mean losing a player’s entire history.
Instead, you must migrate each saved record to the new format, preserving all meaningful content while adapting it to the new schema.

On the server side, relational databases with tools like Entity Framework provide built-in support for schema migrations.  

On the client side, however — where data is typically stored as files — such tools are impractical.  
Instead, developers rely on plain .NET objects and serializers like System.Text.Json, Newtonsoft.Json, MessagePack, or MemoryPack.  
In this setup, there's no built-in versioning, no schema tracking, and no safe or ergonomic way to evolve data formats.

### Common Solution: JSON-based Migrations

One popular approach is to perform manual JSON migrations using tools like [FastMigrations.Json.Net](https://github.com/vangogih/FastMigrations.Json.Net):

```csharp
var oldSoftToken = rawJson["soft"];
var oldHardToken = rawJson["hard"];
var oldSoftValue = oldSoftToken.ToObject<int>();
var oldHardValue = oldHardToken.ToObject<int>();

var newWallet = new Dictionary<Currency, int>
{
    {Currency.Soft, oldSoftValue},
    {Currency.Hard, oldHardValue}
};
rawJson.Remove("soft");
rawJson.Remove("hard");
rawJson.Add("Wallet", JToken.FromObject(newWallet));
```

While this works, it comes with major drawbacks:

- ❌ No compile-time safety — all access is string-based and error-prone. High chance of runtime errors due to incorrect keys or casts.
- ❌ Tightly coupled to JSON format and Newtonsoft deserialization, which has very poor performance compared to modern binary serializers.

---

### An Alternative: Statically Typed Migrations

This [video](https://www.youtube.com/watch?v=d7K_77KRXHU) proposes an alternative approach based on statically typed migrations, where many errors can be caught at compile time — unlike dynamic JSON-based transformations, where most issues only emerge at runtime.

To achieve this, the author introduces a **new data class** for each schema version as it evolves.

Critics may argue that this approach requires maintaining multiple extra classes.  
But realistically, schema changes are not that frequent, and adding a couple of lightweight classes per patch won’t significantly bloat your codebase — especially considering the thousands of classes already present in most .NET projects.  
In many cases, **strong static typing** offers far more value than the inconvenience of a few additional types.

> While the balance between verbosity and safety is not always obvious, the statically typed way certainly deserves serious consideration and implementation — allowing users to decide what works best for their needs.

### Limitations of the Original Approach

Despite its good direction, the implementation shown in the video has some key limitations:

- ❌ It’s **not fully statically typed** — type casts from a base `IMigrationStep` interface are still required.
- ❌ It remains tightly coupled to a specific JSON serializer, though this is relatively easy to improve.

---

## What This Project Does Differently

This project takes the statically typed approach to its logical conclusion:

- ✅ **True static typing** — no casting, no reflection, no runtime type inspection.
- ✅ **Serializer-agnostic** — works with both text and binary formats.
- ✅ **Maximum performance** when using fast serializers.
- ✅ **Clean and concise API** with a fluent syntax for declaring migrations.

---

> In short, statically typed migrations are not only safer — they’re faster, cleaner, and more future-proof.


## Usage
### Example
A usage example is located in the **Example** directory. The entry point is the **UnityExample** component, where an instance of outdated game data is generated and saved to disk, then loaded and successfully migrated to the latest version.
### How It Works
To load and migrate data, you should not call the deserializer directly. Instead, use a Parser function that performs deserialization and applies necessary migrations automatically: It calls deserialization to convert byte[] to objects and calls Migration functions written by users (**MigrateV1V2**(), **MigrateV2V3**()) to migrate one GameState to another.

The project includes two parser implementations:
- **SimpleParser**: a straightforward, easy-to-read parser without iterators.
- **FluentParser**: a concise, chainable parser based on enumerators.

This example also demonstrates the ability to switch serialization formats (e.g., from **Newtonsoft** to **MessagePack**) mid-migration.

### When You Change the Data Schema
If the data schema changes, follow these three steps:
1. Create a new data class based on GameStateBase (in the example they are **GameStateV1**, **GameStateV2**, **GameStateV3**, **GameState**).
2. Implement a migration function from the previous version, e.g., **MigrateV1V2**(), **MigrateV2V3**(), etc.
3. Update your parser:
   - For **FluentParser**: add a new line to the chain.
   - For **SimpleParser**: add a new parsing method (3 lines of code).
     
### Usage Recommendations
I recommend using a consistent name for the latest version of the user data class, such as **GameState** (without any suffix). When the data schema changes and a new class version is needed, rename the previous **GameState** to something like **GameStateV3**, (but don’t refactor all references to GameState throughout the codebase!) and, then, create a new class named **GameState** for the latest version.  

This approach has several benefits:
1. It minimizes modifications to files that use **GameState**.
2. It prevents accidental usage of outdated data objects.
3. It clearly shows where the new data format is incompatible with the previous version.
   
This strategy is demonstrated in the example.

## Implementation Details

### Migration Functions
When creating a new version of game data, you need to implement an algorithm that transforms the data from the old version to the new one.  

To do this, the user needs to write a function that takes an object of the old version as input and returns an object of the new version as output.  

Migration functions are implemented as functions and not as classes because they don't need to store any state — they simply transform the input to the output.  

Here's an example of a migration function that migrates game state from version 2 (**GameStateV2**) to version 3 (**GameStateV3**):

```csharp
public static GameStateV3 MigrateV2V3(this GameStateV2 fromState)
{
    return new GameStateV3
    {
        LastReachedLevel = fromState.LastReachedLevel,
        PlayerProfile = new PlayerProfile
        {
            PlayerName = fromState.PlayerName,
            PlayerLevel = fromState.PlayerLevel,
            Coins = fromState.Coins,
            AvaliableSkins = fromState.AvaliableSkins,
            EquippedSkinId = fromState.AvaliableSkins[0] // Default skin
        }
    };
}
```

### Parser
The key module of this library is the Parser, which takes serialized data (**byte[]**) and returns an object of the latest version, such as **GameState**.  

The parser is implemented as a set of functions, not a class, since it doesn't need to hold any state — it simply transforms input into output.  

The parser uses a "recursive" approach — if deserialization into the latest version fails, it tries an earlier version and then migrates the result forward. The word "recursive" is in quotes because it doesn't call itself directly — instead, it calls the function that handles the previous version.  

The process works like this:
1. Attempt to deserialize the input as the latest version (**GameState**).
2. If that fails (e.g., due to an incompatible format), call the parser for the previous version.
3. Migrate the result forward to the latest version using a migration function.
4. The previous version's parser follows the same pattern, going all the way back if needed.
 
This is implemented in SimpleParser:

```csharp
public static class SimpleParser
{
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
```

### FluentParser
While **SimpleParser** is straightforward and readable, it contains some boilerplate — especially as more versions are added. To reduce repetitive code, a Fluent-style syntax is provided using lazy enumerators.

```csharp
public static class FluentParser
{
    public static GameState Deserialize(this byte[] data) => data
        .Deserialize(d => d.TryDeserializeNewtonsoft<GameStateV1>())
        .DeserializeAndMigrate(d => d.TryDeserializeNewtonsoft<GameStateV2>(), v1 => v1.ToV2()) 
        .DeserializeAndMigrate(d => d.TryDeserializeMessagePack<GameStateV3>(), v2 => v2.ToV3())
        .DeserializeAndMigrate(d => d.TryDeserializeMessagePack<GameState>(), v3 => v3.ToLast())
        .Finish();
}
```

This Fluent chain:
- Builds from oldest to newest version.
- Returns the most recent data object if deserialization and migrations succeed.

#### Under the Hood — Lazy Iterators!
**FluentParser** uses iterator methods (yield return). What flows between stages is not the game state objects themselves, but enumerators that each contain a single object.  

This has some important implications:
- The execution is lazy — nothing runs until needed.
- The last method in the chain is actually executed first.
- If that fails, it uses foreach to invoke the previous parser's enumerator, and so on.

This design is both elegant and efficient, but it does require understanding how IEnumerable<T> and yield work. If you're not comfortable with lazy evaluation and iterator methods, it's better to stick with SimpleParser, which is easier to read and debug.

### Serialization / Deserialization
The serialization and deserialization methods handle saving and reading the version of the game data. This is done using a Version field in the game data classes, set by the class constructor.  

The **TryDeserialize** methods do more than just deserialize; they also verify the version. If the version doesn’t match or can’t be extracted, they return null.  

Example (Newtonsoft):
```csharp
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
```

### Serialization Method Can Be Changed!
The proposed migration method allows easy modification of the serialization/deserialization approach when updating game data versions that is illustrated in the both persers adobe. For versions 1 and 2, **Newtonsoft** is used, while for versions 3 and 4, **MessagePack** is used. 

## Future Development
### Nested Data
In real-world scenarios, game state often consists of multiple user-defined classes nested within each other. It’s likely that changes will affect only a portion of a nested structure, while the root object remains the same.  

The current approach requires the version number to change at the root level, even when only a nested object changes. This can lead to unnecessary code changes and potential human errors.  

A possible long-term solution may include:
- Storing version information not just in the root object, but also in nested objects.
- Applying parsing and migration not only to root objects, but also recursively to nested structures.
- Using polymorphic deserialization.
 
This would require a tightly integrated approach, where the deserializer (e.g., MessagePack) also supports invoking migration logic on nested data. Such a system would need a richer interface and closer coupling between parsing and deserialization logic.
### Other Potential Improvements
1. Migration as Objects: Currently, migrations are implemented as static functions because they do not need to store any state. In the future, there might be valid reasons to make them into objects.
2. Deserialization as Classes: For similar reasons, deserialization is currently done via functions. However, it’s quite likely that they will be converted into objects later. Serialization and deserialization are inherently paired operations, and bundling them into a single class would make sense.
3. Custom Versioning Object: Instead of using an integer for versioning, it is possible to store a more complex object. Supporting this would require only minor changes in the GameStateBase class.

## Dependencies
The repository illustrates compatibility with different serialization frameworks:
1. Newtonsoft.Json is used for GameState versions 1 and 2.
2. MessagePack is used for GameState versions 3 and 4.

The example uses GameState classes and migration code from the [video](https://www.youtube.com/watch?v=d7K_77KRXHU).
