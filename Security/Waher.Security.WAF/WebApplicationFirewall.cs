using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.Cache;
using Waher.Security.WAF.Model;

namespace Waher.Security.WAF
{
	/// <summary>
	/// Web Application Firewall for <see cref="HttpServer"/>.
	/// </summary>
	public class WebApplicationFirewall : IWebApplicationFirewall, IDisposable
	{
		/// <summary>
		/// http://waher.se/Schema/WAF.xsd
		/// </summary>
		public const string Namespace = "http://waher.se/Schema/WAF.xsd";

		/// <summary>
		/// Schema to validate WAF XML files.
		/// </summary>
		private static readonly XmlSchema schema = XSL.LoadSchema(typeof(WebApplicationFirewall).Namespace + ".Schema.WAF.xsd");

		private readonly Dictionary<string, WafAction> actionsById = new Dictionary<string, WafAction>();
		private readonly Cache<string, object> internalCache;
		private readonly ILoginAuditor loginAuditor;
		private readonly Root root;
		private readonly string fileName;
		private readonly string appDataFolder;

		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="FileName">WAF file name.</param>
		/// <param name="LoginAuditor">Login Auditor used by the Firewall.</param>
		/// <param name="AppDataFolder">Application data folder, where content files are stored.</param>
		public WebApplicationFirewall(string Xml, string FileName, ILoginAuditor LoginAuditor,
			string AppDataFolder)
			: this(XML.ParseXml(Xml, true), FileName, LoginAuditor, AppDataFolder)
		{
		}

		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="FileName">WAF file name.</param>
		/// <param name="LoginAuditor">Login Auditor used by the Firewall.</param>
		/// <param name="AppDataFolder">Application data folder, where content files are stored.</param>
		public WebApplicationFirewall(XmlDocument Xml, string FileName, ILoginAuditor LoginAuditor,
			string AppDataFolder)
			: this(Xml.DocumentElement, FileName, LoginAuditor, AppDataFolder)
		{
		}

		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="FileName">WAF file name.</param>
		/// <param name="LoginAuditor">Login Auditor used by the Firewall.</param>
		/// <param name="AppDataFolder">Application data folder, where content files are stored.</param>
		public WebApplicationFirewall(XmlElement Xml, string FileName, ILoginAuditor LoginAuditor,
			string AppDataFolder)
		{
			this.fileName = FileName;
			this.appDataFolder = AppDataFolder;
			this.loginAuditor = LoginAuditor;
			this.root = WafAction.Parse(Xml, null, this) as Root;

			if (this.root is null)
				throw new Exception("Invalid root element of WAF definition.");

			this.internalCache = new Cache<string, object>(int.MaxValue,
				TimeSpan.FromDays(1), TimeSpan.FromDays(1))
			{
				MaxTimerIntervalMs = 1000
			};

			this.root.Prepare();
		}

		/// <summary>
		/// Loads a WAF definition from file.
		/// </summary>
		/// <param name="FileName">WAF file name.</param>
		/// <param name="LoginAuditor">Login Auditor used by the Firewall.</param>
		/// <param name="AppDataFolder">Application data folder, where content files are stored.</param>
		/// <returns>Parsed Web Application Firewall.</returns>
		public static WebApplicationFirewall LoadFromFile(string FileName, ILoginAuditor LoginAuditor,
			string AppDataFolder)
		{
			XmlDocument Doc = XML.LoadFromFile(FileName, true);
			XSL.Validate(FileName, Doc, nameof(Root), Namespace, schema);
			return new WebApplicationFirewall(Doc, FileName, LoginAuditor, AppDataFolder);
		}

		/// <summary>
		/// File Name
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Application data folder, where content files are stored.
		/// </summary>
		public string AppDataFolder => this.appDataFolder;

		/// <summary>
		/// Login Auditor used by the Firewall.
		/// </summary>
		public ILoginAuditor LoginAuditor => this.loginAuditor;

		/// <summary>
		/// Registers an action by its ID.
		/// </summary>
		/// <param name="Action">Action</param>
		internal void RegisterAction(WafAction Action)
		{
			if (!string.IsNullOrEmpty(Action.Id))
				this.actionsById[Action.Id] = Action;
		}

		/// <summary>
		/// Internal cache.
		/// </summary>
		internal Cache<string, object> Cache => this.internalCache;

		/// <summary>
		/// Disposes of the WAF.
		/// </summary>
		public void Dispose()
		{
			this.internalCache.Dispose();
		}

		/// <summary>
		/// Tries to get an action node by its ID.
		/// </summary>
		/// <param name="Id">Action node ID</param>
		/// <param name="Action">Action, if found.</param>
		/// <returns>If an action was found.</returns>
		public bool TryGetActionById(string Id, [NotNullWhen(true)] out WafAction Action)
		{
			return this.actionsById.TryGetValue(Id, out Action);
		}

		/// <summary>
		/// Reviews an incoming request.
		/// </summary>
		/// <param name="Request">Current HTTP Request</param>
		/// <param name="Resource">Corresponding HTTP Resource, if found.</param>
		/// <returns>Action to take.</returns>
		public async Task<WafResult> Review(HttpRequest Request, HttpResource Resource)
		{
			try
			{
				ProcessingState State = new ProcessingState(Request, Resource, this);

				return await this.root.Review(State) ?? this.root.DefaultResult;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return this.root.DefaultResult;
			}
		}
	}
}
