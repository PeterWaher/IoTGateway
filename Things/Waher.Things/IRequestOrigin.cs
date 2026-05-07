using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Things
{
	/// <summary>
	/// Interface for requestors that can act as an origin for distributed requests.
	/// </summary>
	public interface IRequestOrigin : IHasPrivileges
	{
		/// <summary>
		/// Origin of request.
		/// </summary>
		Task<RequestOrigin> GetOrigin();
	}
}
