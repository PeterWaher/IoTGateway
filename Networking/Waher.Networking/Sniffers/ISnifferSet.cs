using System;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Interface for sets of sniffers.
	/// </summary>
	interface ISnifferSet : IDisposable
	{
		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Count">Number of bytes received.</param>
		void ReceiveBinary(string Discriminator, int Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		void ReceiveBinary(string Discriminator, DateTime Timestamp, int Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true);,
		/// or if the contents in the buffer may change after the call (string Discriminator, false);.</param>
		/// <param name="Data">Binary Data.</param>
		void ReceiveBinary(string Discriminator, bool ConstantBuffer, byte[] Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true);,
		/// or if the contents in the buffer may change after the call (string Discriminator, false);.</param>
		/// <param name="Data">Binary Data.</param>
		void ReceiveBinary(string Discriminator, DateTime Timestamp, bool ConstantBuffer, byte[] Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true);,
		/// or if the contents in the buffer may change after the call (string Discriminator, false);.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		void ReceiveBinary(string Discriminator, bool ConstantBuffer, byte[] Data, int Offset, int Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true);,
		/// or if the contents in the buffer may change after the call (string Discriminator, false);.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		void ReceiveBinary(string Discriminator, DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		void TransmitBinary(string Discriminator, int Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		void TransmitBinary(string Discriminator, DateTime Timestamp, int Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true);,
		/// or if the contents in the buffer may change after the call (string Discriminator, false);.</param>
		/// <param name="Data">Binary Data.</param>
		void TransmitBinary(string Discriminator, bool ConstantBuffer, byte[] Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true);,
		/// or if the contents in the buffer may change after the call (string Discriminator, false);.</param>
		/// <param name="Data">Binary Data.</param>
		void TransmitBinary(string Discriminator, DateTime Timestamp, bool ConstantBuffer, byte[] Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true);,
		/// or if the contents in the buffer may change after the call (string Discriminator, false);.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		void TransmitBinary(string Discriminator, bool ConstantBuffer, byte[] Data, int Offset, int Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true);,
		/// or if the contents in the buffer may change after the call (string Discriminator, false);.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		void TransmitBinary(string Discriminator, DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Text">Text</param>
		void ReceiveText(string Discriminator, string Text);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		void ReceiveText(string Discriminator, DateTime Timestamp, string Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Text">Text</param>
		void TransmitText(string Discriminator, string Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		void TransmitText(string Discriminator, DateTime Timestamp, string Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Comment">Comment.</param>
		void Information(string Discriminator, string Comment);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		void Information(string Discriminator, DateTime Timestamp, string Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Warning">Warning.</param>
		void Warning(string Discriminator, string Warning);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		void Warning(string Discriminator, DateTime Timestamp, string Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Error">Error.</param>
		void Error(string Discriminator, string Error);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		void Error(string Discriminator, DateTime Timestamp, string Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Exception">Exception.</param>
		void Exception(string Discriminator, string Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		void Exception(string Discriminator, DateTime Timestamp, string Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Exception">Exception.</param>
		void Exception(string Discriminator, Exception Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		void Exception(string Discriminator, DateTime Timestamp, Exception Exception);
	}
}
