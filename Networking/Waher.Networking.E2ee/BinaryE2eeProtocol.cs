using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Runtime.Collections;
using Waher.Security;
using Waher.Security.E2EE;
using Waher.Security.EllipticCurves.E2EE;
using Waher.Security.PQC.E2EE;

namespace Waher.Networking.E2ee
{
	/// <summary>
	/// Binary End-to-End encrypted protocol.
	/// </summary>
	public class BinaryE2eeProtocol : CommunicationLayer, IBinaryTransportLayer
	{
		private static readonly Random rnd = new Random();

		private readonly TaskCompletionSource<bool> remoteKeysReceived = new TaskCompletionSource<bool>();
		private readonly TaskCompletionSource<bool> ciphersSelected = new TaskCompletionSource<bool>();
		private readonly IBinaryTransportLayer binaryTransport;
		private readonly IE2eSymmetricCipher[] symmetricCiphers;
		private readonly IE2eEndpoint[] endpoints;
		private readonly bool initiator;
		private readonly bool signedTransfers;
		private Dictionary<string, IE2eSymmetricCipher> remoteSymmetricCiphers;
		private Dictionary<string, IE2eEndpoint> remoteEndpoints;
		private IE2eEndpoint selectedEndpoint;
		private IE2eSymmetricCipher selectedSymmetricCipher;
		private bool disposed = false;
		private bool hasSymmetricKey;
		private byte[] symmetricKey;
		private byte[] inputBlock = null;
		private int inputState = 0;
		private int inputBlockLen = 0;
		private int inputBlockPos;

		/// <summary>
		/// Binary End-to-End encrypted protocol.
		/// </summary>
		/// <param name="BinaryTransport">Binary transport layer.</param>
		/// <param name="Initiator">Initiator of the conversation, typically the
		/// part that initiates a connection or conversation.</param>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="SignedTransfers">If all transfers must be signed.</param>
		/// <param name="DecoupledEvents">If events raised from the communication layer 
		/// are decoupled, i.e. executed in parallel with the source that raised them.</param>
		/// <param name="Sniffers">Optional sniffers.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			bool SignedTransfers, bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator,
				  DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength,
				  new Type[] { typeof(EllipticCurveEndpoint), typeof(ModuleLatticeEndpoint) },
				  SignedTransfers, DecoupledEvents, Sniffers)
		{
		}


