using Sandbox;
using System.IO;

namespace Conna.Persistence;

public interface IPersistence : IValid
{
	public bool ShouldSaveState();
	public void SerializeState( BinaryWriter writer );
	public void DeserializeState( BinaryReader reader );
	public void BeforeStateLoaded();
	public void AfterStateLoaded();
	public void Delete();

	public string HammerID { get; }
	public bool IsFromMap { get; }
}
