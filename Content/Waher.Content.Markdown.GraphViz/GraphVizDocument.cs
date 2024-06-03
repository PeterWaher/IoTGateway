namespace Waher.Content.Markdown.GraphViz
{
    /// <summary>
    /// Encapsulates a GraphViz document
    /// </summary>
    public class GraphVizDocument
    {
        /// <summary>
        /// Encapsulates a GraphViz document
        /// </summary>
        /// <param name="GraphDescription">Graph Description in dot file format</param>
        public GraphVizDocument(string GraphDescription)
        {
            this.GraphDescription = GraphDescription;
        }

        /// <summary>
        /// Graph Description in dot file format
        /// </summary>
        public string GraphDescription { get; }
    }
}