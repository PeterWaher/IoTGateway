using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;
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
		private readonly TaskCompletionSource<bool> remoteKeysReceived = new TaskCompletionSource<bool>();
		private readonly IBinaryTransportLayer binaryTransport;
		private readonly IE2eSymmetricCipher[] symmetricCiphers;
		private readonly IE2eSymmetricCipher[] remoteSymmetricCiphers;
		private readonly IE2eEndpoint[] endpoints;
		private readonly IE2eEndpoint[] remoteEndpoints;
		private readonly bool initiator;
		private bool disposed = false;
		private bool hasSymmetricKey;
		private byte[] symmetricKey;

		/// <summary>
		/// Binary End-to-End encrypted protocol.
		/// </summary>
		/// <param name="BinaryTransport">Binary transport layer.</param>
		/// <param name="Initiator">Initiator of the conversation, typically the
		/// part that initiates a connection or conversation.</param>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="DecoupledEvents">If events raised from the communication layer 
		/// are decoupled, i.e. executed in parallel with the source that raised them.</param>
		/// <param name="Sniffers">Optional sniffers.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator,
				  DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength,
				  new Type[] { typeof(EllipticCurveEndpoint), typeof(ModuleLatticeEndpoint) },
				  DecoupledEvents, Sniffers)
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
		/// <param name="Sniffers">Optional sniffers.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			Type[] OnlyIfDerivedFrom, bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator, E2eEndpoint.CreateEndpoints(
				DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength,
				OnlyIfDerivedFrom), DecoupledEvents, Sniffers)
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
		/// <param name="Sniffers">Optional sniffers.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			IE2eEndpoint[] Endpoints, bool DecoupledEvents, params ISniffer[] Sniffers)
			: this(BinaryTransport, Initiator, Endpoints, E2eEndpoint.CreateSymmetricCiphers(),
				  DecoupledEvents, Sniffers)
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
		/// <param name="Sniffers">Optional sniffers.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			IE2eEndpoint[] Endpoints, IE2eSymmetricCipher[] SymmetricCiphers,
			bool DecoupledEvents, params ISniffer[] Sniffers)
			: base(DecoupledEvents, Sniffers)
		{
			this.endpoints = Endpoints;
			if (this.endpoints.Length == 0)
				throw new Exception("No endpoints could be created with the specified security strength.");

			this.symmetricCiphers = SymmetricCiphers;
			if (this.symmetricCiphers.Length == 0)
				throw new Exception("No symmetric ciphers available.");

			this.binaryTransport = BinaryTransport;
			this.initiator = Initiator;
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
		public async Task NegotiateKeys(int Timeout)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(BinaryE2eeProtocol));

			if (this.hasSymmetricKey)
				throw new InvalidOperationException("Keys already negotiated.");

			_ = Task.Delay(Timeout).ContinueWith(_ =>
				this.remoteKeysReceived.TrySetException(new TimeoutException()));

			if (!this.initiator)
				await this.remoteKeysReceived.Task;



			if (this.initiator)
				await this.remoteKeysReceived.Task;
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
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Flushes any pending or intermediate data.
		/// </summary>
		/// <returns>If output has been flushed.</returns>
		public Task<bool> FlushAsync()
		{
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

		/// <summary>
		/// If the reading is paused.
		/// </summary>
		public bool Paused
		{
			get
			{
				throw new NotImplementedException();    // TODO
			}
		}

		/// <summary>
		/// Call this method to continue operation. Operation can be paused, by returning false from <see cref="OnReceived"/>.
		/// </summary>
		public void Continue()
		{
			throw new NotImplementedException();    // TODO
		}

		#endregion
	}
}
