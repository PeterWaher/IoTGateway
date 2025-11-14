using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Script.Units;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Fields.Calculations
{
	/// <summary>
	/// Converts the value of a field to a specific unit.
	/// </summary>
	public class UnitConversion : DecisionTreeLeafStatement
	{
		/// <summary>
		/// Converts the value of a field to a specific unit.
		/// </summary>
		public UnitConversion()
			: base()
		{
		}

		/// <summary>
		/// Unit to convert to.
		/// </summary>
		[Header(3, "Unit:", 20)]
		[Page(1, "Processor", 0)]
		[ToolTip(4, "The unit to convert to.")]
		[Required]
		public string Unit { get; set; }

		/// <summary>
		/// If the unit is missing, sets it to the target unit.
		/// </summary>
		[Header(5, "Set unit if missing.", 30)]
		[Page(1, "Processor", 0)]
		[ToolTip(6, "If a numeric field lacks a unit, the unit will be set to the specified unit.")]
		public bool SetUnitIfMissing { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(UnitConversion), 2, "Unit Conversion");
		}

		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public override Task<Field[]> ProcessField(ISensor Sensor, Field Field)
		{
			if (!Script.Units.Unit.TryParse(this.Unit, out Unit ToUnit))
				return Task.FromResult<Field[]>(null);

			if (Field is QuantityField QuantityField)
			{
				if (string.IsNullOrEmpty(QuantityField.Unit))
				{
					if (this.SetUnitIfMissing)
					{
						return Task.FromResult(new Field[]
						{
							new QuantityField(Field.Thing, Field.Timestamp, Field.Name,
								QuantityField.Value, QuantityField.NrDecimals, this.Unit,
								Field.Type, Field.QoS, Field.Writable, Field.Module,
								Field.StringIdSteps)
						});
					}
					else
						return Task.FromResult<Field[]>(null);
				}

				if (!Script.Units.Unit.TryParse(QuantityField.Unit, out Unit FromUnit))
					return Task.FromResult<Field[]>(null);

				if (!Script.Units.Unit.TryConvert(QuantityField.Value, FromUnit, QuantityField.NrDecimals,
					ToUnit, out double ToValue, out byte ToNrDec))
				{
					return Task.FromResult<Field[]>(null);
				}

				return Task.FromResult(new Field[]
				{
					new QuantityField(Field.Thing, Field.Timestamp, Field.Name,
						ToValue, ToNrDec, this.Unit, Field.Type, Field.QoS, Field.Writable,
						Field.Module, Field.StringIdSteps)
				});
			}
			else if (Field is Int32Field Int32Field)
			{
				if (this.SetUnitIfMissing)
				{
					return Task.FromResult(new Field[]
					{
						new QuantityField(Field.Thing, Field.Timestamp, Field.Name,
							Int32Field.Value, 0, this.Unit, Field.Type, Field.QoS, 
							Field.Writable, Field.Module, Field.StringIdSteps)
					});
				}
				else
					return Task.FromResult<Field[]>(null);
			}
			else if (Field is Int64Field Int64Field)
			{
				if (this.SetUnitIfMissing)
				{
					return Task.FromResult(new Field[]
					{
						new QuantityField(Field.Thing, Field.Timestamp, Field.Name,
							Int64Field.Value, 0, this.Unit, Field.Type, Field.QoS,
							Field.Writable, Field.Module, Field.StringIdSteps)
					});
				}
				else
					return Task.FromResult<Field[]>(null);
			}
			else
				return Task.FromResult<Field[]>(null);
		}
	}
}
