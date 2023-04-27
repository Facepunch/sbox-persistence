using System;
using System.IO;

namespace Conna.Persistence;

public static partial class BinaryReaderExtension
{
	public static PersistenceHandle ReadPersistenceHandle( this BinaryReader buffer )
	{
		var id = buffer.ReadUInt64();
		return new PersistenceHandle( id );
	}

	public static void ReadWrapped( this BinaryReader self, Action<BinaryReader> wrapper )
	{
		var length = self.ReadInt32();
		var data = self.ReadBytes( length );

		using ( var stream = new MemoryStream( data ) )
		{
			using ( var reader = new BinaryReader( stream ) )
			{
				try
				{
					wrapper( reader );
				}
				catch ( Exception e )
				{
					Log.Error( e );
				}
			}
		}
	}
}
