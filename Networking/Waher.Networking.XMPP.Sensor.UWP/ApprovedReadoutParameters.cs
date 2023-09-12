using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Contains information about what sensor data readout parameters have been approved.
	/// </summary>
	public class ApprovedReadoutParameters
	{
		/// <summary>
		/// Contains information about what sensor data readout parameters have been approved.
		/// </summary>
		/// <param name="Nodes">Approved nodes to read.</param>
		/// <param name="FieldNames">Approved field names to read.</param>
		/// <param name="FieldTypes">Approved types to read.</param>
		public ApprovedReadoutParameters(IThingReference[] Nodes, string[] FieldNames, FieldType FieldTypes)
		{
			this.Nodes = Nodes;
			this.FieldNames = FieldNames;
			this.FieldTypes = FieldTypes;
		}

		/// <summary>
		/// Approved nodes to read.
		/// </summary>
		public IThingReference[] Nodes { get; }

		/// <summary>
		/// Approved field names to read.
		/// </summary>
		public string[] FieldNames { get; }

		/// <summary>
		/// Approved types to read.
		/// </summary>
		public FieldType FieldTypes { get; }
	}
}
