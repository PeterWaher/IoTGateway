using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Typed enumerator of sniffers.
	/// </summary>
	public class SnifferEnumerator : IEnumerator<ISniffer>
	{
		private ISniffer[] list;
		private int pos = 0;

		/// <summary>
		/// Typed enumerator of sniffers.
		/// </summary>
		/// <param name="List">Array of sniffers.</param>
		public SnifferEnumerator(ISniffer[] List)
		{
			this.list = List;
		}

		/// <summary>
		/// Current sniffer.
		/// </summary>
		public ISniffer Current
		{
			get
			{
				if (this.pos < this.list.Length)
					return this.list[this.pos];
				else
					return null;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
		}

		object IEnumerator.Current
		{
			get
			{
				if (this.pos < this.list.Length)
					return this.list[this.pos];
				else
					return null;
			}
		}

		/// <summary>
		/// Moves to the next sniffer.
		/// </summary>
		/// <returns>If one was found.</returns>
		public bool MoveNext()
		{
			this.pos++;
			return this.pos < this.list.Length;
		}

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
		public void Reset()
		{
			this.pos = 0;
		}
	}
}
