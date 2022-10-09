namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// Specialized use in conjunction with Modbus Plus gateways, indicates that the gateway was unable
	/// to allocate a Modbus Plus PATH to use to process the request.Usually means that the gateway is
	/// misconfigured.
	/// </summary>
	public class GatewayPathUnavailableException : ModbusException
	{
		/// <summary>
		/// Specialized use in conjunction with Modbus Plus gateways, indicates that the gateway was unable
		/// to allocate a Modbus Plus PATH to use to process the request.Usually means that the gateway is
		/// misconfigured.
		/// </summary>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public GatewayPathUnavailableException(byte FunctionCode, byte[] ExceptionData)
			: base("GATEWAY PATH UNAVAILABLE", FunctionCode, ExceptionData)
		{
		}
	}
}
