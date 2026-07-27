using System;
using System.Collections.Generic;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Runtime.Collections;

namespace Waher.Networking.XMPP.Contracts.PublicKeys
{
	/// <summary>
	/// Contains a list of public key records for an endpoint.
	/// </summary>
	public class PublicKeyRecords
	{
		private readonly Dictionary<string, ChunkedList<PublicKeyRecord>> records =
			new Dictionary<string, ChunkedList<PublicKeyRecord>>();

		/// <summary>
		/// Contains a list of public key records for an endpoint.
		/// </summary>
		public PublicKeyRecords()
		{
		}

		/// <summary>
		/// Adds a public key record to the list.
		/// </summary>
		/// <param name="Address">Address of the public key.</param>
		/// <param name="From">From when the key is valid.</param>
		/// <param name="To">To when the key is valid.</param>
		/// <param name="PublicKey">Public Key.</param>
		public void Add(string Address, DateTime From, DateTime To, KeyEventArgs PublicKey)
		{
			lock (this.records)
			{
				if (!this.records.TryGetValue(Address, out ChunkedList<PublicKeyRecord> List))
				{
					List = new ChunkedList<PublicKeyRecord>();
					this.records[Address] = List;
				}

				int i, c = List.Count;

				for (i = 0; i < c; i++)
				{
					PublicKeyRecord Record = List[i];

					if (Record.PublicKey.Key.PublicKeyBase64 == PublicKey.Key.PublicKeyBase64)
					{
						if (From < Record.From)
							Record.From = From;

						if (To > Record.To)
							Record.To = To;

						return;
					}
				}

				List.Add(new PublicKeyRecord(From, To, PublicKey));
			}
		}

		/// <summary>
		/// Tries to get a public key record valid for a given timestamp.
		/// </summary>
		/// <param name="Address">Address of the public key.</param>
		/// <param name="Timestamp">Timestamp</param>
		/// <param name="PubKey">Public key record valid for the given timestamp, if found.</param>
		/// <returns>True if a valid public key record was found, otherwise false.</returns>
		public bool TryGetRecord(string Address, DateTime Timestamp, out KeyEventArgs PubKey)
		{
			lock (this.records)
			{
				if (this.records.TryGetValue(Address, out ChunkedList<PublicKeyRecord> List))
				{
					foreach (PublicKeyRecord Record in List)
					{
						if (Timestamp >= Record.From && Timestamp <= Record.To)
						{
							PubKey = Record.PublicKey;
							return true;
						}
					}
				}
			}

			PubKey = null;
			return false;
		}

		/// <summary>
		/// Removes all public keys related to a specific address.
		/// </summary>
		/// <param name="Address">Address</param>
		/// <returns>If records were found and removed.</returns>
		public bool Remove(string Address)
		{
			lock (this.records)
			{
				return this.records.Remove(Address);
			}
		}
	}
}
