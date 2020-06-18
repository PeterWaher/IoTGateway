using System;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
#else
using System.Net.Sockets;
#endif
using Waher.Networking.Sniffers;

namespace Waher.Networking
{
	/// <summary>
	/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
	/// Commands are sent as rows, and responses are returned as rows.
	/// </summary>
	public class RowTcpClient : TextTcpClient
	{
		private readonly StringBuilder buffer = new StringBuilder();
		private readonly int maxLen = 0;
		private char prev = (char)0;
		private bool error = false;
		private int len = 0;

		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// Commands are sent as rows, and responses are returned as rows.
		/// </summary>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="MaxLength">Maximum number of characters in a row</param>
		/// <param name="Sniffers">Sniffers.</param>
		public RowTcpClient(Encoding Encoding, int MaxLength, params ISniffer[] Sniffers)
			: base(Encoding, Sniffers)
		{
			this.maxLen = MaxLength;
		}

		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// Commands are sent as rows, and responses are returned as rows.
		/// </summary>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="MaxLength">Maximum number of characters in a row</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public RowTcpClient(Encoding Encoding, int MaxLength, bool SniffText, params ISniffer[] Sniffers)
			: base(Encoding, SniffText, Sniffers)
		{
			this.maxLen = MaxLength;
		}

#if WINDOWS_UWP
		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// Commands are sent as rows, and responses are returned as rows.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="StreamSocket"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="MaxLength">Maximum number of characters in a row</param>
		/// <param name="Sniffers">Sniffers.</param>
		public RowTcpClient(StreamSocket Client, Encoding Encoding, int MaxLength, params ISniffer[] Sniffers)
			: base(Client, Encoding, Sniffers)
		{
			this.maxLen = MaxLength;
		}

		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// Commands are sent as rows, and responses are returned as rows.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="StreamSocket"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="MaxLength">Maximum number of characters in a row</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public RowTcpClient(StreamSocket Client, Encoding Encoding, int MaxLength, bool SniffText, params ISniffer[] Sniffers)
			: base(Client, Encoding, SniffText, Sniffers)
		{
			this.maxLen = MaxLength;
		}
#else
		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// Commands are sent as rows, and responses are returned as rows.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="MaxLength">Maximum number of characters in a row</param>
		/// <param name="Sniffers">Sniffers.</param>
		public RowTcpClient(TcpClient Client, Encoding Encoding, int MaxLength, params ISniffer[] Sniffers)
			: base(Client, Encoding, Sniffers)
		{
			this.maxLen = MaxLength;
		}

		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// Commands are sent as rows, and responses are returned as rows.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="MaxLength">Maximum number of characters in a row</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public RowTcpClient(TcpClient Client, Encoding Encoding, int MaxLength, bool SniffText, params ISniffer[] Sniffers)
			: base(Client, Encoding, SniffText, Sniffers)
		{
			this.maxLen = MaxLength;
		}
#endif

		/// <summary>
		/// Method called when text data has been received.
		/// </summary>
		/// <param name="Data">Text data received.</param>
		protected async override Task<bool> TextDataReceived(string Data)
		{
			foreach (char ch in Data)
			{
				if (ch == '\r' || ch == '\n')
				{
					if (this.len == 0)
					{
						if (this.prev == ch && !await base.TextDataReceived(string.Empty))
							return false;
					}
					else
					{
						if (this.error)
						{
							this.Error("Row too long: " + this.len.ToString() + " characters");
							this.error = false;
						}
						else if (!await base.TextDataReceived(this.buffer.ToString()))
							return false;

						this.len = 0;
						this.buffer.Clear();
						this.prev = ch;
					}
				}
				else
				{
					if (this.len++ > this.maxLen)
						this.error = true;
					else
						this.buffer.Append(ch);

					this.prev = (char)0;
				}
			}

			return true;
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <returns>If data was sent.</returns>
		public override Task<bool> SendAsync(string Packet)
		{
			return this.SendAsync(Packet, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Text">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <returns>If data was sent.</returns>
		public override Task<bool> SendAsync(string Text, EventHandler Callback)
		{
			return base.SendAsync(Text + "\r\n", Callback);
		}

		/// <summary>
		/// Method called when binary data has been sent.
		/// </summary>
		/// <param name="Data">Text data sent.</param>
		protected override Task TextDataSent(string Data)
		{
			return base.TextDataSent(Data.Substring(0, Data.Length - 2));
		}

	}
}
