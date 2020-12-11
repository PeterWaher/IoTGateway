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
using Waher.Persistence.Serialization;

namespace Waher.Utility.Extract.ExportFormats
{
	/// <summary>
	/// Extracts information to files.
	/// </summary>
	public class Extractor : IExportFormat, IDisposable
	{
		private readonly RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
		private readonly AesCryptoServiceProvider aes;
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
				NewLineHandling = NewLineHandling.Entitize,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = false,
				WriteEndDocumentOnClose = true
			};

			this.aes = new AesCryptoServiceProvider()
			{
				BlockSize = 128,
				KeySize = 256,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.None
			};
		}

		public string FileName => string.Empty;
		public string[] CollectionNames => null;

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
			Console.Out.WriteLine("Processing database section.");

			if (this.database)
			{
				string FileName = Path.Combine(this.outputFolder, "Database.xml");
				this.output = XmlWriter.Create(FileName, this.settings);

				await this.output.WriteStartDocumentAsync();
				await this.output.WriteStartElementAsync(string.Empty, "Export", Program.ExportNamepace);
				await this.output.WriteStartElementAsync(string.Empty, "Database", Program.ExportNamepace);
			}
			else
				this.output = null;
		}

		public async Task EndDatabase()
		{
			if (!(this.output is null))
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
			Console.Out.WriteLine("Processing ledger section.");

			if (this.ledger)
			{
				string FileName = Path.Combine(this.outputFolder, "Ledger.xml");
				this.output = XmlWriter.Create(FileName, this.settings);

				await this.output.WriteStartDocumentAsync();
				await this.output.WriteStartElementAsync(string.Empty, "Export", Program.ExportNamepace);
				await this.output.WriteStartElementAsync(string.Empty, "Ledger", Program.ExportNamepace);
			}
			else
				this.output = null;
		}

		public async Task EndLedger()
		{
			if (!(this.output is null))
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
			Console.Out.WriteLine(CollectionName + "...");

			this.inCollection = true;
			this.exportCollection = (this.collections?.ContainsKey(CollectionName) ?? true) && !(this.output is null);

			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Collection", Program.ExportNamepace);
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
				await this.output.WriteStartElementAsync(string.Empty, "Index", Program.ExportNamepace);
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
				await this.output.WriteStartElementAsync(string.Empty, "Field", Program.ExportNamepace);
				await this.output.WriteAttributeStringAsync(string.Empty, "name", string.Empty, FieldName);
				await this.output.WriteAttributeStringAsync(string.Empty, "ascending", string.Empty, CommonTypes.Encode(Ascending));
				await this.output.WriteEndElementAsync();
			}
		}

		public async Task StartBlock(string BlockID)
		{
			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Block", Program.ExportNamepace);
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
					await this.output.WriteStartElementAsync(string.Empty, "MetaData", Program.ExportNamepace);
					this.inMetaData = true;
				}

				await this.ReportProperty(Key, Value);
			}
		}

		public async Task<string> StartObject(string ObjectId, string TypeName)
		{
			if (this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Obj", Program.ExportNamepace);
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

				await this.output.WriteStartElementAsync(string.Empty, EntryType.ToString(), Program.ExportNamepace);
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

		public async Task ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.exportCollection)
			{
				if (PropertyValue is null)
				{
					await this.output.WriteStartElementAsync(string.Empty, "Null", Program.ExportNamepace);
					if (!(PropertyName is null))
						await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
					await this.output.WriteEndElementAsync();
				}
				else if (PropertyValue is Enum)
				{
					await this.output.WriteStartElementAsync(string.Empty, "En", Program.ExportNamepace);
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
							await this.output.WriteStartElementAsync(string.Empty, "Bl", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, CommonTypes.Encode((bool)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Byte:
							await this.output.WriteStartElementAsync(string.Empty, "B", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Char:
							await this.output.WriteStartElementAsync(string.Empty, "Ch", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.DateTime:
							await this.output.WriteStartElementAsync(string.Empty, "DT", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, XML.Encode((DateTime)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Decimal:
							await this.output.WriteStartElementAsync(string.Empty, "Dc", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, ToString((decimal)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Double:
							await this.output.WriteStartElementAsync(string.Empty, "Db", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, ToString((double)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Int16:
							await this.output.WriteStartElementAsync(string.Empty, "I2", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Int32:
							await this.output.WriteStartElementAsync(string.Empty, "I4", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Int64:
							await this.output.WriteStartElementAsync(string.Empty, "I8", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.SByte:
							await this.output.WriteStartElementAsync(string.Empty, "I1", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Single:
							await this.output.WriteStartElementAsync(string.Empty, "Fl", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, ToString((float)PropertyValue));
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.String:
							string s = PropertyValue.ToString();
							try
							{
								XmlConvert.VerifyXmlChars(s);
								await this.output.WriteStartElementAsync(string.Empty, "S", Program.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
								await this.output.WriteEndElementAsync();
							}
							catch (XmlException)
							{
								byte[] Bin = Encoding.UTF8.GetBytes(s);
								s = Convert.ToBase64String(Bin);
								await this.output.WriteStartElementAsync(string.Empty, "S64", Program.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
								await this.output.WriteEndElementAsync();
							}
							break;

						case TypeCode.UInt16:
							await this.output.WriteStartElementAsync(string.Empty, "U2", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.UInt32:
							await this.output.WriteStartElementAsync(string.Empty, "U4", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.UInt64:
							await this.output.WriteStartElementAsync(string.Empty, "U8", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.DBNull:
						case TypeCode.Empty:
							await this.output.WriteStartElementAsync(string.Empty, "Null", Program.ExportNamepace);
							if (!(PropertyName is null))
								await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
							await this.output.WriteEndElementAsync();
							break;

						case TypeCode.Object:
							if (PropertyValue is TimeSpan)
							{
								await this.output.WriteStartElementAsync(string.Empty, "TS", Program.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
								await this.output.WriteEndElementAsync();
							}
							else if (PropertyValue is DateTimeOffset DTO)
							{
								await this.output.WriteStartElementAsync(string.Empty, "DTO", Program.ExportNamepace);
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
									await this.output.WriteStartElementAsync(string.Empty, "CIS", Program.ExportNamepace);
									if (!(PropertyName is null))
										await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
									await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
									await this.output.WriteEndElementAsync();
								}
								catch (XmlException)
								{
									byte[] Bin = Encoding.UTF8.GetBytes(s);
									s = Convert.ToBase64String(Bin);
									await this.output.WriteStartElementAsync(string.Empty, "CIS64", Program.ExportNamepace);
									if (!(PropertyName is null))
										await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
									await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, s);
									await this.output.WriteEndElementAsync();
								}
							}
							else if (PropertyValue is byte[] Bin)
							{
								await this.output.WriteStartElementAsync(string.Empty, "Bin", Program.ExportNamepace);
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
								await this.output.WriteStartElementAsync(string.Empty, "ID", Program.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "v", string.Empty, PropertyValue.ToString());
								await this.output.WriteEndElementAsync();
							}
							else if (PropertyValue is Array A)
							{
								await this.output.WriteStartElementAsync(string.Empty, "Array", Program.ExportNamepace);
								if (!(PropertyName is null))
									await this.output.WriteAttributeStringAsync(string.Empty, "n", string.Empty, PropertyName);
								await this.output.WriteAttributeStringAsync(string.Empty, "elementType", string.Empty, PropertyValue.GetType().GetElementType().FullName);

								foreach (object Obj in A)
									await this.ReportProperty(null, Obj);

								await this.output.WriteEndElementAsync();
							}
							else if (PropertyValue is GenericObject Obj)
							{
								await this.output.WriteStartElementAsync(string.Empty, "Obj", Program.ExportNamepace);
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

		public async Task ReportError(string Message)
		{
			if (!this.inCollection || this.exportCollection)
				await this.output.WriteElementStringAsync(string.Empty, "Error", Program.ExportNamepace, Message);
		}

		public async Task ReportException(Exception Exception)
		{
			if (!this.inCollection || this.exportCollection)
			{
				await this.output.WriteStartElementAsync(string.Empty, "Exception", Program.ExportNamepace);
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

		public Task StartFiles()
		{
			Console.Out.WriteLine("Processing files section.");

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

			Console.Out.WriteLine(FileName);
			FileName = Path.Combine(this.filesFolder, FileName);

			string Folder = Path.GetDirectoryName(FileName);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			using (FileStream fs = System.IO.File.Create(FileName))
			{
				File.Position = 0;
				await File.CopyToAsync(fs);
			}
		}

		public ICryptoTransform CreateDecryptor(byte[] Key, byte[] IV)
		{
			return this.aes.CreateDecryptor(Key, IV);
		}

		public string ToString(double x)
		{
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		public string ToString(float x)
		{
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		public string ToString(decimal x)
		{
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}
	}
}
