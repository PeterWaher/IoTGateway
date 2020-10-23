using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Serialization;

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

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			if (this.output != null)
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
			await this.output.WriteStartElementAsync(string.Empty, "Export", Export.ExportNamepace);
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
			return this.output.WriteStartElementAsync(string.Empty, "Database", Export.ExportNamepace);
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
			return this.output.WriteStartElementAsync(string.Empty, "Ledger", Export.ExportNamepace);
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
				await this.output.WriteStartElementAsync(string.Empty, "Collection", Export.ExportNamepace);
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
				return this.output.WriteStartElementAsync(string.Empty, "Index", Export.ExportNamepace);
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
				await this.output.WriteStartElementAsync(string.Empty, "Field", Export.ExportNamepace);
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
				await this.output.WriteStartElementAsync(string.Empty, "Block", Export.ExportNamepace);
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
					await this.output.WriteStartElementAsync(string.Empty, "MetaData", Export.ExportNamepace);
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
				await this.output.WriteStartElementAsync(string.Empty, "Obj", Export.ExportNamepace);
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

				await this.output.WriteStartElementAsync(string.Empty, EntryType.ToString(), Export.ExportNamepace);
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
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		public override async Task ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.exportCollection)
			{
				if (PropertyValue is null)
				{
					await this.output.WriteStartElementAsync(string.Empty, "Null", Export.ExportNamepace);
					if (!(PropertyName is null))
						await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
					await this.output.WriteEndElementAsync();
				}
				else if (PropertyValue is Enum)
				{
					await this.output.WriteStartElementAsync(string.Empty, "En", Export.ExportNamepace);
					if (!(PropertyName is null))
						await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
					await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
					await this.output.WriteEndElementAsync();
				}
				else
				{
					switch (Type.GetTypeCode(PropertyValue.GetType()))
					{
						case TypeCode.Boolean:
							await this.output.WriteStartElementAsync(string.Empty, "Bl", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((bool)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Byte:
							await this.output.WriteStartElementAsync(string.Empty, "B", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Char:
							await this.output.WriteStartElementAsync(string.Empty, "Ch", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.DateTime:
							await this.output.WriteStartElementAsync(string.Empty, "DT", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, XML.Encode((DateTime)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Decimal:
							await this.output.WriteStartElementAsync(string.Empty, "Dc", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((decimal)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Double:
							await this.output.WriteStartElementAsync(string.Empty, "Db", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((double)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Int16:
							await this.output.WriteStartElementAsync(string.Empty, "I2", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Int32:
							await this.output.WriteStartElementAsync(string.Empty, "I4", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Int64:
							await this.output.WriteStartElementAsync(string.Empty, "I8", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.SByte:
							await this.output.WriteStartElementAsync(string.Empty, "I1", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Single:
							await this.output.WriteStartElementAsync(string.Empty, "Fl", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((float)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.String:
							string s = PropertyValue.ToString();
							try
							{
								XmlConvert.VerifyXmlChars(s);
								await this.output.WriteStartElementAsync(string.Empty, "S", Export.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
								await this.output.WriteEndElementAsync();
							}
							catch (XmlException)
							{
								byte[] Bin = Encoding.UTF8.GetBytes(s);
								s = Convert.ToBase64String(Bin);
								await this.output.WriteStartElementAsync(string.Empty, "S64", Export.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
								await this.output.WriteEndElementAsync();
							}
							break;

						case TypeCode.UInt16:
							await this.output.WriteStartElementAsync(string.Empty, "U2", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.UInt32:
							await this.output.WriteStartElementAsync(string.Empty, "U4", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.UInt64:
							await this.output.WriteStartElementAsync(string.Empty, "U8", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.DBNull:
						case TypeCode.Empty:
							await this.output.WriteStartElementAsync(string.Empty, "Null", Export.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Object:
							if (PropertyValue is TimeSpan)
							{
								await this.output.WriteStartElementAsync(string.Empty, "TS", Export.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
								await this.output.WriteEndElementAsync();
							}
							else if (PropertyValue is DateTimeOffset DTO)
							{
								await this.output.WriteStartElementAsync(string.Empty, "DTO", Export.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, XML.Encode(DTO));
								await this.output.WriteEndElementAsync();
							}
							else if (PropertyValue is CaseInsensitiveString Cis)
							{
								s = Cis.Value;
								try
								{
									XmlConvert.VerifyXmlChars(s);
									await this.output.WriteStartElementAsync(string.Empty, "CIS", Export.ExportNamepace);
									if (!(PropertyName is null))
										await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
									await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
									await this.output.WriteEndElementAsync();
								}
								catch (XmlException)
								{
									byte[] Bin = Encoding.UTF8.GetBytes(s);
									s = Convert.ToBase64String(Bin);
									await this.output.WriteStartElementAsync(string.Empty, "CIS64", Export.ExportNamepace);
									if (!(PropertyName is null))
										await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
									await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
									await this.output.WriteEndElementAsync();
								}
							}
							else if (PropertyValue is byte[] Bin)
							{
								await this.output.WriteStartElementAsync(string.Empty, "Bin", Export.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);

								byte[] Buf = null;
								long c = Bin.Length;
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
									{
										if (i == 0 && j == c)
											Buf = Bin;
										else
											Buf = new byte[j];
									}

									if (Buf != Bin)
										Array.Copy(Bin, i, Buf, 0, j);

									this.output.WriteElementString("Chunk", Convert.ToBase64String(Buf, 0, j, Base64FormattingOptions.None));
									i += j;
								}

								await this.output.WriteEndElementAsync();
							}
							else if (PropertyValue is Guid)
							{
								await this.output.WriteStartElementAsync(string.Empty, "ID", Export.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
								await this.output.WriteEndElementAsync();
							}
							else if (PropertyValue is Array A)
							{
								await this.output.WriteStartElementAsync(string.Empty, "Array", Export.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "elementType", string.Empty, PropertyValue.GetType().GetElementType().FullName);

								foreach (object Obj in A)
									await this.ReportProperty(null, Obj);

								await this.output.WriteEndElementAsync();
							}
							else if (PropertyValue is GenericObject Obj)
							{
								await this.output.WriteStartElementAsync(string.Empty, "Obj", Export.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "type", string.Empty, Obj.TypeName);

								foreach (KeyValuePair<string, object> P in Obj)
									await this.ReportProperty(P.Key, P.Value);

								await this.output.WriteEndElementAsync();
							}
							else
								throw new Exception("Unhandled property value type: " + PropertyValue.GetType().FullName);
							break;

						default:
							throw new Exception("Unhandled property value type: " + PropertyValue.GetType().FullName);
					}
				}
			}
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		public override Task ReportError(string Message)
		{
			if (!this.inCollection || this.exportCollection)
				return this.output.WriteElementStringAsync(string.Empty, "Error", Export.ExportNamepace, Message);
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
				await this.output.WriteStartElementAsync(string.Empty, "Exception", Export.ExportNamepace);
				await this.output.WriteAttributeStringAsync(string.Empty, "message", string.Empty, Exception.Message);
				this.output.WriteElementString("StackTrace", Log.CleanStackTrace(Exception.StackTrace));

				if (Exception is AggregateException AggregateException)
				{
					foreach (Exception ex in AggregateException.InnerExceptions)
						await this.ReportException(ex);
				}
				else if (Exception.InnerException != null)
					await this.ReportException(Exception.InnerException);

				await this.output.WriteEndElementAsync();
			}
		}

		/// <summary>
		/// Starts export of files.
		/// </summary>
		public override Task StartFiles()
		{
			return this.output.WriteStartElementAsync(string.Empty, "Files", Export.ExportNamepace);
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
			await this.output.WriteStartElementAsync(string.Empty, "File", Export.ExportNamepace);

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

				await File.ReadAsync(Buf, 0, j);

				this.output.WriteElementString("Chunk", Convert.ToBase64String(Buf, 0, j, Base64FormattingOptions.None));
				i += j;
			}

			await this.output.WriteEndElementAsync();
		}

	}
}
