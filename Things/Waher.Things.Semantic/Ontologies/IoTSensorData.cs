using System;

namespace Waher.Things.Semantic.Ontologies
{
	/// <summary>
	/// IoT Sensor-Data Ontology
	/// </summary>
	public static class IoTSensorData
	{
		/// <summary>
		/// urn:ieee:iot:sd:1.0:
		/// </summary>
		public const string Namespace = "urn:ieee:iot:sd:1.0:";

		/// <summary>
		/// urn:ieee:iot:sd:1.0:unit:
		/// </summary>
		public const string UnitNamespace = Namespace + "unit:";

		/// <summary>
		/// urn:ieee:iot:sd:1.0:timestamp
		/// </summary>
		public static readonly Uri Timestamp = new Uri(Namespace + "timestamp");

		/// <summary>
		/// urn:ieee:iot:sd:1.0:fields
		/// </summary>
		public static readonly Uri Fields = new Uri(Namespace + "fields");

		/// <summary>
		/// urn:ieee:iot:sd:1.0:errros
		/// </summary>
		public static readonly Uri Errors = new Uri(Namespace + "errros");

		/// <summary>
		/// urn:ieee:iot:sd:1.0:module
		/// </summary>
		public static readonly Uri Module = new Uri(Namespace + "module");

		/// <summary>
		/// urn:ieee:iot:sd:1.0:qos
		/// </summary>
		public static readonly Uri QoS = new Uri(Namespace + "qos");

		/// <summary>
		/// urn:ieee:iot:sd:1.0:fieldType
		/// </summary>
		public const string FieldType = Namespace + "fieldType";

		/// <summary>
		/// urn:ieee:iot:sd:1.0:localization
		/// </summary>
		public static readonly Uri Localization = new Uri(Namespace + "localization");

		/// <summary>
		/// urn:ieee:iot:sd:1.0:stringId
		/// </summary>
		public static readonly Uri StringId = new Uri(Namespace + "stringId");

		/// <summary>
		/// urn:ieee:iot:sd:1.0:seed
		/// </summary>
		public static readonly Uri Seed = new Uri(Namespace + "seed");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:value
		/// </summary>
		public static readonly Uri Value = new Uri(Namespace + "value");

	}
}
