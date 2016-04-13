using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event arguments for CanRead callback event arguments.
	/// </summary>
	public class CanReadEventArgs : NodeEventArgs
	{
		private bool canRead;
		private FieldType fieldTypes;
		private string[] fieldNames;

		internal CanReadEventArgs(IqResultEventArgs e, object State, string JID, bool CanRead, FieldType FieldTypes,
			ThingReference[] Nodes, string[] FieldNames)
			: base(e, State, JID, Nodes)
		{
			this.canRead = CanRead;
			this.fieldTypes = FieldTypes;
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
		/// Fields allowed to read, as long as <see cref="CanRead"/> is true. If null, no field restrictions exist.
		/// </summary>
		public string[] FieldsNames
		{
			get { return this.fieldNames; }
		}
	}
}
