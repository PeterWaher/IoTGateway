using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event arguments for CanRead callbacks.
	/// </summary>
	public class CanReadEventArgs : JidEventArgs
	{
		private bool canRead;
		private FieldType fieldTypes;
		private ThingReference[] nodes;
		private string[] fieldNames;

		internal CanReadEventArgs(IqResultEventArgs e, object State, string JID, bool CanRead, FieldType FieldTypes,
			ThingReference[] Nodes, string[] FieldNames)
			: base(e, State, JID)
		{
			this.canRead = CanRead;
			this.fieldTypes = FieldTypes;
			this.nodes = Nodes;
			this.fieldNames = FieldNames;
		}

		/// <summary>
		/// If the readout can be performed.
		/// </summary>
		public bool CanRead
		{
			get { return this.canRead; }
		}

		/// <summary>
		/// Field types allowed to read, if <see cref="CanRead"/> is true.
		/// </summary>
		public FieldType FieldTypes
		{
			get { return this.fieldTypes; }
		}

		/// <summary>
		/// Nodes allowed to read, as long as <see cref="CanRead"/> is true. If null, no node restrictions exist.
		/// </summary>
		public ThingReference[] Nodes
		{
			get { return this.nodes; }
		}

		/// <summary>
		/// Fields allowed to read, as long as <see cref="CanRead"/> is true. If null, no field restrictions exist.
		/// </summary>
		public string[] FieldsNames
		{
			get { return this.fieldNames; }
		}
	}
}
