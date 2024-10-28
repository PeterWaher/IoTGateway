using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Interface for database exports.
	/// </summary>
	public interface IDatabaseExport
	{
		/// <summary>
		/// Is called when export of database is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		Task<bool> StartDatabase();

		/// <summary>
		/// Is called when export of database is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		Task<bool> EndDatabase();

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If export can continue.</returns>
		Task<bool> StartCollection(string CollectionName);

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		Task<bool> EndCollection();

		/// <summary>
		/// Is called when an index in a collection is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		Task<bool> StartIndex();

		/// <summary>
		/// Is called when an index in a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		Task<bool> EndIndex();

		/// <summary>
		/// Is called when a field in an index is reported.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Ascending">If the field is sorted using ascending sort order.</param>
		/// <returns>If export can continue.</returns>
		Task<bool> ReportIndexField(string FieldName, bool Ascending);

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <returns>Object ID of object, after optional mapping. null means export cannot continue</returns>
		Task<string> StartObject(string ObjectId, string TypeName);

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		Task<bool> EndObject();

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <returns>If export can continue.</returns>
		Task<bool> ReportProperty(string PropertyName, object PropertyValue);

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		/// <returns>If export can continue.</returns>
		Task<bool> ReportError(string Message);

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>If export can continue.</returns>
		Task<bool> ReportException(Exception Exception);
	}
}
