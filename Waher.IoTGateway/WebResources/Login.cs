using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Security;
using Waher.IoTGateway;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Provides a resource that allows the caller to login to the gateway through a POST method call.
	/// </summary>
	public class Login : HttpAsynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Provides a resource that allows the caller to login to the gateway through a POST method call.
		/// </summary>
		public Login()
			: base("/Login")
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
		public async void POST(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				if (!Request.HasData || Request.Session is null)
					throw new BadRequestException();

				object Obj = Request.DecodeData();
				string From;

				if (!(Obj is Dictionary<string, string> Form) ||
					!Form.TryGetValue("UserName", out string UserName) ||
					!Form.TryGetValue("Password", out string Password))
				{
					throw new BadRequestException();
				}

				if (Request.Session.TryGetVariable("from", out Variable v))
				{
					From = v.ValueObject as string;
					if (string.IsNullOrEmpty(From))
						From = "/Index.md";
				}
				else
					From = "/Index.md";

				DateTime? Next = await Gateway.LoginAuditor.GetEarliestLoginOpportunity(Request.RemoteEndPoint, "Web");
				if (Next.HasValue)
				{
					StringBuilder sb = new StringBuilder();
					DateTime TP = Next.Value;
					DateTime Today = DateTime.Today;

					if (Next.Value == DateTime.MaxValue)
					{
						sb.Append("This endpoint (");
						sb.Append(Request.RemoteEndPoint);
						sb.Append(") has been blocked from the system.");
					}
					else
					{
						sb.Append("Too many failed login attempts in a row registered. Try again after ");
						sb.Append(TP.ToLongTimeString());

						if (TP.Date != Today)
						{
							if (TP.Date == Today.AddDays(1))
							{
								sb.Append(TP.ToLongTimeString());
								sb.Append(" tomorrow");
							}
							else
							{
								sb.Append(TP.ToLongTimeString());
								sb.Append(", ");
								sb.Append(TP.ToShortDateString());
							}
						}

						sb.Append('.');
					}

					Request.Session["LoginError"] = sb.ToString();

					throw new SeeOtherException(Request.Header.Referer.Value);
				}

				LoginResult LoginResult = await Gateway.DoMainXmppLogin(UserName, Password, Request.RemoteEndPoint, "Web");
				if (LoginResult == LoginResult.Successful)
					DoLogin(Request, From, false);
				else
				{
					if (LoginResult == LoginResult.InvalidLogin)
						Request.Session["LoginError"] = "Invalid login credentials provided.";
					else
						Request.Session["LoginError"] = "Unable to connect to XMPP server.";

					throw new SeeOtherException(Request.Header.Referer.Value);
				}
			}
			catch (Exception ex)
			{
				Response.SendResponse(ex);
			}
			finally
			{
				Response.Dispose();
			}
		}

		internal static void DoLogin(HttpRequest Request, string From, bool AutoLogin)
		{
			Request.Session["User"] = new User();
			Request.Session[" AutoLogin "] = AutoLogin;
			Request.Session.Remove("LoginError");

			if (!string.IsNullOrEmpty(From))
				throw new SeeOtherException(From);
		}

		private class User : IUser
		{
			public string PasswordHash
			{
				get
				{
					return string.Empty;
				}
			}

			public string PasswordHashType
			{
				get
				{
					return string.Empty;
				}
			}

			public string UserName
			{
				get
				{
					return Gateway.XmppClient.UserName;
				}
			}

			public bool HasPrivilege(string Privilege)
			{
				return true;
			}
		}

	}
}
