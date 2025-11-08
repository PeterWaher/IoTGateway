using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Runtime.Timing;
using Waher.Script;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.Virtual.Commands;

namespace Waher.Things.Virtual
{
	/// <summary>
	/// Virtual node, that can be used as a placeholder for services.
	/// </summary>
	public class VirtualNode : ProvisionedMeteringNode, ICustomFormProperties, ISensor, IActuator
	{
		private static Scheduler scheduler = null;

		private readonly Dictionary<string, SensorData.Field> fields = new Dictionary<string, SensorData.Field>();
		private List<SensorData.Field> toReport = null;
		private DateTime nextReport = DateTime.MinValue;
		private bool hasReport = false;

		/// <summary>
		/// Virtual node, that can be used as a placeholder for services.
		/// </summary>
		public VirtualNode()
			: base()
		{
		}

		/// <summary>
		/// Meta-data attached to virtual node.
		/// </summary>
		public MetaDataValue[] MetaData { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(VirtualNode), 1, "Virtual Node");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override async Task<bool> AcceptsChildAsync(INode Child)
		{
			return Child is VirtualNode || await Child.AcceptsParentAsync(this);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(
				Parent is Root || 
				Parent is NodeCollection ||
				Parent is VirtualNode);
		}

		/// <summary>
		/// Annotates the property form.
		/// </summary>
		/// <param name="Form">Form being built.</param>
		public override async Task AnnotatePropertyForm(FormState Form)
		{
			await base.AnnotatePropertyForm(Form);

			if ((this.MetaData?.Length ?? 0) > 0)
			{
				Language Language = await Translator.GetLanguageAsync(Form.LanguageCode);
				Namespace Namespace = await Language.GetNamespaceAsync(typeof(VirtualNode).Namespace);
				string PageLabel = await Namespace.GetStringAsync(2, "Meta-data");
				string ExternalDescription = await Namespace.GetStringAsync(3, "Meta-data value is defined by external source.");
				Page MetaDataPage = new Page(Form.Form, PageLabel)
				{
					Ordinal = Form.PageOrdinal++
				};
				Field Field;

				Form.Pages.Add(MetaDataPage);
				Form.PageByLabel[PageLabel] = MetaDataPage;

				foreach (MetaDataValue Tag in this.MetaData)
				{
					if (Tag.Value is string s)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { s }, null, ExternalDescription, new StringDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is int i)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { i.ToString() }, null, ExternalDescription, new IntegerDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is long l)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { l.ToString() }, null, ExternalDescription, new LongDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is short sh)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { sh.ToString() }, null, ExternalDescription, new ShortDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is byte b2)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { b2.ToString() }, null, ExternalDescription, new ByteDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is double d)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { CommonTypes.Encode(d) }, null, ExternalDescription, new DoubleDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is decimal d2)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { CommonTypes.Encode(d2) }, null, ExternalDescription, new DecimalDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is bool b)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { CommonTypes.Encode(b) }, null, ExternalDescription, new BooleanDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is TimeSpan TS)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { TS.ToString() }, null, ExternalDescription, new TimeDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is DateTime TP)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { XML.Encode(TP) }, null, ExternalDescription, new DateTimeDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is Uri Uri)
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { Uri.ToString() }, null, ExternalDescription, new AnyUriDataType(),
							null, null, false, false, false);
					}
					else if (Tag.Value is string[] Rows)
					{
						Field = new TextMultiField(Form.Form, Tag.Name, Tag.Name, false,
							Rows, null, ExternalDescription, null, null, null, false, false, false);
					}
					else
					{
						Field = new TextSingleField(Form.Form, Tag.Name, Tag.Name, false,
							new string[] { Tag.Value?.ToString() ?? string.Empty }, null, ExternalDescription, null,
							null, null, false, false, false);
					}

					Field.Ordinal = Form.FieldOrdinal++;
					Form.Fields.Add(Field);
					MetaDataPage.Add(new FieldReference(Form.Form, Field.Var));
				}
			}
		}

		/// <summary>
		/// Tries to get a meta-data value
		/// </summary>
		/// <param name="Name">Meta-data name</param>
		/// <param name="Value">Value, if meta-data tag found.</param>
		/// <returns>If a meta-data tag was found with the corresponding name.</returns>
		public bool TryGetMetaDataValue(string Name, out object Value)
		{
			if (this.metaDataByName is null)
				this.BuildDictionary();

			if (this.metaDataByName.TryGetValue(Name, out MetaDataValue Tag))
			{
				Value = Tag.Value;
				return true;
			}
			else
			{
				Value = null;
				return false;
			}
		}

		private void BuildDictionary()
		{
			SortedDictionary<string, MetaDataValue> ByName = new SortedDictionary<string, MetaDataValue>();

			if (!(this.MetaData is null))
			{
				foreach (MetaDataValue P in this.MetaData)
					ByName[P.Name] = P;
			}

			this.metaDataByName = ByName;
		}

		private SortedDictionary<string, MetaDataValue> metaDataByName = null;

		/// <summary>
		/// Performs custom validation of a property.
		/// </summary>
		/// <param name="Field">Property field.</param>
		public Task ValidateCustomProperty(Field Field)
		{
			if (this.TryGetMetaDataValue(Field.Var, out object Prev))
			{
				try
				{
					if (Prev is string)
						return Task.CompletedTask;
					else if (Prev is int)
					{
						if (!int.TryParse(Field.ValueString, out _))
							Field.Error = "Value must be a valid integer.";
					}
					else if (Prev is long)
					{
						if (!long.TryParse(Field.ValueString, out _))
							Field.Error = "Value must be a valid long integer.";
					}
					else if (Prev is short)
					{
						if (!short.TryParse(Field.ValueString, out _))
							Field.Error = "Value must be a valid short integer.";
					}
					else if (Prev is byte)
					{
						if (!byte.TryParse(Field.ValueString, out _))
							Field.Error = "Value must be a valid byte.";
					}
					else if (Prev is double)
					{
						if (!CommonTypes.TryParse(Field.ValueString, out double _))
							Field.Error = "Value must be a valid double-precision floating-point value.";
					}
					else if (Prev is decimal)
					{
						if (!CommonTypes.TryParse(Field.ValueString, out decimal _))
							Field.Error = "Value must be a valid decimal-precision floating-point value.";
					}
					else if (Prev is bool)
					{
						if (!CommonTypes.TryParse(Field.ValueString, out bool _))
							Field.Error = "Value must be a valid boolean value.";
					}
					else if (Prev is TimeSpan)
					{
						if (!TimeSpan.TryParse(Field.ValueString, out _))
							Field.Error = "Value must be a valid TimeSpan value.";
					}
					else if (Prev is DateTime)
					{
						if (!XML.TryParse(Field.ValueString, out DateTime _))
							Field.Error = "Value must be a valid DateTime value.";
					}
					else if (Prev is Uri)
					{
						if (!Uri.TryCreate(Field.ValueString, UriKind.Absolute, out _))
							Field.Error = "Value must be a valid URI value.";
					}
					else if (Prev is string[])
					{
						return Task.CompletedTask;
					}
					else
					{
						return Task.CompletedTask;
					}
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);
					Field.Error = ex.Message;
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Sets the custom parameter to the value(s) provided in the field.
		/// </summary>
		/// <param name="Field">Field</param>
		public Task SetCustomProperty(Field Field)
		{
			if (this.metaDataByName is null)
				this.BuildDictionary();

			if (this.metaDataByName.TryGetValue(Field.Var, out MetaDataValue Prev))
			{
				try
				{
					if (Prev.Value is string)
						Prev.Value = Field.ValueString;
					else if (Prev.Value is int)
					{
						if (int.TryParse(Field.ValueString, out int i))
							Prev.Value = i;
					}
					else if (Prev.Value is long)
					{
						if (long.TryParse(Field.ValueString, out long l))
							Prev.Value = l;
					}
					else if (Prev.Value is short)
					{
						if (short.TryParse(Field.ValueString, out short sh))
							Field.Error = "Value must be a valid short integer.";
					}
					else if (Prev.Value is byte)
					{
						if (byte.TryParse(Field.ValueString, out byte b))
							Prev.Value = b;
					}
					else if (Prev.Value is double)
					{
						if (CommonTypes.TryParse(Field.ValueString, out double d))
							Prev.Value = d;
					}
					else if (Prev.Value is decimal)
					{
						if (CommonTypes.TryParse(Field.ValueString, out decimal d))
							Prev.Value = d;
					}
					else if (Prev.Value is bool)
					{
						if (CommonTypes.TryParse(Field.ValueString, out bool b))
							Prev.Value = b;
					}
					else if (Prev.Value is TimeSpan)
					{
						if (TimeSpan.TryParse(Field.ValueString, out TimeSpan TS))
							Prev.Value = TS;
					}
					else if (Prev.Value is DateTime)
					{
						if (XML.TryParse(Field.ValueString, out DateTime TP))
							Prev.Value = TP;
					}
					else if (Prev.Value is Uri)
					{
						if (Uri.TryCreate(Field.ValueString, UriKind.Absolute, out Uri Url))
							Prev.Value = Url;
					}
					else if (Prev.Value is string[])
						Prev.Value = Field.ValueStrings;
					else
						Prev.Value = Field.ValueString;
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);
					Field.Error = ex.Message;
				}
			}
			else
				this.SetMetaDataPriv(Field.Var, Field.ValueString);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets a meta-data value, if available.
		/// </summary>
		/// <param name="Name">Name of meta-data tag.</param>
		/// <returns>Meta-data value, if found, or null otherwise.</returns>
		public object GetMetaData(string Name)
		{
			if (this.TryGetMetaDataValue(Name, out object Value))
				return Value;
			else
				return null;
		}

		/// <summary>
		/// Sets a meta-data value.
		/// </summary>
		/// <param name="Name">Name of meta-data tag.</param>
		/// <param name="Value">Value of meta-data tag.</param>
		public async Task SetMetaData(string Name, object Value)
		{
			if (this.metaDataByName is null)
				this.BuildDictionary();

			if (this.metaDataByName.TryGetValue(Name, out MetaDataValue Tag))
				Tag.Value = Value;
			else
				this.SetMetaDataPriv(Name, Value);

			await Database.Update(this);
		}

		private void SetMetaDataPriv(string Name, object Value)
		{
			this.metaDataByName[Name] = new MetaDataValue()
			{
				Name = Name,
				Value = Value
			};

			MetaDataValue[] Values = new MetaDataValue[this.metaDataByName.Count];
			this.metaDataByName.Values.CopyTo(Values, 0);
			this.MetaData = Values;
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

			if (!(this.MetaData is null))
			{
				foreach (MetaDataValue Tag in this.MetaData)
				{
					if (Tag.Value is string s)
						Result.AddLast(new StringParameter(Tag.Name, Tag.Name, s));
					else if (Tag.Value is int i)
						Result.AddLast(new Int32Parameter(Tag.Name, Tag.Name, i));
					else if (Tag.Value is long l)
						Result.AddLast(new Int64Parameter(Tag.Name, Tag.Name, l));
					else if (Tag.Value is short sh)
						Result.AddLast(new Int32Parameter(Tag.Name, Tag.Name, sh));
					else if (Tag.Value is byte b)
						Result.AddLast(new Int32Parameter(Tag.Name, Tag.Name, b));
					else if (Tag.Value is double d)
						Result.AddLast(new DoubleParameter(Tag.Name, Tag.Name, d));
					else if (Tag.Value is decimal d2)
						Result.AddLast(new DoubleParameter(Tag.Name, Tag.Name, (double)d2));
					else if (Tag.Value is bool b2)
						Result.AddLast(new BooleanParameter(Tag.Name, Tag.Name, b2));
					else if (Tag.Value is TimeSpan TS)
						Result.AddLast(new TimeSpanParameter(Tag.Name, Tag.Name, TS));
					else if (Tag.Value is DateTime TP)
						Result.AddLast(new DateTimeParameter(Tag.Name, Tag.Name, TP));
					else if (Tag.Value is Uri Uri)
						Result.AddLast(new StringParameter(Tag.Name, Tag.Name, Uri.ToString()));
				}
			}

			return Result;
		}

		/// <summary>
		/// Reports sensor data on the node.
		/// </summary>
		/// <param name="Field">Parsed field.</param>
		public void ReportSensorData(SensorData.Field Field)
		{
			this.ReportSensorData(new SensorData.Field[] { Field });
		}

		/// <summary>
		/// Reports sensor data on the node.
		/// </summary>
		/// <param name="Fields">Parsed fields.</param>
		public void ReportSensorData(params SensorData.Field[] Fields)
		{
			if (scheduler is null && Types.TryGetModuleParameter("Scheduler", out Scheduler Scheduler))
				scheduler = Scheduler;

			lock (this.fields)
			{
				foreach (SensorData.Field Field in Fields)
				{
					this.fields[Field.Name] = Field;

					if (Field.Type.HasFlag(SensorData.FieldType.Momentary))
					{
						this.toReport ??= new List<SensorData.Field>();
						this.toReport.Add(Field);
						this.hasReport = true;
					}
				}

				if (this.hasReport)
				{
					if (scheduler is null)
					{
						this.NewMomentaryValues(this.toReport.ToArray());
						this.toReport.Clear();
						this.hasReport = false;
					}
					else
					{
						if (this.nextReport != DateTime.MinValue)
							scheduler.Remove(this.nextReport);

						this.nextReport = scheduler.Add(DateTime.Now.AddMilliseconds(250), this.DoReport, null);
					}
				}
			}
		}

		private void DoReport(object _)
		{
			lock (this.fields)
			{
				this.NewMomentaryValues(this.toReport.ToArray());
				this.toReport.Clear();
				this.hasReport = false;
			}
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public Task StartReadout(ISensorReadout Request)
		{
			return this.StartReadout(Request, true);
		}

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public override bool IsReadable
		{
			get
			{
				if (this.Disabled)
					return false;

				lock (this.fields)
				{
					return this.fields.Count > 0;
				}
			}
		}

        /// <summary>
        /// Starts the readout of the sensor.
        /// </summary>
        /// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
        /// <param name="DoneAfter">If readout is done after reporting fields (true), or if more fields will
        /// be reported by the caller (false).</param>
        public virtual Task StartReadout(ISensorReadout Request, bool DoneAfter)
		{
			List<SensorData.Field> ToReport = new List<SensorData.Field>();

			lock (this.fields)
			{
				foreach (SensorData.Field Field in this.fields.Values)
				{
					if (Request.IsIncluded(Field.Name, Field.Timestamp, Field.Type))
						ToReport.Add(Field);
				}
			}

			if (DoneAfter || ToReport.Count > 0)
				Request.ReportFields(DoneAfter, ToReport.ToArray());

			return Task.CompletedTask;
		}

		/// <summary>
		/// If the node can be controlled.
		/// </summary>
		public override bool IsControllable 
		{
			get
			{
				if (this.Disabled)
					return false;

				if (this.TryGetMetaDataValue("Callback", out object Obj) && Obj is string &&
					this.TryGetMetaDataValue("Payload", out Obj) && Obj is string &&
					this.TryGetMetaDataValue("FieldName", out Obj) && Obj is string &&
					this.TryGetMetaDataValue("FieldValue", out object FieldValue))
				{
					return
						FieldValue is double ||
						FieldValue is string ||
						FieldValue is bool ||
						FieldValue is Enum ||
						FieldValue is DateTime ||
						FieldValue is TimeSpan ||
						FieldValue is Duration ||
						FieldValue is sbyte ||
						FieldValue is short ||
						FieldValue is int ||
						FieldValue is long ||
						FieldValue is byte ||
						FieldValue is ushort ||
						FieldValue is uint ||
						FieldValue is ulong;
				}
				else
					return false;
			}
		}

        /// <summary>
        /// Get control parameters for the actuator.
        /// </summary>
        /// <returns>Collection of control parameters for actuator.</returns>
        public virtual Task<ControlParameter[]> GetControlParameters()
		{
			List<ControlParameter> Parameters = new List<ControlParameter>();

			if (this.TryGetMetaDataValue("Callback", out object Obj) && Obj is string CallbackUrl &&
				this.TryGetMetaDataValue("Payload", out Obj) && Obj is string PayloadScript &&
				this.TryGetMetaDataValue("FieldName", out Obj) && Obj is string FieldName &&
				this.TryGetMetaDataValue("FieldValue", out object FieldValue))
			{
				if (FieldValue is double d)
				{
					Parameters.Add(new DoubleControlParameter(FieldName, "Control", FieldName + ":", "Value to set.",
						_ => Task.FromResult<double?>(d),
						async (_, Value) =>
						{
							d = Value;
							await this.DoCallback(CallbackUrl, PayloadScript, d);
						}));
				}
				else if (FieldValue is string s)
				{
					Parameters.Add(new StringControlParameter(FieldName, "Control", FieldName + ":", "Value to set.",
						_ => Task.FromResult<string>(s),
						async (_, Value) =>
						{
							s = Value;
							await this.DoCallback(CallbackUrl, PayloadScript, s);
						}));
				}
				else if (FieldValue is bool b)
				{
					Parameters.Add(new BooleanControlParameter(FieldName, "Control", FieldName + ":", "Value to set.",
						_ => Task.FromResult<bool?>(b),
						async (_, Value) =>
						{
							b = Value;
							await this.DoCallback(CallbackUrl, PayloadScript, b);
						}));
				}
				else if (FieldValue is Enum e)
				{
					Parameters.Add(new EnumControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", e.GetType(),
						_ => Task.FromResult<Enum>(e),
						async (_, Value) =>
						{
							e = Value;
							await this.DoCallback(CallbackUrl, PayloadScript, e);
						}));
				}
				else if (FieldValue is DateTime DT)
				{
					Parameters.Add(new DateTimeControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", null, null,
						_ => Task.FromResult<DateTime?>(DT),
						async (_, Value) =>
						{
							DT = Value;
							await this.DoCallback(CallbackUrl, PayloadScript, DT);
						}));
				}
				else if (FieldValue is TimeSpan TS)
				{
					Parameters.Add(new TimeControlParameter(FieldName, "Control", FieldName + ":", "Value to set.",
						_ => Task.FromResult<TimeSpan?>(TS),
						async (_, Value) =>
						{
							TS = Value;
							await this.DoCallback(CallbackUrl, PayloadScript, TS);
						}));
				}
				else if (FieldValue is Duration D)
				{
					Parameters.Add(new DurationControlParameter(FieldName, "Control", FieldName + ":", "Value to set.",
						_ => Task.FromResult<Duration>(D),
						async (_, Value) =>
						{
							D = Value;
							await this.DoCallback(CallbackUrl, PayloadScript, D);
						}));
				}
				else if (FieldValue is sbyte i8)
				{
					Parameters.Add(new Int32ControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", sbyte.MinValue, sbyte.MaxValue,
						_ => Task.FromResult<int?>(i8),
						async (_, Value) =>
						{
							i8 = (sbyte)Value;
							await this.DoCallback<int>(CallbackUrl, PayloadScript, i8);
						}));
				}
				else if (FieldValue is short i16)
				{
					Parameters.Add(new Int32ControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", short.MinValue, short.MaxValue,
						_ => Task.FromResult<int?>(i16),
						async (_, Value) =>
						{
							i16 = (short)Value;
							await this.DoCallback<int>(CallbackUrl, PayloadScript, i16);
						}));
				}
				else if (FieldValue is int i32)
				{
					Parameters.Add(new Int32ControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", null, null,
						_ => Task.FromResult<int?>(i32),
						async (_, Value) =>
						{
							i32 = Value;
							await this.DoCallback<int>(CallbackUrl, PayloadScript, i32);
						}));
				}
				else if (FieldValue is long i64)
				{
					Parameters.Add(new Int64ControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", null, null,
						_ => Task.FromResult<long?>(i64),
						async (_, Value) =>
						{
							i64 = Value;
							await this.DoCallback<long>(CallbackUrl, PayloadScript, i64);
						}));
				}
				else if (FieldValue is byte ui8)
				{
					Parameters.Add(new Int32ControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", byte.MinValue, byte.MaxValue,
						_ => Task.FromResult<int?>(ui8),
						async (_, Value) =>
						{
							ui8 = (byte)Value;
							await this.DoCallback<int>(CallbackUrl, PayloadScript, ui8);
						}));
				}
				else if (FieldValue is ushort ui16)
				{
					Parameters.Add(new Int32ControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", ushort.MinValue, ushort.MaxValue,
						_ => Task.FromResult<int?>(ui16),
						async (_, Value) =>
						{
							ui16 = (ushort)Value;
							await this.DoCallback<int>(CallbackUrl, PayloadScript, ui16);
						}));
				}
				else if (FieldValue is uint ui32)
				{
					Parameters.Add(new Int64ControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", uint.MinValue, uint.MaxValue,
						_ => Task.FromResult<long?>(ui32),
						async (_, Value) =>
						{
							ui32 = (uint)Value;
							await this.DoCallback<long>(CallbackUrl, PayloadScript, ui32);
						}));
				}
				else if (FieldValue is ulong ui64)
				{
					Parameters.Add(new DoubleControlParameter(FieldName, "Control", FieldName + ":", "Value to set.", ulong.MinValue, ulong.MaxValue,
						_ => Task.FromResult<double?>(ui64),
						async (_, Value) =>
						{
							ui64 = (ulong)Value;
							await this.DoCallback<double>(CallbackUrl, PayloadScript, ui64);
						}));
				}
			}

			return Task.FromResult(Parameters.ToArray());
		}

		private async Task DoCallback<T>(string CallbackUrl, string PayloadScript, T Value)
		{
			Variables v = new Variables
			{
				{ "Value", Value }
			};

			object Payload = await Expression.EvalAsync(PayloadScript, v);
			ContentResponse Content = await InternetContent.PostAsync(new Uri(CallbackUrl), Payload);
			Content.AssertOk();
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Commands = new List<ICommand>();
			Commands.AddRange(await base.Commands);

			Commands.Add(new AddMetaDataString(this));
			Commands.Add(new AddMetaDataInt32(this));
			Commands.Add(new AddMetaDataInt64(this));
			Commands.Add(new AddMetaDataDouble(this));
			Commands.Add(new AddMetaDataBoolean(this));
			Commands.Add(new AddMetaDataDateTime(this));
			Commands.Add(new AddMetaDataTimeSpan(this));
			Commands.Add(new AddMetaDataDuration(this));

			return Commands.ToArray();
		}

	}
}
