using System;
using System.IO;

namespace Conna.Persistence;

public static partial class BinaryWriterExtension
{
	public static void Write( this BinaryWriter self, PersistenceHandle item )
	{
		self.Write( item.Id );
	}

	public static void Write( this BinaryWriter self, Action<BinaryWriter> wrapper )
	{
		using ( var stream = new MemoryStream() )
		{
			using ( var writer = new BinaryWriter( stream ) )
			{
				wrapper( writer );
			}

			var data = stream.ToArray();

			self.Write( data.Length );
			self.Write( data );
		}
	}
}
