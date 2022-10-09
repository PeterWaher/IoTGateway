namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// A value contained in the query data field is not an allowable value for the slave. This indicates a
	/// fault in the structure of the remainder of a complex request, such as that the implied length is
	/// incorrect. It specifically does NOT mean that a data item submitted for storage in a register has a
	/// value outside the expectation of the application program, since the MODBUS protocol is unaware
	/// of the significance of any particular value of any particular register.
	/// </summary>
	public class IllegalDataValueException : ModbusException
	{
		/// <summary>
		/// A value contained in the query data field is not an allowable value for the slave. This indicates a
		/// fault in the structure of the remainder of a complex request, such as that the implied length is
		/// incorrect. It specifically does NOT mean that a data item submitted for storage in a register has a
		/// value outside the expectation of the application program, since the MODBUS protocol is unaware
		/// of the significance of any particular value of any particular register.
		/// </summary>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public IllegalDataValueException(byte FunctionCode, byte[] ExceptionData)
			: base("ILLEGAL DATA VALUE", FunctionCode, ExceptionData)
		{
		}
	}
}
