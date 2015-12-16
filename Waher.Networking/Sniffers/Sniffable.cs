using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Simple abstract base class for sniffable nodes.
	/// </summary>
	public abstract class Sniffable : ISniffable
	{
		private List<ISniffer> sniffers = new List<ISniffer>();
		private ISniffer[] staticList = new ISniffer[0];
		private bool hasSniffers = false;

		/// <summary>
		/// Simple abstract base class for sniffable nodes.
		/// </summary>
		public Sniffable()
		{
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public void Add(ISniffer Sniffer)
		{
			lock (this.sniffers)
			{
				this.sniffers.Add(Sniffer);
				this.staticList = this.sniffers.ToArray();
				this.hasSniffers = this.staticList.Length > 0;
			}
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			lock (this.sniffers)
			{
				this.sniffers.AddRange(Sniffers);
				this.staticList = this.sniffers.ToArray();
				this.hasSniffers = this.staticList.Length > 0;
			}
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public bool Remove(ISniffer Sniffer)
		{
			lock (this.sniffers)
			{
				if (this.sniffers.Remove(Sniffer))
				{
					this.staticList = this.sniffers.ToArray();
					this.hasSniffers = this.staticList.Length > 0;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public IEnumerator<ISniffer> GetEnumerator()
		{
			return new SnifferEnumerator(this.staticList);
		}

		private class SnifferEnumerator : IEnumerator<ISniffer>
		{
			private ISniffer[] list;
			private int pos = 0;

			public SnifferEnumerator(ISniffer[] List)
			{
				this.list = List;
			}

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

			public bool MoveNext()
			{
				this.pos++;
				return this.pos < this.list.Length;
			}

			public void Reset()
			{
				this.pos = 0;
			}
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.staticList.GetEnumerator();
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(byte[] Data)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.ReceiveBinary(Data);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(byte[] Data)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.TransmitBinary(Data);
			}
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.ReceiveText(Text);
			}
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.TransmitText(Text);
			}
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.Information(Comment);
			}
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.Warning(Warning);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.Error(Error);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception)
		{
			string Msg = Exception.Message + "\r\n\r\n" + Exception.StackTrace;

			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.Exception(Msg);
			}
		}

	}
}
