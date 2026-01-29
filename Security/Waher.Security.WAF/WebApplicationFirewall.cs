using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF
{
	/// <summary>
	/// Web Application Firewall for <see cref="HttpServer"/>.
	/// </summary>
	public class WebApplicationFirewall : IWebApplicationFirewall
	{
		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public WebApplicationFirewall(string Xml)
		{
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml);
		}

		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public WebApplicationFirewall(XmlDocument Xml)
		{
		}

		/// <summary>
		/// Web Application Firewall for <see cref="HttpServer"/>.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public WebApplicationFirewall(XmlElement Xml)
		{
		}

		/// <summary>
		/// Loads a WAF definition from file.
		/// </summary>
		/// <param name="FileName">WAF file.</param>
		/// <returns>Parsed Web Application Firewall.</returns>
		public static WebApplicationFirewall LoadFromFile(string FileName)
		{
			XmlDocument Doc = XML.LoadFromFile(FileName, true);
			return new WebApplicationFirewall(Doc);
		}

		/// <summary>
		/// Reviews an incoming request.
		/// </summary>
		/// <param name="Request">Current HTTP Request</param>
		/// <param name="Resource">Corresponding HTTP Resource, if found.</param>
		/// <param name="SubPath">Sub-path within the resource, if applicable.</param>
		/// <returns>Action to take.</returns>
		public Task<WafAction> Review(HttpRequest Request, HttpResource Resource, string SubPath)
		{
			return Task.FromResult(WafAction.Allow);
		}
	}
}
