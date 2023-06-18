using System;

namespace Waher.Networking.Modbus
{
	/// <summary>
	/// Event arguments for write word events.
	/// </summary>
	public class WriteWordEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for write word events.
		/// </summary>
		/// <param name="UnitAddress">Request was made for device at this unit address.</param>
		/// <param name="ReferenceNr">Read is to start at this reference number.</param>
		/// <param name="Value">Value to write.</param>
		public WriteWordEventArgs(ushort UnitAddress, ushort ReferenceNr, ushort Value)
		{
			this.UnitAddress = UnitAddress;
			this.ReferenceNr = ReferenceNr;
			this.Value = Value;
		}

		/// <summary>
		/// Request was made for device at this unit address.
		/// </summary>
		public ushort UnitAddress { get; }

		/// <summary>
		/// Read is to start at this reference number.
		/// </summary>
		public ushort ReferenceNr { get; }

		/// <summary>
		/// Value to write. Can be modifed by the handler of the event to return
		/// the output value, if different.
		/// </summary>
		public ushort Value { get; set; }
	}
}
