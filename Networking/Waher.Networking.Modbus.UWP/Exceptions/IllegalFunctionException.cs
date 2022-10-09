namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// The function code received in the query is not an allowable action for the slave.This may be
	/// because the function code is only applicable to newer controllers, and was not implemented in the
	/// unit selected. It could also indicate that the slave is in the wrong state to process a request of this
	/// type, for example because it is unconfigured and is being asked to return register values
	/// </summary>
	public class IllegalFunctionException : ModbusException
	{
		/// <summary>
		/// The function code received in the query is not an allowable action for the slave.This may be
		/// because the function code is only applicable to newer controllers, and was not implemented in the
		/// unit selected. It could also indicate that the slave is in the wrong state to process a request of this
		/// type, for example because it is unconfigured and is being asked to return register values
		/// </summary>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public IllegalFunctionException(byte FunctionCode, byte[] ExceptionData)
			: base("ILLEGAL FUNCTlON", FunctionCode, ExceptionData)
		{
		}
	}
}
