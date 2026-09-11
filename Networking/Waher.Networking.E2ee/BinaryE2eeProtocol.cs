using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.Networking.E2ee
{
	/// <summary>
	/// Binary End-to-End encrypted protocol.
	/// </summary>
	public class BinaryE2eeProtocol : IBinaryTransportLayer, ICommunicationLayer
	{
		private readonly IBinaryTransportLayer binaryTransport;
		private readonly bool decoupledEvents;
		private readonly bool initiator;
		private bool disposed = false;

		/// <summary>
		/// Binary End-to-End encrypted protocol.
		/// </summary>
		/// <param name="BinaryTransport">Binary transport layer.</param>
		/// <param name="Initiator">Initiator of the conversation, typically the
		/// part that initiates a connection or conversation.</param>
		/// <param name="DecoupledEvents">If events raised from the communication layer 
		/// are decoupled, i.e. executed in parallel with the source that raised them.</param>
		public BinaryE2eeProtocol(IBinaryTransportLayer BinaryTransport, bool Initiator,
			bool DecoupledEvents)
		{
			this.binaryTransport = BinaryTransport;
			this.initiator = Initiator;
			this.decoupledEvents = DecoupledEvents;
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
			throw new NotImplementedException();	// TODO
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

		#region IObservableLayer

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => this.decoupledEvents;

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(DateTime Timestamp, string Comment)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(DateTime Timestamp, string Warning)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public void Error(DateTime Timestamp, string Error)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, string Exception)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, Exception Exception)
		{
			throw new NotImplementedException();    // TODO
		}

		#endregion

		#region IEnumerable<ISniffer>

		/// <summary>
		/// Gets an enumerator of registered sniffers.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<ISniffer> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets an enumerator of registered sniffers.
		/// </summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		#region ICommunicationLayer

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer to add.</param>
		public void Add(ISniffer Sniffer)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers to add.</param>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Removes a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer to remove.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		public bool Remove(ISniffer Sniffer)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers
		{
			get
			{
				throw new NotImplementedException();    // TODO
			}
		}

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers
		{
			get
			{
				throw new NotImplementedException();    // TODO
			}
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(int Count)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, int Count)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(int Count)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, int Count)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(DateTime Timestamp, string Text)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(DateTime Timestamp, string Text)
		{
			throw new NotImplementedException();    // TODO
		}

		#endregion
	}
}
