namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// Specialized use in conjunction with programming commands
	/// </summary>
	public class AcknowledgeException : ModbusException
	{
		/// <summary>
		/// Specialized use in conjunction with programming commands
		/// </summary>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public AcknowledgeException(byte FunctionCode, byte[] ExceptionData)
			: base("ACKNOWLEDGE", FunctionCode, ExceptionData)
		{
		}
	}
}