		/// <summary>
		/// Binary End-to-End encrypted protocol.
		/// </summary>
		/// <param name="BinaryTransport">Binary transport layer.</param>
		/// <param name="Initiator">Initiator of the conversation, typically the
		/// part that initiates a connection or conversation.</param>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="OnlyIfDerivedFrom">Only return endpoints derived from these types.</param>
		/// <param name="DecoupledEvents">If events raised from the communication layer 
		/// are decoupled, i.e. executed in parallel with the source that raised them.</param>
		/// <param name="SignedTransfers">If all transfers must be signed.</param>
		/// <param name="Sniffers">Optional sniffers.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			Type[] OnlyIfDerivedFrom, bool SignedTransfers, bool DecoupledEvents,
			params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator, E2eEndpoint.CreateEndpoints(
				DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength,
				OnlyIfDerivedFrom), SignedTransfers, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// Binary End-to-End encrypted protocol.
		/// </summary>
		/// <param name="BinaryTransport">Binary transport layer.</param>
		/// <param name="Initiator">Initiator of the conversation, typically the
		/// part that initiates a connection or conversation.</param>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="OnlyIfDerivedFrom">Only return endpoints derived from these types.</param>
		/// <param name="DecoupledEvents">If events raised from the communication layer 
		/// are decoupled, i.e. executed in parallel with the source that raised them.</param>
		/// <param name="SignedTransfers">If all transfers must be signed.</param>
		/// <param name="Sniffers">Optional sniffers.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			IE2eEndpoint[] Endpoints, bool SignedTransfers, bool DecoupledEvents,
			params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator, Endpoints, E2eEndpoint.CreateSymmetricCiphers(),
				  SignedTransfers, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// Binary End-to-End encrypted protocol.
		/// </summary>
		/// <param name="BinaryTransport">Binary transport layer.</param>
		/// <param name="Initiator">Initiator of the conversation, typically the
		/// part that initiates a connection or conversation.</param>
		/// <param name="Endpoints">Asymmetric ciphers.</param>
		/// <param name="SymmetricCiphers">Symmetric ciphers.</param>
		/// <param name="DecoupledEvents">If events raised from the communication layer 
		/// are decoupled, i.e. executed in parallel with the source that raised them.</param>
		/// <param name="SignedTransfers">If all transfers must be signed.</param>
		/// <param name="Sniffers">Optional sniffers.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			IE2eEndpoint[] Endpoints, IE2eSymmetricCipher[] SymmetricCiphers,
			bool SignedTransfers, bool DecoupledEvents, params ISniffer[] Sniffers)
			: base(DecoupledEvents, Sniffers)
		{
			if (Endpoints.Length == 0)
			{
				throw new ArgumentException("No endpoints could be created with the " +
					"specified security strength.", nameof(Endpoints));
			}

			if (SignedTransfers)
			{
				ChunkedList<IE2eEndpoint> Filtered = new ChunkedList<IE2eEndpoint>();

				foreach (IE2eEndpoint Endpoint in Endpoints)
				{
					if (Endpoint.SupportsSignatures)
						Filtered.Add(Endpoint);
				}

				Endpoints = Filtered.ToArray();
			}

			this.endpoints = Endpoints;

			this.symmetricCiphers = SymmetricCiphers;
			if (this.symmetricCiphers.Length == 0)
			{
				throw new ArgumentException("No symmetric ciphers available.",
					nameof(SymmetricCiphers));
			}

			this.binaryTransport = BinaryTransport;
			this.initiator = Initiator;
			this.signedTransfers = SignedTransfers;

			this.binaryTransport.OnReceived += this.BinaryTransport_OnReceived;
		}

		#region IDisposable

		/// <summary>
		/// Disposes the object and underlying transport layer.
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.binaryTransport.Dispose();
			}
		}

		#endregion

		/// <summary>
		/// Negotiates the keys to use during encrypted communication.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>If key negotiation concluded successfully.</returns>
		public async Task<bool> NegotiateKeys(int Timeout)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(BinaryE2eeProtocol));

			if (this.hasSymmetricKey)
				throw new InvalidOperationException("Keys already negotiated.");

			_ = Task.Delay(Timeout).ContinueWith(_ =>
			{
				this.remoteKeysReceived.TrySetException(new TimeoutException());
				this.ciphersSelected.TrySetException(new TimeoutException());
			});

			if (!this.initiator)
			{
				if (!await this.remoteKeysReceived.Task)
					return false;
			}

			if (!await this.SendCiphers())
				return false;

			if (this.initiator)
			{
				if (!await this.remoteKeysReceived.Task)
					return false;

				if (!await this.SelectCipher())
					return false;
			}

			if (!await this.ciphersSelected.Task)
				return false;

			return true;
		}

		private static string Key(IE2eSymmetricCipher Cipher)
		{
			return Key(Cipher.LocalName, Cipher.Namespace);
		}

		private static string Key(IE2eEndpoint Endpoint)
		{
			return Key(Endpoint.LocalName, Endpoint.Namespace);
		}

		private static string Key(string LocalName, string Namespace)
		{
			return Namespace + "#" + LocalName;
		}

		private async Task<bool> SelectCipher()
		{
			if (!this.initiator)
				return false;

			IE2eEndpoint BestEndpoint = null;
			IE2eEndpoint BestRemoteEndpoint = null;

			foreach (IE2eEndpoint Endpoint in this.endpoints)
			{
				if (!this.remoteEndpoints.TryGetValue(Key(Endpoint), out IE2eEndpoint RemoteEndpoint))
					continue;

				if (BestEndpoint is null)
				{
					BestEndpoint = Endpoint;
					BestRemoteEndpoint = RemoteEndpoint;
					continue;
				}

				if (BestEndpoint.Safe && !Endpoint.Safe)
					continue;

				if (!BestEndpoint.Safe && Endpoint.Safe)
				{
					BestEndpoint = Endpoint;
					BestRemoteEndpoint = RemoteEndpoint;
					continue;
				}

				// Safe are the same here

				if (Endpoint.Score > BestEndpoint.Score)
				{
					BestEndpoint = Endpoint;
					BestRemoteEndpoint = RemoteEndpoint;
					continue;
				}

				if (Endpoint.Score < BestEndpoint.Score)
					continue;

				// Score are the same here

				if (BestEndpoint.Slow && !Endpoint.Slow)
				{
					BestEndpoint = Endpoint;
					BestRemoteEndpoint = RemoteEndpoint;
					continue;
				}
			}

			if (BestEndpoint is null || BestRemoteEndpoint is null)
			{
				this.ciphersSelected.TrySetResult(false);
				this.Error("No common asymmetric ciphers found.");
				return false;
			}

			BinaryOutput CipherSelection = new BinaryOutput();

			CipherSelection.WriteString(BestEndpoint.LocalName);
			CipherSelection.WriteString(BestEndpoint.Namespace);
			CipherSelection.WriteData(BestEndpoint.PublicKey);

			if (this.HasSniffers)
			{
				this.Information("Asymmetric cipher selected: " + Key(BestEndpoint) + ": " +
					BestEndpoint.PublicKeyBase64);
			}

			ChunkedList<IE2eSymmetricCipher> CommonCiphers = new ChunkedList<IE2eSymmetricCipher>();
			IE2eSymmetricCipher SelectedCipher = null;

			foreach (IE2eSymmetricCipher Cipher in this.symmetricCiphers)
			{
				if (this.remoteSymmetricCiphers.ContainsKey(Key(Cipher)))
					CommonCiphers.Add(Cipher);
			}

			int c = CommonCiphers.Count;
			if (c == 0)
			{
				this.ciphersSelected.TrySetResult(false);
				this.Error("No common symmetric ciphers found.");
				return false;
			}

			lock (rnd)
			{
				int i = rnd.Next(c);
				SelectedCipher = CommonCiphers[i];
			}

			CipherSelection.WriteString(SelectedCipher.LocalName);
			CipherSelection.WriteString(SelectedCipher.Namespace);

			if (this.HasSniffers)
				this.Information("Symmetric cipher selected: " + Key(SelectedCipher));

			this.symmetricKey = BestEndpoint.GetSharedSecretForEncryption(
				BestRemoteEndpoint, SelectedCipher, out byte[] CipherText);

			this.hasSymmetricKey = true;
			this.selectedEndpoint = BestEndpoint;
			this.selectedSymmetricCipher = SelectedCipher;

			CipherSelection.WriteData(CipherText ?? Array.Empty<byte>());

			bool Result = await this.SendAsync(true, CipherSelection.ToArray());

			this.ciphersSelected.TrySetResult(Result);

			return Result;
		}

		private async Task<bool> SendCiphers()
		{
			ICollection<IE2eEndpoint> Endpoints;
			ICollection<IE2eSymmetricCipher> Ciphers;

			if (this.remoteEndpoints is null)
				Endpoints = this.endpoints;
			else
			{
				ChunkedList<IE2eEndpoint> Filtered = new ChunkedList<IE2eEndpoint>();

				foreach (IE2eEndpoint Endpoint in this.endpoints)
				{
					if (this.remoteEndpoints.ContainsKey(Key(Endpoint)))
						Filtered.Add(Endpoint);
				}

				Endpoints = Filtered;
			}

			if (Endpoints.Count == 0)
				return false;

			if (this.remoteSymmetricCiphers is null)
				Ciphers = this.symmetricCiphers;
			else
			{
				ChunkedList<IE2eSymmetricCipher> Filtered = new ChunkedList<IE2eSymmetricCipher>();

				foreach (IE2eSymmetricCipher Cipher in this.symmetricCiphers)
				{
					if (this.remoteSymmetricCiphers.ContainsKey(Key(Cipher)))
						Filtered.Add(Cipher);
				}

				Ciphers = Filtered;
			}

			if (Ciphers.Count == 0)
				return false;

			BinaryOutput Output = new BinaryOutput();
			StringBuilder sb = this.HasSniffers ? new StringBuilder() : null;

			Output.WriteVarLenUInt((uint)Endpoints.Count);
			Output.WriteVarLenUInt((uint)Ciphers.Count);

			foreach (IE2eEndpoint Endpoint in Endpoints)
			{
				Output.WriteString(Endpoint.LocalName);
				Output.WriteString(Endpoint.Namespace);

				if (!(sb is null))
				{
					sb.Append(Endpoint.Namespace);
					sb.Append('#');
					sb.AppendLine(Endpoint.LocalName);
				}
			}

			foreach (IE2eSymmetricCipher Cipher in Ciphers)
			{
				Output.WriteString(Cipher.LocalName);
				Output.WriteString(Cipher.Namespace);

				if (!(sb is null))
				{
					sb.Append(Cipher.Namespace);
					sb.Append('#');
					sb.AppendLine(Cipher.LocalName);
				}
			}

			byte[] CipherInfo = Output.ToArray();

			Output = new BinaryOutput();
			Output.WriteData(CipherInfo);

			byte[] Block = Output.ToArray();

			if (!(sb is null))
				this.TransmitText(sb.ToString());

			return await this.binaryTransport.SendAsync(true, Block);
		}

		#region IBinaryTransmission

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet)
		{
			if (!this.hasSymmetricKey)
				throw new InvalidOperationException("Keys not negotiated.");

			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (!this.hasSymmetricKey)
				throw new InvalidOperationException("Keys not negotiated.");

			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte written.</param>
		/// <param name="Count">Number of bytes written.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			if (!this.hasSymmetricKey)
				throw new InvalidOperationException("Keys not negotiated.");

			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, int Count, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (!this.hasSymmetricKey)
				throw new InvalidOperationException("Keys not negotiated.");

			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Flushes any pending or intermediate data.
		/// </summary>
		/// <returns>If output has been flushed.</returns>
		public Task<bool> FlushAsync()
		{
			if (!this.hasSymmetricKey)
				throw new InvalidOperationException("Keys not negotiated.");

			throw new NotImplementedException();    // TODO
		}

		#endregion

		#region IBinaryTransportLayer

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event BinaryDataWrittenEventHandler OnSent;

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		public event BinaryDataReadEventHandler OnReceived;

		private async Task<bool> BinaryTransport_OnReceived(object Sender, bool ConstantBuffer,
			byte[] Buffer, int Offset, int Count)
		{
			try
			{
				while (Count > 0)
				{
					switch (this.inputState)
					{
						case 0:
						case 2:
						case 4:
							byte b = Buffer[Offset++];
							Count--;

							this.inputBlockLen <<= 7;
							this.inputBlockLen |= b & 0x7f;
							if ((b & 0x80) == 0)
							{
								this.inputBlock = new byte[this.inputBlockLen];

								if (this.inputBlockLen > 0)
								{
									this.inputBlockPos = 0;
									this.inputState++;
								}
							}
							break;

						case 1:
						case 3:
						case 5:
							int c = Math.Min(Count, this.inputBlockLen - this.inputBlockPos);
							System.Buffer.BlockCopy(Buffer, Offset, this.inputBlock, this.inputBlockPos, c);
							Offset += c;
							Count -= c;

							if (this.inputBlockLen == this.inputBlockPos)
							{
								switch (this.inputState)
								{
									case 1:
										if (this.ProcessRemoteCiphers(this.inputBlock))
										{
											if (this.initiator)
												this.inputState += 3;
											else
												this.inputState++;
										}
										else
											this.inputState = -1;
										break;

									case 3:
										if (this.CipherSelected(this.inputBlock))
										{
											this.ciphersSelected.TrySetResult(true);
											this.inputState++;
										}
										else
										{
											this.ciphersSelected.TrySetResult(false);
											this.inputState = -1;
										}
										break;

									case 5:
										await this.ProcessEncryptedBlock(this.inputBlock);
										this.inputState--;
										break;
								}

								this.inputBlockLen = 0;
							}
							break;

						default:
							this.ReceiveBinary(Count);
							break;
					}
				}
			}
			catch (Exception ex)
			{
				this.Exception(ex);
			}

			return true;
		}

		private bool ProcessRemoteCiphers(byte[] Data)
		{
			BinaryInput Input = new BinaryInput(Data);
			StringBuilder sb = this.HasSniffers ? new StringBuilder() : null;

			this.remoteEndpoints = new Dictionary<string, IE2eEndpoint>();
			this.remoteSymmetricCiphers = new Dictionary<string, IE2eSymmetricCipher>();

			string LocalName, Namespace;
			int NrEndpoints = (int)Input.ReadVarLenUInt();
			int NrSymmetricCiphers = (int)Input.ReadVarLenUInt();
			int i;

			for (i = 0; i < NrEndpoints; i++)
			{
				LocalName = Input.ReadString();
				Namespace = Input.ReadString();

				if (!(sb is null))
				{
					sb.Append(Namespace);
					sb.Append('#');
					sb.AppendLine(LocalName);
				}

				if (E2eEndpoint.TryCreateEndpoint(LocalName, Namespace, out IE2eEndpoint Endpoint))
					this.remoteEndpoints[Key(Endpoint)] = Endpoint;
			}

			for (i = 0; i < NrSymmetricCiphers; i++)
			{
				LocalName = Input.ReadString();
				Namespace = Input.ReadString();

				if (!(sb is null))
				{
					sb.Append(Namespace);
					sb.Append('#');
					sb.AppendLine(LocalName);
				}

				if (E2eEndpoint.TryGetSymmetricCipher(LocalName, Namespace, out IE2eSymmetricCipher Cipher))
					this.remoteSymmetricCiphers[Key(Cipher)] = Cipher;
			}

			if (!(sb is null))
				this.ReceiveText(sb.ToString());

			bool Result = this.remoteEndpoints.Count > 0 &&
				this.remoteSymmetricCiphers.Count > 0;

			if (!Result)
				this.Error("Missing algorithms.");

			this.remoteKeysReceived.TrySetResult(Result);

			return Result;
		}

		private bool CipherSelected(byte[] Data)
		{
			BinaryInput Input = new BinaryInput(Data);

			string LocalName = Input.ReadString();
			string Namespace = Input.ReadString();
			byte[] PublicKey = Input.ReadData();

			if (this.remoteEndpoints.TryGetValue(Key(LocalName, Namespace),
				out IE2eEndpoint Endpoint))
			{
				if (this.HasSniffers)
				{
					this.Information("Asymmetric cipher selected: " + Key(Endpoint) + ": " +
						Endpoint.PublicKeyBase64);
				}
			}
			else
			{
				if (this.HasSniffers)
				{
					this.Error("Unable to select asymmetric cipher: " +
						Key(LocalName, Namespace) + ": " + Convert.ToBase64String(PublicKey));
				}

				this.ciphersSelected.TrySetResult(false);
				return false;
			}

			LocalName = Input.ReadString();
			Namespace = Input.ReadString();

			if (this.remoteSymmetricCiphers.TryGetValue(Key(LocalName, Namespace),
				out IE2eSymmetricCipher Cipher))
			{
				if (this.HasSniffers)
					this.Information("Symmetric cipher selected: " + Key(Cipher));
			}
			else
			{
				if (this.HasSniffers)
					this.Error("Unable to select symmetric cipher: " + Key(LocalName, Namespace));

				this.ciphersSelected.TrySetResult(false);
				return false;
			}

			byte[] CipherText = Input.ReadData();

			this.symmetricKey = Endpoint.GetSharedSecretForDecryption(Endpoint, CipherText);
			
			this.hasSymmetricKey = true;
			this.selectedEndpoint = Endpoint;
			this.selectedSymmetricCipher = Cipher;

			this.ciphersSelected.TrySetResult(true);

			return true;
		}

		private async Task ProcessEncryptedBlock(byte[] Data)
		{
		}

		/// <summary>
		/// If the reading is paused.
		/// </summary>
		public bool Paused => this.binaryTransport.Paused;

		/// <summary>
		/// Call this method to continue operation. Operation can be paused, by returning false from <see cref="OnReceived"/>.
		/// </summary>
		public void Continue() => this.binaryTransport.Continue();

		#endregion
	}
}
