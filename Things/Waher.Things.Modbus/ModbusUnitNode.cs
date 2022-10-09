using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;

namespace Waher.Things.Modbus
{
	/// <summary>
	/// Represents a Unit Device on a Modbus network.
	/// </summary>
	public class ModbusUnitNode : ProvisionedMeteringNode
	{
		private int unitId;

		/// <summary>
		/// Represents a Unit Device on a Modbus network.
		/// </summary>
		public ModbusUnitNode()
			: base()
		{
		}

		/// <summary>
		/// If the node is provisioned is not. Property is editable.
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(5, "Unit Address:")]
		[ToolTip(6, "Unit ID on the Modbus network.")]
		[Range(0, 255)]
		public int UnitId
		{
			get => this.unitId;
			set => this.unitId = value;
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 2, "Modbus Unit");
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new Int32Parameter("Address", await Language.GetStringAsync(typeof(ModbusGatewayNode), 7, "Address"), this.unitId));

			return Result;
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is ModbusGatewayNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(Child is ModbusUnitChildNode);
		}

	}
}
