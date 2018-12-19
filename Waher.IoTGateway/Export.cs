using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Events;
using Waher.IoTGateway;
using Waher.Runtime.Settings;
using Waher.IoTGateway.WebResources.ExportFormats;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Static class managing data export.
	/// </summary>
	public static class Export
	{
		/// <summary>
		/// http://waher.se/Schema/Export.xsd
		/// </summary>
		public const string ExportNamepace = "http://waher.se/Schema/Export.xsd";

		/// <summary>
		/// Full path to export folder.
		/// </summary>
		public static string FullExportFolder
		{
			get
			{
				string Path = ExportFolder;

				if (string.IsNullOrEmpty(Path))
					Path = System.IO.Path.Combine(Gateway.AppDataFolder, "Backup");

				return Path;
			}
		}

		/// <summary>
		/// Full path to key folder.
		/// </summary>
		public static string FullKeyExportFolder
		{
			get
			{
				string Path = ExportKeyFolder;

				if (string.IsNullOrEmpty(Path))
					Path = System.IO.Path.Combine(Gateway.AppDataFolder, "Backup");

				return Path;
			}
		}

		/// <summary>
		/// Gets information about exported files.
		/// </summary>
		/// <returns>File information array</returns>
		public static FileInformation[] GetExportFiles()
		{
			SortedDictionary<DateTime, FileInformation> Sorted = new SortedDictionary<DateTime, FileInformation>(new ReverseDateTimeOrder());
			string Path = FullExportFolder;
			if (Directory.Exists(Path))
				GetFiles(Path, Sorted);

			string Path2 = FullKeyExportFolder;
			if (Path != Path2 && Directory.Exists(Path2))
				GetFiles(Path2, Sorted);

			FileInformation[] Result = new FileInformation[Sorted.Count];
			Sorted.Values.CopyTo(Result, 0);

			return Result;
		}

		private static void GetFiles(string Path, SortedDictionary<DateTime, FileInformation> Sorted)
		{
			string[] Files = Directory.GetFiles(Path, "*.*", SearchOption.TopDirectoryOnly);
			int i, c = Files.Length;
			DateTime Created;
			long Size;
			string SizeStr;
			string s;

			Path += System.IO.Path.DirectorySeparatorChar;

			for (i = 0; i < c; i++)
			{
				s = Files[i];

				try
				{
					using (FileStream fs = File.OpenRead(s))
					{
						Size = fs.Length;
						SizeStr = FormatBytes(Size);
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					Size = 0;
					SizeStr = string.Empty;
				}

				Created = File.GetCreationTime(s);

				if (s.StartsWith(Path))
					s = s.Substring(Path.Length);

				while (Sorted.ContainsKey(Created))
					Created = Created.AddTicks(1);

				Sorted[Created] = new FileInformation(s, Created, Size, SizeStr);
			}
		}

		private class ReverseDateTimeOrder : IComparer<DateTime>
		{
			public ReverseDateTimeOrder()
			{
			}

			public int Compare(DateTime x, DateTime y)
			{
				return y.CompareTo(x);
			}
		}

		/// <summary>
		/// Formats a file size using appropriate unit.
		/// </summary>
		/// <param name="Bytes">Number of bytes</param>
		/// <returns>Formatted file size.</returns>
		public static string FormatBytes(double Bytes)
		{
			var i = 0;

			while (Bytes >= 1024 && i < 4)
			{
				Bytes /= 1024;
				i++;
			}

			if (i == 0)
				return Bytes.ToString("F0") + " " + ByteUnits[i];
			else
				return Bytes.ToString("F2") + " " + ByteUnits[i];
		}

		private static readonly string[] ByteUnits = { "B", "kB", "MB", "GB", "TB" };

		/// <summary>
		/// Export folder.
		/// </summary>
		public static string ExportFolder
		{
			get
			{
				if (exportFolderValue is null)
					exportFolderValue = RuntimeSettings.Get("ExportFolder", string.Empty);

				return exportFolderValue;
			}

			set
			{
				if (exportFolderValue != value)
				{
					exportFolderValue = value;
					RuntimeSettings.Set("ExportFolder", exportFolderValue);

					Gateway.UpdateExportFolder(FullExportFolder);
				}
			}
		}

		private static string exportFolderValue = null;

		/// <summary>
		/// Key folder
		/// </summary>
		public static string ExportKeyFolder
		{
			get
			{
				if (exportKeyFolderValue is null)
					exportKeyFolderValue = RuntimeSettings.Get("ExportKeyFolder", string.Empty);

				return exportKeyFolderValue;
			}

			set
			{
				if (exportKeyFolderValue != value)
				{
					exportKeyFolderValue = value;
					RuntimeSettings.Set("ExportKeyFolder", exportKeyFolderValue);

					Gateway.UpdateExportKeyFolder(FullKeyExportFolder);
				}
			}
		}

		private static string exportKeyFolderValue = null;

		/// <summary>
		/// If the database should be exported.
		/// </summary>
		public static bool ExportDatabase
		{
			get
			{
				if (!exportDatabase.HasValue)
					exportDatabase = RuntimeSettings.Get("ExportDatabase", true);

				return exportDatabase.Value;
			}

			set
			{
				if (exportDatabase != value)
				{
					exportDatabase = value;
					RuntimeSettings.Set("ExportDatabase", exportDatabase.Value);
				}
			}
		}

		private static bool? exportDatabase = null;

		/// <summary>
		/// If web content should be exported.
		/// </summary>
		public static bool ExportWebContent
		{
			get
			{
				if (!exportWebContent.HasValue)
					exportWebContent = RuntimeSettings.Get("ExportWebContent", false);

				return exportWebContent.Value;
			}

			set
			{
				if (exportWebContent != value)
				{
					exportWebContent = value;
					RuntimeSettings.Set("ExportWebContent", exportWebContent.Value);
				}
			}
		}

		private static bool? exportWebContent = null;

		/// <summary>
		/// Gets if a folder should be exported or not.
		/// </summary>
		/// <param name="FolderName">Name of folder.</param>
		/// <returns>If folder should be exported.</returns>
		public static async Task<bool> GetExportFolderAsync(string FolderName)
		{
			string Key = "Export." + FolderName;
			bool Result;

			lock (exportFolder)
			{
				if (exportFolder.TryGetValue(Key, out Result))
					return Result;
			}

			Result = await RuntimeSettings.GetAsync(Key, true);

			lock (exportFolder)
			{
				exportFolder[Key] = Result;
			}

			return Result;
		}

		/// <summary>
		/// Sets if a folder should be exported or not.
		/// </summary>
		/// <param name="FolderName">Name of folder.</param>
		/// <param name="Export">If folder should be exported or not.</param>
		public static async Task SetExportFolderAsync(string FolderName, bool Export)
		{
			string Key = "Export." + FolderName;

			lock (exportFolder)
			{
				if (exportFolder.TryGetValue(Key, out bool b) && b == Export)
					return;

				exportFolder[Key] = Export;
			}

			await RuntimeSettings.SetAsync(Key, Export);
		}

		private static readonly Dictionary<string, bool> exportFolder = new Dictionary<string, bool>();

		/// <summary>
		/// Export file type.
		/// </summary>
		public static string ExportType
		{
			get
			{
				if (exportType is null)
					exportType = RuntimeSettings.Get("ExportType", "Encrypted");

				return exportType;
			}

			set
			{
				if (exportType != value)
				{
					exportType = value;
					RuntimeSettings.Set("ExportType", exportType);
				}
			}
		}

		private static string exportType = null;

		/// <summary>
		/// If automatic backups is activated
		/// </summary>
		public static bool AutomaticBackups
		{
			get
			{
				return GetAutomaticBackupsAsync().Result;
			}

			set
			{
				if (automaticBackups != value)
				{
					automaticBackups = value;
					RuntimeSettings.Set("AutomaticBackups", automaticBackups.Value);
				}
			}
		}

		/// <summary>
		/// If automatic backups is activated
		/// </summary>
		public static async Task<bool> GetAutomaticBackupsAsync()
		{
			if (!automaticBackups.HasValue)
				automaticBackups = await RuntimeSettings.GetAsync("AutomaticBackups", true);

			return automaticBackups.Value;
		}

		private static bool? automaticBackups = null;

		/// <summary>
		/// For how many days backups are kept.
		/// </summary>
		public static long BackupKeepDays
		{
			get
			{
				return GetKeepDaysAsync().Result;
			}

			set
			{
				if (backupKeepDays != value)
				{
					backupKeepDays = value;
					RuntimeSettings.Set("BackupKeepDays", backupKeepDays.Value);
				}
			}
		}

		/// <summary>
		/// For how many days backups are kept.
		/// </summary>
		public static async Task<long> GetKeepDaysAsync()
		{
			if (!backupKeepDays.HasValue)
				backupKeepDays = await RuntimeSettings.GetAsync("BackupKeepDays", 3);

			return backupKeepDays.Value;
		}

		private static long? backupKeepDays = null;

		/// <summary>
		/// For how many months the monthly backups are kept.
		/// </summary>
		public static long BackupKeepMonths
		{
			get
			{
				return GetKeepMonthsAsync().Result;
			}

			set
			{
				if (backupKeepMonths != value)
				{
					backupKeepMonths = value;
					RuntimeSettings.Set("BackupKeepMonths", backupKeepMonths.Value);
				}
			}
		}

		/// <summary>
		/// For how many months the monthly backups are kept.
		/// </summary>
		public static async Task<long> GetKeepMonthsAsync()
		{
			if (!backupKeepMonths.HasValue)
				backupKeepMonths = await RuntimeSettings.GetAsync("BackupKeepMonths", 3);

			return backupKeepMonths.Value;
		}

		private static long? backupKeepMonths = null;

		/// <summary>
		/// For how many years the yearly backups are kept.
		/// </summary>
		public static long BackupKeepYears
		{
			get
			{
				return GetKeepYearsAsync().Result;
			}

			set
			{
				if (backupKeepYears != value)
				{
					backupKeepYears = value;
					RuntimeSettings.Set("BackupKeepYears", backupKeepYears.Value);
				}
			}
		}

		/// <summary>
		/// For how many years the yearly backups are kept.
		/// </summary>
		public static async Task<long> GetKeepYearsAsync()
		{
			if (!backupKeepYears.HasValue)
				backupKeepYears = await RuntimeSettings.GetAsync("BackupKeepYears", 3);

			return backupKeepYears.Value;
		}

		private static long? backupKeepYears = null;

		/// <summary>
		/// Time of day to start performing backups.
		/// </summary>
		public static TimeSpan BackupTime
		{
			get
			{
				return GetBackupTimeAsync().Result;
			}

			set
			{
				if (backupTime != value)
				{
					backupTime = value;
					RuntimeSettings.Set("BackupTime", backupTime.Value);
				}
			}
		}

		/// <summary>
		/// Time of day to start performing backups.
		/// </summary>
		public static async Task<TimeSpan> GetBackupTimeAsync()
		{
			if (!backupTime.HasValue)
				backupTime = await RuntimeSettings.GetAsync("BackupTime", TimeSpan.FromHours(5));

			return backupTime.Value;
		}

		private static TimeSpan? backupTime = null;

		/// <summary>
		/// Timestamp of last backup.
		/// </summary>
		public static DateTime LastBackup
		{
			get
			{
				return GetLastBackupAsync().Result;
			}

			set
			{
				if (lastBackup != value)
					SetLastBackupAsync(value).Wait();
			}
		}

		/// <summary>
		/// Get Timestamp of last backup.
		/// </summary>
		public static async Task<DateTime> GetLastBackupAsync()
		{
			if (!lastBackup.HasValue)
				lastBackup = await RuntimeSettings.GetAsync("LastBackup", DateTime.MinValue);

			return lastBackup.Value;
		}

		/// <summary>
		/// Set Timestamp of last backup.
		/// </summary>
		public static async Task SetLastBackupAsync(DateTime Value)
		{
			lastBackup = Value;
			await RuntimeSettings.SetAsync("LastBackup", Value);
		}

		private static DateTime? lastBackup = null;

		/// <summary>
		/// Registers a set of exportable folders under one display name.
		/// </summary>
		/// <param name="CategoryId">Category ID</param>
		/// <param name="DisplayName">Display Name</param>
		/// <param name="Folders">Set of exprtable folders.</param>
		public static void RegisterFolders(string CategoryId, string DisplayName, params string[] Folders)
		{
			lock (folders)
			{
				if (folders.ContainsKey(CategoryId))
					throw new ArgumentException("Category ID already registered.", nameof(CategoryId));

				folders[DisplayName] = new FolderCategory()
				{
					CategoryId = CategoryId,
					DisplayName = DisplayName,
					Folders = Folders
				};
			}
		}

		/// <summary>
		/// Gets registered exportable folders.
		/// </summary>
		/// <returns>Array of registered exportable folders.</returns>
		public static FolderCategory[] GetRegisteredFolders()
		{
			FolderCategory[] Result;

			lock (folders)
			{
				Result = new FolderCategory[folders.Count];
				folders.Values.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Information about an exportable folder category
		/// </summary>
		public class FolderCategory
		{
			/// <summary>
			/// Category ID
			/// </summary>
			public string CategoryId;

			/// <summary>
			/// Display name
			/// </summary>
			public string DisplayName;

			/// <summary>
			/// Set of folders
			/// </summary>
			public string[] Folders;
		}

		private static Dictionary<string, FolderCategory> folders = new Dictionary<string, FolderCategory>();

	}
}
