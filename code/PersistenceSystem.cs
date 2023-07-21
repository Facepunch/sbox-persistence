using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Conna.Persistence;

public static class PersistenceSystem
{
	public static string UniqueId { get; private set; }

	private static List<Action<BinaryReader>> Readers { get; set; } = new();
	private static List<Action<BinaryWriter>> Writers { get; set; } = new();
	private static ulong PersistentId { get; set; }

	public static ulong GenerateId()
	{
		return ++PersistentId;
	}

	[ConCmd.Admin( "game.save" )]
	public static void SaveAll()
	{
		var save = new SavedGame();
		save.Name = Game.CurrentSavedGame?.Name ?? "Untitled";

		using var s = new MemoryStream();
		using var w = new BinaryWriter( s );

		if ( string.IsNullOrEmpty( UniqueId ) )
		{
			UniqueId = Guid.NewGuid().ToString( "N" );
		}
		
		w.Write( UniqueId );
		
		foreach ( var v in Writers )
		{
			v( w );
		}

		SaveEntities( w );

		w.Write( PersistentId );
		
		save.Data = s.ToArray();

		Game.Save( save );
	}

	public static void AddReader( Action<BinaryReader> reader )
	{
		Readers.Add( reader );
	}

	public static void AddWriter( Action<BinaryWriter> writer )
	{
		Writers.Add( writer );
	}

	public static void LoadAll( BinaryReader reader )
	{
		foreach ( var p in Entity.All.OfType<IPersistence>() )
		{
			if ( p.IsFromMap ) continue;
			p.Delete();
		}

		UniqueId = reader.ReadString();

		foreach ( var v in Readers )
		{
			v( reader );
		}

		LoadEntities( reader );
		
		PersistentId = reader.ReadUInt64();

		foreach ( var p in Entity.All.OfType<IPersistence>() )
		{
			p.BeforeStateLoaded();
		}

		foreach ( var p in Entity.All.OfType<IPersistence>() )
		{
			p.AfterStateLoaded();
		}
	}

	private static void SaveEntities( BinaryWriter writer )
	{
		var entities = Entity.All
			.OfType<IPersistence>()
			.Where( e => e.ShouldSaveState() );

		writer.Write( entities.Count() );

		foreach ( var entity in entities )
		{
			var description = TypeLibrary.GetType( entity.GetType() );
			writer.Write( description.Name );
			writer.Write( entity.SerializeState );

			if ( entity.IsFromMap )
			{
				writer.Write( true );
				writer.Write( entity.HammerID );
			}
			else
			{
				writer.Write( false );
			}
		}
	}

	private static void LoadEntities( BinaryReader reader )
	{
		var count = reader.ReadInt32();
		var entitiesAndData = new Dictionary<IPersistence, byte[]>();

		for ( var i = 0; i < count; i++ )
		{
			var typeName = reader.ReadString();
			var type = TypeLibrary.GetType( typeName );
			var length = reader.ReadInt32();
			var data = reader.ReadBytes( length );

			try
			{
				IPersistence entity = null;

				var isFromMap = reader.ReadBoolean();

				if ( isFromMap )
				{
					var hammerId = reader.ReadString();
					entity = Entity.All.FirstOrDefault( e => e.HammerID == hammerId ) as IPersistence;
				}
				else if ( type is not null )
				{
					entity = type.Create<IPersistence>();
				}

				if ( entity.IsValid() )
				{
					entitiesAndData.Add( entity, data );
				}
			}
			catch ( Exception e )
			{
				Log.Error( e );
			}
		}

		foreach ( var kv in entitiesAndData )
		{
			try
			{
				using ( var stream = new MemoryStream( kv.Value ) )
				{
					using ( reader = new BinaryReader( stream ) )
					{
						kv.Key.DeserializeState( reader );
					}
				}
			}
			catch ( Exception e )
			{
				Log.Error( e );
			}
		}
	}
}
