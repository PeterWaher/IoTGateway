using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Temporary;

namespace Waher.Networking.HTTP.WebSockets
{
	internal enum WebSocketOpcode
	{
		Continue = 0,
		Text = 1,
		Binary = 2,
		Close = 8,
		Ping = 9,
		Pong = 10
	}

	/// <summary>
	/// Close status codes.
	/// </summary>
	public enum WebSocketCloseStatus
	{
		/// <summary>
		/// 1000 indicates a normal closure, meaning that the purpose for
		/// which the connection was established has been fulfilled.
		/// </summary>
		Normal = 1000,

		/// <summary>
		/// 1001 indicates that an endpoint is "going away", such as a server
		/// going down or a browser having navigated away from a page.
		/// </summary>
		GoingAway = 1001,

		/// <summary>
		/// 1002 indicates that an endpoint is terminating the connection due
		/// to a protocol error.
		/// </summary>
		ProtocolError = 1002,

		/// <summary>
		/// 1003 indicates that an endpoint is terminating the connection
		/// because it has received a type of data it cannot accept (e.g., an
		/// endpoint that understands only text data MAY send this if it
		/// receives a binary message).
		/// </summary>
		NotAcceptable = 1003,

		/// <summary>
		/// 1007 indicates that an endpoint is terminating the connection
		/// because it has received data within a message that was not
		/// consistent with the type of the message (e.g., non-UTF-8 [RFC3629]
		/// data within a text message).
		/// </summary>
		NotConsistent = 1007,

		/// <summary>
		/// 1008 indicates that an endpoint is terminating the connection
		/// because it has received a message that violates its policy.  This
		/// is a generic status code that can be returned when there is no
		/// other more suitable status code (e.g., 1003 or 1009) or if there
		/// is a need to hide specific details about the policy.
		/// </summary>
		PolicyViolation = 1008,

		/// <summary>
		/// 1009 indicates that an endpoint is terminating the connection
		/// because it has received a message that is too big for it to
		/// process.
		/// </summary>
		TooBig = 1009,

		/// <summary>
		/// 1010 indicates that an endpoint (client) is terminating the
		/// connection because it has expected the server to negotiate one or
		/// more extension, but the server didn't return them in the response
		/// message of the WebSocket handshake.  The list of extensions that
		/// are needed SHOULD appear in the /reason/ part of the Close frame.
		/// Note that this status code is not used by the server, because it
		/// can fail the WebSocket handshake instead.
		/// </summary>
		MissingExtension = 1010,

		/// <summary>
		/// 1011 indicates that a server is terminating the connection because
		/// it encountered an unexpected condition that prevented it from
		/// fulfilling the request.
		/// </summary>
		UnexpectedCondition = 1011
	}

	/// <summary>
	/// Class handling a web-socket.
	/// </summary>
	public class WebSocket : IDisposable
	{
		private object tag = null;

		private readonly WebSocketListener listener;
		private HttpClientConnection connection;
		private readonly HttpRequest httpRequest;
		private readonly HttpResponse httpResponse;
		private WebSocketOpcode opCode;
		private WebSocketOpcode controlOpCode;
		private Stream payload = null;
		private Stream payloadBak = null;
		private readonly byte[] mask = new byte[4];
		private int state = 0;
		private int payloadLen;
		private int payloadOffset;
		private byte rsv;
		private bool fin;
		private bool masked;
		private bool controlFrame;
		private bool closed = false;
		private bool writingFragments = false;

		internal WebSocket(WebSocketListener WebSocketListener, HttpRequest Request,
			HttpResponse Response)
		{
			this.listener = WebSocketListener;
			this.httpRequest = Request;
			this.httpResponse = Response;
			this.connection = Request.clientConnection;
		}

		/// <summary>
		/// Original HTTP request made to upgrade the connection to a WebSocket connection.
		/// </summary>
		public HttpRequest HttpRequest => this.httpRequest;

		/// <summary>
		/// Applications can use this property to attach a value of any type to the 
		/// websocket connection.
		/// </summary>
		public object Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

#if !WINDOWS_UWP
		/// <summary>
		/// If the web-socket is encrypted or not.
		/// </summary>
		public bool Encrypted
		{
			get { return this.httpRequest?.clientConnection?.Encrypted ?? false; }
		}
#endif
		/// <summary>
		/// Current client connection
		/// </summary>
		public BinaryTcpClient ClientConnection
		{
			get { return this.connection?.Client; }
		}

