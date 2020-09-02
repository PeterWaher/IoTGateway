using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Interface for ledger exports.
	/// </summary>
	public interface ILedgerExport
	{
		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		Task StartLedger();

		/// <summary>
		/// Is called when export of ledger is finished.
		/// </summary>
		Task EndLedger();

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
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		Task StartBlock(string BlockID);

		/// <summary>
		/// Reports block meta-data.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Value">Meta-data value.</param>
		Task BlockMetaData(string Key, object Value);

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		Task EndBlock();

		/// <summary>
		/// Is called when an entry is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>Object ID of object, after optional mapping.</returns>
		Task<string> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp);

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		Task EndEntry();

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
