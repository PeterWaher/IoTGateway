using System;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Runtime.Language;
using Waher.IoTGateway.WebResources;
using Waher.Content;
using System.IO;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Restore Configuration
	/// </summary>
	public class BackupConfiguration : SystemConfiguration
	{
		private static BackupConfiguration instance = null;

		private HttpFolderResource exportFolder = null;
		private HttpFolderResource keyFolder = null;
		private StartExport startExport = null;
		private StartAnalyze startAnalyze = null;
		private DeleteExport deleteExport = null;
		private UpdateBackupSettings updateBackupSettings = null;
		private UpdateBackupFolderSettings updateBackupFolderSettings = null;

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static BackupConfiguration Instance => instance;

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Backup.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 175;

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 7, "Backup");
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override async Task ConfigureSystem()
		{
			await this.UpdateExportFolder(await Export.GetFullExportFolderAsync());
			this.UpdateExportKeyFolder(await Export.GetFullKeyExportFolderAsync());
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as BackupConfiguration;
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override async Task InitSetup(HttpServer WebServer)
		{
			HttpAuthenticationScheme Auth = Gateway.LoggedIn(new string[] { this.ConfigPrivilege });

			WebServer.Register(this.exportFolder = new HttpFolderResource("/Export", await Export.GetFullExportFolderAsync(), false, false, false, true, HostDomainOptions.SameForAllDomains, Auth));
			WebServer.Register(this.keyFolder = new HttpFolderResource("/Key", await Export.GetFullKeyExportFolderAsync(), false, false, false, true, HostDomainOptions.SameForAllDomains, Auth));
			WebServer.Register(this.startExport = new StartExport());
			WebServer.Register(this.startAnalyze = new StartAnalyze());
			WebServer.Register(this.deleteExport = new DeleteExport());
			WebServer.Register(this.updateBackupSettings = new UpdateBackupSettings());
			WebServer.Register(this.updateBackupFolderSettings = new UpdateBackupFolderSettings());

			await base.InitSetup(WebServer);
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Data.Backup";

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.exportFolder);
			WebServer.Unregister(this.keyFolder);
			WebServer.Unregister(this.startExport);
			WebServer.Unregister(this.startAnalyze);
			WebServer.Unregister(this.deleteExport);
			WebServer.Unregister(this.updateBackupSettings);
			WebServer.Unregister(this.updateBackupFolderSettings);

			return base.UnregisterSetup(WebServer);
		}

		internal async Task UpdateExportFolder(string Folder)
		{
			if (!(this.exportFolder is null))
				this.exportFolder.FolderPath = Folder;

			if (!(Gateway.InternalDatabase is null))
			{
				Type T = Gateway.InternalDatabase.GetType();
				PropertyInfo PI = T.GetProperty("AutoRepairReportFolder");

				if (!(PI is null) && PI.PropertyType == typeof(string))
					PI.SetValue(Gateway.InternalDatabase, await Export.GetFullExportFolderAsync(), null);
			}
		}

		internal void UpdateExportKeyFolder(string Folder)
		{
			if (!(this.keyFolder is null))
				this.keyFolder.FolderPath = Folder;
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Environment variable name containing a Boolean value if automatic backups should be made or not.
		/// </summary>
		public const string GATEWAY_BACKUP = nameof(GATEWAY_BACKUP);

		/// <summary>
		/// Environment variable name containing a <see cref="TimeSpan"/> value determining the time of day automatic backups are performed.
		/// </summary>
		public const string GATEWAY_BACKUP_TIME = nameof(GATEWAY_BACKUP_TIME);

		/// <summary>
		/// Environment variable name containing the number of days daily backups are kept.
		/// </summary>
		public const string GATEWAY_BACKUP_DAYS = nameof(GATEWAY_BACKUP_DAYS);

		/// <summary>
		/// Environment variable name containing the number of months monthly backups are kept.
		/// </summary>
		public const string GATEWAY_BACKUP_MONTHS = nameof(GATEWAY_BACKUP_MONTHS);

		/// <summary>
		/// Environment variable name containing the number of years yearly backups are kept.
		/// </summary>
		public const string GATEWAY_BACKUP_YEARS = nameof(GATEWAY_BACKUP_YEARS);

		/// <summary>
		/// Environment variable name containing the backup folder, if different from the default.
		/// </summary>
		public const string GATEWAY_BACKUP_FOLDER = nameof(GATEWAY_BACKUP_FOLDER);

		/// <summary>
		/// Environment variable name containing the key folder, if different from the default.
		/// </summary>
		public const string GATEWAY_KEY_FOLDER = nameof(GATEWAY_KEY_FOLDER);

		/// <summary>
		/// Environment variable name containing a comma-delimited list of secondary backup hosts for redundant storage of backup files.
		/// </summary>
		public const string GATEWAY_BACKUP_HOSTS = nameof(GATEWAY_BACKUP_HOSTS);

		/// <summary>
		/// Environment variable name containing a comma-delimited list of secondary key hosts for redundant storage of key files.
		/// </summary>
		public const string GATEWAY_KEY_HOSTS = nameof(GATEWAY_KEY_HOSTS);

		/// <summary>
		/// Environment configuration by configuring values available in environment variables.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override async Task<bool> EnvironmentConfiguration()
		{
			string Value = Environment.GetEnvironmentVariable(GATEWAY_BACKUP);
			if (string.IsNullOrEmpty(Value))
				return false;

			if (!CommonTypes.TryParse(Value, out bool Backup))
			{
				this.LogEnvironmentVariableInvalidBooleanError(GATEWAY_BACKUP, Value);
				return false;
			}

			if (!Backup)
				return true;

			await Export.SetAutomaticBackupsAsync(true);

			string s = Environment.GetEnvironmentVariable(GATEWAY_BACKUP_TIME);
			if (!string.IsNullOrEmpty(s))
			{
				if (!TimeSpan.TryParse(s, out TimeSpan BackupTime) || BackupTime < TimeSpan.Zero || BackupTime.TotalHours >= 24)
				{
					this.LogEnvironmentVariableInvalidTimeError(GATEWAY_BACKUP_TIME, s);
					return false;
				}

				await Export.SetBackupTimeAsync(BackupTime);
			}

			s = Environment.GetEnvironmentVariable(GATEWAY_BACKUP_DAYS);
			if (!string.IsNullOrEmpty(s))
			{
				if (!int.TryParse(s, out int i) || i < 0)
				{
					this.LogEnvironmentVariableInvalidIntegerError(GATEWAY_BACKUP_DAYS, s);
					return false;
				}

				await Export.SetKeepDaysAsync(i);
			}

			s = Environment.GetEnvironmentVariable(GATEWAY_BACKUP_MONTHS);
			if (!string.IsNullOrEmpty(s))
			{
				if (!int.TryParse(s, out int i) || i < 0)
				{
					this.LogEnvironmentVariableInvalidIntegerError(GATEWAY_BACKUP_MONTHS, s);
					return false;
				}

				await Export.SetKeepMonthsAsync(i);
			}

			s = Environment.GetEnvironmentVariable(GATEWAY_BACKUP_YEARS);
			if (!string.IsNullOrEmpty(s))
			{
				if (!int.TryParse(s, out int i) || i < 0)
				{
					this.LogEnvironmentVariableInvalidIntegerError(GATEWAY_BACKUP_YEARS, s);
					return false;
				}

				await Export.SetKeepYearsAsync(i);
			}

			s = Environment.GetEnvironmentVariable(GATEWAY_BACKUP_FOLDER);
			if (!string.IsNullOrEmpty(s))
			{
				try
				{
					if (!Directory.Exists(s))
						Directory.CreateDirectory(s);
			
					await Export.SetExportFolderAsync(s);
				}
				catch (Exception ex)
				{
					this.LogEnvironmentError(ex.Message, GATEWAY_BACKUP_FOLDER, s);
				}
			}

			s = Environment.GetEnvironmentVariable(GATEWAY_KEY_FOLDER);
			if (!string.IsNullOrEmpty(s))
			{
				try
				{
					if (!Directory.Exists(s))
						Directory.CreateDirectory(s);

					await Export.SetExportKeyFolderAsync(s);
				}
				catch (Exception ex)
				{
					this.LogEnvironmentError(ex.Message, GATEWAY_KEY_FOLDER, s);
				}
			}

			s = Environment.GetEnvironmentVariable(GATEWAY_BACKUP_HOSTS);
			if (!string.IsNullOrEmpty(s))
				await Export.SetBackupHostsAsync(s.Split(','));

			s = Environment.GetEnvironmentVariable(GATEWAY_KEY_HOSTS);
			if (!string.IsNullOrEmpty(s))
				await Export.SetKeyHostsAsync(s.Split(','));

			return true;
		}

	}
}
