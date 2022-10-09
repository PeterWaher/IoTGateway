namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// Indicates that the request as framed would generate a response whose size exceeds the available
	/// MODBUS data size. Used only by functions generating a multi-part response, such as functions 20
	/// and 21.
	/// </summary>
	public class IllegalResponseLengthException : ModbusException
	{
		/// <summary>
		/// Indicates that the request as framed would generate a response whose size exceeds the available
		/// MODBUS data size. Used only by functions generating a multi-part response, such as functions 20
		/// and 21.
		/// </summary>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public IllegalResponseLengthException(byte FunctionCode, byte[] ExceptionData)
			: base("ILLEGAL RESPONSE LENGTH", FunctionCode, ExceptionData)
		{
		}
	}
}
