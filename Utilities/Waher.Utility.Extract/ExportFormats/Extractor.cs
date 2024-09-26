using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.XmlLedger;
using Waher.Runtime.Console;

namespace Waher.Utility.Extract.ExportFormats
{
	/// <summary>
	/// Extracts information to files.
	/// </summary>
	public class Extractor : IExportFormat, IDisposable
	{
		private readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private readonly Aes aes;
		private readonly Dictionary<string, bool> collections;
		private readonly Dictionary<string, bool> fileNames;
		private readonly XmlWriterSettings settings;
		private XmlWriter output = null;
		private readonly string outputFolder;
		private readonly string filesFolder = null;
		private readonly bool database;
		private readonly bool ledger;
		private readonly bool files;
		private bool exportCollection = false;
		private bool inCollection = false;
		private bool inMetaData = false;

		public Extractor(string OutputFolder, bool Database, bool Ledger, bool Files,
			Dictionary<string, bool> Collections, Dictionary<string, bool> FileNames)
		{
			this.outputFolder = OutputFolder;
			this.database = Database;
			this.ledger = Ledger;
			this.files = Files;
			this.collections = Collections;
			this.fileNames = FileNames;

			this.filesFolder = Path.Combine(this.outputFolder, "Files");

			this.settings = new XmlWriterSettings()
			{
				Async = true,
				CheckCharacters = false,
				CloseOutput = true,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = false,
				WriteEndDocumentOnClose = true
			};

			this.aes = Aes.Create();

			this.aes.BlockSize = 128;
			this.aes.KeySize = 256;
			this.aes.Mode = CipherMode.CBC;
			this.aes.Padding = PaddingMode.None;
		}

		public string FileName => string.Empty;
		public string[] CollectionNames => null;
		public Dictionary<string, bool> FileNames => this.fileNames;

		public void Dispose()
		{
			this.aes.Dispose();
			this.rnd.Dispose();
		}

		public Task Start()
		{
			if (!Directory.Exists(this.outputFolder))
				Directory.CreateDirectory(this.outputFolder);

			return Task.CompletedTask;
		}

		public Task End()
		{
			return Task.CompletedTask;
		}

		public async Task StartDatabase()
		{
			ConsoleOut.WriteLine("Processing database section.");

			if (this.database)
			{
				string FileName = Path.Combine(this.outputFolder, "Database.xml");
				this.output = XmlWriter.Create(FileName, this.settings);

				await this.output.WriteStartDocumentAsync();
				await this.output.WriteStartElementAsync(string.Empty, "Export", XmlFileLedger.Namespace);
				await this.output.WriteStartElementAsync(string.Empty, "Database", XmlFileLedger.Namespace);
			}
			else
				this.output = null;
		}

		public async Task EndDatabase()
		{
			if (this.output is not null)
			{
				await this.output.WriteEndElementAsync();
				await this.output.WriteEndElementAsync();
				await this.output.WriteEndDocumentAsync();
				await this.output.FlushAsync();
				this.output.Close();
				this.output.Dispose();
				this.output = null;
			}
		}

		public async Task StartLedger()
		{
			ConsoleOut.WriteLine("Processing ledger section.");

			if (this.ledger)
			{
				string FileName = Path.Combine(this.outputFolder, "Ledger.xml");
				this.output = XmlWriter.Create(FileName, this.settings);

				await this.output.WriteStartDocumentAsync();
				await this.output.WriteStartElementAsync(string.Empty, "Export", XmlFileLedger.Namespace);
				await this.output.WriteStartElementAsync(string.Empty, "Ledger", XmlFileLedger.Namespace);
			}
			else
				this.output = null;
		}

		public async Task EndLedger()
		{
			if (this.output is not null)
			{
				await this.output.WriteEndElementAsync();
				await this.output.WriteEndElementAsync();
				await this.output.WriteEndDocumentAsync();
				await this.output.FlushAsync();
				this.output.Close();
				this.output.Dispose();
				this.output = null;
			}
		}

