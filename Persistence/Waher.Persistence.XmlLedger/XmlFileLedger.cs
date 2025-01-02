using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Persistence.Serialization;
using Waher.Runtime.Profiling;
using Waher.Script;

namespace Waher.Persistence.XmlLedger
{
	/// <summary>
	/// Simple ledger that records anything that happens in the database to XML files in the program data folder.
	/// </summary>
	public class XmlFileLedger : ILedgerProvider, ILedgerExport
	{
		/// <summary>
		/// http://waher.se/Schema/Export.xsd
		/// </summary>
		public const string Namespace = "http://waher.se/Schema/Export.xsd";

		private readonly XmlWriterSettings settings;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
		private StreamWriter file;
		private DateTime lastEvent = DateTime.MinValue;
		private XmlWriter output;
		private TextWriter textOutput;
		private ILedgerExternalEvents externalEvents;
		private readonly string fileName;
		private string lastFileName = null;
		private readonly string transform = null;
		private readonly int deleteAfterDays;
		private bool running;

		#region Construction

		/// <summary>
		/// Simple ledger that records anything that happens in the database to XML files in the program data folder.
		/// </summary>
		/// <param name="FileName">File Name. The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.
		/// 
		/// NOTE: Make sure files are stored in a separate folder, as old files will be automatically deleted.
		/// </param>
		public XmlFileLedger(string FileName)
			: this(FileName, string.Empty, 7)
		{
		}

		/// <summary>
		/// Simple ledger that records anything that happens in the database to XML files in the program data folder.
		/// </summary>
		/// <param name="FileName">File Name. The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.
		/// 
		/// NOTE: Make sure files are stored in a separate folder, as old files will be automatically deleted.
		/// </param>
		/// <param name="DeleteAfterDays">Number of days files will be kept. All files older than this
		/// in the corresponding folder will be removed. Default value is 7 days.</param>
		public XmlFileLedger(string FileName, int DeleteAfterDays)
			: this(FileName, string.Empty, DeleteAfterDays)
		{
		}

		/// <summary>
		/// Simple ledger that records anything that happens in the database to XML files in the program data folder.
		/// </summary>
		/// <param name="FileName">File Name. The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.
		/// 
		/// NOTE: Make sure files are stored in a separate folder, as old files will be automatically deleted.
		/// </param>
		/// <param name="Transform">Transform file name.</param>
		public XmlFileLedger(string FileName, string Transform)
			: this(FileName, Transform, 7)
		{
		}

		/// <summary>
		/// Simple ledger that records anything that happens in the database to XML files in the program data folder.
		/// </summary>
		/// <param name="FileName">File Name. The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.
		/// 
		/// NOTE: Make sure files are stored in a separate folder, as old files will be automatically deleted.
		/// </param>
		/// <param name="Transform">Transform file name.</param>
		/// <param name="DeleteAfterDays">Number of days files will be kept. All files older than this
		/// in the corresponding folder will be removed. Default value is 7 days.</param>
		public XmlFileLedger(string FileName, string Transform, int DeleteAfterDays)
		{
			this.file = null;
			this.output = null;
			this.fileName = FileName;
			this.transform = Transform;
			this.deleteAfterDays = DeleteAfterDays;

			this.settings = new XmlWriterSettings()
			{
				CloseOutput = true,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = false,
				WriteEndDocumentOnClose = true,
				Async = true
			};

			string FolderName = Path.GetDirectoryName(FileName);

			if (!string.IsNullOrEmpty(FolderName) && !Directory.Exists(FolderName))
			{
				Log.Informational("Creating folder.", FolderName);
				Directory.CreateDirectory(FolderName);
			}
		}

