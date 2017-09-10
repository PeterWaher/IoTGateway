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
		/// Is called when export is started.
		/// </summary>
		void StartExport();

		/// <summary>
		/// Is called when export is finished.
		/// </summary>
		void EndExport();

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName"></param>
		void StartCollection(string CollectionName);

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		void EndCollection();

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId"></param>
		/// <param name="TypeName"></param>
		void StartObject(string ObjectId, string TypeName);

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		void EndObject();

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		void ReportProperty(string PropertyName, object PropertyValue);

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		void ReportError(string Message);

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		void ReportException(Exception Exception);
	}
}
