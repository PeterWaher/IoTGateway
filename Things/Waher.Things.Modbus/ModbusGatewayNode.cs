using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things.Ip;

namespace Waher.Things.Modbus
{
	public class ModbusGatewayNode : IpHostPort, ISniffable
	{
		private readonly Sniffable sniffers = new Sniffable();

		public ModbusGatewayNode()
			: base()
		{
			this.Port = 502;
			this.Tls = false;
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 1, "Modbus Gateway");
		}

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(Child is ModbusUnitNode);
		}

		#region ISniffable

		public ISniffer[] Sniffers => this.sniffers.Sniffers;
		public bool HasSniffers => this.sniffers.HasSniffers;
		public void Add(ISniffer Sniffer) => this.sniffers.Add(Sniffer);
		public void AddRange(IEnumerable<ISniffer> Sniffers) => this.sniffers.AddRange(Sniffers);
		public bool Remove(ISniffer Sniffer) => this.sniffers.Remove(Sniffer);
		public IEnumerator<ISniffer> GetEnumerator() => this.sniffers.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this.sniffers.GetEnumerator();

		#endregion

	}
}
