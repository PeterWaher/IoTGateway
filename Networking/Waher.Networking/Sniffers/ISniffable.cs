using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Interface for sniffable classes. Sniffable classes can receive <see cref="ISniffer"/> objects that eavesdrop on communication being performed
	/// at the node.
	/// </summary>
	public interface ISniffable : IEnumerable<ISniffer>
	{
		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer to add.</param>
		void Add(ISniffer Sniffer);

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers to add.</param>
		void AddRange(IEnumerable<ISniffer> Sniffers);

		/// <summary>
		/// REmoves a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer to remove.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		bool Remove(ISniffer Sniffer);
	}
}
