namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// Specialized use in conjunction with Modbus Plus gateways, indicates that no response was
	/// obtained from the target device.Usually means that the device is not present on the network.
	/// </summary>
	public class GatewayTargetDeviceFailedToRespondException : ModbusException
	{
		/// <summary>
		/// Specialized use in conjunction with Modbus Plus gateways, indicates that no response was
		/// obtained from the target device.Usually means that the device is not present on the network.
		/// </summary>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public GatewayTargetDeviceFailedToRespondException(byte FunctionCode, byte[] ExceptionData)
			: base("GATEWAY TARGET DEVICE FAILED TO RESPOND", FunctionCode, ExceptionData)
		{
		}
	}
}
