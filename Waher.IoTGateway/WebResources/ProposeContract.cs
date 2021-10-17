using System;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content.Markdown.Web.WebScript;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Contracts;
using Waher.Script;
using Waher.Networking.HTTP.ScriptExtensions;

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
				Gateway.AssertUserAuthenticated(Request, "Admin.Legal.ProposeContract");

				if (Gateway.ContractsClient is null)
					throw new NotSupportedException("Proposing new contracts not permitted. Broker does not support smart contracts.");

				if (!Request.HasData)
					throw new BadRequestException("No data in post.");

				Variables PageVariables = Page.GetPageVariables(Request.Session, "/ProposeContract.md");
				object Posted = Request.DecodeData();

				if (Posted is XmlDocument Doc)
				{
					Contract Contract = Contract.Parse(Doc, out bool HasStatus, out bool ParametersValid);
					if (HasStatus)
						throw new ForbiddenException("Contract must not have a status section.");

					if (!ParametersValid)
						throw new BadRequestException("Contract parameter values not valid.");

					PageVariables["Contract"] = Contract;
				}
				else if (Posted is bool Command)
				{
					if (!PageVariables.TryGetVariable("Contract", out Variable v) ||
						!(v.ValueObject is Contract Contract))
					{
						throw new BadRequestException("No smart contract uploaded.");
					}

					if (Command)
					{
						Contract = await Gateway.ContractsClient.CreateContractAsync(Contract.ForMachines, Contract.ForHumans,
							Contract.Roles, Contract.Parts, Contract.Parameters, Contract.Visibility, Contract.PartsMode, Contract.Duration,
							Contract.ArchiveRequired, Contract.ArchiveOptional, Contract.SignAfter, Contract.SignBefore, Contract.CanActAsTemplate);

						PageVariables["Contract"] = Contract;
					}
					else
						PageVariables.Remove("Contract");
				}
				else
					throw new BadRequestException("Invalid type of posted data.");
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

	}
}
