using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Http
{
	/// <summary>
	/// Node representing the local web server.
	/// </summary>
	public class LocalWebServerNode : MeteringNode, ICommunicationLayer
	{
		/// <summary>
		/// Node representing a connection to an MQTT broker.
		/// </summary>
		public LocalWebServerNode()
			: base()
		{
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(LocalWebServerNode), 1, "Local Web Server");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ExternalWebNode);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root);
		}

		#region ICommunicationLayer

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => HttpModule.WebServer?.DecoupledEvents ?? false;

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer to add.</param>
		public void Add(ISniffer Sniffer)
		{
			HttpModule.WebServer?.Add(Sniffer);
			HttpModule.Scheduler.Add(
				DateTime.Now.AddHours(1),
				(_) => this.Remove(Sniffer),
				null);
		}

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers to add.</param>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			HttpModule.WebServer?.AddRange(Sniffers);
			HttpModule.Scheduler.Add(
				DateTime.Now.AddHours(1),
				(_) =>
				{
					foreach (ISniffer Sniffer in Sniffers)
						this.Remove(Sniffer);
				}, 
				null);
		}

		/// <summary>
		/// Removes a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer to remove.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		public bool Remove(ISniffer Sniffer) => HttpModule.WebServer?.Remove(Sniffer) ?? false;

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers => HttpModule.WebServer?.Sniffers ?? Array.Empty<ISniffer>();

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers => HttpModule.WebServer?.HasSniffers ?? false;

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<ISniffer> GetEnumerator() => HttpModule.WebServer?.GetEnumerator() ?? ((IEnumerable<ISniffer>)(Array.Empty<ISniffer>())).GetEnumerator();

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => HttpModule.WebServer?.GetEnumerator() ?? (Array.Empty<ISniffer>()).GetEnumerator();

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(int Count) => HttpModule.WebServer?.ReceiveBinary(Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data) => HttpModule.WebServer?.ReceiveBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => HttpModule.WebServer?.ReceiveBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(int Count) => HttpModule.WebServer?.TransmitBinary(Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data) => HttpModule.WebServer?.TransmitBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => HttpModule.WebServer?.TransmitBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text) => HttpModule.WebServer?.ReceiveText(Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text) => HttpModule.WebServer?.TransmitText(Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment) => HttpModule.WebServer?.Information(Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning) => HttpModule.WebServer?.Warning(Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error) => HttpModule.WebServer?.Error(Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception) => HttpModule.WebServer?.Exception(Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception) => HttpModule.WebServer?.Exception(Exception);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, int Count) => HttpModule.WebServer?.ReceiveBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => HttpModule.WebServer?.ReceiveBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => HttpModule.WebServer?.ReceiveBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, int Count) => HttpModule.WebServer?.TransmitBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => HttpModule.WebServer?.TransmitBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => HttpModule.WebServer?.TransmitBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(DateTime Timestamp, string Text) => HttpModule.WebServer?.ReceiveText(Timestamp, Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(DateTime Timestamp, string Text) => HttpModule.WebServer?.TransmitText(Timestamp, Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(DateTime Timestamp, string Comment) => HttpModule.WebServer?.Information(Timestamp, Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(DateTime Timestamp, string Warning) => HttpModule.WebServer?.Warning(Timestamp, Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public void Error(DateTime Timestamp, string Error) => HttpModule.WebServer?.Error(Timestamp, Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, string Exception) => HttpModule.WebServer?.Exception(Timestamp, Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, Exception Exception) => HttpModule.WebServer?.Exception(Timestamp, Exception);

		#endregion
	}
}