		public async Task StartCollection(string CollectionName)
		{
			ConsoleOut.WriteLine(CollectionName + "...");

			this.inCollection = true;
			this.exportCollection = (this.collections?.ContainsKey(CollectionName) ?? true) && this.output is not null;

			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Collection", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "name", string.Empty, CollectionName);
			}
		}

		public async Task EndCollection()
		{
			this.inCollection = false;
			if (this.exportCollection)
				await this.output.WriteEndElementAsync();
		}

		public async Task StartIndex()
		{
			if (this.exportCollection)
				await this.output.WriteStartElementAsync(string.Empty, "Index", XmlFileLedger.Namespace);
		}

		public async Task EndIndex()
		{
			if (this.exportCollection)
				await this.output.WriteEndElementAsync();
		}

		public async Task ReportIndexField(string FieldName, bool Ascending)
		{
			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Field", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "name", string.Empty, FieldName);
				await this.output.WriteAttributeStringAsync(string.Empty, "ascending", string.Empty, CommonTypes.Encode(Ascending));
				await this.output.WriteEndElementAsync();
			}
		}

		public async Task StartBlock(string BlockID)
		{
			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Block", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "id", string.Empty, BlockID);
			}
		}

		public async Task EndBlock()
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

		public async Task BlockMetaData(string Key, object Value)
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

		public async Task<string> StartObject(string ObjectId, string TypeName)
		{
			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Obj", XmlFileLedger.Namespace);
				await this.output.WriteAttributeStringAsync(string.Empty, "id", string.Empty, ObjectId);
				await this.output.WriteAttributeStringAsync(string.Empty, "type", string.Empty, TypeName);
			}

			return ObjectId;
		}

		public async Task EndObject()
		{
			if (this.exportCollection)
				await this.output.WriteEndElementAsync();
		}

		public async Task<string> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
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

		public async Task EndEntry()
		{
			if (this.exportCollection)
				await this.output.WriteEndElementAsync();
		}

		/// <summary>
		/// Is called when a collection has been cleared.
		/// </summary>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		public async Task CollectionCleared(DateTimeOffset EntryTimestamp)
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

		public Task ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.exportCollection)
				return XmlFileLedger.ReportProperty(this.output, PropertyName, PropertyValue, XmlFileLedger.Namespace);
			else
				return Task.CompletedTask;
		}

		public async Task ReportError(string Message)
		{
			if (!this.inCollection || this.exportCollection)
				await this.output.WriteElementStringAsync(string.Empty, "Error", XmlFileLedger.Namespace, Message);
		}

		public async Task ReportException(Exception Exception)
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
				else if (Exception.InnerException is not null)
					await this.ReportException(Exception.InnerException);

				await this.output.WriteEndElementAsync();
			}
		}

		public Task StartFiles()
		{
			ConsoleOut.WriteLine("Processing files section.");

			if (this.files)
			{
				if (!Directory.Exists(this.filesFolder))
					Directory.CreateDirectory(this.filesFolder);
			}

			return Task.CompletedTask;
		}

		public Task EndFiles()
		{
			return Task.CompletedTask;
		}

		public async Task ExportFile(string FileName, Stream File)
		{
			if (Path.IsPathRooted(FileName))
				throw new Exception("Absolute path names not allowed: " + FileName);

			ConsoleOut.WriteLine(FileName);
			FileName = Path.Combine(this.filesFolder, FileName);

			string Folder = Path.GetDirectoryName(FileName);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			using FileStream fs = System.IO.File.Create(FileName);

			File.Position = 0;
			await File.CopyToAsync(fs);
		}

		public ICryptoTransform CreateDecryptor(byte[] Key, byte[] IV)
		{
			return this.aes.CreateDecryptor(Key, IV);
		}

		public static string ToString(double x)
		{
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		public static string ToString(float x)
		{
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		public static string ToString(decimal x)
		{
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}
	}
}
