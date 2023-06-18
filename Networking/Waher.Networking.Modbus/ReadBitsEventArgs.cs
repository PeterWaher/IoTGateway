using System;
using System.Collections;

namespace Waher.Networking.Modbus
{
	/// <summary>
	/// Event arguments for read bits event.
	/// </summary>
	public class ReadBitsEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for read bits event.
		/// </summary>
		/// <param name="UnitAddress">Request was made for device at this unit address.</param>
		/// <param name="ReferenceNr">Read is to start at this reference number.</param>
		/// <param name="NrBits">This number of bits has been requested to be read.</param>
		/// <param name="Bits">Bits read is to be placed in this array.</param>
		public ReadBitsEventArgs(ushort UnitAddress, ushort ReferenceNr, ushort NrBits, 
			BitArray Bits)
		{
			this.UnitAddress = UnitAddress;
			this.ReferenceNr = ReferenceNr;
			this.NrBits = NrBits;
			this.Bits = Bits;
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
		/// This number of bits has been requested to be read.
		/// </summary>
		public ushort NrBits { get; }

		/// <summary>
		/// Bits read is to be placed in this array.
		/// </summary>
		public BitArray Bits { get; }

		/// <summary>
		/// Access to register values.
		/// </summary>
		/// <param name="RegisterNr">Register number.</param>
		/// <returns>Boolean value of corresponding register.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="RegisterNr"/> is below
		/// <see cref="ReferenceNr"/> or greater or equal to <see cref="ReferenceNr"/> +
		/// <see cref="NrBits"/>.</exception>
		public bool this[int RegisterNr]
		{
			get
			{
				RegisterNr -= this.ReferenceNr;
				if (RegisterNr < 0 || RegisterNr >= this.Bits.Length)
					throw new ArgumentOutOfRangeException(nameof(RegisterNr), "Register index out of range.");

				return this.Bits[RegisterNr];
			}

			set
			{
				RegisterNr -= this.ReferenceNr;
				if (RegisterNr < 0 || RegisterNr >= this.Bits.Length)
					throw new ArgumentOutOfRangeException(nameof(RegisterNr), "Register index out of range.");

				this.Bits[RegisterNr] = value;
			}
		}
	}
}
