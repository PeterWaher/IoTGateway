using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Waher.IoTGateway;
using Waher.Content;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Script.Objects;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Arduino
{
	public enum AnalogInputPinMode
	{
		Input
	}

	public class AnalogInput : AnalogPin, ISensor
	{
		private AnalogInputPinMode mode = AnalogInputPinMode.Input;
		private string expression = "Raw";
		private string fieldName = "Value";
		private string unit = string.Empty;
		private Expression exp = null;
		private byte nrDecimals = 0;
		private bool initialized = false;

		public AnalogInput()
			: base()
		{
		}

		[Page(5, "Arduino")]
		[Header(9, "Mode:", 20)]
		[ToolTip(10, "Select drive mode of pin.")]
		[DefaultValue(AnalogInputPinMode.Input)]
		[Option(AnalogInputPinMode.Input, 11, "Input")]
		public AnalogInputPinMode Mode
		{
			get { return this.mode; }
			set
			{
				this.mode = value;
				this.Initialize();
			}
		}

		[Page(5, "Arduino")]
		[Header(24, "Field Name:", 30)]
		[ToolTip(25, "Name of calculated field.")]
		[DefaultValue("Value")]
		public string FieldName
		{
			get { return this.fieldName; }
			set { this.fieldName = value; }
		}

		[Page(5, "Arduino")]
		[Header(20, "Value Expression:", 40)]
		[ToolTip(21, "Enter expression to scale the raw value into a physical quantity. The raw value will be available in the variable 'Raw'. Any script construct can be used, including adding units. For more information, see: https://waher.se/Script.md")]
		[DefaultValue("Raw")]
		public string Expression
		{
			get { return this.expression; }
			set
			{
				if (string.IsNullOrEmpty(value))
					this.exp = null;
				else
					this.exp = new Expression(value);

				this.expression = value;
			}
		}

		[Page(5, "Arduino")]
		[Header(27, "Unit:", 50)]
		[ToolTip(28, "Unit to use, if one is not provided by the expression above.")]
		[DefaultValueStringEmpty]
		public string Unit
		{
			get { return this.unit; }
			set { this.unit = value; }
		}

		[Page(5, "Arduino")]
		[Header(22, "Number of decimals:", 60)]
		[ToolTip(23, "Number of decimals to use when presenting calculated value.")]
		[DefaultValue(0)]
		public byte NrDecimals
		{
			get { return this.nrDecimals; }
			set { this.nrDecimals = value; }
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 16, "Analog Input");
		}

		public override void Initialize()
		{
			RemoteDevice Device = this.Device;

			if (Device != null)
			{
				switch (Mode)
				{
					case AnalogInputPinMode.Input:
						Device.pinMode(this.PinNrStr, PinMode.ANALOG);
						break;
				}

				this.initialized = true;
			}
			else
				this.initialized = false;
		}

		public void StartReadout(ISensorReadout Request)
		{
			try
			{
				RemoteDevice Device = this.Device;
				if (Device == null)
					throw new Exception("Device not ready.");

				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (!this.initialized)
					this.Initialize();

				if (Request.IsIncluded(FieldType.Momentary))
				{
					ushort Raw = Device.analogRead(this.PinNrStr);
					this.CalcMomentary(Fields, Now, Raw);
				}

				if (Request.IsIncluded(FieldType.Status))
				{
					Fields.Add(new EnumField(this, Now, "Drive Mode", Device.getPinMode(this.PinNr), FieldType.Status, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 15));
				}

				if (Request.IsIncluded(FieldType.Identity))
				{
					Fields.Add(new Int32Field(this, Now, "Pin Number", this.PinNr, FieldType.Identity, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 14));
				}

				Request.ReportFields(true, Fields);
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		private void CalcMomentary(List<Field> Fields, DateTime Now, ushort Raw)
		{
			Fields.Add(new Int32Field(this, Now, "Raw", Raw, FieldType.Momentary, FieldQoS.AutomaticReadout, typeof(Module).Namespace, 26));

			if (this.exp == null && !string.IsNullOrEmpty(this.expression))
				this.exp = new Expression(this.expression);

			if (this.exp != null)
			{
				Variables v = new Variables()
				{
					{ "Raw", (double)Raw }
				};

				object Value = this.exp.Evaluate(v);
				Field F;

				if (Value is double dbl)
					F = new QuantityField(this, Now, this.fieldName, dbl, this.nrDecimals, this.unit, FieldType.Momentary, FieldQoS.AutomaticReadout);
				else if (Value is PhysicalQuantity qty)
					F = new QuantityField(this, Now, this.fieldName, qty.Magnitude, this.nrDecimals, qty.Unit.ToString(), FieldType.Momentary, FieldQoS.AutomaticReadout);
				else if (Value is bool b)
					F = new BooleanField(this, Now, this.fieldName, b, FieldType.Momentary, FieldQoS.AutomaticReadout);
				else if (Value is DateTime DT)
					F = new DateTimeField(this, Now, this.fieldName, DT, FieldType.Momentary, FieldQoS.AutomaticReadout);
				else if (Value is Duration D)
					F = new DurationField(this, Now, this.fieldName, D, FieldType.Momentary, FieldQoS.AutomaticReadout);
				else if (Value is Enum E)
					F = new EnumField(this, Now, this.fieldName, E, FieldType.Momentary, FieldQoS.AutomaticReadout);
				else if (Value is int i32)
				{
					if (string.IsNullOrEmpty(this.unit))
						F = new Int32Field(this, Now, this.fieldName, i32, FieldType.Momentary, FieldQoS.AutomaticReadout);
					else
						F = new QuantityField(this, Now, this.fieldName, i32, 0, this.unit, FieldType.Momentary, FieldQoS.AutomaticReadout);
				}
				else if (Value is long i64)
				{
					if (string.IsNullOrEmpty(this.unit))
						F = new Int64Field(this, Now, this.fieldName, i64, FieldType.Momentary, FieldQoS.AutomaticReadout);
					else
						F = new QuantityField(this, Now, this.fieldName, i64, 0, this.unit, FieldType.Momentary, FieldQoS.AutomaticReadout);
				}
				else if (Value is string s)
					F = new StringField(this, Now, this.fieldName, s, FieldType.Momentary, FieldQoS.AutomaticReadout);
				else if (Value is TimeSpan TS)
					F = new TimeField(this, Now, this.fieldName, TS, FieldType.Momentary, FieldQoS.AutomaticReadout);
				else
					F = new StringField(this, Now, this.fieldName, Value.ToString(), FieldType.Momentary, FieldQoS.AutomaticReadout);

				if (this.fieldName == "Value")
				{
					F.Module = typeof(Module).Namespace;
					F.StringIdSteps = new LocalizationStep[] { new LocalizationStep(13) };
				}

				Fields.Add(F);
			}
		}

		public void Pin_ValueChanged(ushort Value)
		{
			List<Field> Fields = new List<Field>();

			this.CalcMomentary(Fields, DateTime.Now, Value);

			foreach (Field F in Fields)
				Gateway.NewMomentaryValues(this, F);
		}

		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Mode", await Language.GetStringAsync(typeof(Module), 19, "Mode"), this.mode.ToString()));

			return Result;
		}
	}
}
