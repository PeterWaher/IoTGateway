using System;
using System.Data;

namespace Waher.Things.Semantic.Ontologies
{
	/// <summary>
	/// IoT Sensor-Data Ontology
	/// </summary>
	public static class IoTSensorData
	{
		/// <summary>
		/// urn:nf:iot:sd:1.0:
		/// </summary>
		public const string Namespace = "urn:nf:iot:sd:1.0:";

		/// <summary>
		/// urn:nf:iot:sd:1.0:unit:
		/// </summary>
		public const string UnitNamespace = Namespace + "unit:";

		/// <summary>
		/// urn:nf:iot:sd:1.0:timestamp
		/// </summary>
		public static readonly Uri Timestamp = new Uri(Namespace + "timestamp");

		/// <summary>
		/// urn:nf:iot:sd:1.0:fields
		/// </summary>
		public static readonly Uri Fields = new Uri(Namespace + "fields");

		/// <summary>
		/// urn:nf:iot:sd:1.0:errros
		/// </summary>
		public static readonly Uri Errors = new Uri(Namespace + "errros");

		/// <summary>
		/// urn:nf:iot:sd:1.0:qos
		/// </summary>
		public const string QoS = Namespace + "qos";

		/// <summary>
		/// urn:nf:iot:sd:1.0:qos
		/// </summary>
		public static readonly Uri QoSUri = new Uri(QoS);

		/// <summary>
		/// urn:nf:iot:sd:1.0:fieldType
		/// </summary>
		public const string FieldType = Namespace + "fieldType";

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:value
		/// </summary>
		public static readonly Uri Value = new Uri(Namespace + "value");

	}
}
