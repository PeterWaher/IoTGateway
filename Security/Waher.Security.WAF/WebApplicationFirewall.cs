using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Interfaces;
using Waher.Security.WAF.Model;

namespace Waher.Security.WAF
{
	/// <summary>
	/// Web Application Firewall for <see cref="HttpServer"/>.
	/// </summary>
	public class WebApplicationFirewall : IWebApplicationFirewall
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
		private readonly Root root;

		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public WebApplicationFirewall(string Xml)
			: this(XML.ParseXml(Xml, true))
		{
		}

		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public WebApplicationFirewall(XmlDocument Xml)
			: this(Xml.DocumentElement)
		{
		}

		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public WebApplicationFirewall(XmlElement Xml)
		{
			this.root = WafAction.Parse(Xml, null, this) as Root;
			if (this.root is null)
				throw new Exception("Invalid root element of WAF definition.");	
		}

		/// <summary>
		/// Loads a WAF definition from file.
		/// </summary>
		/// <param name="FileName">WAF file.</param>
		/// <returns>Parsed Web Application Firewall.</returns>
		public static WebApplicationFirewall LoadFromFile(string FileName)
		{
			XmlDocument Doc = XML.LoadFromFile(FileName, true);
			XSL.Validate(FileName, Doc, nameof(Root), Namespace, schema);
			return new WebApplicationFirewall(Doc);
		}

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
		/// Reviews an incoming request.
		/// </summary>
		/// <param name="Request">Current HTTP Request</param>
		/// <param name="Resource">Corresponding HTTP Resource, if found.</param>
		/// <param name="SubPath">Sub-path within the resource, if applicable.</param>
		/// <returns>Action to take.</returns>
		public Task<WafResult> Review(HttpRequest Request, HttpResource Resource, string SubPath)
		{
			return Task.FromResult(this.root.DefaultResult);
		}
	}
}
