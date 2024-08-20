using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Things.Attributes;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
    /// <summary>
    /// Abstract base class for script node commands and queries.
    /// </summary>
    public abstract class ScriptCommandNodeBase : VirtualNode
    {
        /// <summary>
        /// Unparsed script expression.
        /// </summary>
		protected string[] script;

        /// <summary>
        /// Parsed script expression.
        /// </summary>
		protected Expression parsedScript;

		/// <summary>
		/// Represents a command that can be executed on a script node or script reference node.
		/// </summary>
		public ScriptCommandNodeBase()
            : base()
        {
        }

        /// <summary>
        /// ID of command
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(14, "Command ID:")]
        [ToolTip(15, "ID of command, as referenced by the caller.")]
        [Required]
        public string CommandId { get; set; }

        /// <summary>
        /// Displayable name of command
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(20, "Command Name:")]
        [ToolTip(21, "Displayable name of command.")]
        [Required]
        public string CommandName { get; set; }

        /// <summary>
        /// Sort category of command.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(16, "Sort Category:")]
        [ToolTip(17, "Category in which the command will be ordered.")]
        [Required]
        public string SortCategory { get; set; }

        /// <summary>
        /// Sort key of command.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(18, "Sort Key:")]
        [ToolTip(19, "Key used when ordering commands within a sort category.")]
        [Required]
        public string SortKey { get; set; }

        /// <summary>
        /// Optional confirmation message.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(22, "Confirmation Message:")]
        [ToolTip(23, "Optional confirmation message. If defined, it will be presented to users before executing the command.")]
        public string Confirmation { get; set; }

        /// <summary>
        /// Optional confirmation message.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(24, "Success Message:")]
        [ToolTip(25, "Optional success message. If defined, it will be presented to users after the successful execution of the command.")]
        public string Success { get; set; }

        /// <summary>
        /// Optional confirmation message.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(26, "Failure Message:")]
        [ToolTip(27, "Optional failure message. If defined, it will be presented to users after the failed execution of the command.")]
        public string Failure { get; set; }

        /// <summary>
        /// If the children of the node have an intrinsic order (true), or if the order is not important (false).
        /// </summary>
        public override bool ChildrenOrdered => true;

        /// <summary>
        /// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
        /// </summary>
        /// <param name="Child">Presumptive child node.</param>
        /// <returns>If the child is acceptable.</returns>
        public override Task<bool> AcceptsChildAsync(INode Child)
        {
            return Task.FromResult(Child is ScriptParameterNode);
        }

        /// <summary>
        /// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
        /// </summary>
        /// <param name="Parent">Presumptive parent node.</param>
        /// <returns>If the parent is acceptable.</returns>
        public override Task<bool> AcceptsParentAsync(INode Parent)
        {
            return Task.FromResult(Parent is ScriptNode);
        }

		/// <summary>
		/// Parsed command script.
		/// </summary>
		public Expression ParsedScript
		{
			get
			{
				Expression Exp = this.parsedScript;

				if (Exp is null)
				{
					StringBuilder sb = new StringBuilder();

					foreach (string s in this.script)
						sb.AppendLine(s);

					this.parsedScript = Exp = new Expression(sb.ToString());
				}

				return Exp;
			}
		}


		/// <summary>
		/// Gets a node command based on the script command.
		/// </summary>
		/// <param name="Node">Node on which command is executed.</param>
		/// <returns>Command object.</returns>
		public async Task<ScriptParameterNode[]> GetParameters(VirtualNode Node)
		{
			List<ScriptParameterNode> Parameters = new List<ScriptParameterNode>();

			if (this.HasChildren)
			{
				IEnumerable<INode> Children = await this.ChildNodes;
				if (!(Children is null))
				{
					foreach (INode Child in Children)
					{
						if (Child is ScriptParameterNode ParameterNode)
							Parameters.Add(ParameterNode);
					}
				}
			}

			return Parameters.ToArray();
		}

		/// <summary>
		/// Gets a node command based on the script command.
		/// </summary>
		/// <param name="Node">Node on which command is executed.</param>
		/// <returns>Command object.</returns>
		public abstract Task<ICommand> GetCommand(VirtualNode Node);
    }
}