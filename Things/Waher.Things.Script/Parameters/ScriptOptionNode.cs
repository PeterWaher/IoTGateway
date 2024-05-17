using System.Threading.Tasks;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Virtual;

namespace Waher.Things.Script.Parameters
{
    /// <summary>
    /// Represents a string-valued option.
    /// </summary>
    public class ScriptOptionNode : VirtualNode
    {
        /// <summary>
        /// Represents a string-valued option.
        /// </summary>
        public ScriptOptionNode()
            : base()
        {
        }

        /// <summary>
        /// Option value.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(75, "Value:")]
        [ToolTip(85, "Value of parameter option.")]
        [Required]
        public string Value { get; set; }

        /// <summary>
        /// Option label.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(38, "Label:")]
        [ToolTip(86, "Label shown when option is presented.")]
        [Required]
        public string Label { get; set; }

        /// <summary>
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 87, "Selectable option");
        }

        /// <summary>
        /// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
        /// </summary>
        /// <param name="Child">Presumptive child node.</param>
        /// <returns>If the child is acceptable.</returns>
        public override Task<bool> AcceptsChildAsync(INode Child)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
        /// </summary>
        /// <param name="Parent">Presumptive parent node.</param>
        /// <returns>If the parent is acceptable.</returns>
        public override Task<bool> AcceptsParentAsync(INode Parent)
        {
            return Task.FromResult(Parent is ScriptParameterNodeWithOptions);
        }

    }
}