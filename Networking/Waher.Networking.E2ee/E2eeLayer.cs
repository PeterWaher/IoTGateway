using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
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
	/// End-to-End encrypted communication layer.
	/// </summary>
	public class E2eeLayer : CommunicationLayer, IBinaryTransportLayer, ITextTransportLayer
	{
		private static readonly Random rnd = new Random();

		private readonly TaskCompletionSource<bool> greetingPerformed = new TaskCompletionSource<bool>();
		private readonly TaskCompletionSource<bool> remoteKeysReceived = new TaskCompletionSource<bool>();
		private readonly TaskCompletionSource<bool> ciphersSelected = new TaskCompletionSource<bool>();
		private readonly TaskCompletionSource<bool> goAhead = new TaskCompletionSource<bool>();
		private readonly IBinaryTransportLayer binaryTransport;
		private readonly Dictionary<string, IE2eSymmetricCipher> localSymmetricCiphers;
		private readonly Dictionary<string, IE2eEndpoint> localEndpoints;
		private readonly bool initiator;
		private bool signedTransfers;
		private Dictionary<string, IE2eSymmetricCipher> remoteSymmetricCiphers;
		private Dictionary<string, IE2eEndpoint> remoteEndpoints;
		private IE2eEndpoint selectedLocalEndpoint;
		private IE2eEndpoint selectedRemoteEndpoint;
		private IE2eSymmetricCipher selectedSymmetricCipher;
		private Guid id;
		private Guid remoteId;
		private string idStr;
		private string remoteIdStr;
		private string remoteTypeName;
		private string remoteAssemblyName;
		private string remoteImageVersion;
		private uint sendCounter = 0;
		private uint receiveCounter = 0;
		private bool disposed = false;
		private bool hasSymmetricKey;
		private byte[] symmetricKey;
		private byte[] encryptedBlock = null;
		private byte[] inputBlock = null;
		private int inputState = 0;
		private int inputOffset = 0;
		private int inputBlockLen = 0;
		private int inputBlockPos;
		private bool inputIsText;

		/// <summary>
		/// End-to-End encrypted communication layer.
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
		public E2eeLayer(IBinaryTransportLayer BinaryTransport, bool Initiator,
			int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			bool SignedTransfers, bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator,
				  DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength,
				  new Type[] { typeof(EllipticCurveEndpoint), typeof(ModuleLatticeEndpoint) },
				  SignedTransfers, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// End-to-End encrypted communication layer.
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
		public E2eeLayer(IBinaryTransportLayer BinaryTransport, bool Initiator,
			int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			Type[] OnlyIfDerivedFrom, bool SignedTransfers, bool DecoupledEvents,
			params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator, E2eEndpoint.CreateEndpoints(
				DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength,
				OnlyIfDerivedFrom), SignedTransfers, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// End-to-End encrypted communication layer.
		/// </summary>
		/// <param name="BinaryTransport">Binary transport layer.</param>
		/// <param name="Initiator">Initiator of the conversation, typically the
		/// part that initiates a connection or conversation.</param>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="OnlyIfDerivedFromAsymmetric">Only use endpoints derived from these types.</param>
		/// <param name="OnlyIfDerivedFromSymmetric">Only use ciphers derived from these types.</param>
		/// <param name="DecoupledEvents">If events raised from the communication layer 
		/// are decoupled, i.e. executed in parallel with the source that raised them.</param>
		/// <param name="SignedTransfers">If all transfers must be signed.</param>
		/// <param name="Sniffers">Optional sniffers.</param>
		public E2eeLayer(IBinaryTransportLayer BinaryTransport, bool Initiator,
			int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			Type[] OnlyIfDerivedFromAsymmetric, Type[] OnlyIfDerivedFromSymmetric, bool SignedTransfers, 
			bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator, 
				E2eEndpoint.CreateEndpoints(DesiredSecurityStrength, MinSecurityStrength, 
					MaxSecurityStrength, OnlyIfDerivedFromAsymmetric),
				E2eEndpoint.CreateSymmetricCiphers(MinSecurityStrength, MaxSecurityStrength, 
					OnlyIfDerivedFromSymmetric), 
				SignedTransfers, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// End-to-End encrypted communication layer.
		/// </summary>
		/// <param name="BinaryTransport">Binary transport layer.</param>
		/// <param name="Initiator">Initiator of the conversation, typically the
		/// part that initiates a connection or conversation.</param>
		/// <param name="Endpoints">Asymmetric ciphers.</param>
		/// <param name="DecoupledEvents">If events raised from the communication layer 
		/// are decoupled, i.e. executed in parallel with the source that raised them.</param>
		/// <param name="SignedTransfers">If all transfers must be signed.</param>
		/// <param name="Sniffers">Optional sniffers.</param>
		public E2eeLayer(IBinaryTransportLayer BinaryTransport, bool Initiator,
			IE2eEndpoint[] Endpoints, bool SignedTransfers, bool DecoupledEvents,
			params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator, Endpoints, E2eEndpoint.CreateSymmetricCiphers(),
				  SignedTransfers, DecoupledEvents, Sniffers)
		{
		}

		/// <summary>
		/// End-to-End encrypted communication layer.
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
		public E2eeLayer(IBinaryTransportLayer BinaryTransport, bool Initiator,
			IE2eEndpoint[] Endpoints, IE2eSymmetricCipher[] SymmetricCiphers,
			bool SignedTransfers, bool DecoupledEvents, params ISniffer[] Sniffers)
			: base(DecoupledEvents, Sniffers)
		{
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

			if (Endpoints.Length == 0)
			{
				throw new ArgumentException("No endpoints could be created with the " +
					"specified security requirements.", nameof(Endpoints));
			}

			this.localEndpoints = new Dictionary<string, IE2eEndpoint>();
			this.localSymmetricCiphers = new Dictionary<string, IE2eSymmetricCipher>();

			foreach (IE2eEndpoint Endpoint in Endpoints)
				this.localEndpoints[Key(Endpoint)] = Endpoint;

			foreach (IE2eSymmetricCipher Cipher in SymmetricCiphers)
				this.localSymmetricCiphers[Key(Cipher)] = Cipher;

			if (this.localSymmetricCiphers.Count == 0)
			{
				throw new ArgumentException("No symmetric ciphers available.",
					nameof(SymmetricCiphers));
			}

			this.binaryTransport = BinaryTransport;
			this.initiator = Initiator;
			this.signedTransfers = SignedTransfers;

			this.binaryTransport.OnReceived += this.BinaryTransport_OnReceived;

			if (this.binaryTransport.Paused)
				this.binaryTransport.Continue();
		}

		/// <summary>
		/// Remote type name of communication class, as reported by remote party.
		/// </summary>
		public string RemoteTypeName => this.remoteTypeName;

		/// <summary>
		/// Remote assembly name of communication class, as reported by remote party.
		/// </summary>
		public string RemoteAssemblyName => this.remoteAssemblyName;

		/// <summary>
		/// Remote image version of communication class, as reported by remote party.
		/// </summary>
		public string RemoteImageVersion => this.remoteImageVersion;

		#region IDisposable & IDisposableAsync

		/// <summary>
		/// Disposes the object and underlying transport layer.
		/// </summary>
		[Obsolete("Use DisposeAsync() instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync"/>
		/// </summary>
		public async Task DisposeAsync()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				if (this.binaryTransport is IDisposableAsync DisposableAsync)
					await DisposableAsync.DisposeAsync();
				else
					this.binaryTransport.Dispose();
			}
		}

		#endregion

		/// <summary>
		/// Negotiates the keys to use during encrypted communication.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>If key negotiation concluded successfully.</returns>
		public Task<bool> NegotiateKeys(int Timeout)
		{
			return this.NegotiateKeys(Timeout, CancellationToken.None);
		}

		/// <summary>
		/// Negotiates the keys to use during encrypted communication.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="Cancel">Cancellation token.</param>
		/// <returns>If key negotiation concluded successfully.</returns>
		public async Task<bool> NegotiateKeys(int Timeout, CancellationToken Cancel)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(E2eeLayer));

			if (this.hasSymmetricKey)
				throw new InvalidOperationException("Keys already negotiated.");

			void DoCancel()
			{
				this.greetingPerformed.TrySetException(new TimeoutException());
				this.remoteKeysReceived.TrySetException(new TimeoutException());
				this.ciphersSelected.TrySetException(new TimeoutException());
				this.goAhead.TrySetException(new TimeoutException());
			}

			Cancel.Register(DoCancel);
			_ = Task.Delay(Timeout).ContinueWith(_ => DoCancel());

			if (this.initiator)
			{
				if (!await this.greetingPerformed.Task)
					return false;
			}
			else
			{
				await this.SendHello();

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

			if (!await this.goAhead.Task)
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

		private async Task<bool> SendHello()
		{
			BinaryOutput Output = new BinaryOutput();
			StringBuilder sb = this.HasSniffers ? new StringBuilder() : null;
			Type T = this.GetType();
			Assembly A = T.Assembly;

			Output.WriteString(T.FullName);
			Output.WriteString(A.FullName);
			Output.WriteString(A.ImageRuntimeVersion);

			if (!(sb is null))
			{
				sb.AppendLine(T.FullName);
				sb.AppendLine(A.FullName);
				sb.AppendLine(A.ImageRuntimeVersion);

				this.TransmitText(sb.ToString());
			}

			bool Result = await this.SendBlock(false, Output.ToArray(), null, null, null);

			this.greetingPerformed.TrySetResult(Result);

			return Result;
		}

		private async Task<bool> SendCiphers()
		{
			ICollection<IE2eEndpoint> Endpoints;
			ICollection<IE2eSymmetricCipher> Ciphers;

			if (this.remoteEndpoints is null)
				Endpoints = this.localEndpoints.Values;
			else
			{
				ChunkedList<IE2eEndpoint> Filtered = new ChunkedList<IE2eEndpoint>();

				foreach (IE2eEndpoint Endpoint in this.localEndpoints.Values)
				{
					if (this.remoteEndpoints.TryGetValue(Key(Endpoint),
						out IE2eEndpoint RemoteEndpoint) &&
						Endpoint.PublicKey.Length == RemoteEndpoint.PublicKey.Length)
					{
						Filtered.Add(Endpoint);
					}
				}

				Endpoints = Filtered;
			}

			if (Endpoints.Count == 0)
			{
				if (this.remoteEndpoints is null)
					this.Error("No endpoints available.");
				else
				{
					this.Error("No endpoints in common with remote party.");
					await this.SendBlock(false, Array.Empty<byte>(), null, null, null);
				}

				return false;
			}

			if (this.remoteSymmetricCiphers is null)
				Ciphers = this.localSymmetricCiphers.Values;
			else
			{
				ChunkedList<IE2eSymmetricCipher> Filtered = new ChunkedList<IE2eSymmetricCipher>();

				foreach (IE2eSymmetricCipher Cipher in this.localSymmetricCiphers.Values)
				{
					if (this.remoteSymmetricCiphers.ContainsKey(Key(Cipher)))
						Filtered.Add(Cipher);
				}

				Ciphers = Filtered;
			}

			if (Ciphers.Count == 0)
			{
				if (this.remoteEndpoints is null)
					this.Error("No symmetric ciphers available.");
				else
					this.Error("No symmetric ciphers in common with remote party.");

				return false;
			}

			BinaryOutput Output = new BinaryOutput();
			StringBuilder sb = this.HasSniffers ? new StringBuilder() : null;

			this.id = Guid.NewGuid();
			this.idStr = this.id.ToString();

			Output.WriteGuid(this.id);
			Output.WriteVarLenUInt((uint)Endpoints.Count);
			Output.WriteVarLenUInt((uint)Ciphers.Count);

			sb?.AppendLine(this.idStr);

			foreach (IE2eEndpoint Endpoint in Endpoints)
			{
				Output.WriteString(Endpoint.LocalName);
				Output.WriteString(Endpoint.Namespace);
				Output.WriteData(Endpoint.PublicKey);

				if (!(sb is null))
				{
					sb.Append(Endpoint.Namespace);
					sb.Append('#');
					sb.Append(Endpoint.LocalName);
					sb.Append(" (");
					sb.Append(Endpoint.PublicKey.Length.ToString());
					sb.AppendLine(" byte public key)");
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

			if (this.initiator)
			{
				Type T = this.GetType();
				Assembly A = T.Assembly;

				Output.WriteString(T.FullName);
				Output.WriteString(A.FullName);
				Output.WriteString(A.ImageRuntimeVersion);

				if (!(sb is null))
				{
					sb.AppendLine(T.FullName);
					sb.AppendLine(A.FullName);
					sb.AppendLine(A.ImageRuntimeVersion);
				}
			}

			if (!(sb is null))
				this.TransmitText(sb.ToString());

			return await this.SendBlock(false, Output.ToArray(), null, null, null);
		}

		private Task<bool> SendBlock(bool IsText, byte[] Block, byte[] Signature,
			EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			BinaryOutput Output = new BinaryOutput();
			ulong Len = (uint)Block.Length;
			
			Len <<= 1;
			if (IsText)
				Len |= 1;

			Output.WriteVarLenUInt(Len);
			Output.WriteRaw(Block);

			if (!(Signature is null))
			{
				Len = (uint)Signature.Length;
				Len <<= 1;

				Output.WriteVarLenUInt(Len);
				Output.WriteRaw(Signature);
			}

			byte[] Packet = Output.ToArray();

			return this.binaryTransport.SendAsync(true, Packet, Callback, State);
		}

		private async Task<bool> SelectCipher()
		{
			if (!this.initiator)
				return false;

			IE2eEndpoint BestEndpoint = null;
			IE2eEndpoint BestRemoteEndpoint = null;

			foreach (IE2eEndpoint Endpoint in this.localEndpoints.Values)
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
			StringBuilder sb = this.HasSniffers ? new StringBuilder() : null;

			CipherSelection.WriteString(BestEndpoint.LocalName);
			CipherSelection.WriteString(BestEndpoint.Namespace);
			CipherSelection.WriteData(BestEndpoint.PublicKey);

			if (!(sb is null))
			{
				sb.AppendLine(BestEndpoint.LocalName);
				sb.AppendLine(BestEndpoint.Namespace);
				sb.AppendLine(BestEndpoint.PublicKeyBase64);
			}

			if (this.HasSniffers)
				this.Information("Asymmetric cipher selected: " + Key(BestEndpoint));

			ChunkedList<IE2eSymmetricCipher> CommonCiphers = new ChunkedList<IE2eSymmetricCipher>();
			IE2eSymmetricCipher SelectedCipher = null;

			foreach (IE2eSymmetricCipher Cipher in this.localSymmetricCiphers.Values)
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

			this.selectedLocalEndpoint = BestEndpoint;
			this.selectedRemoteEndpoint = BestRemoteEndpoint;
			this.selectedSymmetricCipher = SelectedCipher;

			if (this.selectedSymmetricCipher.AuthenticatedEncryption)
				this.signedTransfers = false;

			CipherSelection.WriteString(SelectedCipher.LocalName);
			CipherSelection.WriteString(SelectedCipher.Namespace);

			if (!(sb is null))
			{
				sb.AppendLine(SelectedCipher.LocalName);
				sb.AppendLine(SelectedCipher.Namespace);
			}

			if (this.HasSniffers)
				this.Information("Symmetric cipher selected: " + Key(SelectedCipher));

			this.symmetricKey = BestEndpoint.GetSharedSecretForEncryption(
				BestRemoteEndpoint, SelectedCipher, out byte[] CipherText);
			this.hasSymmetricKey = true;

			CipherSelection.WriteData(CipherText ?? Array.Empty<byte>());
			CipherSelection.WriteBool(this.signedTransfers && !SelectedCipher.AuthenticatedEncryption);

			if (!(sb is null))
			{
				if (CipherText is null)
					sb.AppendLine("(No additional cipher text)");
				else
					sb.AppendLine(Convert.ToBase64String(CipherText));

				if (this.signedTransfers)
					sb.AppendLine("Asymmetric cipher signatures");
				else if (this.selectedSymmetricCipher.AuthenticatedEncryption)
					sb.AppendLine("Authenticated encryption");
				else
					sb.AppendLine("No signatures");

				this.TransmitText(sb.ToString());
			}

			bool Result = await this.SendBlock(false, CipherSelection.ToArray(), null, null, null);

			this.ciphersSelected.TrySetResult(Result);

			return Result;
		}

		#region IBinaryTransmission

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <returns>If data was sent.</returns>
		public async Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet)
		{
			if (!this.hasSymmetricKey)
			{
				this.Error("Keys not negotiated.");
				return false;
			}

			if (Packet.Length == 0)
				return true;

			this.EncryptPacket(Packet, out byte[] Encrypted, out byte[] Signature);

			return await this.SendEncryptedBlock(false, ConstantBuffer, Packet, null,
				Encrypted, Signature, null, null);
		}

		private async Task<bool> SendEncryptedBlock(bool IsText, bool ConstantBuffer, 
			byte[] Packet, string Text, byte[] Encrypted, byte[] Signature, 
			EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (IsText)
				this.TransmitText(Text);
			else
				this.TransmitBinary(ConstantBuffer, Packet);

			if (!await this.SendBlock(IsText, Encrypted, Signature, 
				async (Sender, e) =>
				{
					if (IsText)
					{
						TextEventHandler h = this.OnTextSent;
						if (!(h is null))
						{
							try
							{
								await h(this, Text);
							}
							catch (Exception ex)
							{
								this.Exception(ex);
								Log.Exception(ex);
							}
						}
					}
					else
					{
						BinaryDataWrittenEventHandler h = this.OnSent;
						if (!(h is null))
						{
							try
							{
								await h(this, ConstantBuffer, Packet, 0, Packet.Length);
							}
							catch (Exception ex)
							{
								this.Exception(ex);
								Log.Exception(ex);
							}
						}
					}

					if (!(Callback is null))
						await Callback.Raise(this, new DeliveryEventArgs(State, e.Ok));

				}, State))
				return false;

			return true;
		}

		private void EncryptPacket(byte[] Packet, out byte[] Encrypted, out byte[] Signature)
		{
			byte[] IV = this.selectedSymmetricCipher.GetIV(string.Empty, string.Empty,
				this.idStr, this.remoteIdStr, this.sendCounter++);

			byte[] AssociatedData = Hashes.ComputeSHA256Hash(IV);

			Encrypted = this.selectedSymmetricCipher.Encrypt(Packet, this.symmetricKey, IV, 
				AssociatedData, E2eBufferFillAlgorithm.Random);

			if (this.signedTransfers)
				Signature = this.selectedLocalEndpoint.Sign(AssociatedData);
			else
				Signature = null;
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
		public async Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (!this.hasSymmetricKey)
			{
				this.Error("Keys not negotiated.");
				return false;
			}

			if (Packet.Length == 0)
				return true;

			this.EncryptPacket(Packet, out byte[] Encrypted, out byte[] Signature);

			return await this.SendEncryptedBlock(false, ConstantBuffer, Packet, null,
				Encrypted, Signature, Callback, State);
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
		public async Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset,
			int Count)
		{
			if (!this.hasSymmetricKey)
			{
				this.Error("Keys not negotiated.");
				return false;
			}

			if (Count == 0)
				return true;

			byte[] Packet = GetPacket(Buffer, Offset, Count, ref ConstantBuffer);
			this.EncryptPacket(Packet, out byte[] Encrypted, out byte[] Signature);

			return await this.SendEncryptedBlock(false, ConstantBuffer, Packet, null, 
				Encrypted, Signature, null, null);
		}

		private static byte[] GetPacket(byte[] Buffer, int Offset, int Count,
			ref bool ConstantBuffer)
		{
			if (Offset > 0 || Count < Buffer.Length)
			{
				ConstantBuffer = true;
				return SnifferBase.CloneSection(Buffer, Offset, Count);
			}
			else
				return Buffer;
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
		public async Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, 
			int Count, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (!this.hasSymmetricKey)
			{
				this.Error("Keys not negotiated.");
				return false;
			}

			if (Count == 0)
				return true;

			byte[] Packet = GetPacket(Buffer, Offset, Count, ref ConstantBuffer);
			this.EncryptPacket(Packet, out byte[] Encrypted, out byte[] Signature);

			return await this.SendEncryptedBlock(false, ConstantBuffer, Packet, null, 
				Encrypted, Signature, Callback, State);
		}

		/// <summary>
		/// Flushes any pending or intermediate data.
		/// </summary>
		/// <returns>If output has been flushed.</returns>
		public Task<bool> FlushAsync()
		{
			return this.binaryTransport.FlushAsync();
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
						case 6:
						case 8:
							byte b = Buffer[Offset++];
							Count--;

							this.inputBlockLen |= (b & 0x7f) << this.inputOffset;
							this.inputOffset += 7;

							if ((b & 0x80) == 0)
							{
								this.inputIsText = (this.inputBlockLen & 1) != 0;
								this.inputBlockLen >>= 1;
								this.inputBlock = new byte[this.inputBlockLen];
								this.inputOffset = 0;

								if (this.inputIsText && !this.hasSymmetricKey)
								{
									this.Error("Protocol error.");
									this.inputState = -1;

									await this.OnProtocolError.Raise(this, EventArgs.Empty);
									break;
								}

								if (this.inputBlockLen > 0)
								{
									this.inputBlockPos = 0;
									this.inputState++;
								}
								else
								{
									switch (this.inputState)
									{
										case 2:
											this.Error("No remote ciphers.");
											this.remoteKeysReceived.TrySetResult(false);
											this.ciphersSelected.TrySetResult(false);
											this.goAhead.TrySetResult(false);
											this.inputState = -1;

											await this.OnProtocolError.Raise(this, EventArgs.Empty);
											break;

										case 4:
											if (this.HasSniffers)
												this.ReceiveText("Go ahead.");

											this.goAhead.TrySetResult(true);
											this.inputState += 2;
											break;

										default:
											if (this.inputIsText && this.inputState != 9)
											{
												this.inputBlock = Array.Empty<byte>();

												if (this.signedTransfers)
												{
													this.encryptedBlock = this.inputBlock;
													this.inputState += 2;
												}
												else if (await this.ProcessEncryptedBlock(this.inputIsText, this.inputBlock, null))
													this.inputBlockLen = 0;
												else
												{
													this.inputState = -1;
													await this.OnProtocolError.Raise(this, EventArgs.Empty);
												}
											}
											else
											{
												this.Error("Protocol error.");
												this.inputState = -1;

												await this.OnProtocolError.Raise(this, EventArgs.Empty);
											}
											break;
									}
								}
							}
							break;

						case 1:
						case 3:
						case 5:
						case 7:
						case 9:
							int c = Math.Min(Count, this.inputBlockLen - this.inputBlockPos);
							System.Buffer.BlockCopy(Buffer, Offset, this.inputBlock, this.inputBlockPos, c);
							Offset += c;
							Count -= c;
							this.inputBlockPos += c;

							if (this.inputBlockLen == this.inputBlockPos)
							{
								switch (this.inputState)
								{
									case 1:
										bool Result;

										if (this.initiator)
											Result = this.ProcessHello(this.inputBlock);
										else
											Result = await this.ProcessRemoteCiphers(this.inputBlock);

										if (Result)
											this.inputState++;
										else
										{
											this.inputState = -1;
											await this.OnProtocolError.Raise(this, EventArgs.Empty);
										}
										break;

									case 3:
										if (this.initiator)
											Result = await this.ProcessRemoteCiphers(this.inputBlock);
										else
										{
											Result = await this.CipherSelected(this.inputBlock);
											this.ciphersSelected.TrySetResult(Result);

											if (Result)
											{
												this.inputState += 2;
												this.goAhead.TrySetResult(true);
											}
										}

										if (Result)
											this.inputState++;
										else
										{
											this.inputState = -1;
											await this.OnProtocolError.Raise(this, EventArgs.Empty);
										}
										break;

									case 5:
										this.goAhead.TrySetResult(true);
										this.inputState++;
										break;

									case 7:
										if (this.signedTransfers)
										{
											this.encryptedBlock = this.inputBlock;
											this.inputState++;
										}
										else if (await this.ProcessEncryptedBlock(this.inputIsText, this.inputBlock, null))
										{
											this.inputBlockLen = 0;
											this.inputState--;
										}
										else
										{
											this.inputState = -1;
											await this.OnProtocolError.Raise(this, EventArgs.Empty);
										}
										break;

									case 9:
										if (this.inputIsText)
										{
											this.inputState = -1;
											await this.OnProtocolError.Raise(this, EventArgs.Empty);
										}
										else if (await this.ProcessEncryptedBlock(this.inputIsText, this.encryptedBlock, this.inputBlock))
										{
											this.inputBlockLen = 0;
											this.inputState -= 3;
										}
										else
										{
											this.inputState = -1;
											await this.OnProtocolError.Raise(this, EventArgs.Empty);
										}
										break;
								}

								this.inputBlockLen = 0;
							}
							break;

						default:
							this.ReceiveBinary(Count);
							Count = 0;
							break;
					}
				}
			}
			catch (Exception ex)
			{
				this.Exception(ex);
				await this.DisposeAsync();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Event raised when a protocol error occurs.
		/// </summary>
		public event EventHandlerAsync OnProtocolError;

		private bool ProcessHello(byte[] Data)
		{
			BinaryInput Input = new BinaryInput(Data);

			this.remoteTypeName = Input.ReadString();
			this.remoteAssemblyName = Input.ReadString();
			this.remoteImageVersion = Input.ReadString();

			if (this.HasSniffers)
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine(this.remoteTypeName);
				sb.AppendLine(this.remoteAssemblyName);
				sb.AppendLine(this.remoteImageVersion);

				this.ReceiveText(sb.ToString());
			}

			this.greetingPerformed.TrySetResult(true);

			return true;
		}

		private async Task<bool> ProcessRemoteCiphers(byte[] Data)
		{
			BinaryInput Input = new BinaryInput(Data);
			StringBuilder sb = this.HasSniffers ? new StringBuilder() : null;

			this.remoteEndpoints = new Dictionary<string, IE2eEndpoint>();
			this.remoteSymmetricCiphers = new Dictionary<string, IE2eSymmetricCipher>();

			this.remoteId = Input.ReadGuid();
			this.remoteIdStr = this.remoteId.ToString();

			sb?.AppendLine(this.remoteIdStr);

			string LocalName, Namespace;
			byte[] PublicKey;
			int NrEndpoints = (int)Input.ReadVarLenUInt();
			int NrSymmetricCiphers = (int)Input.ReadVarLenUInt();
			int i;

			for (i = 0; i < NrEndpoints; i++)
			{
				LocalName = Input.ReadString();
				Namespace = Input.ReadString();
				PublicKey = Input.ReadData();

				if (!(sb is null))
				{
					sb.Append(Namespace);
					sb.Append('#');
					sb.Append(LocalName);
					sb.Append(" (");
					sb.Append(PublicKey.Length.ToString());
					sb.AppendLine(" bytes public key)");
				}

				if (E2eEndpoint.TryCreateEndpoint(LocalName, Namespace, out IE2eEndpoint Endpoint))
					this.remoteEndpoints[Key(Endpoint)] = Endpoint.CreatePublic(PublicKey);
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

			if (!this.initiator)
			{
				this.remoteTypeName = Input.ReadString();
				this.remoteAssemblyName = Input.ReadString();
				this.remoteImageVersion = Input.ReadString();

				if (!(sb is null))
				{
					sb.AppendLine(this.remoteTypeName);
					sb.AppendLine(this.remoteAssemblyName);
					sb.AppendLine(this.remoteImageVersion);
				}
			}

			if (!(sb is null))
				this.ReceiveText(sb.ToString());

			bool Result = this.remoteEndpoints.Count > 0 &&
				this.remoteSymmetricCiphers.Count > 0;

			if (!Result)
				this.Error("Missing algorithms.");
			else
			{
				IE2eEndpoint[] RemoteEndpints = new IE2eEndpoint[this.remoteEndpoints.Count];
				this.remoteEndpoints.Values.CopyTo(RemoteEndpints, 0);

				IE2eSymmetricCipher[] RemoteCiphers = new IE2eSymmetricCipher[this.remoteSymmetricCiphers.Count];
				this.remoteSymmetricCiphers.Values.CopyTo(RemoteCiphers, 0);

				RemoteEndpointsEventArgs e = new RemoteEndpointsEventArgs(this.remoteId,
					RemoteEndpints, RemoteCiphers);

				await this.OnRemoteEndpoints.Raise(this, e);

				if (!e.Valid)
				{
					this.Error("Remote endpoints not valid.");
					Result = false;
				}
				else if (this.disposed)
				{
					this.Error("Protocol disposed.");
					Result = false;
				}
			}

			this.remoteKeysReceived.TrySetResult(Result);

			return Result;
		}

		/// <summary>
		/// Event raised when the remote endpoints and symmetric ciphers have been
		/// received. Provide an event handler to validate the remote endpoint.
		/// </summary>
		public event EventHandlerAsync<RemoteEndpointsEventArgs> OnRemoteEndpoints;

		private async Task<bool> CipherSelected(byte[] Data)
		{
			BinaryInput Input = new BinaryInput(Data);

			string LocalNameAsym = Input.ReadString();
			string NamespaceAsym = Input.ReadString();
			byte[] PublicKeyAsym = Input.ReadData();
			string LocalNameSym = Input.ReadString();
			string NamespaceSym = Input.ReadString();
			byte[] CipherText = Input.ReadData();
			bool Signatures = Input.ReadBool();

			if (CipherText.Length == 0)
				CipherText = null;

			if (this.HasSniffers)
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine(LocalNameAsym);
				sb.AppendLine(NamespaceAsym);
				sb.AppendLine(Convert.ToBase64String(PublicKeyAsym));
				sb.AppendLine(LocalNameSym);
				sb.AppendLine(NamespaceSym);

				if (CipherText is null)
					sb.AppendLine("(No additional cipher text)");
				else
					sb.AppendLine(Convert.ToBase64String(CipherText));

				if (Signatures)
					sb.AppendLine("Explicit signatures added");
				else
					sb.AppendLine("No explicit signatures added");

				this.ReceiveText(sb.ToString());
			}

			if (!this.localEndpoints.TryGetValue(Key(LocalNameAsym, NamespaceAsym),
				out IE2eEndpoint LocalEndpoint) ||
				LocalEndpoint.PublicKey.Length != PublicKeyAsym.Length)
			{
				if (this.HasSniffers)
				{
					this.Error("Asymmetric cipher not supported: " +
						Key(LocalNameAsym, NamespaceAsym) + ": " +
						Convert.ToBase64String(PublicKeyAsym));
				}

				return false;
			}

			if (!this.remoteEndpoints.TryGetValue(Key(LocalNameAsym, NamespaceAsym),
				out IE2eEndpoint RemoteEndpoint))
			{
				if (this.HasSniffers)
				{
					this.Error("Unable to select asymmetric cipher: " +
						Key(LocalNameAsym, NamespaceAsym) + ": " +
						Convert.ToBase64String(PublicKeyAsym));
				}

				return false;
			}

			if (!this.localSymmetricCiphers.TryGetValue(Key(LocalNameSym, NamespaceSym),
				out IE2eSymmetricCipher LocalCipher))
			{
				if (this.HasSniffers)
				{
					this.Error("Symmetric cipher not supported: " +
						Key(LocalNameSym, NamespaceSym));
				}

				return false;
			}

			if (!this.remoteSymmetricCiphers.ContainsKey(Key(LocalNameSym, NamespaceSym)))
			{
				if (this.HasSniffers)
				{
					this.Error("Unable to select symmetric cipher: " +
						Key(LocalNameSym, NamespaceSym));
				}

				return false;
			}

			this.symmetricKey = LocalEndpoint.GetSharedSecretForDecryption(RemoteEndpoint,
				CipherText);

			this.hasSymmetricKey = true;
			this.selectedLocalEndpoint = LocalEndpoint;
			this.selectedRemoteEndpoint = RemoteEndpoint;
			this.selectedSymmetricCipher = LocalCipher;
			this.signedTransfers = Signatures;

			if (this.HasSniffers)
				this.TransmitText("Go ahead.");

			return await this.SendBlock(false, Array.Empty<byte>(), null, null, null);
		}

		private async Task<bool> ProcessEncryptedBlock(bool IsText,byte[] Data, byte[] Signature)
		{
			if (!this.hasSymmetricKey)
			{
				this.Error("Keys not negotiated.");
				return false;
			}

			byte[] IV = this.selectedSymmetricCipher.GetIV(string.Empty, string.Empty,
				this.remoteIdStr, this.idStr, this.receiveCounter++);

			byte[] AssociatedData = Hashes.ComputeSHA256Hash(IV);
			byte[] Decrypted = this.selectedSymmetricCipher.Decrypt(Data,
				this.symmetricKey, IV, AssociatedData);

			if (Decrypted is null)
			{
				this.Error("Unable to decrypt block.");
				return false;
			}

			if (this.signedTransfers)
			{
				if (Signature is null)
				{
					await this.SendBlock(false, Array.Empty<byte>(), null, null, null);
					this.Error("Ignoring incoming packet. Missing signature.");
					return false;
				}

				if (!this.selectedRemoteEndpoint.Verify(AssociatedData, Signature))
				{
					await this.SendBlock(false, Array.Empty<byte>(), null, null, null);
					this.Error("Ignoring incoming packet. Invalid signature.");
					return false;
				}
			}

			if (IsText)
			{
				string Text = Encoding.UTF8.GetString(Decrypted);

				this.ReceiveText(Text);

				TextEventHandler h = this.OnTextReceived;
				if (!(h is null))
				{
					try
					{
						await h(this, Text);
					}
					catch (Exception ex)
					{
						this.Exception(ex);
						Log.Exception(ex);
					}
				}
			}
			else
			{
				this.ReceiveBinary(true, Decrypted);

				BinaryDataReadEventHandler h = this.OnReceived;
				if (!(h is null))
				{
					try
					{
						await h(this, true, Decrypted, 0, Decrypted.Length);
					}
					catch (Exception ex)
					{
						this.Exception(ex);
						Log.Exception(ex);
					}
				}
			}

			return true;
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

		#region ITextTransportLayer

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Text">Text packet.</param>
		/// <returns>If data was sent.</returns>
		public async Task<bool> SendAsync(string Text)
		{
			if (!this.hasSymmetricKey)
			{
				this.Error("Keys not negotiated.");
				return false;
			}

			// Note: Empty strings permitted.

			byte[] Packet = Encoding.UTF8.GetBytes(Text);
			this.EncryptPacket(Packet, out byte[] Encrypted, out byte[] Signature);

			return await this.SendEncryptedBlock(true, true, Packet, Text, 
				Encrypted, Signature, null, null);
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Text">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public async Task<bool> SendAsync(string Text, 
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback, object State)
		{
			if (!this.hasSymmetricKey)
			{
				this.Error("Keys not negotiated.");
				return false;
			}

			// Note: Empty strings permitted.

			byte[] Packet = Encoding.UTF8.GetBytes(Text);
			this.EncryptPacket(Packet, out byte[] Encrypted, out byte[] Signature);

			return await this.SendEncryptedBlock(true, true, Packet, Text,
				Encrypted, Signature, DeliveryCallback, State);
		}

		/// <summary>
		/// Event raised when a text packet has been sent.
		/// </summary>
		public event TextEventHandler OnTextSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		public event TextEventHandler OnTextReceived;

		#endregion
	}
}
