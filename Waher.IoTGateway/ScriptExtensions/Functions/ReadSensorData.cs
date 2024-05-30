using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Security;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Reads sensor data from a sensor node.
	/// </summary>
	public class ReadSensorData : FunctionMultiVariate
	{
		/// <summary>
		/// Reads sensor data from a sensor node.
		/// </summary>
		/// <param name="Sensor">Sensor object.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ReadSensorData(ScriptNode Sensor, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Sensor }, new ArgumentType[] { ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Reads sensor data from a sensor node.
		/// </summary>
		/// <param name="Sensor">Sensor object.</param>
		/// <param name="FieldTypes">Field Types to read.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ReadSensorData(ScriptNode Sensor, ScriptNode FieldTypes, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Sensor, FieldTypes }, new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Reads sensor data from a sensor node.
		/// </summary>
		/// <param name="Sensor">Sensor object.</param>
		/// <param name="FieldTypes">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ReadSensorData(ScriptNode Sensor, ScriptNode FieldTypes, ScriptNode Fields, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Sensor, FieldTypes, Fields },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Reads sensor data from a sensor node.
		/// </summary>
		/// <param name="Sensor">Sensor object.</param>
		/// <param name="FieldTypes">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">Read from this timepoint.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ReadSensorData(ScriptNode Sensor, ScriptNode FieldTypes, ScriptNode Fields, ScriptNode From,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Sensor, FieldTypes, Fields, From },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Reads sensor data from a sensor node.
		/// </summary>
		/// <param name="Sensor">Sensor object.</param>
		/// <param name="FieldTypes">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">Read from this timepoint.</param>
		/// <param name="To">Read to this timepoint.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ReadSensorData(ScriptNode Sensor, ScriptNode FieldTypes, ScriptNode Fields, ScriptNode From, ScriptNode To,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Sensor, FieldTypes, Fields, From, To },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ReadSensorData);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Sensor", "FieldTypes", "Fields", "From", "To" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			int i = 0;
			int c = Arguments.Length;

			if (!(Arguments[i++].AssociatedObjectValue is ISensor Sensor))
				throw new ScriptRuntimeException("Expected sensor in first argument.", this);

			FieldType FieldTypes;
			string[] FieldNames;
			DateTime From, To;
			object Obj;

			if (i < c)
			{
				Obj = Arguments[i++].AssociatedObjectValue;

				if (Obj is FieldType Types || Enum.TryParse(Obj?.ToString() ?? string.Empty, out Types))
					FieldTypes = Types;
				else
					throw new ScriptRuntimeException("Expected a FieldTypes enumeration as second argument.", this);
			}
			else
				FieldTypes = FieldType.Momentary;

			if (i < c)
			{
				Obj = Arguments[i++].AssociatedObjectValue;

				if (Obj is string[] Names)
					FieldNames = Names;
				else if (Obj is object[] Objects)
				{
					int j, d = Objects.Length;

					FieldNames = new string[d];

					for (j = 0; j < d; j++)
					{
						if (Objects[j] is string s)
							FieldNames[j] = s;
						else
							throw new ScriptRuntimeException("Expected a string array or null as third argument.", this);
					}
				}
				else if (Obj is null)
					FieldNames = null;
				else
					throw new ScriptRuntimeException("Expected a string array or null as third argument.", this);
			}
			else
				FieldNames = null;

			if (i < c)
			{
				Obj = Arguments[i++].AssociatedObjectValue;

				if (Obj is DateTime TP)
					From = TP;
				else if (Obj is null)
					From = DateTime.MinValue;
				else
					throw new ScriptRuntimeException("Expected a Date & Time or null as fourth argument.", this);
			}
			else
				From = DateTime.MinValue;

			if (i < c)
			{
				Obj = Arguments[i++].AssociatedObjectValue;

				if (Obj is DateTime TP)
					To = TP;
				else if (Obj is null)
					To = DateTime.MaxValue;
				else
					throw new ScriptRuntimeException("Expected a Date & Time as fifth argument.", this);
			}
			else
				To = DateTime.MaxValue;

			TaskCompletionSource<bool> ReadoutCompleted = new TaskCompletionSource<bool>();
			Dictionary<string, List<Field>> Fields = new Dictionary<string, List<Field>>();
			List<ThingError> Errors = new List<ThingError>();
			bool Error = false;

			RequestOrigin Origin = await GetNode.GetOriginOfRequest(Variables);
			IThingReference[] Nodes = new IThingReference[] { Sensor };
			ApprovedReadoutParameters Approval = await Gateway.ConcentratorServer.SensorServer.CanReadAsync(FieldTypes, Nodes, FieldNames, Origin)
				?? throw new ScriptRuntimeException("Not authorized to read sensor-data from node.", this);
			string Actor = Origin is IUser User ? User.UserName : nameof(ReadSensorData);

			InternalReadoutRequest Request = Gateway.ConcentratorServer.SensorServer.DoInternalReadout(Actor,
				Approval.Nodes, Approval.FieldTypes, Approval.FieldNames, From, To,
				(sender, e) =>
				{
					foreach (Field F in e.Fields)
					{
						if (Fields.TryGetValue(F.Name, out List<Field> List))
							List.Add(F);
						else
							Fields[F.Name] = new List<Field>() { F };
					}

					if (e.Done)
						ReadoutCompleted.TrySetResult(true);

					return Task.CompletedTask;
				},
				(sender, e) =>
				{
					Errors.AddRange(e.Errors);
					Error = true;

					if (e.Done)
						ReadoutCompleted.TrySetResult(true);

					return Task.CompletedTask;
				}, null);

			Task Timeout = Task.Delay(60000);
			Task T = await Task.WhenAny(ReadoutCompleted.Task, Timeout);

			if (!ReadoutCompleted.Task.IsCompleted)
			{
				Errors.Add(new ThingError(Sensor, "Timeout."));
				Error = true;
			}

			Dictionary<string, IElement> Fields2 = new Dictionary<string, IElement>();

			foreach (KeyValuePair<string, List<Field>> P in Fields)
			{
				if (P.Value.Count == 1)
					Fields2[P.Key] = new ObjectValue(P.Value[0]);
				else
					Fields2[P.Key] = new ObjectVector(P.Value.ToArray());
			}

			return new ObjectValue(new Dictionary<string, IElement>()
			{
				{ "Error", Error ? BooleanValue.True : BooleanValue.False },
				{ "Ok", Error ? BooleanValue.False : BooleanValue.True },
				{ "Fields", new ObjectValue(Fields2) },
				{ "Errors", new ObjectVector(Errors.ToArray()) }
			});
		}

		/// <summary>
		/// Default source of nodes, if no source is explicitly provided.
		/// </summary>
		public static string DefaultSource
		{
			get
			{
				if (Types.TryGetModuleParameter("DefaultSource", out object Obj) && Obj is string Source)
					return Source;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Tries to get a data source, from its data source ID.
		/// </summary>
		/// <param name="SourceId">Data Source ID</param>
		/// <param name="Source">Source, if found, null otherwise.</param>
		/// <returns>If a source with the given ID was found.</returns>
		public static bool TryGetDataSource(string SourceId, out IDataSource Source)
		{
			if (!Types.TryGetModuleParameter("Sources", out object Obj) || !(Obj is IDataSource[] Sources))
			{
				Source = null;
				return false;
			}

			if (Sources != sources)
			{
				Dictionary<string, IDataSource> SourceById = new Dictionary<string, IDataSource>();
				Add(SourceById, Sources);
				sourceById = SourceById;
				sources = Sources;
			}

			return sourceById.TryGetValue(SourceId, out Source);
		}

		private static void Add(Dictionary<string, IDataSource> SourceById, IEnumerable<IDataSource> Sources)
		{
			foreach (IDataSource Source in Sources)
			{
				SourceById[Source.SourceID] = Source;

				if (Source.HasChildren)
					Add(SourceById, Source.ChildSources);
			}
		}

		private static IDataSource[] sources = null;
		private static Dictionary<string, IDataSource> sourceById = null;
	}
}
