namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// Specialized use in conjunction with function codes 20 and 21, to indicate that the extended file area
	/// failed to pass a consistency check.
	/// </summary>
	public class MemoryParityException : ModbusException
	{
		/// <summary>
		/// Specialized use in conjunction with function codes 20 and 21, to indicate that the extended file area
		/// failed to pass a consistency check.
		/// </summary>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public MemoryParityException(byte FunctionCode, byte[] ExceptionData)
			: base("MEMORY PARlTY ERROR", FunctionCode, ExceptionData)
		{
		}
	}
}