		/// <summary>
		/// Simple ledger that records anything that happens in the database to a text output stream.
		/// </summary>
		/// <param name="Output">Text Output</param>
		public XmlFileLedger(TextWriter Output)
		{
			this.textOutput = Output;
			this.file = null;
			this.fileName = null;
			this.transform = null;
			this.deleteAfterDays = 0;

			this.settings = new XmlWriterSettings()
			{
				CloseOutput = true,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = false,
				WriteEndDocumentOnClose = true,
				Async = true
			};

			this.output = XmlWriter.Create(this.textOutput, this.settings);
			this.output.WriteStartDocument();
			this.output.WriteStartElement("LedgerExport", Namespace);
			this.output.Flush();
		}

		#endregion

		#region Persistance

		/// <summary>
		/// File Name.
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Transform to use.
		/// </summary>
		public string Transform => this.transform;

		/// <summary>
		/// Timestamp of Last event
		/// </summary>
		public DateTime LastEvent => this.lastEvent;

		/// <summary>
		/// Gets the name of a file, given a file name template.
		/// </summary>
		/// <param name="TemplateFileName">File Name template.</param>
		/// <param name="TP">Timestamp</param>
		/// <returns>File name</returns>
		public static string GetFileName(string TemplateFileName, DateTime TP)
		{
			return TemplateFileName.
				Replace("%YEAR%", TP.Year.ToString("D4")).
				Replace("%MONTH%", TP.Month.ToString("D2")).
				Replace("%DAY%", TP.Day.ToString("D2")).
				Replace("%HOUR%", TP.Hour.ToString("D2")).
				Replace("%MINUTE%", TP.Minute.ToString("D2")).
				Replace("%SECOND%", TP.Second.ToString("D2"));
		}

		/// <summary>
		/// Makes a file name unique.
		/// </summary>
		/// <param name="FileName">File name.</param>
		public static void MakeUnique(ref string FileName)
		{
			if (File.Exists(FileName))
			{
				int i = FileName.LastIndexOf('.');
				int j = 2;

				if (i < 0)
					i = FileName.Length;

				string s;

				do
				{
					s = FileName.Insert(i, " (" + (j++).ToString() + ")");
				}
				while (File.Exists(s));

				FileName = s;
			}
		}

