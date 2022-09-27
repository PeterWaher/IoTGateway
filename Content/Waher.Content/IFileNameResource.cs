namespace Waher.Content
{
	/// <summary>
	/// Basic interface for resources having a FileName property.
	/// </summary>
	public interface IFileNameResource
	{
		/// <summary>
		/// Filename of resource.
		/// </summary>
		string FileName
		{
			get;
			set;
		}
	}
}
