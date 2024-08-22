using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
    /// <summary>
    /// Represents a command that can be executed on a script node or script reference node.
    /// </summary>
    public class ScriptCommandNode : ScriptCommandNodeBase
    {
        /// <summary>
        /// Represents a command that can be executed on a script node or script reference node.
        /// </summary>
        public ScriptCommandNode()
            : base()
        {
        }

        /// <summary>
        /// Script for executing command.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(9, "Command script:")]
        [ToolTip(10, "Script that gets evaluated when then command is executed.")]
        [Text(TextPosition.AfterField, 28, "Script that is evaluated when command is executed. Use the \"this\" variable to refer to the script node publishing the command.")]
        [ContentType("application/x-webscript")]
        [Required]
        public string[] CommandScript
        {
            get => this.script;
            set
            {
                this.script = value;
                this.parsedScript = null;
            }
        }

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
        /// Gets a node command based on the script command.
        /// </summary>
        /// <param name="Node">Node on which command is executed.</param>
        /// <returns>Command object.</returns>
        public override async Task<ICommand> GetCommand(VirtualNode Node)
        {
            return new ScriptCommand(Node, this, await this.GetParameters(Node));
        }
    }
}