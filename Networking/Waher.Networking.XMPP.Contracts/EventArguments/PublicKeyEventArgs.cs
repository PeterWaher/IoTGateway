using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for public key request events.
	/// </summary>
	public class PublicKeyEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for public key request events.
		/// </summary>
		/// <param name="Address">Address whose public key is requested</param>
		/// <param name="Timestamp">Optional Timestamp for when the key was used, in UTC.</param>
		public PublicKeyEventArgs(string Address, DateTime? Timestamp)
			: base()
		{
			this.Address = Address;
			this.Timestamp = Timestamp;
			this.Key = null;
		}

		/// <summary>
		/// Address whose public key is requested
		/// </summary>
		public string Address { get; }

		/// <summary>
		/// Optional Timestamp for when the key was used, in UTC.
		/// </summary>
		public DateTime? Timestamp { get; }

		/// <summary>
		/// Public key of endpoint corresponding to <see cref="Address"/>.
		/// </summary>
		public IE2eEndpoint Key { get; private set; }

		/// <summary>
		/// From when key is valid, in UTC.
		/// </summary>
		public DateTime? ValidFrom { get; private set; }

		/// <summary>
		/// To when key is valid, in UTC.
		/// </summary>
		public DateTime? ValidTo { get; private set; }

		/// <summary>
		/// Returns a public key, and the time period for which it is valid.
		/// </summary>
		/// <param name="Key">Public key.</param>
		/// <param name="From">From when key is valid, in UTC.</param>
		/// <param name="To">To when key is valid, in UTC.</param>
		public void ReturnKey(IE2eEndpoint Key, DateTime From, DateTime? To)
		{
			this.Key = Key;
			this.ValidFrom = From;
			this.ValidTo = To;
		}
	}
}
