using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.HTTP.HTTP2;
using Waher.Runtime.Collections;
using Waher.Runtime.IO;
using Waher.Runtime.Temporary;

namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Class handling a web-socket.
	/// </summary>
	public class WebSocket : IDisposableAsync
	{
		private object tag = null;

		private readonly WebSocketListener listener;
		private readonly HttpRequest httpRequest;
		private readonly HttpResponse httpResponse;
		private readonly Http2Stream http2Stream;
		private readonly byte[] mask = new byte[4];
		private readonly bool http2;
		private HttpClientConnection connection;
		private WebSocketOpcode opCode;
		private WebSocketOpcode controlOpCode;
		private Stream payload = null;
		private Stream payloadBak = null;
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
			this.http2Stream = null;
			this.connection = Request.clientConnection;
			this.http2 = false;
		}

		internal WebSocket(WebSocketListener WebSocketListener, Http2Stream Stream,
			HttpRequest Request, HttpResponse Response)
		{
			this.listener = WebSocketListener;
			this.httpRequest = Request;
			this.httpResponse = Response;
			this.http2Stream = Stream;
			this.connection = Stream.Connection;
			this.http2 = true;
		}

		/// <summary>
		/// Original HTTP request made to upgrade the connection to a WebSocket connection.
		/// </summary>
		public HttpRequest HttpRequest => this.httpRequest;

		/// <summary>
		/// Original HTTP response used when connection was upgrades to a WebSocket connection.
		/// </summary>
		public HttpResponse HttpResponse => this.httpResponse;

		/// <summary>
		/// HTTP/2 stream upgraded to a WebSocket.
		/// </summary>
		public Http2Stream HttpStream => this.http2Stream;

		/// <summary>
		/// If Web Socket is streamed over an HTTP/2 stream.
		/// </summary>
		public bool Http2 => this.http2;

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
		[Obsolete("Use the DisposeAsync() method.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		public async Task DisposeAsync()
		{
			this.payload?.Dispose();
			this.payload = null;

			if (!(this.connection is null))
			{
				await this.connection.DisposeAsync();
				this.connection = null;
			}

			await this.Disposed.Raise(this, EventArgs.Empty, false);
		}

		/// <summary>
		/// Event raised when socket has been disposed.
		/// </summary>
		public event EventHandlerAsync Disposed = null;

		internal async Task<bool> WebSocketDataReceived(bool ConstantBuffer, byte[] Data, int Offset, int NrRead)
		{
			if (this.connection?.HasSniffers ?? false)
				this.connection?.ReceiveBinary(ConstantBuffer, Data, Offset, NrRead);

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
						this.payloadLen = b & 127;

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
											await this.payload.ReadAllAsync(Bin);
											Reason = Encoding.UTF8.GetString(Bin);
										}
									}

									await this.RaiseClosed(Code, Reason);

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
									await this.RaiseHeartbeat();
									break;

								case WebSocketOpcode.Pong:
									await this.RaiseHeartbeat();
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
											await this.RaiseBinaryReceived(this.payload);
										else
										{
											byte[] Bin = new byte[i];
											await this.payload.ReadAllAsync(Bin, 0, i);
											string Text = Encoding.UTF8.GetString(Bin);
											await this.RaiseTextReceived(Text);
										}
										break;

									case WebSocketOpcode.Binary:
										await this.RaiseBinaryReceived(this.payload);
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
		public event EventHandlerAsync<WebSocketClosedEventArgs> Closed = null;

		private Task RaiseClosed(ushort? Code, string Reason)
		{
			return this.Closed.Raise(this, new WebSocketClosedEventArgs(this, Code, Reason), false);
		}

		/// <summary>
		/// Event raised when binary payload has been received.
		/// </summary>
		public event EventHandlerAsync<WebSocketBinaryEventArgs> BinaryReceived = null;

		private Task RaiseBinaryReceived(Stream Payload)
		{
			return this.BinaryReceived.Raise(this, new WebSocketBinaryEventArgs(this, Payload));
		}

		/// <summary>
		/// Event raised when text payload has been received.
		/// </summary>
		public event EventHandlerAsync<WebSocketTextEventArgs> TextReceived = null;

		internal Task RaiseTextReceived(string Payload)
		{
			return this.TextReceived.Raise(this, new WebSocketTextEventArgs(this, Payload));
		}

		private async Task BeginWriteRaw(bool ConstantBuffer, byte[] Frame, bool Last, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (Frame is null)
				throw new ArgumentException("Frame cannot be null.", nameof(Frame));

			try
			{
				lock (this.queue)
				{
					if (this.writing)
					{
						this.queue.Add(new OutputRec()
						{
							Data = Frame,
							Callback = Callback,
							State = State
						});

						return;
					}
					else
						this.writing = true;
				}

				while (!(Frame is null))
				{
					if (this.http2)
					{
						if (!await this.http2Stream.TryWriteAllData(ConstantBuffer, Frame, 0, Frame.Length, Last, null))
						{
							lock (this.queue)
							{
								this.writing = false;
								this.queue.Clear();
							}

							try
							{
								await this.DisposeAsync();
							}
							catch (Exception ex2)
							{
								Log.Exception(ex2);
							}
							return;
						}
					}
					else
						await this.httpResponse.WriteRawAsync(ConstantBuffer, Frame);

					if (!(Callback is null))
						await Callback.Raise(this, new DeliveryEventArgs(State, true));

					lock (this.queue)
					{
						if (this.queue.HasFirstItem)
						{
							OutputRec Next = this.queue.RemoveFirst();
							Frame = Next.Data;
							Callback = Next.Callback;
							State = Next.State;
						}
						else
						{
							this.writing = false;
							Frame = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				lock (this.queue)
				{
					this.writing = false;
					this.queue.Clear();
				}

				try
				{
					await this.DisposeAsync();
				}
				catch (Exception ex2)
				{
					Log.Exception(ex2);
				}
			}
		}

		private readonly ChunkedList<OutputRec> queue = new ChunkedList<OutputRec>();
		private bool writing = false;

		private class OutputRec
		{
			public byte[] Data;
			public EventHandlerAsync<DeliveryEventArgs> Callback;
			public object State;
		}

		/// <summary>
		/// Sends a text payload, possibly in multiple frames.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		/// <param name="MaxFrameLength">Maximum number of characters in each frame.</param>
		public async Task Send(string Payload, int MaxFrameLength)
		{
			int c = Payload.Length;

			if (c <= MaxFrameLength)
				await this.Send(Payload, false);
			else
			{
				int i = 0;
				while (i < c)
				{
					int j = Math.Min(i + MaxFrameLength, c);
					string s = Payload.Substring(i, j - i);
					i = j;
					await this.Send(s, i < c);
				}
			}
		}

		/// <summary>
		/// Sends a text payload.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		public Task Send(string Payload)
		{
			return this.Send(Payload, false, null, null);
		}

		/// <summary>
		/// Sends a text payload.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		/// <param name="More">If text is fragmented, and more is to come.</param>
		public Task Send(string Payload, bool More)
		{
			return this.Send(Payload, More, null, null);
		}

		/// <summary>
		/// Sends a text payload.
		/// </summary>
		/// <param name="Payload">Text to send.</param>
		/// <param name="More">If text is fragmented, and more is to come.</param>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task Send(string Payload, bool More, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			byte[] Frame = this.CreateFrame(Payload, More);
			await this.BeginWriteRaw(true, Frame, false, Callback, State);

			this.connection?.Server?.DataTransmitted(Frame.Length);
			this.connection?.TransmitText(Payload);
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
			await this.Send(Payload, More, (Sender, e) =>
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			}, null);
			await Result.Task;
		}

		/// <summary>
		/// Sends a binary payload, possibly in multiple frames.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		/// <param name="MaxFrameSize">Maximum number of bytes in each frame.</param>
		public async Task Send(byte[] Payload, int MaxFrameSize)
		{
			int c = Payload.Length;

			if (c <= MaxFrameSize)
				await this.Send(Payload, false);
			else
			{
				int i = 0;
				while (i < c)
				{
					int j = Math.Min(i + MaxFrameSize, c);
					byte[] Bin = new byte[j - i];
					Array.Copy(Payload, i, Bin, 0, j - i);
					i = j;
					await this.Send(Bin, i < c);
				}
			}
		}

		/// <summary>
		/// Sends a binary payload.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		public Task Send(byte[] Payload)
		{
			return this.Send(Payload, false, null, null);
		}

		/// <summary>
		/// Sends a binary payload.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		/// <param name="More">If the data is fragmented, and more is to come.</param>
		public Task Send(byte[] Payload, bool More)
		{
			return this.Send(Payload, More, null, null);
		}

		/// <summary>
		/// Sends a binary payload.
		/// </summary>
		/// <param name="Payload">Data to send.</param>
		/// <param name="More">If the data is fragmented, and more is to come.</param>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task Send(byte[] Payload, bool More, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			byte[] Frame = this.CreateFrame(Payload, More);
			await this.BeginWriteRaw(true, Frame, false, Callback, State);

			this.connection?.Server?.DataTransmitted(Frame.Length);
			this.connection?.TransmitBinary(true, Frame);
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
			await this.Send(Payload, More, (Sender, e) =>
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			}, null);
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
			if (this.connection?.HasSniffers ?? false)
				this.connection.Information(OpCode.ToString());

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
			return this.Close(null, null);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task Close(EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			this.closed = true;

			byte[] Frame = this.CreateFrame(null, WebSocketOpcode.Close, false);
			await this.BeginWriteRaw(true, Frame, true, Callback, State);

			this.connection?.Server?.DataTransmitted(Frame.Length);
			this.connection?.TransmitBinary(true, Frame);

			if (!(this.http2Stream is null))
				this.connection.FlowControl?.RemoveStream(this.http2Stream);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		public Task Close(WebSocketCloseStatus Code, string Reason)
		{
			if (this.connection.HasSniffers)
				this.LogError(Code, Reason);

			return this.Close((ushort)Code, Reason, null, null);
		}

		private void LogError(WebSocketCloseStatus Code, string Reason)
		{
			switch (Code)
			{
				case WebSocketCloseStatus.Normal:
				case WebSocketCloseStatus.GoingAway:
					this.connection.Information("WebSocket closed (" + Code.ToString() + "): " + Reason);
					break;

				case WebSocketCloseStatus.ProtocolError:
				case WebSocketCloseStatus.NotAcceptable:
				case WebSocketCloseStatus.NotConsistent:
				case WebSocketCloseStatus.PolicyViolation:
				case WebSocketCloseStatus.TooBig:
				case WebSocketCloseStatus.MissingExtension:
				case WebSocketCloseStatus.UnexpectedCondition:
				default:
					this.connection.Error("WebSocket closed (" + Code.ToString() + "): " + Reason);
					break;
			}
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		public Task Close(ushort Code, string Reason)
		{
			return this.Close(Code, Reason, null, null);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Close(WebSocketCloseStatus Code, string Reason, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (this.connection.HasSniffers)
				this.LogError(Code, Reason);

			return this.Close((ushort)Code, Reason, Callback, State);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		/// <param name="Callback">Method to call when callback has been sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task Close(ushort Code, string Reason, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			this.closed = true;

			byte[] Frame = this.Encode(Code, Reason);
			await this.BeginWriteRaw(true, Frame, true, Callback, State);

			this.connection?.Server?.DataTransmitted(Frame.Length);
			this.connection?.TransmitBinary(true, Frame);

			if (!(this.http2Stream is null))
				this.connection.FlowControl?.RemoveStream(this.http2Stream);
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public async Task CloseAsync()
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			await this.Close((Sender, e) =>
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			}, null);
			await Result.Task;
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="Code">Code</param>
		/// <param name="Reason">Reason</param>
		public Task CloseAsync(WebSocketCloseStatus Code, string Reason)
		{
			if (this.connection.HasSniffers)
				this.LogError(Code, Reason);

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
			await this.Close(Code, Reason, (Sender, e) =>
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			}, null);
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
				this.payload.ReadAll(Bin, 0, c);
			}

			Frame = this.CreateFrame(Bin, WebSocketOpcode.Pong, false);

			await this.BeginWriteRaw(true, Frame, false, null, null);

			this.connection?.Server?.DataTransmitted(Frame.Length);
			this.connection?.TransmitBinary(true, Frame);
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
		public event EventHandlerAsync<WebSocketEventArgs> Heartbeat = null;

		internal Task RaiseHeartbeat()
		{
			return this.Heartbeat.Raise(this, new WebSocketEventArgs(this));
		}

	}
}
