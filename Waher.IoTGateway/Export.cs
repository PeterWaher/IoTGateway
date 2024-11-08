using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.IoTGateway.Setup;
using Waher.IoTGateway.WebResources.ExportFormats;
using Waher.Networking;
using Waher.Runtime.Settings;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Static class managing data export.
	/// </summary>
	public static class Export
	{
		/// <summary>
		/// Full path to export folder.
		/// </summary>
		public static async Task<string> GetFullExportFolderAsync()
		{
			string Path = await GetExportFolderAsync();

			if (string.IsNullOrEmpty(Path))
				Path = System.IO.Path.Combine(Gateway.AppDataFolder, "Backup");

			return Path;
		}

		/// <summary>
		/// Full path to key folder.
		/// </summary>
		public static async Task<string> GetFullKeyExportFolderAsync()
		{
			string Path = await GetExportKeyFolderAsync();

			if (string.IsNullOrEmpty(Path))
				Path = System.IO.Path.Combine(Gateway.AppDataFolder, "Backup");

			return Path;
		}

		/// <summary>
		/// Gets information about exported files.
		/// </summary>
		/// <returns>File information array</returns>
		public static async Task<FileInformation[]> GetExportFilesAsync()
		{
			SortedDictionary<DateTime, FileInformation> Sorted = new SortedDictionary<DateTime, FileInformation>(new ReverseDateTimeOrder());
			string Path = await GetFullExportFolderAsync();
			if (Directory.Exists(Path))
				GetFiles(Path, Sorted);

			string Path2 = await GetFullKeyExportFolderAsync();
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
					Log.Exception(ex);
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
			int i = 0;

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
		public static async Task<string> GetExportFolderAsync()
		{
			if (exportFolderValue is null)
				exportFolderValue = await RuntimeSettings.GetAsync("ExportFolder", string.Empty);

			return exportFolderValue;
		}

		/// <summary>
		/// Export folder.
		/// </summary>
		/// <param name="Value">New value.</param>
		public static async Task SetExportFolderAsync(string Value)
		{
			if (exportFolderValue != Value)
			{
				exportFolderValue = Value;
				await RuntimeSettings.SetAsync("ExportFolder", exportFolderValue);

				if (!(BackupConfiguration.Instance is null))
					await BackupConfiguration.Instance.UpdateExportFolder(await GetFullExportFolderAsync());

				await OnExportFolderUpdated.Raise(BackupConfiguration.Instance, EventArgs.Empty);
			}
		}

		private static string exportFolderValue = null;

		/// <summary>
		/// Event raised when the export folder has been updated.
		/// </summary>
		public static event EventHandlerAsync OnExportFolderUpdated;

		/// <summary>
		/// Key folder
		/// </summary>
		public static async Task<string> GetExportKeyFolderAsync()
		{
			if (exportKeyFolderValue is null)
				exportKeyFolderValue = await RuntimeSettings.GetAsync("ExportKeyFolder", string.Empty);

			return exportKeyFolderValue;
		}

		/// <summary>
		/// Key folder
		/// </summary>
		/// <param name="Value">New value.</param>
		public static async Task SetExportKeyFolderAsync(string Value)
		{
			if (exportKeyFolderValue != Value)
			{
				exportKeyFolderValue = Value;
				await RuntimeSettings.SetAsync("ExportKeyFolder", exportKeyFolderValue);

				BackupConfiguration.Instance?.UpdateExportKeyFolder(await GetFullKeyExportFolderAsync());

				await OnExportKeyFolderUpdated.Raise(BackupConfiguration.Instance, EventArgs.Empty);
			}
		}

		private static string exportKeyFolderValue = null;

		/// <summary>
		/// Event raised when the export key folder has been updated.
		/// </summary>
		public static event EventHandlerAsync OnExportKeyFolderUpdated;

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
		/// If the ledger should be exported.
		/// </summary>
		public static bool ExportLedger
		{
			get
			{
				if (!exportLedger.HasValue)
					exportLedger = RuntimeSettings.Get("ExportLedger", false);

				return exportLedger.Value;
			}

			set
			{
				if (exportLedger != value)
				{
					exportLedger = value;
					RuntimeSettings.Set("ExportLedger", exportLedger.Value);
				}
			}
		}

		private static bool? exportLedger = null;

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
		/// If automatic backups are activated
		/// </summary>
		public static async Task SetAutomaticBackupsAsync(bool Value)
		{
			if (automaticBackups != Value)
			{
				automaticBackups = Value;
				await RuntimeSettings.SetAsync("AutomaticBackups", automaticBackups.Value);
			}
		}

		/// <summary>
		/// If automatic backups are activated
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
		/// <param name="Value">Number of days.</param>
		public static async Task SetKeepDaysAsync(long Value)
		{
			if (backupKeepDays != Value)
			{
				backupKeepDays = Value;
				await RuntimeSettings.SetAsync("BackupKeepDays", backupKeepDays.Value);
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
		/// <param name="Value">Number of months.</param>
		public static async Task SetKeepMonthsAsync(long Value)
		{
			if (backupKeepMonths != Value)
			{
				backupKeepMonths = Value;
				await RuntimeSettings.SetAsync("BackupKeepMonths", backupKeepMonths.Value);
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
		/// <param name="Value">Number of years.</param>
		public static async Task SetKeepYearsAsync(long Value)
		{
			if (backupKeepYears != Value)
			{
				backupKeepYears = Value;
				await RuntimeSettings.SetAsync("BackupKeepYears", backupKeepYears.Value);
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
		/// <param name="Value">Time of day.</param>
		public static async Task SetBackupTimeAsync(TimeSpan Value)
		{
			if (backupTime != Value)
			{
				backupTime = Value;
				await RuntimeSettings.SetAsync("BackupTime", backupTime.Value);
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
			if (lastBackup != Value)
			{
				lastBackup = Value;
				await RuntimeSettings.SetAsync("LastBackup", Value);
			}
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

		private static readonly Dictionary<string, FolderCategory> folders = new Dictionary<string, FolderCategory>();

		/// <summary>
		/// Secondary backup hosts.
		/// </summary>
		public static async Task<string[]> GetBackupHostsAsync()
		{
			if (backupHosts is null)
				backupHosts = StringToArray(await RuntimeSettings.GetAsync("BackupHosts", string.Empty));

			return backupHosts;
		}

		/// <summary>
		/// Secondary backup hosts.
		/// </summary>
		/// <param name="Value">New value.</param>
		public static async Task SetBackupHostsAsync(string[] Value)
		{
			if (backupHosts != Value)
			{
				backupHosts = Value;
				await RuntimeSettings.SetAsync("BackupHosts", ArrayToString(backupHosts));
			}
		}

		private static string[] backupHosts = null;

		/// <summary>
		/// Secondary key hosts.
		/// </summary>
		public static async Task<string[]> GetKeyHostsAsync()
		{
			if (keyHosts is null)
				keyHosts = StringToArray(await RuntimeSettings.GetAsync("KeyHosts", string.Empty));

			return keyHosts;
		}

		/// <summary>
		/// Secondary key hosts.
		/// </summary>
		/// <param name="Value">New value.</param>
		public static async Task SetKeyHostsAsync(string[] Value)
		{
			if (keyHosts != Value)
			{
				keyHosts = Value;
				await RuntimeSettings.SetAsync("KeyHosts", ArrayToString(keyHosts));
			}
		}

		private static string[] keyHosts = null;

		private static string[] StringToArray(string s)
		{
			return s.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries);
		}

		private static string ArrayToString(string[] Items)
		{
			StringBuilder Result = new StringBuilder();
			bool First = true;

			foreach (string Item in Items)
			{
				if (First)
					First = false;
				else
					Result.AppendLine();

				Result.Append(Item);
			}

			return Result.ToString();
		}
	}
}