		/// <summary>
		/// Disposes the object.
		/// </summary>
		public void Dispose()
		{
			this.payload?.Dispose();
			this.payload = null;

			this.connection?.Dispose();
			this.connection = null;

			EventHandler h = this.Disposed;
			if (!(h is null))
			{
				try
				{
					h(this, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when socket has been disposed.
		/// </summary>
		public event EventHandler Disposed = null;

		internal async Task<bool> WebSocketDataReceived(byte[] Data, int Offset, int NrRead)
		{
			if (this.connection.HasSniffers)
			{
				if (Offset != 0 || Data.Length != NrRead)
				{
					byte[] Received = new byte[NrRead];
					Array.Copy(Data, Offset, Received, 0, NrRead);
					Data = Received;
					Offset = 0;
				}

				this.connection.ReceiveBinary(Data);
			}

			int End = Offset + NrRead;
			bool Again = false;

			while (Offset < End || Again)
			{
				switch (this.state)
				{
					case 0: // Waiting for first byte
						byte b = Data[Offset++];

						this.fin = (b & 128) != 0;
						this.rsv = (byte)(b & 0x70);
						this.opCode = (WebSocketOpcode)(b & 15);
						this.controlFrame = (b & 8) != 0;
						this.payload = null;
						this.state++;

						if (this.controlFrame)
							this.controlOpCode = this.opCode;

						if (this.rsv != 0)
						{
							await this.Close(WebSocketCloseStatus.ProtocolError, "RSV must be 0.");
							return false;
						}

						if (this.controlFrame && !this.fin)
						{
							await this.Close(WebSocketCloseStatus.ProtocolError, "FIN must be 1 in control frames.");
							return false;
						}

						break;

					case 1: // Waiting for payload length
						b = Data[Offset++];

						this.masked = (b & 128) != 0;
						this.payloadLen = (b & 127);

						if (!this.masked)
						{
							await this.Close(WebSocketCloseStatus.ProtocolError, "Payload must be masked.");
							return false;
						}

						if (this.controlFrame && this.payloadLen >= 126)
						{
							await this.Close(WebSocketCloseStatus.ProtocolError, "Control frame too large.");
							return false;
						}

						switch (this.payloadLen)
						{
							case 126:
								this.payloadLen = 0;
								this.state += 7;
								break;

							case 127:
								this.payloadLen = 0;
								this.state++;
								break;

							default:
								this.state += 9;
								break;
						}
						break;

					case 2: // Waiting for extended payload length
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
					case 8:
					case 9:
						this.payloadLen <<= 8;
						this.payloadLen |= Data[Offset++];

						if (++this.state == 10)
						{
							if (this.payloadLen > this.listener.MaximumBinaryPayloadSize)
							{
								await this.Close(WebSocketCloseStatus.TooBig, "Payload larger than maximum allowed payload.");
								return false;
							}

							Again = true;
						}
						break;

					case 10:    // Creating payload recipient
						Again = false;
						if (this.controlFrame)
						{
							this.payloadBak = this.payload;

							if (this.payloadLen > 0)
								this.payload = new MemoryStream(new byte[this.payloadLen]);
							else
								this.payload = null;
						}
						else if (this.payload is null)
						{
							if (!this.fin || this.payloadLen >= 65536)
								this.payload = new TemporaryStream();
							else if (this.payloadLen > 0)
								this.payload = new MemoryStream(new byte[this.payloadLen]);
						}

						this.payloadOffset = 0;

						if (this.masked)
							this.state++;
						else
						{
							this.state += 5;
							Again = true;
						}
						break;

					case 11:    // Receiving bit-mask
					case 12:
					case 13:
					case 14:
						this.mask[this.state - 11] = Data[Offset++];
						this.state++;
						break;

					case 15:    // Checking if there's any payload to receive in the frame.
						if (this.payloadLen == 0)
							this.state += 2;
						else
						{
							this.state++;
							Again = false;
						}
						break;

					case 16:    // Receiving payload
						int i = End - Offset;
						int j = this.payloadLen - this.payloadOffset;
						if (i > j)
							i = j;

						if (this.masked)
						{
							int i1 = Offset;
							int i2 = this.payloadOffset & 3;

							for (j = 0; j < i; j++)
							{
								Data[i1++] ^= this.mask[i2++];
								i2 &= 3;
							}
						}

						this.payload.Write(Data, Offset, i);

						Offset += i;
						this.payloadOffset += i;

						if (this.payloadOffset >= this.payloadLen)
						{
							this.state++;
							Again = true;
						}
						break;

					case 17:    // Process payload
						Again = false;
						if (this.controlFrame)
						{
							switch (this.controlOpCode)
							{
								case WebSocketOpcode.Close:
									if (!(this.payload is null))
										this.payload.Position = 0;

									ushort? Code = null;
									string Reason = null;

									if (this.payloadLen >= 2)
									{
										i = this.payload.ReadByte();
										i <<= 8;
										i |= this.payload.ReadByte();
										Code = (ushort)i;

										if (this.payloadLen > 2)
										{
											byte[] Bin = new byte[this.payloadLen - 2];
											this.payload.Read(Bin, 0, this.payloadLen - 2);
											Reason = Encoding.UTF8.GetString(Bin);
										}
									}

									this.RaiseClosed(Code, Reason);

									if (!this.closed)
									{
										if (Code.HasValue)
											await this.Close(Code.Value, Reason);
										else
											await this.Close();
									}

									return false;

								case WebSocketOpcode.Ping:
									await this.Pong();
									this.RaiseHeartbeat();
									break;

								case WebSocketOpcode.Pong:
									this.RaiseHeartbeat();
									break;
							}

							this.payload?.Dispose();
							this.payload = this.payloadBak;

							if (!(this.payload is null))
								this.state++;
							else
								this.state = 0;
						}
						else if (this.fin)
						{
							try
							{
								this.payload.Position = 0;

								i = (int)this.payload.Length;

								switch (this.opCode)
								{
									case WebSocketOpcode.Text:
										if (i > this.listener.MaximumTextPayloadSize)
											this.RaiseBinaryReceived(this.payload);
										else
										{
											byte[] Bin = new byte[i];
											this.payload.Read(Bin, 0, i);
											string Text = Encoding.UTF8.GetString(Bin);
											this.RaiseTextReceived(Text);
										}
										break;

									case WebSocketOpcode.Binary:
										this.RaiseBinaryReceived(this.payload);
										break;
								}
							}
							finally
							{
								this.state = 0;

								this.payload?.Dispose();
								this.payload = null;
							}
						}
						else
							this.state++;
						break;

					case 18:    // Receiving first byte of next fragment
						b = Data[Offset++];

						this.fin = (b & 128) != 0;
						this.rsv = (byte)(b & 0x70);
						this.controlFrame = (b & 8) != 0;

						if (this.controlFrame)
							this.controlOpCode = (WebSocketOpcode)(b & 15);

						this.state = 1;

						if (this.rsv != 0)
						{
							await this.Close(WebSocketCloseStatus.ProtocolError, "RSV must be 0.");
							return false;
						}
						break;
				}
			}

			return true;
		}
		/// <summary>
		/// Event raised when a remote party closed a connection.
		/// </summary>
		public event WebSocketClosedEventHandler Closed = null;

		private void RaiseClosed(ushort? Code, string Reason)
		{
			try
			{
				this.Closed?.Invoke(this, new WebSocketClosedEventArgs(this, Code, Reason));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when binary payload has been received.
		/// </summary>
		public event WebSocketBinaryEventHandler BinaryReceived = null;

		private void RaiseBinaryReceived(Stream Payload)
		{
			try
			{
				this.BinaryReceived?.Invoke(this, new WebSocketBinaryEventArgs(this, Payload));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when text payload has been received.
		/// </summary>
		public event WebSocketTextEventHandler TextReceived = null;

		internal void RaiseTextReceived(string Payload)
		{
			try
			{
				this.TextReceived?.Invoke(this, new WebSocketTextEventArgs(this, Payload));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task BeginWriteRaw(byte[] Frame, EventHandler Callback)
		{
			if (Frame is null)
				throw new ArgumentException("Frame cannot be null.", nameof(Frame));

			try
			{
				lock (this.queue)
				{
					if (this.writing)
					{
						this.queue.AddLast(new KeyValuePair<byte[], EventHandler>(Frame, Callback));
						return;
					}
					else
						this.writing = true;
				}

				while (!(Frame is null))
				{
					await this.httpResponse.WriteRawAsync(Frame);

					if (!(Callback is null))
					{
						try
						{
							Callback(this, new EventArgs());
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					lock (this.queue)
					{
						LinkedListNode<KeyValuePair<byte[], EventHandler>> Next;

						if ((Next = this.queue.First) is null)
						{
							this.writing = false;
							Frame = null;
						}
						else
						{
							Frame = Next.Value.Key;
							Callback = Next.Value.Value;
							this.queue.RemoveFirst();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				lock (this.queue)
				{
					this.writing = false;
					this.queue.Clear();
				}
			}
		}

		private readonly LinkedList<KeyValuePair<byte[], EventHandler>> queue = new LinkedList<KeyValuePair<byte[], EventHandler>>();
		private bool writing = false;

		/// <summary>
		/// Sends a text payload, possibly in multiple frames.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		/// <param name="MaxFrameLength">Maximum number of characters in each frame.</param>
		public void Send(string Payload, int MaxFrameLength)
		{
			int c = Payload.Length;

			if (c <= MaxFrameLength)
				this.Send(Payload, false);
			else
			{
				int i = 0;
				while (i < c)
				{
					int j = Math.Min(i + MaxFrameLength, c);
					string s = Payload.Substring(i, j - i);
					i = j;
					this.Send(s, i < c);
				}
			}
		}

		/// <summary>
		/// Sends a text payload.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		public Task Send(string Payload)
		{
			return this.Send(Payload, false, null);
		}

		/// <summary>
		/// Sends a text payload.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		/// <param name="More">If text is fragmented, and more is to come.</param>
		public Task Send(string Payload, bool More)
		{
			return this.Send(Payload, More, null);
		}

		/// <summary>
		/// Sends a text payload.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		/// <param name="More">If text is fragmented, and more is to come.</param>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		public async Task Send(string Payload, bool More, EventHandler Callback)
		{
			byte[] Frame = this.CreateFrame(Payload, More);
			await this.BeginWriteRaw(Frame, Callback);

			this.connection.Server.DataTransmitted(Frame.Length);
			this.connection.TransmitText(Payload);
		}

		/// <summary>
		/// Sends a text payload.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		public Task SendAsync(string Payload)
		{
			return this.SendAsync(Payload, false);
		}

		/// <summary>
		/// Sends a text payload.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		/// <param name="More">If text is fragmented, and more is to come.</param>
		public async Task SendAsync(string Payload, bool More)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			await this.Send(Payload, More, (sender, e) => Result.SetResult(true));
			await Result.Task;
		}

		/// <summary>
		/// Sends a binary payload, possibly in multiple frames.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		/// <param name="MaxFrameSize">Maximum number of bytes in each frame.</param>
		public void Send(byte[] Payload, int MaxFrameSize)
		{
			int c = Payload.Length;

			if (c <= MaxFrameSize)
				this.Send(Payload, false);
			else
			{
				int i = 0;
				while (i < c)
				{
					int j = Math.Min(i + MaxFrameSize, c);
					byte[] Bin = new byte[j - i];
					Array.Copy(Payload, i, Bin, 0, j - i);
					i = j;
					this.Send(Bin, i < c);
				}
			}
		}

		/// <summary>
		/// Sends a binary payload.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		public Task Send(byte[] Payload)
		{
			return this.Send(Payload, false, null);
		}

		/// <summary>
		/// Sends a binary payload.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		/// <param name="More">If the data is fragmented, and more is to come.</param>
		public Task Send(byte[] Payload, bool More)
		{
			return this.Send(Payload, More, null);
		}

		/// <summary>
		/// Sends a binary payload.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		/// <param name="More">If the data is fragmented, and more is to come.</param>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		public async Task Send(byte[] Payload, bool More, EventHandler Callback)
		{
			byte[] Frame = this.CreateFrame(Payload, More);
			await this.BeginWriteRaw(Frame, Callback);

			this.connection.Server.DataTransmitted(Frame.Length);
			this.connection.TransmitBinary(Payload);
		}

		/// <summary>
		/// Sends a binary payload.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		public Task SendAsync(byte[] Payload)
		{
			return this.SendAsync(Payload, false);
		}

		/// <summary>
		/// Sends a binary payload.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		/// <param name="More">If the data is fragmented, and more is to come.</param>
		public async Task SendAsync(byte[] Payload, bool More)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			await this.Send(Payload, More, (sender, e) => Result.SetResult(true));
			await Result.Task;
		}

		private byte[] CreateFrame(string Payload, bool More)
		{
			return this.CreateFrame(Encoding.UTF8.GetBytes(Payload), WebSocketOpcode.Text, More);
		}

		private byte[] CreateFrame(byte[] Payload, bool More)
		{
			return this.CreateFrame(Payload, WebSocketOpcode.Binary, More);
		}

		private byte[] CreateFrame(byte[] Bin, WebSocketOpcode OpCode, bool More)
		{
			int Len = Bin?.Length ?? 0;
			int c;

			c = Len + 2;
			if (Len >= 126)
			{
				c += 2;
				if (Len >= 65536)
					c += 6;
			}

			byte[] Packet = new byte[c];
			int i, j;
			bool ControlFrame = (int)OpCode >= 8;

			if (this.writingFragments && !ControlFrame)
				Packet[0] = 0;
			else
				Packet[0] = (byte)OpCode;

			if (More)
			{
				if (!ControlFrame)
					this.writingFragments = true;
			}
			else
			{
				Packet[0] |= 0x80;

				if (!ControlFrame)
					this.writingFragments = false;
			}

			if (Len < 126)
			{
				Packet[1] = (byte)Len;
				i = 2;
			}
			else if (Len < 65536)
			{
				Packet[1] = 126;
				Packet[2] = (byte)(Len >> 8);
				Packet[3] = (byte)Len;
				i = 4;
			}
			else
			{
				Packet[1] = 127;

				i = Len;

				for (j = 0; j < 4; j++)
				{
					Packet[5 - j] = (byte)i;
					i >>= 8;
				}

				i = 6;
				for (j = 0; j < 4; j++)
					Packet[i++] = 0;
			}

			if (Len > 0)
				Array.Copy(Bin, 0, Packet, i, Len);

			return Packet;
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public Task Close()
		{
			return this.Close(null);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		public async Task Close(EventHandler Callback)
		{
			this.closed = true;

			byte[] Frame = this.CreateFrame(null, WebSocketOpcode.Close, false);
			await this.BeginWriteRaw(Frame, Callback);

			this.connection.Server.DataTransmitted(Frame.Length);
			this.connection.TransmitBinary(Frame);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		public Task Close(WebSocketCloseStatus Code, string Reason)
		{
			return this.Close((ushort)Code, Reason, null);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		public Task Close(ushort Code, string Reason)
		{
			return this.Close(Code, Reason, null);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		public Task Close(WebSocketCloseStatus Code, string Reason, EventHandler Callback)
		{
			return this.Close((ushort)Code, Reason, Callback);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		public async Task Close(ushort Code, string Reason, EventHandler Callback)
		{
			this.closed = true;

			byte[] Frame = this.Encode(Code, Reason);
			await this.BeginWriteRaw(Frame, Callback);

			this.connection.Server.DataTransmitted(Frame.Length);
			this.connection.TransmitBinary(Frame);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public async Task CloseAsync()
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			await this.Close((sender, e) => Result.SetResult(true));
			await Result.Task;
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		public Task CloseAsync(WebSocketCloseStatus Code, string Reason)
		{
			return this.CloseAsync((ushort)Code, Reason);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		public async Task CloseAsync(ushort Code, string Reason)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			await this.Close(Code, Reason, (sender, e) => Result.SetResult(true));
			await Result.Task;
		}

		private byte[] Encode(ushort Code, string Reason)
		{
			byte[] Bin = Encoding.UTF8.GetBytes(Reason ?? string.Empty);
			int c = Bin.Length;
			byte[] Payload = new byte[c + 2];

			Payload[0] = (byte)(Code >> 8);
			Payload[1] = (byte)Code;
			Array.Copy(Bin, 0, Payload, 2, c);

			return this.CreateFrame(Payload, WebSocketOpcode.Close, false);
		}

		private async Task Pong()
		{
			byte[] Frame;
			byte[] Bin;

			if (this.payload is null)
				Bin = null;
			else
			{
				int c = (int)this.payload.Length;
				Bin = new byte[c];
				this.payload.Position = 0;
				this.payload.Read(Bin, 0, c);
			}

			Frame = this.CreateFrame(Bin, WebSocketOpcode.Pong, false);

			await this.BeginWriteRaw(Frame, null);

			this.connection.Server.DataTransmitted(Frame.Length);
			this.connection.TransmitBinary(Frame);
		}

		/// <summary>
		/// Checks if the connection is live.
		/// </summary>
		/// <returns>If the connection is still live.</returns>
		public bool CheckLive()
		{
			return this.connection?.CheckLive() ?? false;
		}

		/// <summary>
		/// Event raised when a client heart-beat has been received.
		/// </summary>
		public event WebSocketEventHandler Heartbeat = null;

		internal void RaiseHeartbeat()
		{
			try
			{
				this.Heartbeat?.Invoke(this, new WebSocketEventArgs(this));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

	}
}
