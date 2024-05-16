using System;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things.Attributes;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
	/// <summary>
	/// Node referencing a script node.
	/// </summary>
	public class ScriptReferenceNode : VirtualNode, ISensor
	{
		/// <summary>
		/// Node referencing a script node.
		/// </summary>
		public ScriptReferenceNode()
			: base()
		{
		}

		/// <summary>
		/// ID of node containing script defining node.
		/// </summary>
		[Page(2, "Script", 100)]
		[Header(6, "Script Node:")]
		[ToolTip(7, "ID of template node that defines how the node operates.")]
		[Required]
		public string ScriptNodeId { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ScriptNode), 8, "Script Reference Node");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is ScriptNode || Child is ScriptReferenceNode);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root || Parent is VirtualNode || Parent is ScriptNode || Parent is ScriptReferenceNode);
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

				if (string.IsNullOrEmpty(this.ScriptNodeId))
					throw new Exception("Script Node ID not defined.");

				INode Node = await MeteringTopology.GetNode(this.ScriptNodeId)
					?? throw new Exception("Node not found: " + this.ScriptNodeId);

				if (!(Node is ScriptNode ScriptNode))
					throw new Exception("Script Node reference does not point to a script node.");

				Variables v = new Variables()
				{
					["this"] = this
				};

				this.PopulateVariables(v);

				v.OnPreview += (sender, e) =>
				{
					ScriptNode.ReportFields(Request, e.Preview.AssociatedObjectValue, false);
				};

				object Obj = await ScriptNode.ParsedSensorDataScript.EvaluateAsync(v);
				ScriptNode.ReportFields(Request, Obj, true);

				await this.RemoveErrorAsync("ScriptError");
			}
			catch (Exception ex)
			{
				await this.LogErrorAsync("ScriptError", ex.Message);
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		/// <summary>
		/// Populates a variable collection with variables before script execution.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public virtual void PopulateVariables(Variables Variables)
		{
			if (!(this.MetaData is null))
			{
				foreach (MetaDataValue Tag in this.MetaData)
					Variables[Tag.Name] = Tag.Value;
			}
		}

	}
}
