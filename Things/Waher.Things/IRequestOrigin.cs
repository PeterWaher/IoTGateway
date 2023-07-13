namespace Waher.Things
{
	/// <summary>
	/// Interface for requestors that can act as an origin for distributed requests.
	/// </summary>
	public interface IRequestOrigin
	{
		/// <summary>
		/// Origin of request.
		/// </summary>
		RequestOrigin Origin { get; }
	}
}
