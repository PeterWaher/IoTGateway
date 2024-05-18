using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things.Attributes;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
    /// <summary>
    /// Represents a command that can be executed on a script node or script reference node.
    /// </summary>
    public class ScriptCommandNode : VirtualNode
    {
        private string[] commandScript;
        private Expression parsedCommandScript;

        /// <summary>
        /// Represents a command that can be executed on a script node or script reference node.
        /// </summary>
        public ScriptCommandNode()
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
        /// Script for executing command.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(9, "Command script:")]
        [ToolTip(10, "Script that gets evaluated when then command is executed.")]
        [Text(TextPosition.AfterField, 28, "Script that is evaluated when command is executed. Use the \"this\" variable to refer to this node.")]
        [ContentType("application/x-webscript")]
        [Required]
        public string[] CommandScript
        {
            get => this.commandScript;
            set
            {
                this.commandScript = value;
                this.parsedCommandScript = null;
            }
        }

        /// <summary>
        /// If the children of the node have an intrinsic order (true), or if the order is not important (false).
        /// </summary>
        public override bool ChildrenOrdered => true;

        /// <summary>
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 11, "Script Command");
        }

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
        public Expression ParsedCommandDataScript
        {
            get
            {
                Expression Exp = this.parsedCommandScript;

                if (Exp is null)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (string s in this.commandScript)
                        sb.AppendLine(s);

                    this.parsedCommandScript = Exp = new Expression(sb.ToString());
                }

                return Exp;
            }
        }

        /// <summary>
        /// Gets a node command based on the script command.
        /// </summary>
        /// <param name="Node">Node on which command is executed.</param>
        /// <returns>Command object.</returns>
        public async Task<ICommand> GetCommand(VirtualNode Node)
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

            return new ScriptCommand(Node, this, Parameters.ToArray());
        }


    }
}