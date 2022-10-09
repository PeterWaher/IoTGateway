using System;

namespace Waher.Networking.Modbus.Exceptions
{
	/// <summary>
	/// Base class for Modbus exceptions.
	/// </summary>
	public class ModbusException : Exception
	{
		/// <summary>
		/// Base class for Modbus exceptions.
		/// </summary>
		/// <param name="Message">Exception message</param>
		/// <param name="FunctionCode">Function code returned by device.</param>
		/// <param name="ExceptionData">Binary data</param>
		public ModbusException(string Message, byte FunctionCode, byte[] ExceptionData)
			: base(Message)
		{
			this.FunctionCode = FunctionCode;
			this.ExceptionData = ExceptionData;
		}

		/// <summary>
		/// Function code returned by device.
		/// </summary>
		public byte FunctionCode
		{
			get;
			private set;
		}

		/// <summary>
		/// Binary exception data
		/// </summary>
		public byte[] ExceptionData
		{
			get;
			private set;
		}
	}
}
