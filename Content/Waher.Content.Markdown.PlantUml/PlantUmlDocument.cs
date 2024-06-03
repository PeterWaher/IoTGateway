namespace Waher.Content.Markdown.PlantUml
{
    /// <summary>
    /// Encapsulates a PlantUML document
    /// </summary>
    public class PlantUmlDocument
    {
        /// <summary>
        /// Encapsulates a PlantUML document
        /// </summary>
        /// <param name="GraphDescription">Graph Description</param>
        public PlantUmlDocument(string GraphDescription)
        {
            this.GraphDescription = GraphDescription;
        }

        /// <summary>
        /// Graph Description
        /// </summary>
        public string GraphDescription { get; }
    }
}