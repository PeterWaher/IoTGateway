using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things.Attributes;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.SensorData;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
	/// <summary>
	/// Node defined by script.
	/// </summary>
	public class ScriptNode : VirtualNode
	{
		private string[] sensorScript;
		private Expression parsedSensorScript;

		/// <summary>
		/// Node defined by script.
		/// </summary>
		public ScriptNode()
			: base()
		{
		}

		/// <summary>
		/// Script for generating sensor-data.
		/// </summary>
		[Page(2, "Script", 100)]
		[Header(3, "Sensor-data script:")]
		[ToolTip(4, "Script that returns sensor-data fields.")]
		[Text(TextPosition.AfterField, 5, "Script that returns sensor-data fields. Intermediate fields can be returned using the preview function. Script should not return previewed fields, as these have already been reported. Use the \"this\" variable to refer to this node, and the \"Request\" to refer to the current readout-request for process optimization.")]
		[ContentType("application/x-webscript")]
		[Required]
		public string[] SensorScript
		{
			get => this.sensorScript;
			set
			{
				this.sensorScript = value;
				this.parsedSensorScript = null;
			}
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ScriptNode), 1, "Script Node");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(
				Child is ScriptNode || 
				Child is ScriptReferenceNode || 
				Child is ScriptCommandNodeBase);
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
				Parent is VirtualNode || 
				Parent is ScriptNode || 
				Parent is ScriptReferenceNode);
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		/// <param name="DoneAfter">If readout is done after reporting fields (true), or if more fields will
		/// be reported by the caller (false).</param>
		public override async Task StartReadout(ISensorReadout Request, bool DoneAfter)
		{
			try
			{
				await base.StartReadout(Request, false);

				Variables v = new Variables()
				{
					["this"] = this,
					["Request"] = Request
				};

				if (!(this.MetaData is null))
				{
					foreach (MetaDataValue Tag in this.MetaData)
						v[Tag.Name] = Tag.Value;
				}

				v.OnPreview += (Sender, e) =>
				{
					ReportFields(Request, e.Preview.AssociatedObjectValue, false);
					return Task.CompletedTask;
				};

				object Obj = await this.ParsedSensorDataScript.EvaluateAsync(v);
				ReportFields(Request, Obj, DoneAfter);

				await this.RemoveErrorAsync("ScriptError");
			}
			catch (Exception ex)
			{
				await this.LogErrorAsync("ScriptError", ex.Message);
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		/// <summary>
		/// Parsed sensor-data script.
		/// </summary>
		public Expression ParsedSensorDataScript
		{
			get
			{
				Expression Exp = this.parsedSensorScript;

				if (Exp is null)
				{
					StringBuilder sb = new StringBuilder();

					foreach (string s in this.sensorScript)
						sb.AppendLine(s);

					this.parsedSensorScript = Exp = new Expression(sb.ToString());
				}

				return Exp;
			}
		}

		internal static void ReportFields(ISensorReadout Request, object Obj, bool Done)
		{
			if (Obj is Field Field)
				Request.ReportFields(Done, Field);
			else if (Obj is Field[] Fields)
				Request.ReportFields(Done, Fields);
			else if (Obj is IEnumerable<Field> Fields2)
			{
				List<Field> Fields3 = new List<Field>();

				foreach (Field F in Fields2)
					Fields3.Add(F);

				Request.ReportFields(Done, Fields3);
			}
			else if (Obj is IEnumerable Enumerable)
			{
				List<Field> Fields3 = new List<Field>();

				foreach (object Obj2 in Enumerable)
				{
					if (Obj2 is Field F)
						Fields3.Add(F);
					else if (!(Obj2 is null))
						throw new Exception("Expected script to return sensor data fields. Received object of type: " + Obj2.GetType().FullName);
				}

				Request.ReportFields(Done, Fields3);
			}
			else if (!(Obj is null))
				throw new Exception("Expected script to return sensor data fields. Received object of type: " + Obj.GetType().FullName);
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		internal async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Commands = new List<ICommand>();
			Commands.AddRange(await base.Commands);

			if (this.HasChildren)
			{
				IEnumerable<INode> Children = await this.ChildNodes;
				if (!(Children is null))
				{
					foreach (INode Child in Children)
					{
						if (Child is ScriptCommandNodeBase CommandNode)
							Commands.Add(await CommandNode.GetCommand(this));
					}
				}
			}

			return Commands.ToArray();
		}

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public override bool IsReadable
		{
			get
			{
				if (base.IsReadable)
					return true;

				if (this.sensorScript is null || this.sensorScript.Length == 0)
					return false;

				try
				{
					return !(this.ParsedSensorDataScript?.Root is null);
				}
				catch (Exception)
				{
					return false;
				}
			}
		}
    }
}
