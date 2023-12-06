using System;
using System.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Interface for RSS extensions
	/// </summary>
	public interface IRssExtension : IProcessingSupport<XmlElement>
	{
		/// <summary>
		/// Creates a new instance of the RSS extension.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="BaseUri">Base URI.</param>
		/// <returns>New RSS instance.</returns>
		IRssExtension Create(XmlElement Xml, Uri BaseUri);
	}
}
