using System;
using System.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Default RSS extension class.
	/// </summary>
	public class RssExtension : IRssExtension
	{
		/// <summary>
		/// Default RSS extension class.
		/// </summary>
		public RssExtension()
			: this(null, null)
		{
		}

		/// <summary>
		/// Default RSS extension class.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="BaseUri">Base URI</param>
		public RssExtension(XmlElement Xml, Uri BaseUri)
		{
			this.Xml = Xml;
			this.BaseUri = BaseUri;
		}

		/// <summary>
		/// XML Definition
		/// </summary>
		public XmlElement Xml { get; }

		/// <summary>
		/// Base URI
		/// </summary>
		public Uri BaseUri { get; }

		/// <summary>
		/// Creates a new instance of the RSS extension.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="BaseUri">Base URI.</param>
		/// <returns>New RSS instance.</returns>
		public IRssExtension Create(XmlElement Xml, Uri BaseUri)
		{
			return new RssExtension(Xml, BaseUri);
		}

		/// <summary>
		/// How well the class supports an XML element.
		/// </summary>
		/// <param name="Object">XML Element being parsed.</param>
		/// <returns>Support grade.</returns>
		public Grade Supports(XmlElement Object)
		{
			return Grade.Barely;
		}
	}
}
