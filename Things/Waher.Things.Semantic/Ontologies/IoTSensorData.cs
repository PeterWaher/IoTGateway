using System;

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
		public static readonly Uri timestamp = new Uri(Namespace + "timestamp");

		/// <summary>
		/// urn:nf:iot:sd:1.0:fields
		/// </summary>
		public static readonly Uri fields = new Uri(Namespace + "fields");

		/// <summary>
		/// urn:nf:iot:sd:1.0:errros
		/// </summary>
		public static readonly Uri errors = new Uri(Namespace + "errros");

		/// <summary>
		/// urn:nf:iot:sd:1.0:qos
		/// </summary>
		public static readonly Uri qos = new Uri(Namespace + "qos");

		/// <summary>
		/// urn:nf:iot:sd:1.0:fieldType
		/// </summary>
		public static readonly Uri fieldType = new Uri(Namespace + "fieldType");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:value
		/// </summary>
		public static readonly Uri value = new Uri(Namespace + "value");

	}
}
