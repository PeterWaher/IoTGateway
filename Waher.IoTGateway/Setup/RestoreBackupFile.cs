using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Class restoring the contents of a backup file.
	/// </summary>
	public class RestoreBackupFile : ValidateBackupFile
	{
		private readonly List<object> objects = new List<object>();
		private GenericObject obj = null;
		private int nrObjectsInBulk = 0;

		/// <summary>
		/// Class restoring the contents of a backup file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		public RestoreBackupFile(string FileName)
			: base(FileName)
		{
		}

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
			await Database.Clear(CollectionName);
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
				await Database.Insert(this.objects);
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
			return Database.AddIndex(this.collectionName, this.index.ToArray());
		}

		/// <summary>
		/// Is called when an object is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		public override Task StartObject(string ObjectId, string TypeName)
		{
			this.obj = new GenericObject(this.collectionName, TypeName, Guid.Parse(ObjectId));
			return base.StartObject(ObjectId, TypeName);
		}

		/// <summary>
		/// Is called when an object is finished.
		/// </summary>
		public override async Task EndObject()
		{
			await base.EndObject();

			if (this.obj != null)
			{
				this.objects.Add(this.obj);
				this.obj = null;

				int c = this.objects.Count;
				if (c >= 100)
				{
					await Database.Insert(this.objects);
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
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		public override Task ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.obj != null)
				this.obj[PropertyName] = PropertyValue;

			return base.ReportProperty(PropertyName, PropertyValue);
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