		/// <summary>
		/// Method is called before writing something to the text file.
		/// </summary>
		private async Task BeforeWrite()
		{
			if (this.fileName is null)
				return;

			DateTime TP = DateTime.Now;
			string s = GetFileName(this.fileName, TP);
			this.lastEvent = TP;

			if (!(this.lastFileName is null) && this.lastFileName == s && !(this.file is null) && this.file.BaseStream.CanWrite)
				return;

			try
			{
				if (!(this.output is null))
				{
					await this.output.WriteEndElementAsync();
					await this.output.WriteEndDocumentAsync();
					await this.output.FlushAsync();
				}

				this.file?.Dispose();
			}
			catch (Exception)
			{
				try
				{
					await this.DisposeOutput();
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			this.file = null;
			this.output = null;

			string s2 = s;
			MakeUnique(ref s2);

			try
			{
				this.file = File.CreateText(s2);
				this.lastFileName = s;
				this.output = XmlWriter.Create(this.file, this.settings);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				this.output = null;
				return;
			}

			await this.output.WriteStartDocumentAsync();

			if (!string.IsNullOrEmpty(this.transform))
			{
				if (File.Exists(this.transform))
				{
					try
					{
						byte[] XsltBin = await Resources.ReadAllBytesAsync(this.transform);

						await this.output.WriteProcessingInstructionAsync("xml-stylesheet", "type=\"text/xsl\" href=\"data:text/xsl;base64," +
							Convert.ToBase64String(XsltBin) + "\"");
					}
					catch (Exception)
					{
						await this.output.WriteProcessingInstructionAsync("xml-stylesheet", "type=\"text/xsl\" href=\"" + this.transform + "\"");
					}
				}
				else
					await this.output.WriteProcessingInstructionAsync("xml-stylesheet", "type=\"text/xsl\" href=\"" + this.transform + "\"");
			}

			await this.output.WriteStartElementAsync(string.Empty, "LedgerExport", Namespace);
			await this.output.FlushAsync();

			if (this.deleteAfterDays > 0 && this.deleteAfterDays < int.MaxValue)
			{
				string FolderName = Path.GetDirectoryName(s);

				if (!string.IsNullOrEmpty(FolderName))
				{
					string[] Files = Directory.GetFiles(FolderName, "*.*");

					foreach (string FileName in Files)
					{
						if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(FileName)).TotalDays >= this.deleteAfterDays)
						{
							try
							{
								File.Delete(FileName);
							}
							catch (IOException ex)
							{
								Log.Error("Unable to delete file: " + ex.Message, FileName);
							}
							catch (Exception ex)
							{
								Log.Exception(ex, FileName);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Disposes of the current output.
		/// </summary>
		private async Task DisposeOutput()
		{
			if (!(this.output is null))
			{
				await this.output.FlushAsync();
				this.output.Dispose();
				this.output = null;
			}

			this.file?.Dispose();
			this.file = null;

			this.textOutput?.Flush();
			this.textOutput = null;
		}

		#endregion

		#region ILedgerProvider

		/// <summary>
		/// Called when processing starts.
		/// </summary>
		public Task Start()
		{
			this.running = true;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when processing ends.
		/// </summary>
		public async Task Stop()
		{
			if (this.running)
			{
				this.running = false;

				if (!(this.output is null))
				{
					await this.output.WriteEndElementAsync();
					await this.output.WriteEndDocumentAsync();
				}
			}

			await this.Flush();
		}

		/// <summary>
		/// Persists any pending changes.
		/// </summary>
		public async Task Flush()
		{
			if (!(this.output is null))
				await this.output.FlushAsync();

			if (!(this.file is null))
				await this.file.FlushAsync();

			if (!(this.textOutput is null))
				await this.textOutput.FlushAsync();
		}

		/// <summary>
		/// Adds an entry to the ledger.
		/// </summary>
		/// <param name="Object">New object.</param>
		public Task NewEntry(object Object)
		{
			return this.OutputEntry("New", Object);
		}

		/// <summary>
		/// Updates an entry in the ledger.
		/// </summary>
		/// <param name="Object">Updated object.</param>
		public Task UpdatedEntry(object Object)
		{
			return this.OutputEntry("Update", Object);
		}

		/// <summary>
		/// Deletes an entry in the ledger.
		/// </summary>
		/// <param name="Object">Deleted object.</param>
		public Task DeletedEntry(object Object)
		{
			return this.OutputEntry("Delete", Object);
		}

		private async Task OutputEntry(string Method, object Object)
		{
			if (!this.running)
				return;

			GenericObject Obj = await Database.Generalize(Object);
			DateTime Timestamp = DateTime.UtcNow;

			await this.semaphore.WaitAsync();
			try
			{
				try
				{
					await this.BeforeWrite();

					if (!(this.output is null))
					{
						await this.output.WriteStartElementAsync(string.Empty, Method, Namespace);
						await this.output.WriteAttributeStringAsync(string.Empty, "timestamp", string.Empty, XML.Encode(Timestamp));
						await this.output.WriteAttributeStringAsync(string.Empty, "collection", string.Empty, Obj.CollectionName);
						await this.output.WriteAttributeStringAsync(string.Empty, "type", string.Empty, Obj.TypeName);
						await this.output.WriteAttributeStringAsync(string.Empty, "id", string.Empty, Obj.ObjectId.ToString());

						foreach (KeyValuePair<string, object> P in Obj.Properties)
							await ReportProperty(this.output, P.Key, P.Value, Namespace);

						await this.output.WriteEndElementAsync();
						await this.output.FlushAsync();
					}
				}
				catch (Exception)
				{
					try
					{
						await this.DisposeOutput();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// Serializes a property to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <param name="Namespace">XML Namespace to use.</param>
		public static async Task ReportProperty(XmlWriter Output, string PropertyName, object PropertyValue, string Namespace)
		{
			if (PropertyValue is null)
			{
				await Output.WriteStartElementAsync(string.Empty, "Null", Namespace);
				if (!(PropertyName is null))
					await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
				await Output.WriteEndElementAsync();
			}
			else if (PropertyValue is Enum)
			{
				await Output.WriteStartElementAsync(string.Empty, "En", Namespace);
				if (!(PropertyName is null))
					await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
				await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
				await Output.WriteEndElementAsync();
			}
			else
			{
				switch (Type.GetTypeCode(PropertyValue.GetType()))
				{
					case TypeCode.Boolean:
						await Output.WriteStartElementAsync(string.Empty, "Bl", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((bool)PropertyValue));
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Byte:
						await Output.WriteStartElementAsync(string.Empty, "B", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Char:
						await Output.WriteStartElementAsync(string.Empty, "Ch", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.DateTime:
						await Output.WriteStartElementAsync(string.Empty, "DT", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, XML.Encode((DateTime)PropertyValue));
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Decimal:
						await Output.WriteStartElementAsync(string.Empty, "Dc", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((decimal)PropertyValue));
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Double:
						await Output.WriteStartElementAsync(string.Empty, "Db", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((double)PropertyValue));
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Int16:
						await Output.WriteStartElementAsync(string.Empty, "I2", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Int32:
						await Output.WriteStartElementAsync(string.Empty, "I4", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Int64:
						await Output.WriteStartElementAsync(string.Empty, "I8", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.SByte:
						await Output.WriteStartElementAsync(string.Empty, "I1", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Single:
						await Output.WriteStartElementAsync(string.Empty, "Fl", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((float)PropertyValue));
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.String:
						string s = PropertyValue.ToString();
						try
						{
							XmlConvert.VerifyXmlChars(s);
							await Output.WriteStartElementAsync(string.Empty, "S", Namespace);
							if (!(PropertyName is null))
								await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
							await Output.WriteEndElementAsync();
						}
						catch (XmlException)
						{
							byte[] Bin = System.Text.Encoding.UTF8.GetBytes(s);
							s = Convert.ToBase64String(Bin);
							await Output.WriteStartElementAsync(string.Empty, "S64", Namespace);
							if (!(PropertyName is null))
								await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
							await Output.WriteEndElementAsync();
						}
						break;

					case TypeCode.UInt16:
						await Output.WriteStartElementAsync(string.Empty, "U2", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.UInt32:
						await Output.WriteStartElementAsync(string.Empty, "U4", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.UInt64:
						await Output.WriteStartElementAsync(string.Empty, "U8", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
						await Output.WriteEndElementAsync();
						break;

					case (TypeCode)2:   // DBNull:
					case TypeCode.Empty:
						await Output.WriteStartElementAsync(string.Empty, "Null", Namespace);
						if (!(PropertyName is null))
							await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
						await Output.WriteEndElementAsync();
						break;

					case TypeCode.Object:
						if (PropertyValue is TimeSpan)
						{
							await Output.WriteStartElementAsync(string.Empty, "TS", Namespace);
							if (!(PropertyName is null))
								await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await Output.WriteEndElementAsync();
						}
						else if (PropertyValue is DateTimeOffset DTO)
						{
							await Output.WriteStartElementAsync(string.Empty, "DTO", Namespace);
							if (!(PropertyName is null))
								await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, XML.Encode(DTO));
							await Output.WriteEndElementAsync();
						}
						else if (PropertyValue is CaseInsensitiveString Cis)
						{
							s = Cis.Value;
							try
							{
								XmlConvert.VerifyXmlChars(s);
								await Output.WriteStartElementAsync(string.Empty, "CIS", Namespace);
								if (!(PropertyName is null))
									await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
								await Output.WriteEndElementAsync();
							}
							catch (XmlException)
							{
								byte[] Bin = System.Text.Encoding.UTF8.GetBytes(s);
								s = Convert.ToBase64String(Bin);
								await Output.WriteStartElementAsync(string.Empty, "CIS64", Namespace);
								if (!(PropertyName is null))
									await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
								await Output.WriteEndElementAsync();
							}
						}
						else if (PropertyValue is byte[] Bin)
						{
							await Output.WriteStartElementAsync(string.Empty, "Bin", Namespace);
							if (!(PropertyName is null))
								await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);

							byte[] Buf = null;
							int c = Bin.Length;
							int i = 0;
							int d;
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

								await Output.WriteElementStringAsync(string.Empty, "Chunk", Namespace, Convert.ToBase64String(Buf, 0, j));
								i += j;
							}

							await Output.WriteEndElementAsync();
						}
						else if (PropertyValue is Guid)
						{
							await Output.WriteStartElementAsync(string.Empty, "ID", Namespace);
							if (!(PropertyName is null))
								await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await Output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await Output.WriteEndElementAsync();
						}
						else if (PropertyValue is Array A)
						{
							await Output.WriteStartElementAsync(string.Empty, "Array", Namespace);
							if (!(PropertyName is null))
								await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await Output.WriteAttributeStringAsync(string.Empty, "elementType", string.Empty, PropertyValue.GetType().GetElementType().FullName);

							foreach (object Obj in A)
								await ReportProperty(Output, null, Obj, Namespace);

							await Output.WriteEndElementAsync();
						}
						else if (PropertyValue is GenericObject Obj)
						{
							await Output.WriteStartElementAsync(string.Empty, "Obj", Namespace);
							if (!(PropertyName is null))
								await Output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await Output.WriteAttributeStringAsync(string.Empty, "type", string.Empty, Obj.TypeName);

							foreach (KeyValuePair<string, object> P in Obj)
								await ReportProperty(Output, P.Key, P.Value, Namespace);

							await Output.WriteEndElementAsync();
						}
						else
							throw new Exception("Unhandled property value type: " + PropertyValue.GetType().FullName);
						break;

					default:
						throw new Exception("Unhandled property value type: " + PropertyValue.GetType().FullName);
				}
			}
		}

		/// <summary>
		/// Clears a collection in the ledger.
		/// </summary>
		/// <param name="Collection">Cleared collection.</param>
		public async Task ClearedCollection(string Collection)
		{
			if (!this.running)
				return;

			DateTime Timestamp = DateTime.UtcNow;

			await this.semaphore.WaitAsync();
			try
			{
				try
				{
					await this.BeforeWrite();

					if (!(this.output is null))
					{
						await this.output.WriteStartElementAsync(string.Empty, "Clear", Namespace);
						await this.output.WriteAttributeStringAsync(string.Empty, "timestamp", string.Empty, XML.Encode(Timestamp));
						await this.output.WriteAttributeStringAsync(string.Empty, "collection", string.Empty, Collection);
						await this.output.WriteEndElementAsync();
						await this.output.FlushAsync();
					}
				}
				catch (Exception)
				{
					try
					{
						await this.DisposeOutput();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		/// <summary>
		/// Registers a recipient of external events.
		/// </summary>
		/// <param name="ExternalEvents">Interface for recipient of external events.</param>
		/// <exception cref="Exception">If another recipient has been previously registered.</exception>
		public void Register(ILedgerExternalEvents ExternalEvents)
		{
			if (!(this.externalEvents is null) && this.externalEvents != ExternalEvents)
				throw new Exception("An interface for external events has already been registered.");

			this.externalEvents = ExternalEvents;
		}

		/// <summary>
		/// Unregisters a recipient of external events.
		/// </summary>
		/// <param name="ExternalEvents">Interface for recipient of external events.</param>
		/// <exception cref="Exception">If the recipient is not the currently registered recipient.</exception>
		public void Unregister(ILedgerExternalEvents ExternalEvents)
		{
			if (!(this.externalEvents is null) && this.externalEvents != ExternalEvents)
				throw new Exception("The registered interface for external events differs from the one presented.");

			this.externalEvents = null;
		}

		/// <summary>
		/// Gets an eumerator for objects of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Type of object entries to enumerate.</typeparam>
		/// <returns>Enumerator object.</returns>
		public Task<ILedgerEnumerator<T>> GetEnumerator<T>()
		{
			return Task.FromResult<ILedgerEnumerator<T>>(new NoEntries<T>());
		}

		/// <summary>
		/// Gets an eumerator for objects in a collection.
		/// </summary>
		/// <param name="CollectionName">Collection to enumerate.</param>
		/// <returns>Enumerator object.</returns>
		public Task<ILedgerEnumerator<object>> GetEnumerator(string CollectionName)
		{
			return Task.FromResult<ILedgerEnumerator<object>>(new NoEntries<object>());
		}

		private class NoEntries<T> : ILedgerEnumerator<T>
		{
			public ILedgerEntry<T> Current => null;
			object IEnumerator.Current => null;
			public void Dispose() { }
			public bool MoveNext() => false;
			public Task<bool> MoveNextAsync() => Task.FromResult(false);
			public void Reset() { }
		}

		/// <summary>
		/// Gets an array of available collections.
		/// </summary>
		/// <returns>Array of collections.</returns>
		public Task<string[]> GetCollections()
		{
			return Task.FromResult(new string[0]);
		}

		/// <summary>
		/// Performs an export of the entire ledger.
		/// </summary>
		/// <param name="Output">Ledger will be output to this interface.</param>
		/// <param name="Restriction">Optional restrictions to apply.
		/// If null, all information available in the ledger will be exported.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public Task<bool> Export(ILedgerExport Output, LedgerExportRestriction Restriction)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Performs an export of the entire ledger.
		/// </summary>
		/// <param name="Output">Ledger will be output to this interface.</param>
		/// <param name="Restriction">Optional restrictions to apply.
		/// If null, all information available in the ledger will be exported.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public Task<bool> Export(ILedgerExport Output, LedgerExportRestriction Restriction, ProfilerThread Thread)
		{
			return Task.FromResult(true);
		}

		#endregion

		#region ILedgerExport

		private readonly LinkedList<KeyValuePair<string, object>> blockMetaData = new LinkedList<KeyValuePair<string, object>>();
		private string writtenCollection = null;
		private string writtenBlockId = null;
		private string currentCollection = null;
		private string currentBlockId = null;
		private string currentEntryObjectId = null;
		private string currentEntryObjectType = null;
		private EntryType currentEntryType = EntryType.New;
		private DateTimeOffset currentEntryTimestamp = DateTimeOffset.MinValue;
		private Dictionary<string, object> currentEntryProperties = null;

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> StartLedger()
		{
			this.currentCollection = null;
			await this.Start();
			return true;
		}

		/// <summary>
		/// Is called when export of ledger is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> EndLedger()
		{
			await this.Stop();
			return true;
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartCollection(string CollectionName)
		{
			this.currentCollection = CollectionName;
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> EndCollection()
		{
			if (!(this.writtenCollection is null))
			{
				await this.output.WriteEndElementAsync();
				this.writtenCollection = null;
			}

			this.currentCollection = null;
			return true;
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartBlock(string BlockID)
		{
			this.currentBlockId = BlockID;
			return Task.FromResult(true);
		}

		/// <summary>
		/// Reports block meta-data.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Value">Meta-data value.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> BlockMetaData(string Key, object Value)
		{
			this.blockMetaData.AddLast(new KeyValuePair<string, object>(Key, Value));
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> EndBlock()
		{
			if (!(this.writtenBlockId is null))
			{
				await this.output.WriteEndElementAsync();
				this.writtenBlockId = null;
				this.blockMetaData.Clear();
			}

			this.currentBlockId = null;
			return true;
		}

		/// <summary>
		/// Is called when an entry is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>Object ID of object, after optional mapping.</returns>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
		{
			this.currentEntryObjectId = ObjectId;
			this.currentEntryObjectType = TypeName;
			this.currentEntryType = EntryType;
			this.currentEntryTimestamp = EntryTimestamp;

			if (this.currentEntryProperties is null)
				this.currentEntryProperties = new Dictionary<string, object>();
			else
				this.currentEntryProperties.Clear();

			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> EndEntry()
		{
			if (!this.running)
				return false;

			await this.semaphore.WaitAsync();
			try
			{
				try
				{
					await this.BeforeWrite();

					if (!(this.output is null))
					{
						await this.WritePendingInfoLocked();

						await this.output.WriteStartElementAsync(string.Empty, this.currentEntryType.ToString(), Namespace);
						await this.output.WriteAttributeStringAsync(string.Empty, "timestamp", string.Empty, XML.Encode(this.currentEntryTimestamp));
						await this.output.WriteAttributeStringAsync(string.Empty, "type", string.Empty, this.currentEntryObjectType);
						await this.output.WriteAttributeStringAsync(string.Empty, "id", string.Empty, this.currentEntryObjectId);

						foreach (KeyValuePair<string, object> P in this.currentEntryProperties)
							await ReportProperty(this.output, P.Key, P.Value, Namespace);

						await this.output.WriteEndElementAsync();
						await this.output.FlushAsync();
					}
				}
				catch (Exception)
				{
					try
					{
						await this.DisposeOutput();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
			finally
			{
				this.semaphore.Release();
			}

			return true;
		}

		private async Task WritePendingInfoLocked()
		{
			if (!(this.currentBlockId is null) &&
				!(this.writtenBlockId is null) &&
				this.currentBlockId != this.writtenBlockId)
			{
				await this.output.WriteEndElementAsync();
				this.writtenBlockId = null;
				this.blockMetaData.Clear();
			}

			if (!(this.currentCollection is null) &&
				!(this.writtenCollection is null) &&
				this.currentCollection != this.writtenCollection)
			{
				await this.output.WriteEndElementAsync();
				this.writtenCollection = null;
			}

			if (this.writtenCollection is null)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Collection", Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "name", string.Empty, this.currentCollection);
				this.writtenCollection = this.currentCollection;
			}

			if (this.writtenBlockId is null)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Block", Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "id", string.Empty, this.currentBlockId);
				this.writtenBlockId = this.currentBlockId;

				if (!(this.blockMetaData.First is null))
				{
					foreach (KeyValuePair<string, object> P in this.blockMetaData)
						await ReportProperty(this.output, P.Key, P.Value, Namespace);
				}
			}
		}

		/// <summary>
		/// Is called when the collection has been cleared.
		/// </summary>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		public async Task<bool> CollectionCleared(DateTimeOffset EntryTimestamp)
		{
			if (!this.running)
				return false;

			await this.semaphore.WaitAsync();
			try
			{
				try
				{
					await this.BeforeWrite();

					if (!(this.output is null))
					{
						await this.WritePendingInfoLocked();

						await this.output.WriteStartElementAsync(string.Empty, "Clear", Namespace);
						await this.output.WriteAttributeStringAsync(string.Empty, "timestamp", string.Empty, XML.Encode(EntryTimestamp));
						await this.output.WriteEndElementAsync();
						await this.output.FlushAsync();
					}
				}
				catch (Exception)
				{
					try
					{
						await this.DisposeOutput();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
			finally
			{
				this.semaphore.Release();
			}

			return true;
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportProperty(string PropertyName, object PropertyValue)
		{
			this.currentEntryProperties[PropertyName] = PropertyValue;
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		/// <returns>If export can continue.</returns>
		public async Task<bool> ReportError(string Message)
		{
			if (!this.running)
				return false;

			await this.semaphore.WaitAsync();
			try
			{
				try
				{
					await this.BeforeWrite();

					if (!(this.output is null))
					{
						await this.WritePendingInfoLocked();

						await this.output.WriteCommentAsync(Message);
						await this.output.FlushAsync();
					}
				}
				catch (Exception)
				{
					try
					{
						await this.DisposeOutput();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
			finally
			{
				this.semaphore.Release();
			}

			return true;
		}

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportException(Exception Exception)
		{
			return this.ReportError(Exception.Message);
		}

		#endregion

	}
}
