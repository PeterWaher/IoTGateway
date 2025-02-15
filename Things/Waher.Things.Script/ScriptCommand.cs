using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things.Queries;
using Waher.Things.Virtual;

namespace Waher.Things.Script
{
    /// <summary>
    /// Represents a command on a script node.
    /// </summary>
    public class ScriptCommand : ICommand, IEditableObject
    {
        private readonly Variables values;
        private readonly ScriptParameterNode[] parameters;
        private readonly VirtualNode node;
        private readonly ScriptCommandNode commandNode;

		/// <summary>
		/// Represents a command on a script node.
		/// </summary>
		/// <param name="Node">Script node publishing the command.</param>
		/// <param name="CommandNode">Script command node, defining the script for executing the command.</param>
		/// <param name="Parameters">Command parameter definitions.</param>
		public ScriptCommand(VirtualNode Node, ScriptCommandNode CommandNode, ScriptParameterNode[] Parameters)
        {
            this.node = Node;
            this.commandNode = CommandNode;
            this.parameters = Parameters;

            this.values = new Variables()
            {
                ["this"] = this.node
            };
        }

        /// <summary>
        /// ID of command.
        /// </summary>
        public string CommandID => this.commandNode.CommandId;

        /// <summary>
        /// Type of command.
        /// </summary>
        public CommandType Type => this.parameters.Length == 0 ? CommandType.Simple : CommandType.Parametrized;

        /// <summary>
        /// Sort Category, if available.
        /// </summary>
        public string SortCategory => this.commandNode.SortCategory;

        /// <summary>
        /// Sort Key, if available.
        /// </summary>
        public string SortKey => this.commandNode.SortKey;

        /// <summary>
        /// Gets the name of data source.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        public Task<string> GetNameAsync(Language Language) => Task.FromResult(this.commandNode.CommandName);

        /// <summary>
        /// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        public Task<string> GetConfirmationStringAsync(Language Language) => Task.FromResult(this.commandNode.Confirmation);

        /// <summary>
        /// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        public Task<string> GetFailureStringAsync(Language Language) => Task.FromResult(this.commandNode.Failure);

        /// <summary>
        /// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        public Task<string> GetSuccessStringAsync(Language Language) => Task.FromResult(this.commandNode.Success);

        /// <summary>
        /// If the command can be executed by the caller.
        /// </summary>
        /// <param name="Caller">Information about caller.</param>
        /// <returns>If the command can be executed by the caller.</returns>
        public Task<bool> CanExecuteAsync(RequestOrigin Caller) => Task.FromResult(true);   // TODO: Configure access rights.

        /// <summary>
        /// Executes the command.
        /// </summary>
        public Task ExecuteCommandAsync()
        {
            return this.commandNode.ParsedScript.EvaluateAsync(this.values);
        }

        /// <summary>
        /// Starts the execution of a query.
        /// </summary>
        /// <param name="Query">Query data receptor.</param>
        /// <param name="Language">Language to use.</param>
        public Task StartQueryExecutionAsync(Query Query, Language Language)
        {
            throw new Exception("Script command is not a query.");
        }

        /// <summary>
        /// Creates a copy of the command object.
        /// </summary>
        /// <returns>Copy of command object.</returns>
        public ICommand Copy()
        {
            return new ScriptCommand(this.node, this.commandNode, this.parameters);
        }

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="Origin">Origin of request.</param>
		public async Task PopulateForm(DataForm Parameters, Language Language, IRequestOrigin Origin)
        {
            object Value;

            foreach (ScriptParameterNode Parameter in this.parameters)
            {
                lock (this.values)
                {
                    if (this.values.TryGetVariable(Parameter.ParameterName, out Variable v))
                        Value = v.ValueObject;
                    else
                        Value = null;
                }

                await Parameter.PopulateForm(Parameters, Language, Value);
            }
        }

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <param name="Origin">Origin of request.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public async Task<SetEditableFormResult> SetParameters(DataForm Parameters, Language Language, 
            bool OnlySetChanged, IRequestOrigin Origin)
        {
            SetEditableFormResult Result = new SetEditableFormResult();

            foreach (ScriptParameterNode Parameter in this.parameters)
                await Parameter.SetParameter(Parameters, Language, OnlySetChanged, this.values, Result);

            return Result;
        }

    }
}