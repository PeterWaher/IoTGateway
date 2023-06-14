using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Patterns
{
    /// <summary>
    /// Interface for SPARQL patterns.
    /// </summary>
    public interface ISparqlPattern
    {
        /// <summary>
        /// If pattern is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Searches for the pattern on information in a semantic cube.
        /// </summary>
        /// <param name="Cube">Semantic cube.</param>
        /// <param name="Variables">Script variables.</param>
        /// <param name="ExistingMatches">Existing matches.</param>
        /// <param name="Query">SPARQL-query being executed.</param>
        /// <returns>Matches.</returns>
        Task<IEnumerable<Possibility>> Search(ISemanticCube Cube,
            Variables Variables, IEnumerable<Possibility> ExistingMatches, SparqlQuery Query);

        /// <summary>
        /// Sets the parent node. Can only be used when expression is being parsed.
        /// </summary>
        /// <param name="Parent">Parent Node</param>
        void SetParent(ScriptNode Parent);

        /// <summary>
        /// Calls the callback method for all child nodes.
        /// </summary>
        /// <param name="Callback">Callback method to call.</param>
        /// <param name="State">State object to pass on to the callback method.</param>
        /// <param name="Order">Order to traverse the nodes.</param>
        /// <returns>If the process was completed.</returns>
        bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order);

        /// <summary>
        /// Calls the callback method for all child nodes.
        /// </summary>
        /// <param name="Callback">Callback method to call.</param>
        /// <param name="State">State object to pass on to the callback method.</param>
        /// <param name="Order">Order to traverse the nodes.</param>
        /// <returns>If the process was completed.</returns>
        bool ForAll(ScriptNodeEventHandler Callback, object State, SearchMethod Order);


	}
}
