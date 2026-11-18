namespace Waher.Things.Matter.DataModel
{
	/// <summary>
	/// Identifiers of commonly used Matter clusters (Matter Application Cluster Specification).
	/// </summary>
	public static class ClusterIds
	{
		/// <summary>Identify</summary>
		public const uint Identify = 0x0003;
		/// <summary>Groups</summary>
		public const uint Groups = 0x0004;
		/// <summary>On/Off</summary>
		public const uint OnOff = 0x0006;
		/// <summary>Level Control</summary>
		public const uint LevelControl = 0x0008;
		/// <summary>Descriptor</summary>
		public const uint Descriptor = 0x001d;
		/// <summary>Binding</summary>
		public const uint Binding = 0x001e;
		/// <summary>Access Control</summary>
		public const uint AccessControl = 0x001f;
		/// <summary>Actions</summary>
		public const uint Actions = 0x0025;
		/// <summary>Basic Information</summary>
		public const uint BasicInformation = 0x0028;
		/// <summary>OTA Software Update Provider</summary>
		public const uint OtaSoftwareUpdateProvider = 0x0029;
		/// <summary>OTA Software Update Requestor</summary>
		public const uint OtaSoftwareUpdateRequestor = 0x002a;
		/// <summary>Localization Configuration</summary>
		public const uint LocalizationConfiguration = 0x002b;
		/// <summary>Time Format Localization</summary>
		public const uint TimeFormatLocalization = 0x002c;
		/// <summary>Unit Localization</summary>
		public const uint UnitLocalization = 0x002d;
		/// <summary>Power Source Configuration</summary>
		public const uint PowerSourceConfiguration = 0x002e;
		/// <summary>Power Source</summary>
		public const uint PowerSource = 0x002f;
		/// <summary>General Commissioning</summary>
		public const uint GeneralCommissioning = 0x0030;
		/// <summary>Network Commissioning</summary>
		public const uint NetworkCommissioning = 0x0031;
		/// <summary>Diagnostic Logs</summary>
		public const uint DiagnosticLogs = 0x0032;
		/// <summary>General Diagnostics</summary>
		public const uint GeneralDiagnostics = 0x0033;
		/// <summary>Software Diagnostics</summary>
		public const uint SoftwareDiagnostics = 0x0034;
		/// <summary>Thread Network Diagnostics</summary>
		public const uint ThreadNetworkDiagnostics = 0x0035;
		/// <summary>Wi-Fi Network Diagnostics</summary>
		public const uint WiFiNetworkDiagnostics = 0x0036;
		/// <summary>Ethernet Network Diagnostics</summary>
		public const uint EthernetNetworkDiagnostics = 0x0037;
		/// <summary>Time Synchronization</summary>
		public const uint TimeSynchronization = 0x0038;
		/// <summary>Bridged Device Basic Information</summary>
		public const uint BridgedDeviceBasicInformation = 0x0039;
		/// <summary>Switch</summary>
		public const uint Switch = 0x003b;
		/// <summary>Administrator Commissioning</summary>
		public const uint AdministratorCommissioning = 0x003c;
		/// <summary>Operational Credentials</summary>
		public const uint OperationalCredentials = 0x003e;
		/// <summary>Group Key Management</summary>
		public const uint GroupKeyManagement = 0x003f;
		/// <summary>Fixed Label</summary>
		public const uint FixedLabel = 0x0040;
		/// <summary>User Label</summary>
		public const uint UserLabel = 0x0041;
		/// <summary>Boolean State</summary>
		public const uint BooleanState = 0x0045;
		/// <summary>ICD Management</summary>
		public const uint IcdManagement = 0x0046;
		/// <summary>Mode Select</summary>
		public const uint ModeSelect = 0x0050;
		/// <summary>Air Quality</summary>
		public const uint AirQuality = 0x005b;
		/// <summary>Smoke CO Alarm</summary>
		public const uint SmokeCoAlarm = 0x005c;
		/// <summary>Electrical Power Measurement</summary>
		public const uint ElectricalPowerMeasurement = 0x0090;
		/// <summary>Electrical Energy Measurement</summary>
		public const uint ElectricalEnergyMeasurement = 0x0091;
		/// <summary>Door Lock</summary>
		public const uint DoorLock = 0x0101;
		/// <summary>Window Covering</summary>
		public const uint WindowCovering = 0x0102;
		/// <summary>Pump Configuration and Control</summary>
		public const uint PumpConfigurationAndControl = 0x0200;
		/// <summary>Thermostat</summary>
		public const uint Thermostat = 0x0201;
		/// <summary>Fan Control</summary>
		public const uint FanControl = 0x0202;
		/// <summary>Thermostat User Interface Configuration</summary>
		public const uint ThermostatUserInterfaceConfiguration = 0x0204;
		/// <summary>Color Control</summary>
		public const uint ColorControl = 0x0300;
		/// <summary>Ballast Configuration</summary>
		public const uint BallastConfiguration = 0x0301;
		/// <summary>Illuminance Measurement</summary>
		public const uint IlluminanceMeasurement = 0x0400;
		/// <summary>Temperature Measurement</summary>
		public const uint TemperatureMeasurement = 0x0402;
		/// <summary>Pressure Measurement</summary>
		public const uint PressureMeasurement = 0x0403;
		/// <summary>Flow Measurement</summary>
		public const uint FlowMeasurement = 0x0404;
		/// <summary>Relative Humidity Measurement</summary>
		public const uint RelativeHumidityMeasurement = 0x0405;
		/// <summary>Occupancy Sensing</summary>
		public const uint OccupancySensing = 0x0406;
		/// <summary>Carbon Monoxide Concentration Measurement</summary>
		public const uint CarbonMonoxideConcentrationMeasurement = 0x040c;
		/// <summary>Carbon Dioxide Concentration Measurement</summary>
		public const uint CarbonDioxideConcentrationMeasurement = 0x040d;
		/// <summary>Nitrogen Dioxide Concentration Measurement</summary>
		public const uint NitrogenDioxideConcentrationMeasurement = 0x0413;
		/// <summary>Ozone Concentration Measurement</summary>
		public const uint OzoneConcentrationMeasurement = 0x0415;
		/// <summary>PM2.5 Concentration Measurement</summary>
		public const uint Pm25ConcentrationMeasurement = 0x042a;
		/// <summary>Formaldehyde Concentration Measurement</summary>
		public const uint FormaldehydeConcentrationMeasurement = 0x042b;
		/// <summary>PM1 Concentration Measurement</summary>
		public const uint Pm1ConcentrationMeasurement = 0x042c;
		/// <summary>PM10 Concentration Measurement</summary>
		public const uint Pm10ConcentrationMeasurement = 0x042d;
		/// <summary>Total Volatile Organic Compounds Concentration Measurement</summary>
		public const uint TvocConcentrationMeasurement = 0x042e;
		/// <summary>Radon Concentration Measurement</summary>
		public const uint RadonConcentrationMeasurement = 0x042f;
		/// <summary>Oven Cavity Operational State</summary>
		public const uint OvenCavityOperationalState = 0x0048;
		/// <summary>Oven Mode</summary>
		public const uint OvenMode = 0x0049;
		/// <summary>Laundry Washer Mode</summary>
		public const uint LaundryWasherMode = 0x0051;
		/// <summary>Refrigerator And Temperature Controlled Cabinet Mode</summary>
		public const uint RefrigeratorAndTemperatureControlledCabinetMode = 0x0052;
		/// <summary>Laundry Washer Controls</summary>
		public const uint LaundryWasherControls = 0x0053;
		/// <summary>RVC Run Mode</summary>
		public const uint RvcRunMode = 0x0054;
		/// <summary>RVC Clean Mode</summary>
		public const uint RvcCleanMode = 0x0055;
		/// <summary>Temperature Control</summary>
		public const uint TemperatureControl = 0x0056;
		/// <summary>Refrigerator Alarm</summary>
		public const uint RefrigeratorAlarm = 0x0057;
		/// <summary>Dishwasher Mode</summary>
		public const uint DishwasherMode = 0x0059;
		/// <summary>Dishwasher Alarm</summary>
		public const uint DishwasherAlarm = 0x005d;
		/// <summary>Microwave Oven Mode</summary>
		public const uint MicrowaveOvenMode = 0x005e;
		/// <summary>Operational State</summary>
		public const uint OperationalState = 0x0060;
		/// <summary>RVC Operational State</summary>
		public const uint RvcOperationalState = 0x0061;
		/// <summary>Scenes Management</summary>
		public const uint ScenesManagement = 0x0062;
		/// <summary>HEPA Filter Monitoring</summary>
		public const uint HepaFilterMonitoring = 0x0071;
		/// <summary>Activated Carbon Filter Monitoring</summary>
		public const uint ActivatedCarbonFilterMonitoring = 0x0072;
		/// <summary>Boolean State Configuration</summary>
		public const uint BooleanStateConfiguration = 0x0080;
		/// <summary>Valve Configuration and Control</summary>
		public const uint ValveConfigurationAndControl = 0x0081;
		/// <summary>Device Energy Management</summary>
		public const uint DeviceEnergyManagement = 0x0098;
		/// <summary>Energy EVSE</summary>
		public const uint EnergyEvse = 0x0099;
		/// <summary>Power Topology</summary>
		public const uint PowerTopology = 0x009c;
		/// <summary>Energy EVSE Mode</summary>
		public const uint EnergyEvseMode = 0x009d;
		/// <summary>Water Heater Mode</summary>
		public const uint WaterHeaterMode = 0x009e;
		/// <summary>Device Energy Management Mode</summary>
		public const uint DeviceEnergyManagementMode = 0x009f;
		/// <summary>Smoke Concentration Measurement</summary>
		public const uint SmokeConcentrationMeasurement = 0x0434;
	}

	/// <summary>
	/// Global attribute identifiers, present on all clusters.
	/// </summary>
	public static class GlobalAttributeIds
	{
		/// <summary>GeneratedCommandList</summary>
		public const uint GeneratedCommandList = 0xfff8;
		/// <summary>AcceptedCommandList</summary>
		public const uint AcceptedCommandList = 0xfff9;
		/// <summary>EventList</summary>
		public const uint EventList = 0xfffa;
		/// <summary>AttributeList</summary>
		public const uint AttributeList = 0xfffb;
		/// <summary>FeatureMap</summary>
		public const uint FeatureMap = 0xfffc;
		/// <summary>ClusterRevision</summary>
		public const uint ClusterRevision = 0xfffd;

		/// <summary>
		/// If an attribute ID is a global attribute.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>If global.</returns>
		public static bool IsGlobal(uint AttributeId) => AttributeId >= 0xfff8 && AttributeId <= 0xffff;
	}

	/// <summary>
	/// Well-known device type identifiers.
	/// </summary>
	public static class DeviceTypeIds
	{
		/// <summary>Root Node</summary>
		public const uint RootNode = 0x0016;
		/// <summary>Aggregator</summary>
		public const uint Aggregator = 0x000e;
		/// <summary>Bridged Node</summary>
		public const uint BridgedNode = 0x0013;
		/// <summary>Power Source</summary>
		public const uint PowerSource = 0x0011;
		/// <summary>Door Lock</summary>
		public const uint DoorLock = 0x000a;
		/// <summary>Window Covering</summary>
		public const uint WindowCovering = 0x0202;
		/// <summary>Thermostat</summary>
		public const uint Thermostat = 0x0301;
		/// <summary>Contact Sensor</summary>
		public const uint ContactSensor = 0x0015;
		/// <summary>Water Freeze Detector</summary>
		public const uint WaterFreezeDetector = 0x0041;
		/// <summary>Water Valve</summary>
		public const uint WaterValve = 0x0042;
		/// <summary>Water Leak Detector</summary>
		public const uint WaterLeakDetector = 0x0043;
		/// <summary>Rain Sensor</summary>
		public const uint RainSensor = 0x0044;
		/// <summary>On/Off Light</summary>
		public const uint OnOffLight = 0x0100;
		/// <summary>Dimmable Light</summary>
		public const uint DimmableLight = 0x0101;
		/// <summary>On/Off Plug-in Unit</summary>
		public const uint OnOffPlugInUnit = 0x010a;
		/// <summary>Dimmable Plug-in Unit</summary>
		public const uint DimmablePlugInUnit = 0x010b;
		/// <summary>Color Temperature Light</summary>
		public const uint ColorTemperatureLight = 0x010c;
		/// <summary>Extended Color Light</summary>
		public const uint ExtendedColorLight = 0x010d;
		/// <summary>Temperature Sensor</summary>
		public const uint TemperatureSensor = 0x0302;
		/// <summary>Humidity Sensor</summary>
		public const uint HumiditySensor = 0x0307;
		/// <summary>Occupancy Sensor</summary>
		public const uint OccupancySensor = 0x0107;
		/// <summary>Light Sensor</summary>
		public const uint LightSensor = 0x0106;
		/// <summary>Pressure Sensor</summary>
		public const uint PressureSensor = 0x0305;
		/// <summary>Flow Sensor</summary>
		public const uint FlowSensor = 0x0306;
		/// <summary>Fan</summary>
		public const uint Fan = 0x002b;
		/// <summary>Generic Switch</summary>
		public const uint GenericSwitch = 0x000f;
		/// <summary>Electrical Sensor</summary>
		public const uint ElectricalSensor = 0x0510;
	}
}
