using System;
using System.Threading.Tasks;
using Waher.Things.Metering;

namespace Waher.Things.Modbus
{
	/// <summary>
	/// Abstract base class for child nodes to Mobus Unit nodes.
	/// </summary>
	public abstract class ModbusUnitChildNode : ProvisionedMeteringNode
	{
		/// <summary>
		/// Abstract base class for child nodes to Mobus Unit nodes.
		/// </summary>
		public ModbusUnitChildNode()
			: base()
		{
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is ModbusUnitNode || Parent is ModbusCompositeChildNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Under what node fields are to be reported.
		/// </summary>
		public async Task<ThingReference> GetReportAs()
		{
			if (await this.GetParent() is ModbusCompositeChildNode Composite)
				return Composite;
			else
				return this;
		}

		/// <summary>
		/// Modbus Gateway node.
		/// </summary>
		public async Task<ModbusGatewayNode> GetGateway()
		{
			INode Loop = await this.GetParent();
			while (!(Loop is null))
			{
				if (Loop is ModbusGatewayNode Gateway)
					return Gateway;
				else if (Loop is MeteringNode MeteringNode)
					Loop = await MeteringNode.GetParent();
				else
					Loop = Loop.Parent;
			}

			throw new Exception("Modbus Gateway node not found.");
		}

		/// <summary>
		/// Modbus Unit node.
		/// </summary>
		public async Task<ModbusUnitNode> GetUnitNode()
		{
			INode Loop = await this.GetParent();
			while (!(Loop is null))
			{
				if (Loop is ModbusUnitNode UnitNode)
					return UnitNode;
				else if (Loop is MeteringNode MeteringNode)
					Loop = await MeteringNode.GetParent();
				else
					Loop = Loop.Parent;
			}

			throw new Exception("Modbus Unit node not found.");
		}

	}
}
