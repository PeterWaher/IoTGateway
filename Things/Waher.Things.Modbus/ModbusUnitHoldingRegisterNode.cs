using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Modbus;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Modbus
{
	/// <summary>
	/// Represents a holding register on a Modbus unit node.
	/// </summary>
	public class ModbusUnitHoldingRegisterNode : ModbusUnitChildNode, ISensor, IActuator
	{
		/// <summary>
		/// Represents a register on a Modbus unit node.
		/// </summary>
		public ModbusUnitHoldingRegisterNode()
			: base()
		{
			this.Multiplier = 1.0;
			this.Divisor = 1.0;
		}

		/// <summary>
		/// Register number
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(13, "Register Number:")]
		[ToolTip(14, "Register number on the Modbus unit.")]
		[Range(0, 65535)]
		[Required]
		public int RegisterNr { get; set; }

		/// <summary>
		/// Custom field name
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(25, "Raw Name:")]
		[ToolTip(26, "Custom field name for raw value.")]
		[DefaultValueStringEmpty]
		public string RawName { get; set; }

		/// <summary>
		/// Custom field name
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(11, "Field Name:")]
		[ToolTip(12, "Custom field name for value.")]
		[DefaultValueStringEmpty]
		public string FieldName { get; set; }

		/// <summary>
		/// Multiplier
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(18, "Multiplier:")]
		[ToolTip(19, "Multiplier will be multiplied to the register before reporting value.")]
		[DefaultValue(1.0)]
		[Text(TextPosition.BeforeField, 24, "Raw value will be transformed as follows: Value = ((Raw * Multiplier) / Divisor) + Offset. To this transformed value, the unit will be added.")]
		public double Multiplier { get; set; }

		/// <summary>
		/// Divisor
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(20, "Divisor:")]
		[ToolTip(21, "Divisor will be divided from the register (after multiplication) before reporting value.")]
		[DefaultValue(1.0)]
		public double Divisor { get; set; }

		/// <summary>
		/// Divisor
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(22, "Offset:")]
		[ToolTip(23, "Offset will be addded to the register (after division) before reporting value.")]
		[DefaultValue(0.0)]
		public double Offset { get; set; }

		/// <summary>
		/// Unit
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(16, "Unit:")]
		[ToolTip(17, "Unit of register value (after scaling and offset).")]
		[DefaultValueStringEmpty]
		public string Unit { get; set; }

		/// <summary>
		/// If the byte order in words should be switched.
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(46, "Switch byte order.")]
		[ToolTip(47, "If checked, byte order in registers will be reversed.")]
		[DefaultValue(false)]
		public bool SwitchByteOrder { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 27, "Holding Register (4x)");
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

			Result.AddLast(new Int32Parameter("Nr", await Language.GetStringAsync(typeof(ModbusGatewayNode), 10, "Nr"), this.RegisterNr));

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
				ushort[] Values = await Client.ReadMultipleRegisters((byte)this.UnitNode.UnitId, (ushort)this.RegisterNr, 1);
				ushort Raw = CheckOrder(this.SwitchByteOrder, Values[0]);
				double Value = ((Raw * this.Multiplier) / this.Divisor) + this.Offset;
				int NrDec = Math.Min(255, Math.Max(0, (int)Math.Ceiling(-Math.Log10(this.Multiplier / this.Divisor))));
				DateTime TP = DateTime.UtcNow;

				ThingReference This = this.ReportAs;
				List<Field> Fields = new List<Field>
				{
					new QuantityField(This, TP, this.GetFieldName(), Value, (byte)NrDec, this.Unit, FieldType.Momentary, FieldQoS.AutomaticReadout, true)
				};

				if (!string.IsNullOrEmpty(this.RawName))
					Fields.Add(new Int32Field(This, TP, this.RawName, Raw, FieldType.Momentary, FieldQoS.AutomaticReadout, true));

				Request.ReportFields(true, Fields.ToArray());
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
			finally
			{
				await Client.Leave();
			}
		}

		public string GetFieldName()
		{
			if (string.IsNullOrEmpty(this.FieldName))
				return "Value";
			else
				return this.FieldName;
		}

		/// <summary>
		/// Get control parameters for the actuator.
		/// </summary>
		/// <returns>Collection of control parameters for actuator.</returns>
		public Task<ControlParameter[]> GetControlParameters()
		{
			List<ControlParameter> Parameters = new List<ControlParameter>()
			{
				new DoubleControlParameter("Value", "Modbus", this.GetFieldName(), "Register output",
					this.Offset, ((65535 * this.Multiplier) / this.Divisor) + this.Offset,
					async (Node) =>
					{
						ModbusTcpClient Client = await this.Gateway.GetTcpIpConnection();
						await Client.Enter();
						try
						{
							ushort[] Values = await Client.ReadMultipleRegisters((byte)this.UnitNode.UnitId, (ushort)this.RegisterNr, 1);
							return ((CheckOrder(this.SwitchByteOrder, Values[0]) * this.Multiplier) / this.Divisor) + this.Offset;
						}
						finally
						{
							await Client.Leave();
						}
					},
					async (Node, Value) =>
					{
						ModbusTcpClient Client = await this.Gateway.GetTcpIpConnection();
						await Client.Enter();
						try
						{
							ushort Raw = (ushort)Math.Min(65535, Math.Max(0, ((Value - this.Offset) * this.Divisor) / this.Multiplier));
							ushort WritenValue = await Client.WriteRegister((byte)this.UnitNode.UnitId, (ushort)this.RegisterNr,
								CheckOrder(this.SwitchByteOrder, Raw));

							if (WritenValue != Value)
								throw new Exception("Register value not changed correctly.");
						}
						finally
						{
							await Client.Leave();
						}
					})
			};

			if (!string.IsNullOrEmpty(this.RawName))
			{
				Parameters.Add(new Int32ControlParameter("Value", "Modbus", this.RawName, "Raw register output", 0, 65535,
					async (Node) =>
					{
						ModbusTcpClient Client = await this.Gateway.GetTcpIpConnection();
						await Client.Enter();
						try
						{
							ushort[] Values = await Client.ReadMultipleRegisters((byte)this.UnitNode.UnitId, (ushort)this.RegisterNr, 1);
							return CheckOrder(this.SwitchByteOrder, Values[0]);
						}
						finally
						{
							await Client.Leave();
						}
					},
					async (Node, Value) =>
					{
						ModbusTcpClient Client = await this.Gateway.GetTcpIpConnection();
						await Client.Enter();
						try
						{
							ushort WritenValue = await Client.WriteRegister((byte)this.UnitNode.UnitId, (ushort)this.RegisterNr,
								CheckOrder(this.SwitchByteOrder, (ushort)Value));

							if (WritenValue != Value)
								throw new Exception("Register value not changed correctly.");
						}
						finally
						{
							await Client.Leave();
						}
					}));
			}

			return Task.FromResult(Parameters.ToArray());
		}

		internal static ushort CheckOrder(bool SwitchByteOrder, ushort Value)
		{
			if (SwitchByteOrder)
			{
				ushort Value2 = (ushort)(Value & 0xff);
				Value2 <<= 8;
				Value2 |= (ushort)(Value >> 8);

				return Value2;
			}
			else
				return Value;
		}
	}
}
