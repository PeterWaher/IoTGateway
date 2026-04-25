using Waher.Networking.HTTP;

namespace Waher.Things.Http
{
	/// <summary>
	/// Web Service REST API that receives sensor data from external sources.
	/// </summary>
	public class SensorDataReceptorResource : HttpSynchronousResource
	{
		/// <summary>
		/// Web Service REST API that receives sensor data from external sources.
		/// </summary>
		/// <param name="ResourceName"></param>
		public SensorDataReceptorResource(string ResourceName)
			: base(ResourceName)
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => true;
	}
}
