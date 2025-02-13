using System.Threading.Tasks;

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
		Task<RequestOrigin> GetOrigin();

		/// <summary>
		/// If the origin has a given privilege.
		/// </summary>
		/// <param name="Privilege">Privilege.</param>
		/// <returns>If the origin has the corresponding privilege.</returns>
		bool HasPrivilege(string Privilege);
	}
}
