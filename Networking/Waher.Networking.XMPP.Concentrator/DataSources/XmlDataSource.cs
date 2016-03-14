using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Networking.XMPP.Concentrator.DataSources
{
	/// <summary>
	/// Maintains a data source of nodes, persisted as an XML file.
	/// </summary>
	public class XmlDataSource : IDataSource
	{
		private string sourceId;
		private string name;
		private string fileName;
		private DateTime lastChanged;

		/// <summary>
		/// Maintains a data source of nodes, persisted as an XML file.
		/// </summary>
		/// <param name="FileName">Name of XML file.</param>
		public XmlDataSource(string SourceID, string Name, string FileName)
		{
			this.sourceId = SourceID;
			this.name = Name;
			this.fileName = FileName;
			this.lastChanged = DateTime.MinValue;
		}

		/// <summary>
		/// ID of data source.
		/// </summary>
		public string SourceID
		{
			get { return this.sourceId; }
		}

		/// <summary>
		/// Name of data source.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Child sources. If no child sources are available, null is returned.
		/// </summary>
		public IEnumerable<IDataSource> ChildSources
		{
			get { return null; }
		}

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		public bool HasChildren
		{
			get { return false; }
		}

		/// <summary>
		/// When the source was last updated.
		/// </summary>
		public DateTime LastChanged
		{
			get { return this.lastChanged; }
		}

		/// <summary>
		/// Root node references. If no root nodes are available, null is returned.
		/// </summary>
		public IEnumerable<ThingReference> RootNodes
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// If the source contains a node.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <returns>If the source contains the node.</returns>
		public bool ContainsNode(ThingReference Node)
		{
			throw new NotImplementedException();
		}
	}
}
