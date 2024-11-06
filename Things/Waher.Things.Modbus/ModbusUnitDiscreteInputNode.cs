using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Modbus;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Modbus
{
	/// <summary>
	/// Represents a deiscrete input register on a Modbus unit node.
	/// </summary>
	public class ModbusUnitDiscreteInputNode : ModbusUnitChildNode, ISensor
	{
		/// <summary>
		/// Represents a deiscrete input register on a Modbus unit node.
		/// </summary>
		public ModbusUnitDiscreteInputNode()
			: base()
		{
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
		[Header(11, "Field Name:")]
		[ToolTip(12, "Custom field name for value.")]
		[DefaultValueStringEmpty]
		public string FieldName { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ModbusGatewayNode), 29, "Discrete Input (1x)");
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
			ModbusTcpClient Client = await (await this.GetGateway()).GetTcpIpConnection();
			await Client.Enter();
			try
			{
				BitArray Values = await Client.ReadInputDiscretes((byte)(await this.GetUnitNode()).UnitId, (ushort)this.RegisterNr, 1);
				DateTime TP = DateTime.UtcNow;

				ThingReference This = await this.GetReportAs();

				await Request.ReportFields(true,
					new BooleanField(This, TP, this.GetFieldName(), Values[0], FieldType.Momentary, FieldQoS.AutomaticReadout));
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
			finally
			{
				await Client.Leave();
			}
		}

		/// <summary>
		/// Gets the field name of the node.
		/// </summary>
		/// <returns>Field name</returns>
		public string GetFieldName()
		{
			if (string.IsNullOrEmpty(this.FieldName))
				return "Value";
			else
				return this.FieldName;
		}
	}
}
