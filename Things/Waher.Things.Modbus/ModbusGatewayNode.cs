using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.Modbus;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Runtime.Language;
using Waher.Runtime.Threading;
using Waher.Things.Ip;

namespace Waher.Things.Modbus
{
    /// <summary>
    /// Node representing a TCP/IP connection to a Modbus Gateway
    /// </summary>
    public class ModbusGatewayNode : IpHostPort, ICommunicationLayer
	{
		private readonly CommunicationLayer sniffers = new CommunicationLayer(false);

		/// <summary>
		/// Node representing a TCP/IP connection to a Modbus Gateway
		/// </summary>
		public ModbusGatewayNode()
			: base()
		{
			this.Port = 502;
			this.Tls = false;
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 1, "Modbus Gateway");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ModbusUnitNode);
		}

		#region ICommunicationLayer

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => this.sniffers.DecoupledEvents;

		/// <summary>
		/// Registered sniffers
		/// </summary>
		public ISniffer[] Sniffers => this.sniffers.Sniffers;

		/// <summary>
		/// If sniffers are registered
		/// </summary>
		public bool HasSniffers => this.sniffers.HasSniffers;

		/// <summary>
		/// Adds a sniffer
		/// </summary>
		/// <param name="Sniffer">Sniffer</param>
		public void Add(ISniffer Sniffer)
		{
			if (!this.sniffers.Contains(Sniffer))
				this.sniffers.Add(Sniffer);
		}

		/// <summary>
		/// Adds a range of sniffers
		/// </summary>
		/// <param name="Sniffers">Sniffrs</param>
		public void AddRange(IEnumerable<ISniffer> Sniffers) => this.sniffers.AddRange(Sniffers);

		/// <summary>
		/// Removes a sniffer
		/// </summary>
		/// <param name="Sniffer">Sniffer</param>
		/// <returns>If sniffer was found and removed.</returns>
		public bool Remove(ISniffer Sniffer) => this.sniffers.Remove(Sniffer);

		/// <summary>
		/// Gets an enumerator of registered sniffers.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<ISniffer> GetEnumerator() => this.sniffers.GetEnumerator();

		/// <summary>
		/// Gets an enumerator of registered sniffers.
		/// </summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator() => this.sniffers.GetEnumerator();

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(int Count) => this.sniffers.ReceiveBinary(Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data) => this.sniffers.ReceiveBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.sniffers.ReceiveBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(int Count) => this.sniffers.TransmitBinary(Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data) => this.sniffers.TransmitBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.sniffers.TransmitBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text) => this.sniffers.ReceiveText(Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text) => this.sniffers.TransmitText(Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment) => this.sniffers.Information(Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning) => this.sniffers.Warning(Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error) => this.sniffers.Error(Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception) => this.sniffers.Exception(Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception) => this.sniffers.Exception(Exception);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, int Count) => this.sniffers.ReceiveBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => this.sniffers.ReceiveBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.sniffers.ReceiveBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, int Count) => this.sniffers.TransmitBinary(Timestamp, Count);


		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => this.sniffers.TransmitBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.sniffers.TransmitBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(DateTime Timestamp, string Text) => this.sniffers.ReceiveText(Timestamp, Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(DateTime Timestamp, string Text) => this.sniffers.TransmitText(Timestamp, Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(DateTime Timestamp, string Comment) => this.sniffers.Information(Timestamp, Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(DateTime Timestamp, string Warning) => this.sniffers.Warning(Timestamp, Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public void Error(DateTime Timestamp, string Error) => this.sniffers.Error(Timestamp, Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, string Exception) => this.sniffers.Exception(Timestamp, Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, Exception Exception) => this.sniffers.Exception(Timestamp, Exception);

		#endregion

		#region TCP/IP connections

		private readonly static Cache<string, ModbusTcpClient> clients = GetCache();
		private readonly static MultiReadSingleWriteObject clientsSynchObject = new MultiReadSingleWriteObject(typeof(ModbusGatewayNode));

		private static Cache<string, ModbusTcpClient> GetCache()
		{
			Cache<string, ModbusTcpClient> Result = new Cache<string, ModbusTcpClient>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromMinutes(5));

			Result.Removed += Result_Removed;

			return Result;
		}

		private static Task Result_Removed(object Sender, CacheItemEventArgs<string, ModbusTcpClient> e)
		{
			try
			{
				e.Value.Dispose();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets the TCP/IP connection associated with this gateway.
		/// </summary>
		/// <returns>TCP/IP connection</returns>
		public async Task<ModbusTcpClient> GetTcpIpConnection()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.Host);
			sb.Append(this.Port.ToString());
			sb.Append(this.Tls.ToString());

			string Key = sb.ToString();

			if (clients.TryGetValue(Key, out ModbusTcpClient Client))
			{
				if (Client.Connected)
				{
					this.CheckSniffers(Client);
					return Client;
				}
				else
					clients.Remove(Key);
			}

			await clientsSynchObject.BeginWrite();
			try
			{
				if (clients.TryGetValue(Key, out Client))
					return Client;

				Client = await ModbusTcpClient.Connect(this.Host, this.Port, this.Tls, this.sniffers.Sniffers);

				clients[Key] = Client;
			}
			finally
			{
				await clientsSynchObject.EndWrite();
			}

			return Client;
		}

		private void CheckSniffers(ModbusTcpClient Client)
		{
			if (!Client.HasSniffers && !this.sniffers.HasSniffers)
				return;

			foreach (ISniffer Sniffer in Client.Sniffers)
			{
				bool Found = false;

				foreach (ISniffer Sniffer2 in this.sniffers.Sniffers)
				{
					if (Sniffer == Sniffer2)
					{
						Found = true;
						break;
					}
				}

				if (!Found)
					Client.Remove(Sniffer);
			}

			foreach (ISniffer Sniffer in this.sniffers.Sniffers)
			{
				bool Found = false;

				foreach (ISniffer Sniffer2 in Client.Sniffers)
				{
					if (Sniffer == Sniffer2)
					{
						Found = true;
						break;
					}
				}

				if (!Found)
					Client.Add(Sniffer);
			}
		}

		#endregion

		#region Commands

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Commands = new List<ICommand>();
			Commands.AddRange(await base.Commands);
			return Commands.ToArray();
		}

		#endregion
	}
}
