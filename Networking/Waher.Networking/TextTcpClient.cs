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
	/// </summary>
	public class TextTcpClient : BinaryTcpClient, ITextTransportLayer
	{
		private Encoding encoding;
		private readonly bool sniffText;

		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// </summary>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(Encoding Encoding, params ISniffer[] Sniffers)
			: this(Encoding, true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// </summary>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(Encoding Encoding, bool SniffText, params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			this.encoding = Encoding;
			this.sniffText = SniffText;
		}

#if WINDOWS_UWP
		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(StreamSocket Client, Encoding Encoding, params ISniffer[] Sniffers)
			: this(Client, Encoding, true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(StreamSocket Client, Encoding Encoding, bool SniffText, params ISniffer[] Sniffers)
			: base(Client, false, Sniffers)
		{
			this.encoding = Encoding;
			this.sniffText = SniffText;
		}
#else
		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(TcpClient Client, Encoding Encoding, params ISniffer[] Sniffers)
			: this(Client, Encoding, true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a text-based TCP Client, by using the thread-safe full-duplex <see cref="BinaryTcpClient"/>.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(TcpClient Client, Encoding Encoding, bool SniffText, params ISniffer[] Sniffers)
			: base(Client, false, Sniffers)
		{
			this.encoding = Encoding;
			this.sniffText = SniffText;
		}
#endif

		/// <summary>
		/// Text encoding to use.
		/// </summary>
		public Encoding Encoding
		{
			get => this.encoding;
			set => this.encoding = value;
		}

		/// <summary>
		/// Method called when binary data has been received.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte read.</param>
		/// <param name="Count">Number of bytes read.</param>
		/// <returns>If the process should be continued.</returns>
		protected override Task<bool> BinaryDataReceived(byte[] Buffer, int Offset, int Count)
		{
			string Text = this.encoding.GetString(Buffer, Offset, Count);
			return this.TextDataReceived(Text);
		}

		/// <summary>
		/// Method called when text data has been received.
		/// </summary>
		/// <param name="Data">Text data received.</param>
		protected virtual Task<bool> TextDataReceived(string Data)
		{
			if (this.sniffText && this.HasSniffers)
				this.ReceiveText(Data);

			return this.OnReceived?.Invoke(this, Data) ?? Task.FromResult<bool>(true);
		}

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		public new event TextEventHandler OnReceived;

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Text">Text packet.</param>
		/// <returns>If data was sent.</returns>
		public virtual Task<bool> Send(string Text)
		{
			return this.Send(Text, null);
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Text">Text packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <returns>If data was sent.</returns>
		public async virtual Task<bool> Send(string Text, EventHandler Callback)
		{
			byte[] Data = this.encoding.GetBytes(Text);
			bool Result = await base.SendAsync(Data, Callback);
			await this.TextDataSent(Text);
			return Result;
		}

		/// <summary>
		/// Method called when text data has been sent.
		/// </summary>
		/// <param name="Text">Text data sent.</param>
		protected virtual Task TextDataSent(string Text)
		{
			if (this.sniffText && this.HasSniffers)
				this.TransmitText(Text);

			return this.OnSent?.Invoke(this, Text) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public new event TextEventHandler OnSent;

	}
}
