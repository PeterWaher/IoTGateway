using System;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Web service that supports the rssCloud interface.
	/// 
	/// Ref:
	/// https://www.rssboard.org/rsscloud-interface
	/// </summary>
	public class RssCloud
	{
		/// <summary>
		/// Web service that supports the rssCloud interface
		/// 
		/// Ref:
		/// https://www.rssboard.org/rsscloud-interface
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="BaseUri">Base URI</param>
		public RssCloud(XmlElement Xml, Uri BaseUri)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			this.Domain = XML.Attribute(Xml, "domain");
			this.Port = XML.Attribute(Xml, "port", 80);
			this.Path = XML.Attribute(Xml, "path");
			this.RegisterProcedure = XML.Attribute(Xml, "registerProcedure");
			this.Protocol = XML.Attribute(Xml, "protocol");
		}

		/// <summary>
		/// Domain
		/// </summary>
		public string Domain { get; }

		/// <summary>
		/// Port number
		/// </summary>
		public int Port { get; }

		/// <summary>
		/// Path
		/// </summary>
		public string Path { get; }

		/// <summary>
		/// Register procedure
		/// </summary>
		public string RegisterProcedure { get; }

		/// <summary>
		/// Protocol
		/// </summary>
		public string Protocol { get; }
	}
}
