# sbox-persistence
💾 A simple persistence system that can be used to serialize and deserialize the game state.

# IPersistence
Implement this interface for any entity you'd like to be serialized when saving the state of the game.

### ShouldSaveState()
Whether or not this entity should be saved at all.

### public void SerializeState( BinaryWriter writer )
Called when serializing the entity's state. Stuff should be serialized in the same order it'll be deserialized.

### public void DeserializeState( BinaryReader reader )
Called when deserializing the entity's state. Stuff should be deserialized in the same order it was serialized.

### public void BeforeStateLoaded()
Called before the state is loaded for any setup.

### public void AfterStateLoaded()
Called after all entity states have been loaded. You can now reference other entity's by their persistence handles.

# PersistenceSystem

### SaveAll()
Serialize and save all entities (uses the user chosen Saved Game name).

### AddReader( Action<BinaryReader> reader )
This should be called and setup within the game's `Spawn` method. Add a custom reader for deserializing any extra Saved Game data that is not related to entities.
  
### AddWriter( Action<BinaryWriter> writer )
This should be called and setup within the game's `Spawn` method. Add a custom writer for serializing any extra Saved Game data that is not related to entities.
  
### LoadAll( BinaryReader reader )
Deserialize and load all data from a Saved Game. Should be called from the game's `LoadSavedGame` method. Here's an example:
  
```csharp
public override void LoadSavedGame( SavedGame save )
{
  using var s = new MemoryStream( save.Data );
  using var r = new BinaryReader( s );

  PersistenceSystem.LoadAll( r );
}
```
  
# PersistenceHandle
A persistence handle is a unique identifier for an entity for each saved games. Entities can reference other saved entities this way.
  
On your entity you might do this:
  
```csharp
public PersistenceHandle Handle { get; private set; }
  
public virtual void SerializeState( BinaryWriter writer )
{
  writer.Write( Handle );
}

public virtual void DeserializeState( BinaryReader reader )
{
  Handle = reader.ReadPersistenceHandle();
}
```
  
In another entity you could serialize the handle of that entity in `SerializeState`. Then in `AfterStateLoaded` you could search all entities to see if their `Handle` property matches to find that entity again.
