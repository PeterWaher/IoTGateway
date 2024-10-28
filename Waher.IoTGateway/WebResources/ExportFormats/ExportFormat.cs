using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Persistence;

namespace Waher.IoTGateway.WebResources.ExportFormats
{
	/// <summary>
	/// Abstract base class for export formats.
	/// </summary>
	public abstract class ExportFormat : IExportFormat, IDisposable
	{
		private DateTime lastUpdate = DateTime.Now;
		private readonly DateTime created;
		private FileStream fs;
		private readonly string fileName;
		private readonly bool onlySelectedCollections;
		private readonly Array selectedCollections;

		/// <summary>
		/// Abstract base class for export formats.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="Created">When file was created</param>
		/// <param name="File">File stream</param>
		/// <param name="OnlySelectedCollections">If only selected collections should be exported.</param>
		/// <param name="SelectedCollections">Array of selected collections.</param>
		public ExportFormat(string FileName, DateTime Created, FileStream File, bool OnlySelectedCollections, Array SelectedCollections)
		{
			this.fileName = FileName;
			this.created = Created;
			this.fs = File;
			this.onlySelectedCollections = OnlySelectedCollections;
			this.selectedCollections = SelectedCollections;
		}

		/// <summary>
		/// Optional array of collection nmes to export. If null, all collections will be exported.
		/// </summary>
		public string[] CollectionNames
		{
			get
			{
				if (this.onlySelectedCollections)
				{
					int i, c = this.selectedCollections.Length;
					string[] Result = new string[c];

					for (i = 0; i < c; i++)
						Result[i] = this.selectedCollections.GetValue(i).ToString();

					return Result;
				}
				else
					return null;
			}
		}

		/// <summary>
		/// File name
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// When file was created
		/// </summary>
		public DateTime Created => this.created;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			if (!(this.fs is null))
			{
				if (this.fs.CanWrite)
					this.fs.Flush();

				this.fs.Dispose();
				this.fs = null;
			}
		}

		/// <summary>
		/// Checks if a collection is to be exported or not.
		/// </summary>
		/// <param name="CollectionName">Collection Name.</param>
		/// <returns>If the collection is to be exported or not.</returns>
		protected bool ExportCollection(string CollectionName)
		{
			return !this.onlySelectedCollections || Array.IndexOf(this.selectedCollections, CollectionName) >= 0;
		}

		/// <summary>
		/// Starts export
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> Start();

		/// <summary>
		/// Ends export
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> End();

		/// <summary>
		/// Is called when export of database is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> StartDatabase();

		/// <summary>
		/// Is called when export of database is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> EndDatabase();

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> StartLedger();

		/// <summary>
		/// Is called when export of ledger is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> EndLedger();

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> StartCollection(string CollectionName);

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> EndCollection();

		/// <summary>
		/// Is called when an index in a collection is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> StartIndex();

		/// <summary>
		/// Is called when an index in a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> EndIndex();

		/// <summary>
		/// Is called when a field in an index is reported.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Ascending">If the field is sorted using ascending sort order.</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> ReportIndexField(string FieldName, bool Ascending);

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> StartBlock(string BlockID);

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> EndBlock();

		/// <summary>
		/// Reports block meta-data.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Value">Meta-data value.</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> BlockMetaData(string Key, object Value);

		/// <summary>
		/// Is called when an entry is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp);

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> EndEntry();

		/// <summary>
		/// Is called when a collection has been cleared.
		/// </summary>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public virtual async Task<bool> CollectionCleared(DateTimeOffset EntryTimestamp)
		{
			return
				await this.StartEntry(string.Empty, string.Empty, EntryType.Clear, EntryTimestamp) &&
				await this.EndEntry();
		}

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <returns>Object ID of object, after optional mapping. null means export cannot continue</returns>
		public abstract Task<string> StartObject(string ObjectId, string TypeName);

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> EndObject();

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> ReportProperty(string PropertyName, object PropertyValue);

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> ReportError(string Message);

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> ReportException(Exception Exception);

		/// <summary>
		/// Starts export of files.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> StartFiles();

		/// <summary>
		/// Ends export of files.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> EndFiles();

		/// <summary>
		/// Export file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="File">File stream</param>
		/// <returns>If export can continue.</returns>
		public abstract Task<bool> ExportFile(string FileName, Stream File);

		/// <summary>
		/// If any clients should be updated about export status.
		/// </summary>
		/// <param name="ForceUpdate">If updates should be forced.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> UpdateClient(bool ForceUpdate)
		{
			DateTime TP = DateTime.Now;

			if (ForceUpdate || (TP - this.lastUpdate).TotalSeconds >= 1)
			{
				this.lastUpdate = TP;
				UpdateClientsFileUpdated(this.fileName, this.fs.Length, this.created);
			}

			return Task.FromResult(true);
		}

		/// <summary>
		/// Updates the status of a file on all pages viewing backup files
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="Length">Size of file</param>
		/// <param name="Created">When file was created</param>
		public static void UpdateClientsFileUpdated(string FileName, long Length, DateTime Created)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("{\"fileName\":\"");
			sb.Append(CommonTypes.JsonStringEncode(FileName));
			sb.Append("\", \"size\": \"");
			if (Length >= 0)
				sb.Append(CommonTypes.JsonStringEncode(Export.FormatBytes(Length)));
			sb.Append("\", \"created\": \"");
			sb.Append(CommonTypes.JsonStringEncode(Created.ToString()));
			sb.Append("\", \"button\": \"");
			sb.Append(CommonTypes.JsonStringEncode("<button class=\"negButtonSm\" onclick=\"DeleteExport('" + FileName + "');\">Delete</button>"));
			sb.Append("\", \"isKey\": ");
			sb.Append(FileName.EndsWith(".key", StringComparison.CurrentCultureIgnoreCase) ? "true" : "false");
			sb.Append("}");

			string[] TabIDs = ClientEvents.GetTabIDsForLocation("/Settings/Backup.md");

			Task _ = ClientEvents.PushEvent(TabIDs, "UpdateExport", sb.ToString(), true, "User");
		}

		/// <summary>
		/// Removes a file from all pages viewing backup files
		/// </summary>
		/// <param name="FileName">Name of file</param>
		public static void UpdateClientsFileDeleted(string FileName)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("{\"fileName\":\"");
			sb.Append(CommonTypes.JsonStringEncode(FileName));
			sb.Append("\"}");

			string[] TabIDs = ClientEvents.GetTabIDsForLocation("/Settings/Backup.md");

			Task _ = ClientEvents.PushEvent(TabIDs, "FileDeleted", sb.ToString(), true, "User");
		}
	}
}
