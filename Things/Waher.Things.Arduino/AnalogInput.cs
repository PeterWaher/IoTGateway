using Microsoft.Maker.RemoteWiring;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
	/// <summary>
	/// TODO
	/// </summary>
	public enum AnalogInputPinMode
	{
		/// <summary>
		/// TODO
		/// </summary>
		Input
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class AnalogInput : AnalogPin, ISensor
	{
		private AnalogInputPinMode mode = AnalogInputPinMode.Input;
		private string expression = "Raw";
		private string fieldName = "Value";
		private string unit = string.Empty;
		private Expression exp = null;
		private byte nrDecimals = 0;
		private bool initialized = false;

		/// <summary>
		/// TODO
		/// </summary>
		public AnalogInput()
			: base()
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(5, "Arduino")]
		[Header(9, "Mode:", 20)]
		[ToolTip(10, "Select drive mode of pin.")]
		[DefaultValue(AnalogInputPinMode.Input)]
		[Option(AnalogInputPinMode.Input, 11, "Input")]
		public AnalogInputPinMode Mode
		{
			get => this.mode;
			set
			{
				this.mode = value;
				this.Initialize();
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(5, "Arduino")]
		[Header(24, "Field Name:", 30)]
		[ToolTip(25, "Name of calculated field.")]
		[DefaultValue("Value")]
		public string FieldName
		{
			get => this.fieldName;
			set => this.fieldName = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(5, "Arduino")]
		[Header(20, "Value Expression:", 40)]
		[ToolTip(21, "Enter expression to scale the raw value into a physical quantity. The raw value will be available in the variable 'Raw'. Any script construct can be used, including adding units. For more information, see: https://waher.se/Script.md")]
		[DefaultValue("Raw")]
		public string Expression
		{
			get => this.expression;
			set
			{
				if (string.IsNullOrEmpty(value))
					this.exp = null;
				else
					this.exp = new Expression(value);

				this.expression = value;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(5, "Arduino")]
		[Header(27, "Unit:", 50)]
		[ToolTip(28, "Unit to use, if one is not provided by the expression above.")]
		[DefaultValueStringEmpty]
		public string Unit
		{
			get => this.unit;
			set => this.unit = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		[Page(5, "Arduino")]
		[Header(22, "Number of decimals:", 60)]
		[ToolTip(23, "Number of decimals to use when presenting calculated value.")]
		[DefaultValue(0)]
		public byte NrDecimals
		{
			get => this.nrDecimals;
			set => this.nrDecimals = value;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 16, "Analog Input");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void Initialize()
		{
			RemoteDevice Device = this.GetDevice().Result;	// TODO: Avoid blocking call.

			if (!(Device is null))
			{
				switch (this.Mode)
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

		/// <summary>
		/// TODO
		/// </summary>
		public async Task StartReadout(ISensorReadout Request)
		{
			try
			{
				RemoteDevice Device = await this.GetDevice()
					?? throw new Exception("Device not ready.");

				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (!this.initialized)
					this.Initialize();

				if (Request.IsIncluded(FieldType.Momentary))
				{
					ushort Raw = Device.analogRead(this.PinNrStr);
					await this.CalcMomentary(Fields, Now, Raw);
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

					this.AddIdentityReadout(Fields, Now);
				}

				await Request.ReportFields(true, Fields);
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		private async Task CalcMomentary(List<Field> Fields, DateTime Now, ushort Raw)
		{
			Fields.Add(new Int32Field(this, Now, "Raw", Raw, FieldType.Momentary, FieldQoS.AutomaticReadout, typeof(Module).Namespace, 26));

			if (this.exp is null && !string.IsNullOrEmpty(this.expression))
				this.exp = new Expression(this.expression);

			if (!(this.exp is null))
			{
				Variables v = new Variables()
				{
					{ "Raw", (double)Raw }
				};

				object Value = await this.exp.EvaluateAsync(v);
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

		/// <summary>
		/// TODO
		/// </summary>
		public async Task Pin_ValueChanged(ushort Value)
		{
			List<Field> Fields = new List<Field>();

			await this.CalcMomentary(Fields, DateTime.Now, Value);

			foreach (Field F in Fields)
				this.NewMomentaryValues(F);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Mode", await Language.GetStringAsync(typeof(Module), 19, "Mode"), this.mode.ToString()));

			return Result;
		}
	}
}
