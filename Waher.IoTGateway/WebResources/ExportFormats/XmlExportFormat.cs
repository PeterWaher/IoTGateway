using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.XmlLedger;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.WebResources.ExportFormats
{
	/// <summary>
	/// XML File export
	/// </summary>
	public class XmlExportFormat : ExportFormat
	{
		private XmlWriter output;
		private bool exportCollection = false;
		private bool inCollection = false;
		private bool inMetaData = false;

		/// <summary>
		/// XML File export
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="Created">When file was created</param>
		/// <param name="Output">XML Output</param>
		/// <param name="File">File stream</param>
		/// <param name="OnlySelectedCollections">If only selected collections should be exported.</param>
		/// <param name="SelectedCollections">Array of selected collections.</param>
		public XmlExportFormat(string FileName, DateTime Created, XmlWriter Output, FileStream File,
			bool OnlySelectedCollections, Array SelectedCollections)
			: base(FileName, Created, File, OnlySelectedCollections, SelectedCollections)
		{
			this.output = Output;
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			if (!(this.output is null))
			{
				this.output.Flush();
				this.output.Close();
				this.output.Dispose();
				this.output = null;
			}

			base.Dispose();
		}

		/// <summary>
		/// Starts export
		/// </summary>
		public override async Task Start()
		{
			await this.output.WriteStartDocumentAsync();
			await this.output.WriteStartElementAsync(string.Empty, "Export", XmlFileLedger.Namespace);
		}

		/// <summary>
		/// Ends export
		/// </summary>
		public override async Task End()
		{
			await this.output.WriteEndElementAsync();
			await this.output.WriteEndDocumentAsync();
			await this.UpdateClient(true);
		}

		/// <summary>
		/// Is called when export of database is started.
		/// </summary>
		public override Task StartDatabase()
		{
			return this.output.WriteStartElementAsync(string.Empty, "Database", XmlFileLedger.Namespace);
		}

		/// <summary>
		/// Is called when export of database is finished.
		/// </summary>
		public override async Task EndDatabase()
		{
			await this.output.WriteEndElementAsync();
			await this.UpdateClient(false);
		}

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		public override Task StartLedger()
		{
			return this.output.WriteStartElementAsync(string.Empty, "Ledger", XmlFileLedger.Namespace);
		}

		/// <summary>
		/// Is called when export of ledger is finished.
		/// </summary>
		public override async Task EndLedger()
		{
			await this.output.WriteEndElementAsync();
			await this.UpdateClient(false);
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		public override async Task StartCollection(string CollectionName)
		{
			this.inCollection = true;
			if (this.exportCollection = this.ExportCollection(CollectionName))
			{
				await this.output.WriteStartElementAsync(string.Empty, "Collection", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "name", string.Empty, CollectionName);
			}
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		public override Task EndCollection()
		{
			this.inCollection = false;
			if (this.exportCollection)
				return this.output.WriteEndElementAsync();
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an index in a collection is started.
		/// </summary>
		public override Task StartIndex()
		{
			if (this.exportCollection)
				return this.output.WriteStartElementAsync(string.Empty, "Index", XmlFileLedger.Namespace);
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an index in a collection is finished.
		/// </summary>
		public override Task EndIndex()
		{
			if (this.exportCollection)
				return this.output.WriteEndElementAsync();
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a field in an index is reported.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Ascending">If the field is sorted using ascending sort order.</param>
		public override async Task ReportIndexField(string FieldName, bool Ascending)
		{
			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Field", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "name", string.Empty, FieldName);
				await this.output.WriteAttributeStringAsync(string.Empty, "ascending", string.Empty, CommonTypes.Encode(Ascending));
				await this.output.WriteEndElementAsync();
			}
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		public override async Task StartBlock(string BlockID)
		{
			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Block", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "id", string.Empty, BlockID);
			}
		}

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		public override async Task EndBlock()
		{
			if (this.exportCollection)
			{
				if (this.inMetaData)
				{
					await this.output.WriteEndElementAsync();
					this.inMetaData = false;
				}

				await this.output.WriteEndElementAsync();
			}
		}

		/// <summary>
		/// Reports block meta-data.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Value">Meta-data value.</param>
		public override async Task BlockMetaData(string Key, object Value)
		{
			if (this.exportCollection)
			{
				if (!this.inMetaData)
				{
					await this.output.WriteStartElementAsync(string.Empty, "MetaData", XmlFileLedger.Namespace);
					this.inMetaData = true;
				}

				await this.ReportProperty(Key, Value);
			}
		}

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		public override async Task<string> StartObject(string ObjectId, string TypeName)
		{
			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Obj", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "id", string.Empty, ObjectId);
				await this.output.WriteAttributeStringAsync(string.Empty, "type", string.Empty, TypeName);
			}

			return ObjectId;
		}

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		public override async Task EndObject()
		{
			if (this.exportCollection)
			{
				await this.output.WriteEndElementAsync();
				await this.UpdateClient(false);
			}
		}

		/// <summary>
		/// Is called when an entry is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>Object ID of object, after optional mapping.</returns>
		public override async Task<string> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
		{
			if (this.exportCollection)
			{
				if (this.inMetaData)
				{
					await this.output.WriteEndElementAsync();
					this.inMetaData = false;
				}

				await this.output.WriteStartElementAsync(string.Empty, EntryType.ToString(), XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "id", string.Empty, ObjectId);
				await this.output.WriteAttributeStringAsync(string.Empty, "type", string.Empty, TypeName);
				await this.output.WriteAttributeStringAsync(string.Empty, "ts", string.Empty, XML.Encode(EntryTimestamp));
			}

			return ObjectId;
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		public override async Task EndEntry()
		{
			if (this.exportCollection)
			{
				await this.output.WriteEndElementAsync();
				await this.UpdateClient(false);
			}
		}

		/// <summary>
		/// Is called when a collection has been cleared.
		/// </summary>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		public override async Task CollectionCleared(DateTimeOffset EntryTimestamp)
		{
			if (this.exportCollection)
			{
				if (this.inMetaData)
				{
					await this.output.WriteEndElementAsync();
					this.inMetaData = false;
				}

				await this.output.WriteStartElementAsync(string.Empty, nameof(EntryType.Clear), XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "ts", string.Empty, XML.Encode(EntryTimestamp));
			}
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		public override Task ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.exportCollection)
				return XmlFileLedger.ReportProperty(this.output, PropertyName, PropertyValue, XmlFileLedger.Namespace);
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		public override Task ReportError(string Message)
		{
			if (!this.inCollection || this.exportCollection)
				return this.output.WriteElementStringAsync(string.Empty, "Error", XmlFileLedger.Namespace, Message);
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		public override async Task ReportException(Exception Exception)
		{
			if (!this.inCollection || this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Exception", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "message", string.Empty, Exception.Message);
				this.output.WriteElementString("StackTrace", Log.CleanStackTrace(Exception.StackTrace));

				if (Exception is AggregateException AggregateException)
				{
					foreach (Exception ex in AggregateException.InnerExceptions)
						await this.ReportException(ex);
				}
				else if (!(Exception.InnerException is null))
					await this.ReportException(Exception.InnerException);

				await this.output.WriteEndElementAsync();
			}
		}

		/// <summary>
		/// Starts export of files.
		/// </summary>
		public override Task StartFiles()
		{
			return this.output.WriteStartElementAsync(string.Empty, "Files", XmlFileLedger.Namespace);
		}

		/// <summary>
		/// Ends export of files.
		/// </summary>
		public override Task EndFiles()
		{
			return this.output.WriteEndElementAsync();
		}

		/// <summary>
		/// Export file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="File">File stream</param>
		public override async Task ExportFile(string FileName, Stream File)
		{
			await this.output.WriteStartElementAsync(string.Empty, "File", XmlFileLedger.Namespace);

			await this.output.WriteAttributeStringAsync(string.Empty, "fileName", string.Empty, FileName);

			byte[] Buf = null;
			long c = File.Length;
			long i = 0;
			long d;
			int j;

			while (i < c)
			{
				d = c - i;
				if (d > 49152)
					j = 49152;
				else
					j = (int)d;

				if (Buf is null)
					Buf = new byte[j];

				await File.ReadAllAsync(Buf, 0, j);

				this.output.WriteElementString("Chunk", Convert.ToBase64String(Buf, 0, j, Base64FormattingOptions.None));
				i += j;
			}

			await this.output.WriteEndElementAsync();
		}

	}
}
