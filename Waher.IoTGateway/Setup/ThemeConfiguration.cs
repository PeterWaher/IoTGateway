using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Theme Configuration
	/// </summary>
	public class ThemeConfiguration : SystemMultiStepConfiguration
	{
		private static ThemeConfiguration instance = null;
		private static Dictionary<string, ThemeDefinition> themeDefinitions = new Dictionary<string, ThemeDefinition>();

		private string themeId = string.Empty;

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static ThemeConfiguration Instance => instance;

		/// <summary>
		/// ID of Selected theme
		/// </summary>
		[DefaultValueStringEmpty]
		public string ThemeId
		{
			get { return this.themeId; }
			set { this.themeId = value; }
		}

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Theme.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 500;

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override Task ConfigureSystem()
		{
			if (!string.IsNullOrEmpty(this.themeId) && themeDefinitions.TryGetValue(this.themeId, out ThemeDefinition Def))
				Theme.CurrerntTheme = Def;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as ThemeConfiguration;
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override async Task InitSetup(HttpServer WebServer)
		{
			this.themeId = "CactusRose";

			XmlSchema Schema = XSL.LoadSchema(typeof(Gateway).Namespace + ".Schema.Theme.xsd", typeof(Gateway).Assembly);
			string ThemesFolder = Path.Combine(Gateway.AppDataFolder, "Root", "Themes");
			ThemeDefinition Def;

			await base.InitSetup(WebServer);

			if (Directory.Exists(ThemesFolder))
			{
				foreach (string FileName in Directory.GetFiles(ThemesFolder, "*.xml", SearchOption.AllDirectories))
				{
					try
					{
						XmlDocument Doc = new XmlDocument();
						Doc.Load(FileName);

						XSL.Validate(FileName, Doc, "Theme", "http://waher.se/Schema/Theme.xsd", Schema);

						Def = new ThemeDefinition(Doc);
						themeDefinitions[Def.Id] = Def;
					}
					catch (Exception ex)
					{
						Log.Critical(ex, FileName);
						continue;
					}
				}
			}

			bool Update = false;

			if (!string.IsNullOrEmpty(this.themeId) && !themeDefinitions.ContainsKey(this.themeId))
			{
				this.themeId = string.Empty;
				this.Step = 0;
				this.Completed = DateTime.MinValue;
				this.Complete = false;

				Update = true;
			}

			if (string.IsNullOrEmpty(this.themeId) && themeDefinitions.Count == 1)
			{
				foreach (ThemeDefinition Def2 in themeDefinitions.Values)
				{
					this.themeId = Def2.Id;

					await this.MakeCompleted();
					Update = false;

					break;
				}
			}

			if (Update)
			{
				this.Updated = DateTime.Now;
				await Database.Update(this);
			}

			if (!string.IsNullOrEmpty(this.themeId) && themeDefinitions.TryGetValue(this.themeId, out Def))
				Theme.CurrerntTheme = Def;
			else if (themeDefinitions.TryGetValue("CactusRose", out Def))
				Theme.CurrerntTheme = Def;
			else
			{
				foreach (ThemeDefinition Def2 in themeDefinitions.Values)
				{
					Theme.CurrerntTheme = Def2;
					break;
				}
			}

			WebServer.Register("/Settings/SetTheme", null, this.SetTheme, true, false, true);
		}

		private void SetTheme(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			if (!(Obj is string ThemeId))
				throw new BadRequestException();

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
				throw new BadRequestException();

			if (themeDefinitions.TryGetValue(ThemeId, out ThemeDefinition Def))
				Theme.CurrerntTheme = Def;
			else
				throw new NotFoundException();

			Response.StatusCode = 200;

			this.UpdateTheme(Def, TabID);
		}

		private async void UpdateTheme(ThemeDefinition Def, string TabID)
		{
			try
			{
				this.themeId = Def.Id;

				if (this.Step <= 0)
					this.Step = 1;

				this.Updated = DateTime.Now;
				await Database.Update(this);

				ClientEvents.PushEvent(new string[] { TabID }, "ThemeOk", JSON.Encode(new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("themeId", Def.Id),
						new KeyValuePair<string, object>("cssUrl", Def.CSSX),
					}, false), true);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Gets available theme definitions.
		/// </summary>
		/// <returns>Array of theme definitions.</returns>
		public static ThemeDefinition[] GetDefinitions()
		{
			ThemeDefinition[] Result = new ThemeDefinition[themeDefinitions.Count];
			themeDefinitions.Values.CopyTo(Result, 0);

			Array.Sort<ThemeDefinition>(Result, (t1, t2) => t1.Title.CompareTo(t2.Title));

			return Result;
		}


	}
}
