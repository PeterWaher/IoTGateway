using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Metering;

namespace Waher.Things.Modbus
{
	public class ModbusUnitNode : ProvisionedMeteringNode
	{
		public ModbusUnitNode()
			: base()
		{
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 2, "Modbus Unit");
		}

		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is ModbusGatewayNode);
		}

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
		}

	}
}
