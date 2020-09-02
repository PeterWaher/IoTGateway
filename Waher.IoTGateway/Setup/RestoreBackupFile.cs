using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Security;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Class restoring the contents of a backup file.
	/// </summary>
	public class RestoreBackupFile : ValidateBackupFile
	{
		private readonly List<object> objects = new List<object>();
		private GenericObject obj = null;
		private EntryType entryType;
		private int nrObjectsInBulk = 0;
		private int nrObjectsFailed = 0;

		/// <summary>
		/// Class restoring the contents of a backup file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="ObjectIdMap">Object ID Mapping, if available.</param>
		public RestoreBackupFile(string FileName, Dictionary<string, string> ObjectIdMap)
			: base(FileName, ObjectIdMap)
		{
		}

		/// <summary>
		/// Number of objects that have not been possible to import.
		/// </summary>
		public int NrObjectsFailed => this.nrObjectsFailed;

		/// <summary>
		/// Starts export
		/// </summary>
		public override Task Start()
		{
			PropertyInfo PI = Database.Provider.GetType().GetProperty("DeleteObsoleteKeys");
			if (PI != null && PI.PropertyType == typeof(bool))
				PI.SetValue(Database.Provider, false);

			return base.Start();
		}

		/// <summary>
		/// Ends export
		/// </summary>
		public override Task End()
		{
			PropertyInfo PI = Database.Provider.GetType().GetProperty("DeleteObsoleteKeys");
			if (PI != null && PI.PropertyType == typeof(bool))
				PI.SetValue(Database.Provider, true);

			return base.End();
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName"></param>
		public override async Task StartCollection(string CollectionName)
		{
			await base.StartCollection(CollectionName);
			await Database.Provider.Clear(CollectionName);
			await Database.StartBulk();
			this.nrObjectsInBulk = 0;
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		public override async Task EndCollection()
		{
			int c = this.objects.Count;
			if (c > 0)
			{
				try
				{
					await Database.Provider.Insert(this.objects);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					this.nrObjectsFailed += this.objects.Count;
				}

				this.objects.Clear();

				this.nrObjectsInBulk += c;
			}

			await Database.EndBulk();
			await base.EndCollection();
		}

		/// <summary>
		/// Is called when an index in a collection is finished.
		/// </summary>
		public override Task EndIndex()
		{
			return Database.Provider.AddIndex(this.collectionName, this.index.ToArray());
		}

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		public override async Task<string> StartObject(string ObjectId, string TypeName)
		{
			ObjectId = await base.StartObject(ObjectId, TypeName);
			this.obj = new GenericObject(this.collectionName, TypeName, Guid.Parse(ObjectId));
			return ObjectId;
		}

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		public override async Task EndObject()
		{
			if (this.obj != null)
			{
				this.objects.Add(this.obj);
				this.obj = null;

				int c = this.objects.Count;
				if (c >= 100)
				{
					try
					{
						await Database.Provider.Insert(this.objects);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						this.nrObjectsFailed += this.objects.Count;
					}

					this.objects.Clear();

					this.nrObjectsInBulk += c;
					if (this.nrObjectsInBulk >= 1000)
					{
						await Database.EndBulk();
						await Database.StartBulk();
						this.nrObjectsInBulk = 0;
					}
				}
			}

			await base.EndObject();
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		public override Task ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.obj != null)
			{
				if (this.mapObjectIds)
				{
					if (PropertyValue is string s)
					{
						if (this.objectIdMap.TryGetValue(s, out s))
							PropertyValue = s;
					}
					else if (PropertyValue is Guid id)
					{
						s = id.ToString();

						if (this.objectIdMap.TryGetValue(s, out string s2))
						{
							if (Guid.TryParse(s2, out id))
								PropertyValue = id;
							else
								PropertyValue = s2;
						}
					}
					else if (PropertyValue is byte[] bin)
					{
						s = Hashes.BinaryToString(bin);

						if (this.objectIdMap.TryGetValue(s, out string s2))
						{
							if (Guid.TryParse(s2, out id))
								PropertyValue = id;
							else
							{
								byte[] bin2 = Hashes.StringToBinary(s2);

								if (bin2 is null)
									PropertyValue = s2;
								else if (bin2.Length == 16)
									PropertyValue = new Guid(bin2);
								else
									PropertyValue = bin2;
							}
						}
					}
				}

				this.obj[PropertyName] = PropertyValue;
			}

			return base.ReportProperty(PropertyName, PropertyValue);
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
			ObjectId = await base.StartEntry(ObjectId, TypeName, EntryType, EntryTimestamp);
			this.obj = new GenericObject(this.collectionName, TypeName, string.IsNullOrEmpty(ObjectId) ? Guid.Empty : Guid.Parse(ObjectId));
			this.entryType = EntryType;
			return ObjectId;
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		public override async Task EndEntry()
		{
			if (this.entryType == EntryType.Clear)
				await Database.Provider.Clear(this.collectionName);
			else if (this.obj != null)
			{
				GenericObject Obj2 = await Database.Provider.TryLoadObject<GenericObject>(this.collectionName, this.objectId);

				switch (this.entryType)
				{
					case EntryType.New:
					case EntryType.Update:
						if (Obj2 is null)
							await Database.Provider.Insert(this.obj);
						else if (!Obj2.Equals(this.obj))
							await Database.Provider.Update(this.obj);
						break;

					case EntryType.Delete:
						if (!(Obj2 is null))
							await Database.Provider.Delete(Obj2);
						break;
				}

				this.obj = null;
				this.nrObjectsInBulk++;
				if (this.nrObjectsInBulk >= 1000)
				{
					await Database.EndBulk();
					await Database.StartBulk();
					this.nrObjectsInBulk = 0;
				}
			}

			await base.EndEntry();
		}

		/// <summary>
		/// Starts export of files.
		/// </summary>
		public override Task StartFiles()
		{
			string Folder2;

			foreach (Export.FolderCategory Rec in Export.GetRegisteredFolders())
			{
				foreach (string Folder in Rec.Folders)
				{
					if (Directory.Exists(Folder2 = Path.Combine(Gateway.RootFolder, Folder)))
						Directory.Delete(Folder2, true);
				}
			}

			return base.StartFiles();
		}

		/// <summary>
		/// Export file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="File">File stream</param>
		public override async Task ExportFile(string FileName, Stream File)
		{
			await base.ExportFile(FileName, File);

			string Folder = Path.GetDirectoryName(FileName);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			try
			{
				using (FileStream f = System.IO.File.Create(FileName))
				{
					await File.CopyToAsync(f);
				}
			}
			catch (Exception ex)
			{
				using (FileStream f = System.IO.File.OpenRead(FileName))
				{
					if (File.Length == f.Length)
					{
						File.Position = 0;

						byte[] Buf1 = new byte[65536];
						byte[] Buf2 = new byte[65536];
						long l = File.Length;
						int i, c;
						bool Same = true;

						while ((c = (int)Math.Min(l - File.Position, 65536)) > 0)
						{
							File.Read(Buf1, 0, c);
							f.Read(Buf2, 0, c);

							for (i = 0; i < c; i++)
							{
								if (Buf1[i] != Buf2[i])
								{
									Same = false;
									break;
								}
							}
						}

						if (!Same)
							ExceptionDispatchInfo.Capture(ex).Throw();
					}
				}
			}
		}

	}
}
