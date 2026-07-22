using System;
using System.Threading.Tasks;
using Waher.Runtime.Cache;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Maintains a set of sniffers.
	/// </summary>
	public class SnifferSet<T> : ISnifferSet, IDisposable
		where T : ISniffer
	{
		private readonly Cache<string, T> sniffers;
		private readonly Func<string, T> createSniffer;

		/// <summary>
		/// Maintains a set of sniffers.
		/// </summary>
		/// <param name="MaxTimeUnused">Maximum time unused, before being removed.</param>
		/// <param name="CreateSniffer">A function that creates a sniffer, given its
		/// corresponding discrimintaor.</param>
		public SnifferSet(TimeSpan MaxTimeUnused, Func<string, T> CreateSniffer)
		{
			this.createSniffer = CreateSniffer;
			this.sniffers = new Cache<string, T>(int.MaxValue, TimeSpan.MaxValue, MaxTimeUnused);
			this.sniffers.Removed += this.Sniffers_Removed;
		}

		private Task Sniffers_Removed(object Sender, CacheItemEventArgs<string, T> e)
		{
			if (e.Value is IDisposable Disposable)
				Disposable.Dispose();

			return Task.CompletedTask;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.sniffers.Clear();
		}

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public T[] Sniffers => this.sniffers.GetValues();

		/// <summary>
		/// Tries to get a sniffer from the set of registered sniffers.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Sniffer">Retrieved sniffer, if found.</param>
		/// <returns>True if the sniffer was found, false otherwise.</returns>
		public bool TryGetSniffer(string Discriminator, out T Sniffer)
		{
			return this.TryGetSniffer(Discriminator, false, out Sniffer);
		}

		/// <summary>
		/// Tries to get a sniffer from the set of registered sniffers.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="CreateIfNot">If a sniffer should be creaed, if one does
		/// not exist.</param>
		/// <param name="Sniffer">Retrieved sniffer, if found.</param>
		/// <returns>True if the sniffer was found, false otherwise.</returns>
		public bool TryGetSniffer(string Discriminator, bool CreateIfNot, out T Sniffer)
		{
			if (this.sniffers.TryGetValue(Discriminator, out Sniffer))
				return true;

			if (!CreateIfNot)
				return false;

			Sniffer = this.createSniffer(Discriminator);
			this.sniffers.Add(Discriminator, Sniffer);

			return true;
		}

		private T GetSniffer(string Discriminator)
		{
			this.TryGetSniffer(Discriminator, true, out T Sniffer);
			return Sniffer;
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(string Discriminator, int Count)
			=> this.GetSniffer(Discriminator).ReceiveBinary(Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(string Discriminator, DateTime Timestamp, int Count)
			=> this.GetSniffer(Discriminator).ReceiveBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true),
		/// or if the contents in the buffer may change after the call (string Discriminator, false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(string Discriminator, bool ConstantBuffer, byte[] Data)
			=> this.GetSniffer(Discriminator).ReceiveBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true),
		/// or if the contents in the buffer may change after the call (string Discriminator, false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(string Discriminator, DateTime Timestamp, bool ConstantBuffer, byte[] Data)
			=> this.GetSniffer(Discriminator).ReceiveBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true),
		/// or if the contents in the buffer may change after the call (string Discriminator, false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(string Discriminator, bool ConstantBuffer, byte[] Data, int Offset, int Count)
			=> this.GetSniffer(Discriminator).ReceiveBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true),
		/// or if the contents in the buffer may change after the call (string Discriminator, false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(string Discriminator, DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
			=> this.GetSniffer(Discriminator).ReceiveBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(string Discriminator, int Count)
			=> this.GetSniffer(Discriminator).TransmitBinary(Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(string Discriminator, DateTime Timestamp, int Count)
			=> this.GetSniffer(Discriminator).TransmitBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true),
		/// or if the contents in the buffer may change after the call (string Discriminator, false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(string Discriminator, bool ConstantBuffer, byte[] Data)
			=> this.GetSniffer(Discriminator).TransmitBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true),
		/// or if the contents in the buffer may change after the call (string Discriminator, false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(string Discriminator, DateTime Timestamp, bool ConstantBuffer, byte[] Data)
			=> this.GetSniffer(Discriminator).TransmitBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true),
		/// or if the contents in the buffer may change after the call (string Discriminator, false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(string Discriminator, bool ConstantBuffer, byte[] Data, int Offset, int Count)
			=> this.GetSniffer(Discriminator).TransmitBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (string Discriminator, true),
		/// or if the contents in the buffer may change after the call (string Discriminator, false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(string Discriminator, DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
			=> this.GetSniffer(Discriminator).TransmitBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Discriminator, string Text)
			=> this.GetSniffer(Discriminator).ReceiveText(Text);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Discriminator, DateTime Timestamp, string Text)
			=> this.GetSniffer(Discriminator).ReceiveText(Timestamp, Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(string Discriminator, string Text)
			=> this.GetSniffer(Discriminator).TransmitText(Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(string Discriminator, DateTime Timestamp, string Text)
			=> this.GetSniffer(Discriminator).TransmitText(Timestamp, Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(string Discriminator, string Comment)
			=> this.GetSniffer(Discriminator).Information(Comment);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(string Discriminator, DateTime Timestamp, string Comment)
			=> this.GetSniffer(Discriminator).Information(Timestamp, Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Discriminator, string Warning)
			=> this.GetSniffer(Discriminator).Warning(Warning);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Discriminator, DateTime Timestamp, string Warning)
			=> this.GetSniffer(Discriminator).Warning(Timestamp, Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Error">Error.</param>
		public void Error(string Discriminator, string Error)
			=> this.GetSniffer(Discriminator).Error(Error);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public void Error(string Discriminator, DateTime Timestamp, string Error)
			=> this.GetSniffer(Discriminator).Error(Timestamp, Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Discriminator, string Exception)
			=> this.GetSniffer(Discriminator).Exception(Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Discriminator, DateTime Timestamp, string Exception)
			=> this.GetSniffer(Discriminator).Exception(Timestamp, Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Discriminator, Exception Exception)
			=> this.GetSniffer(Discriminator).Exception(Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Discriminator">Discriminator of the sniffer.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Discriminator, DateTime Timestamp, Exception Exception)
			=> this.GetSniffer(Discriminator).Exception(Timestamp, Exception);
	}
}
