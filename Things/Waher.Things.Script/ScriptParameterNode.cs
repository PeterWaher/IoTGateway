using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things.Attributes;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
    /// <summary>
    /// Represents a parameter on a command.
    /// </summary>
    public abstract class ScriptParameterNode : VirtualNode
    {
        /// <summary>
        /// Represents a parameter on a command.
        /// </summary>
        public ScriptParameterNode()
        {
        }

        /// <summary>
        /// Parameter page.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(36, "Page:")]
        [ToolTip(37, "Title of page on which parameter will appear.")]
        [Required]
        public string Page { get; set; }

        /// <summary>
        /// Parameter name.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(12, "Parameter Name:")]
        [ToolTip(13, "Parameter value can be referenced in script as a variable using this parameter name.")]
        [Required]
        public string ParameterName { get; set; }

        /// <summary>
        /// Parameter label.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(38, "Label:")]
        [ToolTip(39, "Label shown with parameter when it is presented.")]
        [Required]
        public string Label { get; set; }

        /// <summary>
        /// Parameter description.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(33, "Description:")]
        [ToolTip(34, "Description of parameter.")]
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// If parameter is required.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(40, "Required")]
        [ToolTip(41, "If parameter is a required parameter.")]
        public bool Required { get; set; }

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
            return Task.FromResult(Parent is ScriptCommandNodeBase);
        }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public abstract Task PopulateForm(DataForm Parameters, Language Language, object Value);

        /// <summary>
        /// Sets the parameters of the object, based on contents in the data form.
        /// </summary>
        /// <param name="Parameters">Data form with parameter values.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
        /// <param name="Values">Collection of parameter values.</param>
        /// <param name="Result">Result set to return to caller.</param>
        /// <returns>Any errors encountered, or null if parameters was set properly.</returns>
        public abstract Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Values,
            SetEditableFormResult Result);

    }
}