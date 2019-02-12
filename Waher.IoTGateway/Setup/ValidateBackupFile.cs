using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.IoTGateway.WebResources.ExportFormats;
using Waher.Persistence;
using Waher.Security;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Class validating the status of a backup file.
	/// </summary>
	public class ValidateBackupFile : IExportFormat
	{
		/// <summary>
		/// Current index being exported.
		/// </summary>
		protected List<string> index = new List<string>();

		/// <summary>
		/// Current collection being exported.
		/// </summary>
		protected string collectionName = null;

		/// <summary>
		/// ID of current object being exported.
		/// </summary>
		protected string objectId = null;

		/// <summary>
		/// Type Name of current object being exported.
		/// </summary>
		protected string typeName = null;

		/// <summary>
		/// Object ID map, if available
		/// </summary>
		protected readonly Dictionary<string, string> objectIdMap;

		/// <summary>
		/// If Object IDs are mapped.
		/// </summary>
		protected readonly bool mapObjectIds;

		private readonly string fileName;
		private readonly int objectIdByteCount;
		private long nrCollections = 0;
		private long nrIndices = 0;
		private long nrObjects = 0;
		private long nrProperties = 0;
		private long nrFiles = 0;
		private long nrFileBytes = 0;
		/// <summary>
		/// Class validating the status of a backup file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="ObjectIdMap">Object ID Mapping, if available.</param>
		public ValidateBackupFile(string FileName, Dictionary<string, string> ObjectIdMap)
		{
			this.fileName = FileName;

			if ((this.objectIdByteCount = Database.Provider.ObjectIdByteCount) < 16)
			{
				this.objectIdMap = ObjectIdMap ?? new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
				this.mapObjectIds = true;
			}
			else
			{
				this.objectIdMap = null;
				this.mapObjectIds = false;
			}
		}

		/// <summary>
		/// Name of file
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Number of collections processed.
		/// </summary>
		public long NrCollections => this.nrCollections;

		/// <summary>
		/// Number of indices processed.
		/// </summary>
		public long NrIndices => this.nrIndices;

		/// <summary>
		/// Number of objects processed.
		/// </summary>
		public long NrObjects => this.nrObjects;

		/// <summary>
		/// Number of propeties processed.
		/// </summary>
		public long NrProperties => this.nrProperties;

		/// <summary>
		/// Number of files processed.
		/// </summary>
		public long NrFiles => this.nrFiles;

		/// <summary>
		/// Number of file byets processed.
		/// </summary>
		public long NrFileBytes => this.nrFileBytes;

		/// <summary>
		/// Object ID mapping, if available.
		/// </summary>
		public Dictionary<string, string> ObjectIdMap => this.objectIdMap;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Starts export
		/// </summary>
		public virtual Task Start()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Ends export
		/// </summary>
		public virtual Task End()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when export is started.
		/// </summary>
		public virtual Task StartExport()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when export is finished.
		/// </summary>
		public virtual Task EndExport()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName"></param>
		public virtual Task StartCollection(string CollectionName)
		{
			this.collectionName = CollectionName;
			this.nrCollections++;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		public virtual Task EndCollection()
		{
			this.collectionName = null;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an index in a collection is started.
		/// </summary>
		public virtual Task StartIndex()
		{
			this.index.Clear();
			this.nrIndices++;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an index in a collection is finished.
		/// </summary>
		public virtual Task EndIndex()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a field in an index is reported.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Ascending">If the field is sorted using ascending sort order.</param>
		public virtual Task ReportIndexField(string FieldName, bool Ascending)
		{
			if (Ascending)
				this.index.Add(FieldName);
			else
				this.index.Add("-" + FieldName);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		public virtual Task<string> StartObject(string ObjectId, string TypeName)
		{
			if (this.mapObjectIds)
			{
				string ObjectId0 = ObjectId;

				if (!this.objectIdMap.TryGetValue(ObjectId0, out ObjectId))
				{
					byte[] A;
					int i, c;

					if (System.Guid.TryParse(ObjectId0, out Guid Guid))
						A = Guid.ToByteArray();
					else
						A = Hashes.StringToBinary(ObjectId0);

					if (!(A is null) && (c = A.Length) != this.objectIdByteCount)
					{
						if (c > this.objectIdByteCount)
						{
							if (c == 16)
							{
								for (i = this.objectIdByteCount; i < c; i++)
									A[i] = 0;

								ObjectId = new Guid(A).ToString();
							}
							else
							{
								Array.Resize<byte>(ref A, this.objectIdByteCount);
								ObjectId = Hashes.BinaryToString(A);
							}
						}
						else
						{
							Array.Resize<byte>(ref A, this.objectIdByteCount);

							if (this.objectIdByteCount == 16)
								ObjectId = new Guid(A).ToString();
							else
								ObjectId = Hashes.BinaryToString(A);
						}

						while (this.objectIdMap.ContainsKey(ObjectId))
						{
							if (this.objectIdByteCount == 16)
								ObjectId = Guid.NewGuid().ToString();
							else
							{
								A = Gateway.NextBytes(this.objectIdByteCount);
								ObjectId = Hashes.BinaryToString(A);
							}
						}

						this.objectIdMap[ObjectId0] = ObjectId;
					}
					else
						ObjectId = ObjectId0;
				}
			}

			this.objectId = ObjectId;
			this.typeName = TypeName;
			this.nrObjects++;

			return Task.FromResult<string>(ObjectId);
		}

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		public virtual Task EndObject()
		{
			this.objectId = null;
			this.typeName = null;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		public virtual Task ReportProperty(string PropertyName, object PropertyValue)
		{
			this.nrProperties++;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Starts export of files.
		/// </summary>
		public virtual Task StartFiles()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Ends export of files.
		/// </summary>
		public virtual Task EndFiles()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Export file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="File">File stream</param>
		public virtual Task ExportFile(string FileName, Stream File)
		{
			this.nrFiles++;
			this.nrFileBytes += File.Length;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		public virtual Task ReportError(string Message)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		public virtual Task ReportException(Exception Exception)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// If any clients should be updated about export status.
		/// </summary>
		/// <param name="ForceUpdate">If updates should be forced.</param>
		public virtual Task UpdateClient(bool ForceUpdate)
		{
			return Task.CompletedTask;
		}
	}
}
