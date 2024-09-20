using System.Threading.Tasks;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Interface for database exports that filter objects.
	/// </summary>
	public interface IDatabaseExportFilter : IDatabaseExport
	{
		/// <summary>
		/// If a collection can be exported.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If the collection can be exported.</returns>
		bool CanExportCollection(string CollectionName);

		/// <summary>
		/// If an object can be exported.
		/// </summary>
		/// <param name="Object">Object to be exported.</param>
		/// <returns>If the object can be exported.</returns>
		bool CanExportObject(GenericObject Object);
	}
}
