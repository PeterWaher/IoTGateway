using System;

namespace Waher.Networking.Modbus
{
	/// <summary>
	/// Event arguments for read words event.
	/// </summary>
	public class ReadWordsEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for read words event.
		/// </summary>
		/// <param name="UnitAddress">Request was made for device at this unit address.</param>
		/// <param name="ReferenceNr">Read is to start at this reference number.</param>
		/// <param name="NrWords">This number of words has been requested to be read.</param>
		/// <param name="Words">Words read is to be placed in this array.</param>
		public ReadWordsEventArgs(ushort UnitAddress, ushort ReferenceNr, ushort NrWords, 
			ushort[] Words)
		{
			this.UnitAddress = UnitAddress;
			this.ReferenceNr = ReferenceNr;
			this.NrWords = NrWords;
			this.Words = Words;
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
		/// This number of words has been requested to be read.
		/// </summary>
		public ushort NrWords { get; }

		/// <summary>
		/// Words read is to be placed in this array.
		/// </summary>
		public ushort[] Words { get; }

		/// <summary>
		/// Access to register values.
		/// </summary>
		/// <param name="RegisterNr">Register number.</param>
		/// <returns>Boolean value of corresponding register.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="RegisterNr"/> is below
		/// <see cref="ReferenceNr"/> or greater or equal to <see cref="ReferenceNr"/> +
		/// <see cref="NrWords"/>.</exception>
		public ushort this[int RegisterNr]
		{
			get
			{
				RegisterNr -= this.ReferenceNr;
				if (RegisterNr < 0 || RegisterNr >= this.Words.Length)
					throw new ArgumentOutOfRangeException(nameof(RegisterNr), "Register index out of range.");

				return this.Words[RegisterNr];
			}

			set
			{
				RegisterNr -= this.ReferenceNr;
				if (RegisterNr < 0 || RegisterNr >= this.Words.Length)
					throw new ArgumentOutOfRangeException(nameof(RegisterNr), "Register index out of range.");

				this.Words[RegisterNr] = value;
			}
		}
	}
}
