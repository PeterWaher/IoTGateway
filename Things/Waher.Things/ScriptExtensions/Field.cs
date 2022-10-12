using System;
using Waher.Content;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Things.SensorData;

namespace Waher.Things.ScriptExtensions.SensorData
{
	/// <summary>
	/// Creates a Boolean field.
	/// </summary>
	public class Field : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a Boolean field.
		/// </summary>
		/// <param name="Thing">Thing reference</param>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Field value.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Field(ScriptNode Thing, ScriptNode Name, ScriptNode Value, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Thing, Name, Value }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a Boolean field.
		/// </summary>
		/// <param name="Thing">Thing reference</param>
		/// <param name="Timestamp">Timestamp of value.</param>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Field value.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Field(ScriptNode Thing, ScriptNode Timestamp, ScriptNode Name, ScriptNode Value,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Thing, Timestamp, Name, Value }, argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a Boolean field.
		/// </summary>
		/// <param name="Thing">Thing reference</param>
		/// <param name="Timestamp">Timestamp of value.</param>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Field value.</param>
		/// <param name="Type">Field Type</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Field(ScriptNode Thing, ScriptNode Timestamp, ScriptNode Name, ScriptNode Value, ScriptNode Type,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Thing, Timestamp, Name, Value, Type }, argumentTypes5Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a Boolean field.
		/// </summary>
		/// <param name="Thing">Thing reference</param>
		/// <param name="Timestamp">Timestamp of value.</param>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Field value.</param>
		/// <param name="Type">Field Type</param>
		/// <param name="QoS">Quality of Service</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Field(ScriptNode Thing, ScriptNode Timestamp, ScriptNode Name, ScriptNode Value, ScriptNode Type, ScriptNode QoS,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Thing, Timestamp, Name, Value, Type, QoS }, argumentTypes6Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a Boolean field.
		/// </summary>
		/// <param name="Thing">Thing reference</param>
		/// <param name="Timestamp">Timestamp of value.</param>
		/// <param name="Name">Field name.</param>
		/// <param name="Value">Field value.</param>
		/// <param name="Type">Field Type</param>
		/// <param name="QoS">Quality of Service</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		/// <param name="Writable">If field is writable</param>
		public Field(ScriptNode Thing, ScriptNode Timestamp, ScriptNode Name, ScriptNode Value, ScriptNode Type, ScriptNode QoS,
			ScriptNode Writable, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Thing, Timestamp, Name, Value, Type, QoS, Writable }, argumentTypes7Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Field);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Thing", "Timestamp", "Name", "Value", "Type", "QoS" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0].AssociatedObjectValue is IThingReference Thing))
				throw new ScriptRuntimeException("First argument must be a thing reference.", this);

			if (!(Thing is Things.ThingReference ThingRef))
				ThingRef = new Things.ThingReference(Thing.NodeId, Thing.SourceId, Thing.Partition);

			int c = Arguments.Length;
			int i;
			DateTime TP;
			object Obj;

			if (c > 3)
			{
				if ((Obj = Arguments[1].AssociatedObjectValue) is DateTime TP0)
					TP = TP0;
				else
					TP = (DateTime)Expression.ConvertTo(Obj, typeof(DateTime), this);
				i = 2;
			}
			else
			{
				TP = DateTime.UtcNow;
				i = 1;
			}

			string FieldName = Arguments[i++].AssociatedObjectValue?.ToString() ?? string.Empty;
			object ValueObject = Arguments[i++].AssociatedObjectValue;
			FieldType Type = FieldType.Momentary;
			FieldQoS QoS = FieldQoS.AutomaticReadout;
			bool Writable = false;

			if (i < c)
			{
				Obj = Arguments[i++].AssociatedObjectValue;

				if (Obj is FieldType T || Enum.TryParse<FieldType>(Obj?.ToString() ?? string.Empty, out T))
					Type = T;
				else
					throw new ScriptRuntimeException("Expected field type.", this);

				if (i < c)
				{
					Obj = Arguments[i++].AssociatedObjectValue;

					if (Obj is FieldQoS Q || Enum.TryParse<FieldQoS>(Obj?.ToString() ?? string.Empty, out Q))
						QoS = Q;
					else
						throw new ScriptRuntimeException("Expected field quality of service.", this);

					if (i < c)
					{
						bool? b = ToBoolean(Arguments[i++]);
						if (b.HasValue)
							Writable = b.Value;
						else
							throw new ScriptRuntimeException("Expected boolean writable argument.", this);
					}
				}
			}

			if (ValueObject is double d)
				return new ObjectValue(new QuantityField(ThingRef, TP, FieldName, d, CommonTypes.GetNrDecimals(d), string.Empty, Type, QoS, Writable));
			if (ValueObject is PhysicalQuantity PQ)
				return new ObjectValue(new QuantityField(ThingRef, TP, FieldName, PQ.Magnitude, CommonTypes.GetNrDecimals(PQ.Magnitude), PQ.Unit.ToString(), Type, QoS, Writable));
			else if (ValueObject is string s)
				return new ObjectValue(new StringField(ThingRef, TP, FieldName, s, Type, QoS, Writable));
			else if (ValueObject is bool b)
				return new ObjectValue(new BooleanField(ThingRef, TP, FieldName, b, Type, QoS, Writable));
			else if (ValueObject is Enum e)
				return new ObjectValue(new EnumField(ThingRef, TP, FieldName, e, Type, QoS, Writable));
			else if (ValueObject is DateTime DT)
				return new ObjectValue(new DateTimeField(ThingRef, TP, FieldName, DT, Type, QoS, Writable));
			else if (ValueObject is TimeSpan TS)
				return new ObjectValue(new TimeField(ThingRef, TP, FieldName, TS, Type, QoS, Writable));
			else if (ValueObject is Duration D)
				return new ObjectValue(new DurationField(ThingRef, TP, FieldName, D, Type, QoS, Writable));
			else if (ValueObject is sbyte i8)
				return new ObjectValue(new Int32Field(ThingRef, TP, FieldName, i8, Type, QoS, Writable));
			else if (ValueObject is short i16)
				return new ObjectValue(new Int32Field(ThingRef, TP, FieldName, i16, Type, QoS, Writable));
			else if (ValueObject is int i32)
				return new ObjectValue(new Int32Field(ThingRef, TP, FieldName, i32, Type, QoS, Writable));
			else if (ValueObject is long i64)
				return new ObjectValue(new Int64Field(ThingRef, TP, FieldName, i64, Type, QoS, Writable));
			else if (ValueObject is byte ui8)
				return new ObjectValue(new Int32Field(ThingRef, TP, FieldName, ui8, Type, QoS, Writable));
			else if (ValueObject is ushort ui16)
				return new ObjectValue(new Int32Field(ThingRef, TP, FieldName, ui16, Type, QoS, Writable));
			else if (ValueObject is uint ui32)
				return new ObjectValue(new Int64Field(ThingRef, TP, FieldName, ui32, Type, QoS, Writable));
			else if (ValueObject is ulong ui64)
				return new ObjectValue(new QuantityField(ThingRef, TP, FieldName, ui64, 0, string.Empty, Type, QoS, Writable));
			else
				throw new ScriptRuntimeException("Field type not supported.", this);
		}

	}
}
