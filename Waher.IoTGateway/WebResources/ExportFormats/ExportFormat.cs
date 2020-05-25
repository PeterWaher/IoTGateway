using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.IoTGateway;
using Waher.Networking.XMPP;

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
		public string FileName
		{
			get { return this.fileName; }
		}

		/// <summary>
		/// When file was created
		/// </summary>
		public DateTime Created
		{
			get { return this.created; }
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			if (this.fs != null)
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
		public abstract Task Start();

		/// <summary>
		/// Ends export
		/// </summary>
		public abstract Task End();

		/// <summary>
		/// Is called when export of database is started.
		/// </summary>
		public abstract Task StartExport();

		/// <summary>
		/// Is called when export of database is finished.
		/// </summary>
		public abstract Task EndExport();

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		public abstract Task StartCollection(string CollectionName);

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		public abstract Task EndCollection();

		/// <summary>
		/// Is called when an index in a collection is started.
		/// </summary>
		public abstract Task StartIndex();

		/// <summary>
		/// Is called when an index in a collection is finished.
		/// </summary>
		public abstract Task EndIndex();

		/// <summary>
		/// Is called when a field in an index is reported.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Ascending">If the field is sorted using ascending sort order.</param>
		public abstract Task ReportIndexField(string FieldName, bool Ascending);

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		public abstract Task<string> StartObject(string ObjectId, string TypeName);

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		public abstract Task EndObject();

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		public abstract Task ReportProperty(string PropertyName, object PropertyValue);

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		public abstract Task ReportError(string Message);

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		public abstract Task ReportException(Exception Exception);

		/// <summary>
		/// Starts export of files.
		/// </summary>
		public abstract Task StartFiles();

		/// <summary>
		/// Ends export of files.
		/// </summary>
		public abstract Task EndFiles();

		/// <summary>
		/// Export file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="File">File stream</param>
		public abstract Task ExportFile(string FileName, Stream File);

		/// <summary>
		/// If any clients should be updated about export status.
		/// </summary>
		/// <param name="ForceUpdate">If updates should be forced.</param>
		public Task UpdateClient(bool ForceUpdate)
		{
			DateTime TP = DateTime.Now;

			if (ForceUpdate || (TP - this.lastUpdate).TotalSeconds >= 1)
			{
				this.lastUpdate = TP;
				UpdateClientsFileUpdated(this.fileName, this.fs.Length, this.created);
			}

			return Task.CompletedTask;
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
			sb.Append(CommonTypes.JsonStringEncode("<button class=\"posButtonSm\" onclick=\"DeleteExport('" + FileName + "');\">Delete</button>"));
			sb.Append("\", \"isKey\": ");
			sb.Append(FileName.EndsWith(".key", StringComparison.CurrentCultureIgnoreCase) ? "true" : "false");
			sb.Append("}");

			string[] TabIDs = ClientEvents.GetTabIDsForLocation("/Settings/Backup.md");

			ClientEvents.PushEvent(TabIDs, "UpdateExport", sb.ToString(), true, "User");
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

			ClientEvents.PushEvent(TabIDs, "FileDeleted", sb.ToString(), true, "User");
		}
	}
}
