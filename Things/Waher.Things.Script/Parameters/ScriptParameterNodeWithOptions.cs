using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Things.Attributes;

namespace Waher.Things.Script.Parameters
{
    /// <summary>
    /// Represents a parameter with possible options on a command.
    /// </summary>
    public abstract class ScriptParameterNodeWithOptions : ScriptParameterNode
    {
        /// <summary>
        /// Represents a parameter with possible options on a command.
        /// </summary>
        public ScriptParameterNodeWithOptions()
        {
        }

		/// <summary>
		/// If only values defined in options are valid values.
		/// </summary>
		[Page(2, "Script", 100)]
        [Header(88, "Restrict to options.")]
        [ToolTip(89, "If only values defined in options are valid values.")]
        public bool RestrictToOptions { get; set; }

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
            return Task.FromResult(Child is ScriptOptionNode);
        }

        /// <summary>
        /// Gets available options, if any are defined.
        /// </summary>
        /// <returns>Array of options</returns>
        public async Task<KeyValuePair<string, string>[]> GetOptions()
        {
            if (!this.HasChildren)
                return null;

            IEnumerable<INode> Children = await this.ChildNodes;
            if (Children is null)
                return null;

            List<KeyValuePair<string, string>> Result = null;

            foreach (INode Child in Children)
            {
                if (Child is ScriptOptionNode Option)
                {
                    if (Result is null)
                        Result = new List<KeyValuePair<string, string>>();

                    Result.Add(new KeyValuePair<string, string>(Option.Label, Option.Value));
                }
            }

            return Result?.ToArray();
        }
    }
}
