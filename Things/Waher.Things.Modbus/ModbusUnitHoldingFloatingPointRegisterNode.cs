using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.Modbus;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Modbus
{
	/// <summary>
	/// Order of bytes in floating-point value.
	/// </summary>
	public enum FloatByteOrder
	{
		/// <summary>
		/// A B C D
		/// </summary>
		NetworkOrder,

		/// <summary>
		/// B A D C
		/// </summary>
		ByteSwap,

		/// <summary>
		/// C D A B
		/// </summary>
		WordSwap,

		/// <summary>
		/// D C B A
		/// </summary>
		ByteAndWordSwap
	}

	/// <summary>
	/// Represents a floating-point holding register on a Modbus unit node.
	/// </summary>
	public class ModbusUnitHoldingFloatingPointRegisterNode : ModbusUnitChildNode, ISensor
	{
		/// <summary>
		/// Represents a floating-point register on a Modbus unit node.
		/// </summary>
		public ModbusUnitHoldingFloatingPointRegisterNode()
			: base()
		{
			this.Multiplier = 1.0;
			this.Divisor = 1.0;
			this.ByteOrder = FloatByteOrder.NetworkOrder;
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
		[Header(48, "Floating-point byte order:")]
		[ToolTip(49, "Select in which order the bytes in the floating-point value are interpreted.")]
		[DefaultValue(FloatByteOrder.NetworkOrder)]
		[Option(FloatByteOrder.NetworkOrder, 50, "Network Order (A B C D)")]
		[Option(FloatByteOrder.ByteSwap, 51, "Byte Swap Order (B A D C)")]
		[Option(FloatByteOrder.WordSwap, 52, "Word Swap Order (C D A B)")]
		[Option(FloatByteOrder.ByteAndWordSwap, 53, "Byte and Word Swap Order (D C B A)")]
		public FloatByteOrder ByteOrder { get; set; }

		/// <summary>
		/// If the number of decimals should be fixed.
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(56, "Fix number of decimals.")]
		[ToolTip(57, "If checked, the number of decimals of the value fieldd will be fixed.")]
		[DefaultValue(false)]
		public bool FixNrDecimals { get; set; }

		/// <summary>
		/// If the number of decimals should be fixed.
		/// </summary>
		[Page(4, "Modbus", 100)]
		[Header(58, "Number of decimals:")]
		[ToolTip(59, "If number of decimals is fixed, this field determines the number of decimals to fix values to.")]
		[DefaultValue((byte)0)]
		public byte NrDecimals { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 54, "Holding Floatig-point Register (4x)");
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
				ushort[] Values = await Client.ReadMultipleRegisters((byte)this.UnitNode.UnitId, (ushort)this.RegisterNr, 2);
				float Raw = CheckOrder(this.ByteOrder, Values[0], Values[1]);
				double Value = ((Raw * this.Multiplier) / this.Divisor) + this.Offset;
				int NrDec = this.FixNrDecimals ? this.NrDecimals : Math.Min(255, Math.Max(0, (int)Math.Ceiling(-Math.Log10(this.Multiplier / this.Divisor)))) + CommonTypes.GetNrDecimals(Value);
				DateTime TP = DateTime.UtcNow;

				ThingReference This = this.ReportAs;
				List<Field> Fields = new List<Field>
				{
					new QuantityField(This, TP, this.GetFieldName(), Value, (byte)NrDec, this.Unit, FieldType.Momentary, FieldQoS.AutomaticReadout, true)
				};

				if (!string.IsNullOrEmpty(this.RawName))
					Fields.Add(new QuantityField(This, TP, this.RawName, Raw, CommonTypes.GetNrDecimals(Raw), string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout, true));

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

		internal static float CheckOrder(FloatByteOrder Order, ushort Value1, ushort Value2)
		{
			byte A = (byte)(Value1 >> 8);
			byte B = (byte)Value1;
			byte C = (byte)(Value2 >> 8);
			byte D = (byte)Value2;
			byte[] Bin = new byte[4];

			switch (Order)
			{
				case FloatByteOrder.NetworkOrder:
				default:
					Bin[0] = A;
					Bin[1] = B;
					Bin[2] = C;
					Bin[3] = D;
					break;

				case FloatByteOrder.ByteSwap:
					Bin[0] = B;
					Bin[1] = A;
					Bin[2] = D;
					Bin[3] = C;
					break;

				case FloatByteOrder.WordSwap:
					Bin[0] = C;
					Bin[1] = D;
					Bin[2] = A;
					Bin[3] = B;
					break;

				case FloatByteOrder.ByteAndWordSwap:
					Bin[0] = D;
					Bin[1] = C;
					Bin[2] = B;
					Bin[3] = A;
					break;
			}

			return BitConverter.ToSingle(Bin, 0);
		}
	}
}
