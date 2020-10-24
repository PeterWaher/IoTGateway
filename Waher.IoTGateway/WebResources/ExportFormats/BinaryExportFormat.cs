using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.IoTGateway.WebResources.ExportFormats
{
	/// <summary>
	/// Binary File export
	/// </summary>
	public class BinaryExportFormat : ExportFormat
	{
		internal const string Preamble = "IoT Gateway Export";

		/// <summary>
		/// Represents a <see cref="Boolean"/>
		/// </summary>
		public const byte TYPE_BOOLEAN = 0;

		/// <summary>
		/// Represents a <see cref="Byte"/>
		/// </summary>
		public const byte TYPE_BYTE = 1;

		/// <summary>
		/// Represents a <see cref="Int16"/>
		/// </summary>
		public const byte TYPE_INT16 = 2;

		/// <summary>
		/// Represents a <see cref="Int32"/>
		/// </summary>
		public const byte TYPE_INT32 = 3;

		/// <summary>
		/// Represents a <see cref="Int64"/>
		/// </summary>
		public const byte TYPE_INT64 = 4;

		/// <summary>
		/// Represents a <see cref="SByte"/>
		/// </summary>
		public const byte TYPE_SBYTE = 5;

		/// <summary>
		/// Represents a <see cref="UInt16"/>
		/// </summary>
		public const byte TYPE_UINT16 = 6;

		/// <summary>
		/// Represents a <see cref="UInt32"/>
		/// </summary>
		public const byte TYPE_UINT32 = 7;

		/// <summary>
		/// Represents a <see cref="UInt64"/>
		/// </summary>
		public const byte TYPE_UINT64 = 8;

		/// <summary>
		/// Represents a <see cref="Decimal"/>
		/// </summary>
		public const byte TYPE_DECIMAL = 9;

		/// <summary>
		/// Represents a <see cref="Double"/>
		/// </summary>
		public const byte TYPE_DOUBLE = 10;

		/// <summary>
		/// Represents a <see cref="Single"/>
		/// </summary>
		public const byte TYPE_SINGLE = 11;

		/// <summary>
		/// Represents a <see cref="DateTime"/>
		/// </summary>
		public const byte TYPE_DATETIME = 12;

		/// <summary>
		/// Represents a <see cref="TimeSpan"/>
		/// </summary>
		public const byte TYPE_TIMESPAN = 13;

		/// <summary>
		/// Represents a <see cref="Char"/>
		/// </summary>
		public const byte TYPE_CHAR = 14;

		/// <summary>
		/// Represents a <see cref="String"/>
		/// </summary>
		public const byte TYPE_STRING = 15;

		/// <summary>
		/// Represents an enumerated value.
		/// </summary>
		public const byte TYPE_ENUM = 16;

		/// <summary>
		/// Represents a byte array.
		/// </summary>
		public const byte TYPE_BYTEARRAY = 17;

		/// <summary>
		/// Represents a <see cref="Guid"/>
		/// </summary>
		public const byte TYPE_GUID = 18;

		/// <summary>
		/// Represents a <see cref="DateTimeOffset"/>
		/// </summary>
		public const byte TYPE_DATETIMEOFFSET = 19;

		/// <summary>
		/// Represents a <see cref="CaseInsensitiveString"/>
		/// </summary>
		public const byte TYPE_CI_STRING = 20;

		/// <summary>
		/// Represents the smallest possible value for the field type being searched or filtered.
		/// </summary>
		public const byte TYPE_MIN = 27;

		/// <summary>
		/// Represents the largest possible value for the field type being searched or filtered.
		/// </summary>
		public const byte TYPE_MAX = 28;

		/// <summary>
		/// Represents a null value.
		/// </summary>
		public const byte TYPE_NULL = 29;

		/// <summary>
		/// Represents an arary.
		/// </summary>
		public const byte TYPE_ARRAY = 30;

		/// <summary>
		/// Represents an object.
		/// </summary>
		public const byte TYPE_OBJECT = 31;

		private BinaryWriter w;
		private Stream output;
		private CryptoStream cs;
		private readonly int blockSize;
		private LinkedList<string> errors = null;
		private LinkedList<Exception> exceptions = null;
		private bool exportCollection = false;

		/// <summary>
		/// Binary File export
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="Created">When file was created</param>
		/// <param name="Output">Binary Output</param>
		/// <param name="File">File stream</param>
		/// <param name="OnlySelectedCollections">If only selected collections should be exported.</param>
		/// <param name="SelectedCollections">Array of selected collections.</param>
		public BinaryExportFormat(string FileName, DateTime Created, Stream Output, FileStream File, bool OnlySelectedCollections, Array SelectedCollections)
			: this(FileName, Created, Output, File, null, 0, OnlySelectedCollections, SelectedCollections)
		{
		}

		/// <summary>
		/// Binary File export
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="Created">When file was created</param>
		/// <param name="Output">Binary Output</param>
		/// <param name="File">File stream</param>
		/// <param name="CryptoStream">Cryptographic stream</param>
		/// <param name="BlockSize">Cryptographic block size</param>
		/// <param name="OnlySelectedCollections">If only selected collections should be exported.</param>
		/// <param name="SelectedCollections">Array of selected collections.</param>
		public BinaryExportFormat(string FileName, DateTime Created, Stream Output, FileStream File, CryptoStream CryptoStream, int BlockSize, 
			bool OnlySelectedCollections, Array SelectedCollections)
			: base(FileName, Created, File, OnlySelectedCollections, SelectedCollections)
		{
			this.output = Output;
			this.w = new BinaryWriter(this.output, Encoding.UTF8);
			this.cs = CryptoStream;
			this.blockSize = BlockSize;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			if (!(this.w is null))
			{
				this.w.Flush();
				this.output.Flush();

				if (this.cs != null && this.blockSize > 1)
				{
					Type T = typeof(CryptoStream);
					int i;
					FieldInfo FI = null;

                    PropertyInfo PI = T.GetProperty("_inputBufferIndex", BindingFlags.Instance | BindingFlags.NonPublic);
					if (PI is null)
					{
						FI = T.GetField("_inputBufferIndex", BindingFlags.Instance | BindingFlags.NonPublic);
						if (FI is null)
						{
							PI = T.GetProperty("_InputBufferIndex", BindingFlags.Instance | BindingFlags.NonPublic);
							if (PI is null)
								FI = T.GetField("_InputBufferIndex", BindingFlags.Instance | BindingFlags.NonPublic);
						}
					}

					do
					{
						if (!(PI is null))
							i = (int)PI.GetValue(this.cs);
						else if (!(FI is null))
							i = (int)FI.GetValue(this.cs);
						else
							i = 0;

						if (i > 0)
						{
							this.w.Write((byte)0);
							this.w.Flush();
							this.output.Flush();
						}
					}
					while (i != 0);

					this.cs.FlushFinalBlock();
				}
			}

			this.w?.Dispose();
			this.w = null;

			this.output?.Dispose();
			this.output = null;

			this.cs?.Dispose();
			this.cs = null;

			base.Dispose();
		}

		/// <summary>
		/// Starts export
		/// </summary>
		public override Task Start()
		{
			this.w.Write((byte)1); // Version.
			this.w.Write(Preamble);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Ends export
		/// </summary>
		public override Task End()
		{
			if (!(this.errors is null))
			{
				this.w.Write((byte)4); // Errors

				foreach (string Message in this.errors)
					this.w.Write(Message);

				this.w.Write(string.Empty);
				this.errors = null;
			}

			if (!(this.exceptions is null))
			{
				this.w.Write((byte)5); // Exceptions

				foreach (Exception ex in this.exceptions)
					this.OutputException(ex);

				this.w.Write(string.Empty);
				this.exceptions = null;
			}

			this.w.Write((byte)0);
			this.UpdateClient(true);

			return Task.CompletedTask;
		}

		private Task OutputException(Exception Exception)
		{
			this.w.Write(Exception.Message);
			this.w.Write(Log.CleanStackTrace(Exception.StackTrace));

			if (Exception is AggregateException AggregateException)
			{
				foreach (Exception ex in AggregateException.InnerExceptions)
					this.OutputException(ex);
			}
			else if (Exception.InnerException != null)
				this.OutputException(Exception.InnerException);

			this.w.Write(string.Empty);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when export of database is started.
		/// </summary>
		public override Task StartDatabase()
		{
			this.w.Write((byte)2); // Database
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when export of database is finished.
		/// </summary>
		public override Task EndDatabase()
		{
			this.w.Write(string.Empty);
			return this.UpdateClient(false);
		}

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		public override Task StartLedger()
		{
			this.w.Write((byte)6); // Ledger
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when export of ledger is finished.
		/// </summary>
		public override Task EndLedger()
		{
			this.w.Write(string.Empty);
			return this.UpdateClient(false);
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		public override Task StartCollection(string CollectionName)
		{
			if (this.exportCollection = this.ExportCollection(CollectionName))
				this.w.Write(CollectionName);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		public override Task EndCollection()
		{
			if (this.exportCollection)
				this.w.Write((byte)0);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an index in a collection is started.
		/// </summary>
		public override Task StartIndex()
		{
			if (this.exportCollection)
				this.w.Write((byte)1);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an index in a collection is finished.
		/// </summary>
		public override Task EndIndex()
		{
			if (this.exportCollection)
				this.w.Write(string.Empty);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a field in an index is reported.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Ascending">If the field is sorted using ascending sort order.</param>
		public override Task ReportIndexField(string FieldName, bool Ascending)
		{
			if (this.exportCollection)
			{
				this.w.Write(FieldName);
				this.w.Write(Ascending);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		public override Task StartBlock(string BlockID)
		{
			if (this.exportCollection)
			{
				this.w.Write((byte)1);
				this.w.Write(BlockID);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		public override Task EndBlock()
		{
			if (this.exportCollection)
				this.w.Write((byte)3);

			return Task.CompletedTask;
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
				this.w.Write((byte)4);
				this.w.Write(Key);
				await this.ReportProperty(null, Value);
			}
		}

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		public override Task<string> StartObject(string ObjectId, string TypeName)
		{
			if (this.exportCollection)
			{
				this.w.Write((byte)2);
				this.w.Write(ObjectId);
				this.w.Write(TypeName);
			}

			return Task.FromResult<string>(ObjectId);
		}

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		public override Task EndObject()
		{
			if (this.exportCollection)
			{
				this.w.Write(TYPE_NULL);
				this.w.Write(string.Empty);
			}

			return this.UpdateClient(false);
		}

		/// <summary>
		/// Is called when an entry is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>Object ID of object, after optional mapping.</returns>
		public override Task<string> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
		{
			if (this.exportCollection)
			{
				this.w.Write((byte)2);
				this.w.Write(ObjectId);
				this.w.Write(TypeName);
				this.w.Write((byte)EntryType);

				DateTime DT = EntryTimestamp.DateTime;
				TimeSpan TS = EntryTimestamp.Offset;

				this.w.Write((byte)DT.Kind);
				this.w.Write(DT.Ticks);
				this.w.Write(TS.Ticks);
			}

			return Task.FromResult<string>(ObjectId);
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		public override Task EndEntry()
		{
			if (this.exportCollection)
			{
				this.w.Write(TYPE_NULL);
				this.w.Write(string.Empty);
			}

			return this.UpdateClient(false);
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		public override Task ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.exportCollection)
			{
				if (PropertyValue is null)
				{
					this.w.Write(TYPE_NULL);
					if (!(PropertyName is null))
						this.w.Write(PropertyName);
				}
				else if (PropertyValue is Enum)
				{
					this.w.Write(TYPE_ENUM);
					if (!(PropertyName is null))
						this.w.Write(PropertyName);
					this.w.Write(PropertyValue.ToString());
				}
				else
				{
					switch (Type.GetTypeCode(PropertyValue.GetType()))
					{
						case TypeCode.Boolean:
							this.w.Write(TYPE_BOOLEAN);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((bool)PropertyValue);
							break;

						case TypeCode.Byte:
							this.w.Write(TYPE_BYTE);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((byte)PropertyValue);
							break;

						case TypeCode.Char:
							this.w.Write(TYPE_CHAR);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((char)PropertyValue);
							break;

						case TypeCode.DateTime:
							this.w.Write(TYPE_DATETIME);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);

							DateTime DT = (DateTime)PropertyValue;

							this.w.Write((byte)DT.Kind);
							this.w.Write(DT.Ticks);
							break;

						case TypeCode.Decimal:
							this.w.Write(TYPE_DECIMAL);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((decimal)PropertyValue);
							break;

						case TypeCode.Double:
							this.w.Write(TYPE_DOUBLE);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((double)PropertyValue);
							break;

						case TypeCode.Int16:
							this.w.Write(TYPE_INT16);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((short)PropertyValue);
							break;

						case TypeCode.Int32:
							this.w.Write(TYPE_INT32);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((int)PropertyValue);
							break;

						case TypeCode.Int64:
							this.w.Write(TYPE_INT64);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((long)PropertyValue);
							break;

						case TypeCode.SByte:
							this.w.Write(TYPE_SBYTE);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((sbyte)PropertyValue);
							break;

						case TypeCode.Single:
							this.w.Write(TYPE_SINGLE);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((float)PropertyValue);
							break;

						case TypeCode.String:
							this.w.Write(TYPE_STRING);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((string)PropertyValue);
							break;

						case TypeCode.UInt16:
							this.w.Write(TYPE_UINT16);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((ushort)PropertyValue);
							break;

						case TypeCode.UInt32:
							this.w.Write(TYPE_UINT32);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((uint)PropertyValue);
							break;

						case TypeCode.UInt64:
							this.w.Write(TYPE_UINT64);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							this.w.Write((ulong)PropertyValue);
							break;

						case TypeCode.DBNull:
						case TypeCode.Empty:
							this.w.Write(TYPE_NULL);
							if (!(PropertyName is null))
								this.w.Write(PropertyName);
							break;

						case TypeCode.Object:
							if (PropertyValue is TimeSpan TS)
							{
								this.w.Write(TYPE_TIMESPAN);
								if (!(PropertyName is null))
									this.w.Write(PropertyName);
								this.w.Write(TS.Ticks);
							}
							else if (PropertyValue is DateTimeOffset DTO)
							{
								this.w.Write(TYPE_DATETIMEOFFSET);
								if (!(PropertyName is null))
									this.w.Write(PropertyName);

								DT = DTO.DateTime;
								TS = DTO.Offset;

								this.w.Write((byte)DT.Kind);
								this.w.Write(DT.Ticks);
								this.w.Write(TS.Ticks);
							}
							else if (PropertyValue is CaseInsensitiveString CiString)
							{
								this.w.Write(TYPE_CI_STRING);
								if (!(PropertyName is null))
									this.w.Write(PropertyName);
								this.w.Write(CiString.Value);
							}
							else if (PropertyValue is byte[] Bin)
							{
								this.w.Write(TYPE_BYTEARRAY);
								if (!(PropertyName is null))
									this.w.Write(PropertyName);
								this.w.Write(Bin.Length);
								this.w.Write(Bin);
							}
							else if (PropertyValue is Guid Id)
							{
								this.w.Write(TYPE_GUID);
								if (!(PropertyName is null))
									this.w.Write(PropertyName);
								this.w.Write(Id.ToByteArray());
							}
							else if (PropertyValue is Array A)
							{
								this.w.Write(TYPE_ARRAY);
								if (!(PropertyName is null))
									this.w.Write(PropertyName);
								this.w.Write(PropertyValue.GetType().GetElementType().FullName);

								this.w.Write(A.LongLength);
								foreach (object Obj in A)
									this.ReportProperty(null, Obj);
							}
							else if (PropertyValue is GenericObject Obj)
							{
								this.w.Write(TYPE_OBJECT);
								if (!(PropertyName is null))
									this.w.Write(PropertyName);
								this.w.Write(Obj.TypeName);

								foreach (KeyValuePair<string, object> P in Obj)
									this.ReportProperty(P.Key, P.Value);

								this.w.Write(TYPE_NULL);
								this.w.Write(string.Empty);
							}
							else
								throw new Exception("Unhandled property value type: " + PropertyValue.GetType().FullName);
							break;

						default:
							throw new Exception("Unhandled property value type: " + PropertyValue.GetType().FullName);
					}
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		public override Task ReportError(string Message)
		{
			if (!string.IsNullOrEmpty(Message))
			{
				if (this.errors is null)
					this.errors = new LinkedList<string>();

				this.errors.AddLast(Message);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		public override Task ReportException(Exception Exception)
		{
			if (this.exceptions is null)
				this.exceptions = new LinkedList<Exception>();

			this.exceptions.AddLast(Exception);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Starts export of files.
		/// </summary>
		public override Task StartFiles()
		{
			this.w.Write((byte)3); // Files
			return Task.CompletedTask;
		}

		/// <summary>
		/// Ends export of files.
		/// </summary>
		public override Task EndFiles()
		{
			this.w.Write(string.Empty);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Export file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="File">File stream</param>
		public override Task ExportFile(string FileName, Stream File)
		{
			this.w.Write(FileName);
			this.w.Write(File.Length);
			this.w.Flush();

			return File.CopyToAsync(this.output);
		}

	}
}
