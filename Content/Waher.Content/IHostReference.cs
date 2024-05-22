namespace Waher.Content
{
    /// <summary>
    /// Interface for objects that contain a reference to a host.
    /// </summary>
    public interface IHostReference
    {
        /// <summary>
        /// Host reference.
        /// </summary>
        string Host { get; }
    }
}