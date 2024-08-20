using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
	/// <summary>
	/// Represents a query that can be executed on a script node or script reference node.
	/// </summary>
	public class ScriptQueryNode : ScriptCommandNodeBase
    {
		/// <summary>
		/// Represents a query that can be executed on a script node or script reference node.
		/// </summary>
		public ScriptQueryNode()
            : base()
        {
        }

        /// <summary>
        /// Script for executing command.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(90, "Query script:")]
        [ToolTip(91, "Script that gets evaluated when then query is executed.")]
		[Text(TextPosition.AfterField, 92, "Script that is evaluated when query is executed. Use the \"this\" variable to refer to the script node publishing the query. The \"Query\" variable will contain the current state of the query, while the \"Language\" variable contains the selected language.")]
		[ContentType("application/x-webscript")]
        [Required]
        public string[] QueryScript
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
            return Language.GetStringAsync(typeof(ScriptNode), 93, "Script Query");
        }

        /// <summary>
        /// Gets a node command based on the script command.
        /// </summary>
        /// <param name="Node">Node on which command is executed.</param>
        /// <returns>Command object.</returns>
        public override async Task<ICommand> GetCommand(VirtualNode Node)
        {
            return new ScriptQuery(Node, this, await this.GetParameters(Node));
        }
    }
}