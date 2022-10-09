using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Modbus;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Modbus
{
	/// <summary>
	/// Represents a coil on a Modbus unit node.
	/// </summary>
	public class ModbusUnitCoilNode : ModbusUnitChildNode, ISensor, IActuator
	{
		private int coilNr;

		/// <summary>
		/// Represents a coil on a Modbus unit node.
		/// </summary>
		public ModbusUnitCoilNode()
			: base()
		{
		}

		/// <summary>
		/// If the node is provisioned is not. Property is editable.
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(8, "Coil Number:")]
		[ToolTip(9, "Coil number on the Modbus unit.")]
		[Range(0, 65535)]
		public int CoilNr
		{
			get => this.coilNr;
			set => this.coilNr = value;
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 3, "Coil");
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

			Result.AddLast(new Int32Parameter("Nr", await Language.GetStringAsync(typeof(ModbusGatewayNode), 10, "Nr"), this.coilNr));

			return Result;
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async Task StartReadout(ISensorReadout Request)
		{
			ModbusTcpClient Client = await this.Gateway.GetTcpIpConnection();
			await Client.Enter();
			try
			{
				BitArray Bits = await Client.ReadCoils((byte)this.UnitNode.UnitId, (ushort)this.coilNr, 1);

				Request.ReportFields(true, new BooleanField(this, DateTime.UtcNow, "Value", Bits[0], FieldType.Momentary, FieldQoS.AutomaticReadout, true));
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
			finally
			{
				Client.Leave();
			}
		}

		/// <summary>
		/// Get control parameters for the actuator.
		/// </summary>
		/// <returns>Collection of control parameters for actuator.</returns>
		public Task<ControlParameter[]> GetControlParameters()
		{
			return Task.FromResult(new ControlParameter[]
			{
				new BooleanControlParameter("Value", "Modbus", "Value","Coil output",
					async (Node) =>
					{
						ModbusTcpClient Client = await this.Gateway.GetTcpIpConnection();
						await Client.Enter();
						try
						{
							BitArray Bits = await Client.ReadCoils((byte)this.UnitNode.UnitId, (ushort)this.coilNr, 1);
							return Bits[0];
						}
						finally
						{
							Client.Leave();
						}
					},
					async (Node, Value) =>
					{
						ModbusTcpClient Client = await this.Gateway.GetTcpIpConnection();
						await Client.Enter();
						try
						{
							bool WritenValue = await Client.WriteCoil((byte)this.UnitNode.UnitId, (ushort)this.coilNr, Value);

							if (WritenValue!=Value)
								throw new Exception("Coil value not changed.");
						}
						finally
						{
							Client.Leave();
						}
					})
			});
		}
	}
}
