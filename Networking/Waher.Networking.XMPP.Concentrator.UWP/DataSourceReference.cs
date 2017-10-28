using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Runtime.Language;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Data Source Reference
	/// </summary>
	public class DataSourceReference
	{
		private string sourceID;
		private string name;
		private bool hasChildren;
		private DateTime lastChanged;

		/// <summary>
		/// Data Source Reference
		/// </summary>
		/// <param name="SourceId">Data Source ID</param>
		/// <param name="Name">Human readable name of data source.</param>
		/// <param name="HasChildren">If the source has child data sources.</param>
		/// <param name="LastChanged">Timestamp of last change.</param>
		public DataSourceReference(string SourceId, string Name, bool HasChildren, DateTime LastChanged)
		{
			this.sourceID = SourceId;
			this.name = Name;
			this.hasChildren = HasChildren;
			this.lastChanged = LastChanged;
		}

		/// <summary>
		/// Data Source Reference
		/// </summary>
		/// <param name="E">XML Definition.</param>
		public DataSourceReference(XmlElement E)
		{
			this.sourceID = XML.Attribute(E, "src");
			this.name = XML.Attribute(E, "name");
			this.hasChildren = XML.Attribute(E, "hasChildren", false);
			this.lastChanged = XML.Attribute(E, "lastChanged", DateTime.MinValue);
		}

		/// <summary>
		/// Source ID
		/// </summary>
		public string SourceID => this.sourceID;

		/// <summary>
		/// Human readable name of data source.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// If the source has child data sources.
		/// </summary>
		public bool HasChildren => this.hasChildren;

		/// <summary>
		/// Timestamp of last change.
		/// </summary>
		public DateTime LastChanged => this.lastChanged;
	}
}
