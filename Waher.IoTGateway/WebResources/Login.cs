﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Security;
using Waher.Security.JWT;
using Waher.Security.Users;
using Waher.Things;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Provides a resource that allows the caller to login to the gateway through a POST method call.
	/// </summary>
	public class Login : HttpSynchronousResource, IHttpPostMethod
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
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => true;

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
			if (!Request.HasData || Request.Session is null)
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			string From;

			if (Content.HasError ||
				!(Content.Decoded is Dictionary<string, string> Form) ||
				!Form.TryGetValue("UserName", out string UserName) ||
				!Form.TryGetValue("Password", out string Password))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (Request.Session.TryGetVariable("from", out Variable v))
			{
				From = v.ValueObject as string;
				if (string.IsNullOrEmpty(From))
					From = "/";
			}
			else
				From = "/";

			LoginResult Result = await Users.Login(UserName, Password, Request.RemoteEndPoint, "Web");

			switch (Result.Type)
			{
				case LoginResultType.PermanentlyBlocked:
					StringBuilder sb = new StringBuilder();

					sb.Append("This endpoint (");
					sb.Append(Request.RemoteEndPoint);
					sb.Append(") has been blocked from the system.");

					Request.Session["LoginError"] = sb.ToString();
					await Response.SendResponse(new SeeOtherException(Request.Header.Referer.Value));
					return;

				case LoginResultType.TemporarilyBlocked:
					sb = new StringBuilder();
					DateTime TP = Result.Next.Value;
					DateTime Today = DateTime.Today;

					sb.Append("Too many failed login attempts in a row registered. Try again after ");
					sb.Append(TP.ToLongTimeString());

					if (TP.Date != Today)
					{
						if (TP.Date == Today.AddDays(1))
							sb.Append(" tomorrow");
						else
						{
							sb.Append(", ");
							sb.Append(TP.ToShortDateString());
						}
					}

					sb.Append(". Remote Endpoint: ");
					sb.Append(Request.RemoteEndPoint);

					Request.Session["LoginError"] = sb.ToString();
					await Response.SendResponse(new SeeOtherException(Request.Header.Referer.Value));
					return;

				case LoginResultType.NoPassword:
					Request.Session["LoginError"] = "No password provided.";
					await Response.SendResponse(new SeeOtherException(Request.Header.Referer.Value));
					return;

				case LoginResultType.InvalidCredentials:
				default:
					Request.Session["LoginError"] = "Invalid login credentials provided.";
					await Response.SendResponse(new SeeOtherException(Request.Header.Referer.Value));
					return;

				case LoginResultType.Success:
					await DoLogin(Request, Response, From, Result.User, false);
					break;
			}
		}

		internal static Task DoLogin(HttpRequest Request, HttpResponse Response, 
			string From, User User, bool ThrowRedirection)
		{
			return DoLogin(Request, Response, From, false, User, ThrowRedirection);
		}

		internal static Task DoLogin(HttpRequest Request, HttpResponse Response, 
			string From, bool ThrowRedirection)
		{
			return DoLogin(Request, Response, From, true, new InternalUser(), ThrowRedirection);
		}

		private static async Task DoLogin(HttpRequest Request, HttpResponse Response, 
			string From, bool AutoLogin, IUser User, bool ThrowRedirection)
		{
			Request.Session["User"] = User;
			Request.Session[" AutoLogin "] = AutoLogin;
			Request.Session.Remove("LoginError");

			if (!string.IsNullOrEmpty(From))
				await RedirectBackToFrom(Response, From, ThrowRedirection);
		}

		internal static async Task RedirectBackToFrom(HttpResponse Response, string From, 
			bool ThrowRedirection)
		{
			if (ThrowRedirection)
				throw new SeeOtherException(From);
			else
				await Response.SendResponse(new SeeOtherException(From));

		}

		private class InternalUser : IUserWithClaims, IRequestOrigin
		{
			public string PasswordHash => string.Empty;
			public string PasswordHashType => string.Empty;
			public string UserName => Gateway.XmppClient?.UserName;

			public bool HasPrivilege(string Privilege)
			{
				return true;
			}

			public Task<RequestOrigin> GetOrigin()
			{
				return Task.FromResult(new RequestOrigin(Gateway.XmppClient?.BareJID, null, null, null, this));
			}

			public Task<IEnumerable<KeyValuePair<string, object>>> CreateClaims(bool Encrypted)
			{
				int IssuedAt = (int)Math.Round(DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds);
				int Expires = IssuedAt + 3600;

				List<KeyValuePair<string, object>> Claims = new List<KeyValuePair<string, object>>()
				{
					new KeyValuePair<string, object>(JwtClaims.JwtId, Convert.ToBase64String(Gateway.NextBytes(32))),
					new KeyValuePair<string, object>(JwtClaims.Subject, Environment.UserName),
					new KeyValuePair<string, object>(JwtClaims.IssueTime, IssuedAt),
					new KeyValuePair<string, object>(JwtClaims.ExpirationTime, Expires),
					new KeyValuePair<string, object>(JwtClaims.Issuer, Environment.UserDomainName)
				};

				string OsName = Environment.OSVersion.ToString();
				string OsVersion = Environment.OSVersion.Version.ToString();

				if (OsName.EndsWith(OsVersion))
					OsName = OsName[..^OsVersion.Length].TrimEnd();

				Claims.Add(new KeyValuePair<string, object>(JwtClaims.HardwareName, Environment.MachineName));
				Claims.Add(new KeyValuePair<string, object>(JwtClaims.SoftwareName, OsName));
				Claims.Add(new KeyValuePair<string, object>(JwtClaims.SoftwareVersion, OsVersion));

				return Task.FromResult<IEnumerable<KeyValuePair<string, object>>>(Claims);
			}

			public async Task<string> CreateToken(JwtFactory Factory, bool Encrypted)
			{
				return Factory.Create(await this.CreateClaims(Encrypted));
			}
		}

	}
}
