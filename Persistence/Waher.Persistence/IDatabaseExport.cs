using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Filters;

namespace Waher.Persistence
{
	/// <summary>
	/// Interface for database exports.
	/// </summary>
	public interface IDatabaseExport
	{
		void StartExport();
		void EndExport();

		void StartCollection(string CollectionName);
		void EndCollection();

		void StartObject(string ObjectId, string TypeName);
		void EndObject();

		void ReportProperty(string PropertyName, object PropertyValue);

		void ReportError(string Message);
		void ReportException(Exception Exception);
	}
}
