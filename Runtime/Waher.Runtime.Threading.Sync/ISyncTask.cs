namespace Waher.Runtime.Threading.Sync
{
    /// <summary>
    /// Interface for tasks to be synckronized.
    /// </summary>
    public interface ISyncTask
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        void Execute();
    }
}
