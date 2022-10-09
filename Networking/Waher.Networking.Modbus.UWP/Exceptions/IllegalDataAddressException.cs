namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// The data address received in the query is not an allowable address for the slave. More specifically,
	/// the combination of reference number and transfer length is invalid. For a controller with 100
	/// registers, a request with offset 96 and length 4 would succeed, a request with offset 96 and length 5
	/// will generate exception 02.
	/// </summary>
	public class IllegalDataAddressException : ModbusException
	{
		/// <summary>
		/// The data address received in the query is not an allowable address for the slave. More specifically,
		/// the combination of reference number and transfer length is invalid. For a controller with 100
		/// registers, a request with offset 96 and length 4 would succeed, a request with offset 96 and length 5
		/// will generate exception 02.
		/// </summary>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public IllegalDataAddressException(byte FunctionCode, byte[] ExceptionData)
			: base("ILLEGAL DATA ADDRESS", FunctionCode, ExceptionData)
		{
		}
	}
}
