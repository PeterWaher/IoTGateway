using System;
using System.Xml;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Contracts;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Proposes a new smart contract
	/// </summary>
	public class ProposeContract : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Proposes a new smart contract
		/// </summary>
		public ProposeContract()
			: base("/ProposeContract")
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				Gateway.AssertUserAuthenticated(Request);

				if (Gateway.ContractsClient is null)
					throw new NotSupportedException("Proposing new contracts not permitted. Broker does not support smart contracts.");

				if (!Request.HasData)
					throw new BadRequestException("No contract posted.");

				if (!(Request.DecodeData() is XmlDocument Doc))
					throw new BadRequestException("Invalid contract.");

				Contract Contract = Contract.Parse(Doc, out bool HasStatus);
				if (HasStatus)
					throw new ForbiddenException("Contract must not have a status section.");

				Contract UploadedContract = await Gateway.ContractsClient.CreateContractAsync(Contract.ForMachines, Contract.ForHumans,
					Contract.Roles, Contract.Parts, Contract.Parameters, Contract.Visibility, Contract.PartsMode, Contract.Duration, 
					Contract.ArchiveRequired, Contract.ArchiveOptional, Contract.SignAfter, Contract.SignBefore, Contract.CanActAsTemplate);

				Response.StatusCode = 200;
				Response.ContentType = "text/plain";
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

	}
}
