using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.Networking
{
	/// <summary>
	/// Implements a text TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
	/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and tramitted in the
	/// permitted pace.
	/// </summary>
	public class TextTcpClient : BinaryTcpClient, ITextTransportLayer
	{
		private Encoding encoding;
		private readonly bool sniffText;

		/// <summary>
		/// Implements a text TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and tramitted in the
		/// permitted pace.
		/// </summary>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(Encoding Encoding, params ISniffer[] Sniffers)
			: this(Encoding, true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a text TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and tramitted in the
		/// permitted pace.
		/// </summary>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		protected TextTcpClient(Encoding Encoding, bool SniffText, params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			this.encoding = Encoding;
			this.sniffText = SniffText;
		}

#if WINDOWS_UWP
		/// <summary>
		/// Implements a text TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and tramitted in the
		/// permitted pace.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(StreamSocket Client, Encoding Encoding, params ISniffer[] Sniffers)
			: this(Client, Encoding, true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a text TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and tramitted in the
		/// permitted pace.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		protected TextTcpClient(StreamSocket Client, Encoding Encoding, bool SniffText, params ISniffer[] Sniffers)
			: base(Client, false, Sniffers)
		{
			this.encoding = Encoding;
			this.sniffText = SniffText;
		}
#else
		/// <summary>
		/// Implements a text TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and tramitted in the
		/// permitted pace.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public TextTcpClient(TcpClient Client, Encoding Encoding, params ISniffer[] Sniffers)
			: this(Client, Encoding, true, Sniffers)
		{
		}

		/// <summary>
		/// Implements a text TCP Client, by encapsulating a <see cref="TcpClient"/>. It also makes the use of <see cref="TcpClient"/>
		/// safe, making sure it can be disposed, even during an active connection attempt. Outgoing data is queued and tramitted in the
		/// permitted pace.
		/// </summary>
		/// <param name="Client">Encapsulate this <see cref="TcpClient"/> connection.</param>
		/// <param name="Encoding">Text encoding to use.</param>
		/// <param name="SniffText">If text communication is to be forwarded to registered sniffers.</param>
		/// <param name="Sniffers">Sniffers.</param>
		protected TextTcpClient(TcpClient Client, Encoding Encoding, bool SniffText, params ISniffer[] Sniffers)
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
		/// <param name="Data">Binary data received.</param>
		/// <returns>If the process should be continued.</returns>
		protected override Task<bool> BinaryDataReceived(byte[] Data)
		{
			string Text = this.encoding.GetString(Data);
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
		/// <param name="Packet">Text packet.</param>
		public void Send(string Packet)
		{
			this.Send(Packet, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		public void Send(string Packet, EventHandler Callback)
		{
			byte[] Data = this.encoding.GetBytes(Packet);
			base.Send(Data, Callback);
			this.TextDataSent(Packet);
		}

		/// <summary>
		/// Method called when binary data has been sent.
		/// </summary>
		/// <param name="Data">Text data sent.</param>
		protected virtual Task TextDataSent(string Data)
		{
			if (this.sniffText && this.HasSniffers)
				this.TransmitText(Data);

			return this.OnSent?.Invoke(this, Data) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public new event TextEventHandler OnSent;

	}
}
