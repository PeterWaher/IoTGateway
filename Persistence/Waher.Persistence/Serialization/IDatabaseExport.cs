using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Filters;

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
		Task StartExport();

		/// <summary>
		/// Is called when export of database is finished.
		/// </summary>
		Task EndExport();

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		Task StartCollection(string CollectionName);

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		Task EndCollection();

		/// <summary>
		/// Is called when an index in a collection is started.
		/// </summary>
		Task StartIndex();

		/// <summary>
		/// Is called when an index in a collection is finished.
		/// </summary>
		Task EndIndex();

		/// <summary>
		/// Is called when a field in an index is reported.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Ascending">If the field is sorted using ascending sort order.</param>
		Task ReportIndexField(string FieldName, bool Ascending);

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		Task StartObject(string ObjectId, string TypeName);

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		Task EndObject();

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		Task ReportProperty(string PropertyName, object PropertyValue);

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		Task ReportError(string Message);

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		Task ReportException(Exception Exception);
	}
}
