using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Content.Getters;
using Waher.Content.Html;
using Waher.Content.Html.Elements;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Runtime.Collections;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Test
{
	public enum LoginMethod
	{
		CodeForm,
		CodePost,
		CodeFormWithPkceDefault,
		CodeFormWithPkcePlain,
		CodeFormWithPkceS256,
		CodePostWithPkcePlain,
		CodePostWithPkceS256,
		ImplicitGet,
		ImplicitPost,
		Password,
		ClientCredentials,
		ClientCredentialsBasicAuth
	}

	[TestClass]
	public class OAuthTests : IDynamicUserSource, IThingRegistryUserSource
	{
		private const string BaseUrl = "http://localhost:8081";
		private const string CallbackResource = "/Callback";
		private const string ProtectedResource = "/Hello";
		private const string Realm = "Test";
		private const string TestUserName = "User";
		private const string TestPassword = "Password";
		private const string DeviceUserName = "Device";
		private const string DevicePassword = "Password2";
		private const string TestScopeRead = "read";
		private const string TestScopeWrite = "write";
		private const string TestScopeReadWrite = TestScopeRead + " " + TestScopeWrite;
		private const string TestScopeOther = "scope-not-originally-granted";

		private Dictionary<string, User> users;
		private HttpServer server;
		private ConsoleEventSink sink = null;
		private XmlFileSniffer xmlSniffer = null;
		private JwtFactory jwtFactory = null;

		/// <summary>
		/// Test context
		/// </summary>
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public async Task TestInitialize()
		{
			string SnifferFileName = this.TestContext.TestName;
			if (string.IsNullOrEmpty(SnifferFileName))
				SnifferFileName = "OAuth";

			SnifferFileName = "Sniffers" + Path.DirectorySeparatorChar + SnifferFileName + ".xml";

			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);

			File.Delete(SnifferFileName);
			this.xmlSniffer = new XmlFileSniffer(SnifferFileName,
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				int.MaxValue, BinaryPresentationMethod.ByteCount);

			this.jwtFactory = JwtFactory.CreateHmacSha256(BaseUrl);
			this.server = new HttpServer(8081, this.xmlSniffer);

			await Database.Clear("OAuthRedirectUris");
			await Database.Clear("OAuthClients");

			OAuth2Environment Environment = new();
			Environment.Register(this.jwtFactory);
			Environment.Register(this);

			this.server.Register(new ProtectedResourceMetaData(Environment));
			this.server.Register(new OAuthTokenResource(Environment));
			this.server.Register(new OAuthIntrospectionResource(Environment));
			this.server.Register(new OAuthRegistrationResource(Environment));
			this.server.Register(new OAuthManagementResource(Environment));
			this.server.Register(new OAuthDeviceAuthorizationResource(Environment));
			this.server.Register(new OAuthAuthorizeResource(Environment));
			this.server.Register(new AuthorizationServerMetaData(Environment));

			this.server.Register(CallbackResource, Callback);
			this.server.Register(new Hello(this.jwtFactory, this));

			this.users = new Dictionary<string, User>()
			{
				{ TestUserName, new User(TestUserName, TestPassword,
					[OAuthResource.OAuthScopePrivilegePrefix + TestScopeRead,
					OAuthResource.OAuthScopePrivilegePrefix + TestScopeWrite,
					OAuthIntrospectionResource.OAuthIntrospectionPrivilege]) },
				{ DeviceUserName, new User(DeviceUserName, DevicePassword, TestUserName,
					[OAuthResource.OAuthScopePrivilegePrefix + TestScopeRead,
					OAuthResource.OAuthScopePrivilegePrefix + TestScopeWrite,
					OAuthIntrospectionResource.OAuthIntrospectionPrivilege]) }
			};

			Environment.AuthorizeResource.ImplicitAuthenticationRequest += async (_, e) =>
			{
				if (e.Request.Header.TryGetHeaderField("X-Context", out HttpField Context))
					e.User = await this.TryGetUser(Context.Value) as IUserWithClaims;
			};
		}

		private static async Task Callback(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.Header.TryGetQueryParameter("state", out string State))
				State = string.Empty;

			if (Request.Header.TryGetQueryParameter("error", out string Error) &&
				!string.IsNullOrEmpty(Error))
			{
				Dictionary<string, object> Result = new()
				{
					{ "error", Error },
					{ "state", State }
				};

				if (Request.Header.TryGetQueryParameter("error_description", out string ErrorDescription))
					Result["error_description"] = ErrorDescription;

				if (Request.Header.TryGetQueryParameter("error_uri", out string ErrorUri))
					Result["error_uri"] = ErrorUri;

				if (Request.Header.TryGetQueryParameter("iss", out string Issuer))
					Result["iss"] = Issuer;

				await Response.Return(Result);
				return;
			}

			if (Request.Header.TryGetQueryParameter("code", out string Code) &&
				!string.IsNullOrEmpty(Code))
			{
				Dictionary<string, object> Result = new()
				{
					{ "code", Code },
					{ "state", State }
				};

				if (Request.Header.TryGetQueryParameter("iss", out string Issuer))
					Result["iss"] = Issuer;

				await Response.Return(Result);
				return;
			}

			if (Request.Header.TryGetQueryParameter("access_token", out string AccessToken) &&
				!string.IsNullOrEmpty(AccessToken))
			{
				Dictionary<string, object> Result = new()
				{
					{ "access_token", AccessToken },
					{ "state", State }
				};

				if (Request.Header.TryGetQueryParameter("token_type", out string TokenType))
					Result["token_type"] = TokenType;

				if (Request.Header.TryGetQueryParameter("expires_in", out string ExpiresIn))
					Result["expires_in"] = ExpiresIn;

				if (Request.Header.TryGetQueryParameter("refresh_token", out string RefreshToken))
					Result["refresh_token"] = RefreshToken;

				if (Request.Header.TryGetQueryParameter("iss", out string Issuer))
					Result["iss"] = Issuer;

				if (Request.Header.TryGetQueryParameter("scope", out string Scope))
					Result["scope"] = Scope;

				await Response.Return(Result);
				return;
			}

			await Response.SendResponse(new BadRequestException("Missing code or access_token."));
			return;
		}

		[OAuthResourceName("Hello Test Web Service")]
		private class Hello(JwtFactory JwtFactory, IUserSource Users)
			: HttpProtectedResource(ProtectedResource), IHttpGetMethod
		{
			private readonly HttpAuthenticationScheme[] authenticationSchemes =
				[
					new JwtAuthentication(Realm, Users, JwtFactory,
						new Uri(BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource))
				];

			public bool AllowsGET => true;
			public override bool Synchronous => true;
			public override bool HandlesSubPaths => false;
			public override bool UserSessions => false;

			public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
			{
				return this.authenticationSchemes;
			}

			public async Task GET(HttpRequest Request, HttpResponse Response)
			{
				if (Request.User is null)
				{
					await Response.SendResponse(new ForbiddenException());
					return;
				}

				StringBuilder sb = new();

				sb.Append("Hello ");
				sb.Append(Request.User.UserName);
				sb.AppendLine(".");

				await Response.Return(sb.ToString());
			}
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.server is not null)
			{
				await this.server.DisposeAsync();
				this.server = null;
			}

			if (this.xmlSniffer is not null)
			{
				await this.xmlSniffer.DisposeAsync();
				this.xmlSniffer = null;
			}

			if (this.sink is not null)
			{
				Log.Unregister(this.sink);
				await this.sink.DisposeAsync();
				this.sink = null;
			}

			if (this.jwtFactory is not null)
			{
				this.jwtFactory.Dispose();
				this.jwtFactory = null;
			}
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (this.users.TryGetValue(UserName, out User User))
				return Task.FromResult<IUser>(User);
			else
				return Task.FromResult<IUser>(null);
		}

		public Task<IRegistration> RegisterUser(IRegistrationRequest RegistrationRequest)
		{
			string UserName;
			string Password;

			do
			{
				UserName = Guid.NewGuid().ToString();
			}
			while (this.users.ContainsKey(UserName));

			Password = Guid.NewGuid().ToString();

			User User = new(UserName, Password, []);
			this.users[User.UserName] = User;

			return Task.FromResult<IRegistration>(
				new Registration(UserName, Password, RegistrationRequest));
		}

		public async Task<IRegistration> UpdateUser(string UserName, IRegistrationRequest RegistrationRequest)
		{
			IUser User = await this.TryGetUser(UserName);
			if (User is null)
				return null;

			if (!string.IsNullOrEmpty(RegistrationRequest.ClientSecret))
			{
				if (User is not User TypedUser)
					return null;

				this.users[User.UserName] = new User(TypedUser.UserName,
					RegistrationRequest.ClientSecret, TypedUser.Owner,
					TypedUser.Privileges);
			}

			return new Registration(UserName, RegistrationRequest.ClientSecret ?? string.Empty,
				RegistrationRequest);
		}

		public Task<bool> DeleteUser(string UserName, string RemoteEndPoint)
		{
			return Task.FromResult(this.users.Remove(UserName));
		}

		public Task<IUser> TryGetOwner(IUser Device)
		{
			string OwnerId = (Device as User)?.Owner;
			if (string.IsNullOrEmpty(OwnerId))
				return Task.FromResult<IUser>(null);

			return this.TryGetUser(OwnerId);
		}

		private class Registration(string UserName, string Password,
			IRegistrationRequest Request) : IRegistration
		{
			public string ClientId { get; } = UserName;
			public string ClientSecret { get; } = Password;
			public DateTime? ClientSecretExpiresAt => null;
			public IRegistrationRequest Request { get; } = Request;
		}

		[TestMethod]
		public async Task Test_001_Metadata_Discovery()
		{
			ContentResponse MetaDataResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource));
			MetaDataResponse.AssertOk();

			object MetaData = MetaDataResponse.Decoded;
			Assert.AreEqual(BaseUrl + ProtectedResource, Required<string>(MetaData, "resource"));
			Assert.Contains(BaseUrl, Required<string[]>(MetaData, "authorization_servers"));
			Assert.Contains("header", Required<string[]>(MetaData, "bearer_methods_supported"));

			ContentResponse ServerMetaDataResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + AuthorizationServerMetaData.WellKnowResourcePath));
			ServerMetaDataResponse.AssertOk();

			object ServerMetaData = ServerMetaDataResponse.Decoded;
			Assert.AreEqual(BaseUrl, Required<string>(ServerMetaData, "issuer"));
			Assert.AreEqual(BaseUrl + OAuthAuthorizeResource.DefaultResourcePath, Required<string>(ServerMetaData, "authorization_endpoint"));
			Assert.AreEqual(BaseUrl + OAuthTokenResource.DefaultResourcePath, Required<string>(ServerMetaData, "token_endpoint"));
			Assert.AreEqual(BaseUrl + OAuthIntrospectionResource.DefaultResourcePath, Required<string>(ServerMetaData, "introspection_endpoint"));
			Assert.AreEqual(BaseUrl + OAuthRegistrationResource.DefaultResourcePath, Required<string>(ServerMetaData, "registration_endpoint"));
			Assert.AreEqual(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath, Required<string>(ServerMetaData, "device_authorization_endpoint"));
			Assert.Contains("code", Required<string[]>(ServerMetaData, "response_types_supported"));
			Assert.Contains("token", Required<string[]>(ServerMetaData, "response_types_supported"));
			Assert.Contains("authorization_code", Required<string[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("password", Required<string[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("client_credentials", Required<string[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("refresh_token", Required<string[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains(OAuthDeviceAuthorizationResource.GrantType, Required<string[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("plain", Required<string[]>(ServerMetaData, "code_challenge_methods_supported"));
			Assert.Contains("S256", Required<string[]>(ServerMetaData, "code_challenge_methods_supported"));
			Assert.Contains("client_secret_basic",
				Required<string[]>(ServerMetaData, "token_endpoint_auth_methods_supported"));
			Assert.Contains("client_secret_post",
				Required<string[]>(ServerMetaData, "token_endpoint_auth_methods_supported"));
			Assert.IsTrue(Required<bool>(ServerMetaData, "authorization_response_iss_parameter_supported"));

			string[] IntrospectionAuthenticationMethods = Required<string[]>(ServerMetaData,
				"introspection_endpoint_auth_methods_supported");
			Assert.Contains("client_secret_basic", IntrospectionAuthenticationMethods);
			Assert.Contains("client_secret_post", IntrospectionAuthenticationMethods);
			Assert.IsTrue(Array.IndexOf(IntrospectionAuthenticationMethods, "none") < 0,
				"The introspection endpoint must require authentication.");
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_002_Login(LoginMethod Method)
		{
			TokenResult Token = await Login(Method);
			await AssertHello(Token.AccessToken, TestUserName);
		}

		private static async Task AssertHello(string AccessToken, string UserName)
		{
			ContentResponse HelloResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer " + AccessToken));

			HelloResponse.AssertOk();
			Assert.AreEqual("Hello " + UserName + "." + Environment.NewLine, HelloResponse.Decoded);
		}

		[TestMethod]
		public async Task Test_003_NoBearerToken()
		{
			ContentResponse Response = await InternetContent.GetAsync(new Uri(BaseUrl + ProtectedResource));
			OAuthError Error = AssertOAuthError(Response, null, UnauthorizedException.Code);
			AssertBearerChallenge(Error);
		}

		private static void AssertBearerChallenge(OAuthError Error)
		{
			bool FoundChallenge = false;

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Error.Headers)
			{
				if (Header.Key == "WWW-Authenticate")
				{
					foreach (string Value in Header.Value)
					{
						if (Value.StartsWith("Bearer "))
						{
							FoundChallenge = true;
							string Expected = "resource_metadata=\"" + BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource + "\"";
							Assert.Contains(Expected, Value);
						}
					}
				}
			}

			Assert.IsTrue(FoundChallenge);
		}

		private static void AssertBearerChallenge(OAuthError Error, string ErrorCode)
		{
			bool FoundChallenge = false;

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Error.Headers)
			{
				if (Header.Key == "WWW-Authenticate")
				{
					foreach (string Value in Header.Value)
					{
						if (Value.StartsWith("Bearer ") &&
							Value.Contains("error=\"" + ErrorCode + "\""))
						{
							FoundChallenge = true;
							break;
						}
					}
				}
			}

			Assert.IsTrue(FoundChallenge, "Expected Bearer challenge with error=\"" + ErrorCode + "\".");
		}

		[TestMethod]
		public async Task Test_004_InvalidBearerToken()
		{
			ContentResponse Response = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer this-is-not-a-jwt"));

			OAuthError Error = AssertOAuthError(Response, null, UnauthorizedException.Code);
			AssertBearerChallenge(Error, "invalid_token");

		}

		[TestMethod]
		public async Task Test_005_InvalidAuthorizationCode()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", "invalid-code" },
					{ "client_id", TestUserName },
					{ "redirect_uri", BaseUrl + CallbackResource },
					{ "code_verifier", "not-used" }
				});

			OAuthError Error = AssertOAuthError(Response, "invalid_grant", ForbiddenException.Code);
			Assert.Contains("Invalid code", Error.Description);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_006_MissingPkceVerifier(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri }
				});

			OAuthError Error = AssertOAuthError(Response, BadRequestException.Code);
			Assert.Contains("Missing code_verifier", Error.Description);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_007_InvalidPkceVerifier(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "code_verifier", CreatePkceCodeVerifier() },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri }
				});

			OAuthError Error = AssertOAuthError(Response, "invalid_grant", ForbiddenException.Code);
			Assert.Contains("Invalid code_verifier", Error.Description);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_008_ReusedAuthorizationCode(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse FirstResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateAuthorizationCodeTokenRequest(AuthorizationCode, TestUserName));
			FirstResponse.AssertOk();

			ContentResponse SecondResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateAuthorizationCodeTokenRequest(AuthorizationCode, TestUserName));

			Assert.IsTrue(SecondResponse.HasError);
			OAuthError Error = AssertOAuthError(SecondResponse, ForbiddenException.Code);
			Assert.Contains("Invalid code", Error.Description);
		}

		private static Dictionary<string, string> CreateAuthorizationCodeTokenRequest(
			AuthorizationResult AuthorizationCode, string ClientId)
		{
			Dictionary<string, string> Request = new()
			{
				{ "grant_type", "authorization_code" },
				{ "code", AuthorizationCode.Code },
				{ "client_id", ClientId },
				{ "redirect_uri", AuthorizationCode.RedirectUri }
			};

			if (!string.IsNullOrEmpty(AuthorizationCode.CodeVerifier))
				Request["code_verifier"] = AuthorizationCode.CodeVerifier;

			return Request;
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_009_InvalidUserName(LoginMethod Method)
		{
			await Assert.ThrowsAsync<LoginError>(async () => await Login(
				Method, "Invalid User", TestPassword));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_010_InvalidPassword(LoginMethod Method)
		{
			await Assert.ThrowsAsync<LoginError>(async () => await Login(
				Method, TestUserName, "Invalid Password"));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_011_MissingUserName(LoginMethod Method)
		{
			await Assert.ThrowsAsync<LoginError>(async () => await Login(
				Method, null, TestPassword));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_012_MissingPassword(LoginMethod Method)
		{
			await Assert.ThrowsAsync<LoginError>(async () => await Login(
				Method, TestUserName, null));
		}

		[TestMethod]
		public async Task Test_013_MissingResponseType()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State }
			}, "invalid_request");
		}

		[TestMethod]
		public async Task Test_014_UnsupportedResponseType()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=unsupported" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State },
				{ "response_type", "unsupported" }
			}, "unsupported_response_type");
		}

		[TestMethod]
		public async Task Test_015_AuthorizationEndpointIgnoresUnknownParameter()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;
			Dictionary<string, string> FormPostback = new()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State }
			};

			ContentResponse AuthorizeResponse = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
				"&unknown_parameter=ignored"));

			AuthorizeResponse.AssertOk();

			HtmlDocument Form = AuthorizeResponse.Decoded as HtmlDocument;
			Assert.IsNotNull(Form);

			Dictionary<string, object> Response = await CompleteAuthorizationForm(
				Form, FormPostback);

			Assert.AreEqual(State, Required<string>(Response, "state"));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "code")));
		}

		[TestMethod]
		public async Task Test_016_MissingGrantType()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "code", "not-used" }
				});

			AssertOAuthError(Response, "invalid_request", BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_017_UnsupportedGrantType()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "unsupported_grant_type" }
				});

			AssertOAuthError(Response, "unsupported_grant_type", BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_018_MissingAuthorizationCode()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "client_id", TestUserName },
					{ "code_verifier", "not-used" }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_019_TokenEndpointIgnoresUnknownParameter(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", AuthorizationCode.CodeVerifier },
					{ "unknown_parameter", "ignored" }
				});

			TokenResult Token = AssertAccessTokenResponse(TokenResponse);
			await AssertHello(Token.AccessToken, TestUserName);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_020_AuthorizationCodeBoundToClientId(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", "Invalid User" },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", AuthorizationCode.CodeVerifier }
				});

			AssertOAuthError(Response, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_021_UnsupportedPkceCodeChallengeMethod()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;
			string CodeVerifier = CreatePkceCodeVerifier();

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
				"&code_challenge=" + Uri.EscapeDataString(CodeVerifier) +
				"&code_challenge_method=unsupported"));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State },
				{ "response_type", "code" },
				{ "code_challenge", CodeVerifier },
				{ "code_challenge_method", "unsupported" }
			}, "invalid_request");
		}

		[TestMethod]
		public async Task Test_022_PasswordGrantRejectsClientCredentialParameters()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "password" },
					{ "client_id", TestUserName },
					{ "client_secret", TestPassword }
				});

			AssertOAuthError(Response, "invalid_request", BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_023_ClientCredentialsGrantRejectsPasswordParameters()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "client_credentials" },
					{ "username", TestUserName },
					{ "password", TestPassword }
				});

			AssertOAuthError(Response, "invalid_request", BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_024_TokenEndpointGetIsRejected()
		{
			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthTokenResource.DefaultResourcePath +
				"?grant_type=password" +
				"&username=" + Uri.EscapeDataString(TestUserName) +
				"&password=" + Uri.EscapeDataString(TestPassword)));

			Assert.IsTrue(Response.HasError);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_025_AuthorizationCodeRequiresRedirectUri(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "code_verifier", AuthorizationCode.CodeVerifier }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_026_AuthorizationCodeBoundToRedirectUri(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", BaseUrl + "/WrongCallback" },
					{ "code_verifier", AuthorizationCode.CodeVerifier }
				});

			AssertOAuthError(Response, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_027_EmptyResponseTypeIsTreatedAsMissing()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State }
			}, "invalid_request");
		}

		[TestMethod]
		public async Task Test_028_EmptyGrantTypeIsTreatedAsMissing()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", string.Empty },
					{ "code", "not-used" }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_029_AuthorizationCodeRequiresClientId(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", AuthorizationCode.CodeVerifier }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_030_ImplicitAndClientGrantsDoesNotIssueRefreshToken(LoginMethod Method)
		{
			AuthorizationResult Result = await Authorize(Method);

			Assert.IsTrue(Result.HasToken);
			Assert.IsTrue(string.IsNullOrEmpty(Result.RefreshToken));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_031_TokenEndpointSuccessfulResponseIsNotCacheable(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);
			Dictionary<string, string> Request = CreateAuthorizationCodeTokenRequest(
				AuthorizationCode, TestUserName);

			using HttpClient Client = new();
			using FormUrlEncodedContent Content = new(Request);
			using HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);
			AssertNoStoreHeaders(Response);

			Dictionary<string, object> Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
			Assert.IsNotNull(Parsed);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "access_token")));
			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));
		}

		private static void AssertNoStoreHeaders(HttpResponseMessage Response)
		{
			Assert.IsNotNull(Response.Headers.CacheControl, "Missing Cache-Control header.");
			Assert.IsTrue(Response.Headers.CacheControl.NoStore, "Expected Cache-Control: no-store.");

			bool FoundPragmaNoCache = false;
			foreach (NameValueHeaderValue Header in Response.Headers.Pragma)
			{
				if (string.Equals(Header.Name, "no-cache", StringComparison.OrdinalIgnoreCase))
				{
					FoundPragmaNoCache = true;
					break;
				}
			}

			Assert.IsTrue(FoundPragmaNoCache, "Expected Pragma: no-cache.");
		}

		[TestMethod]
		public async Task Test_032_DuplicateAuthorizationParameterIsRejected()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&response_type=token" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State },
				{ "response_type", "code" }
			}, "invalid_request");
		}

		private static async Task<Dictionary<string, object>> AssertAuthorizationError(
			ContentResponse Response, Dictionary<string, string> FormPostback,
			string ExpectedErrorCode)
		{
			if (Response.HasError)
			{
				OAuthError Error = AssertOAuthError(Response, BadRequestException.Code);
				Assert.AreEqual(ExpectedErrorCode, Error.Code);
				return null;
			}

			Dictionary<string, object> Values;

			if (Response.Decoded is HtmlDocument HtmlDocument)
				Values = await CompleteAuthorizationForm(HtmlDocument, FormPostback);
			else
			{
				Values = Response.Decoded as Dictionary<string, object>;
				Assert.IsNotNull(Values);
			}

			Assert.IsTrue(Values.ContainsKey("error"), "Expected OAuth authorization error response.");

			if (Values.TryGetValue("error_description", out object Description))
			{
				Assert.IsTrue(Description is string, "error_description is not a string.");
				Assert.IsFalse(string.IsNullOrEmpty((string)Description));
			}

			if (FormPostback is not null &&
				FormPostback.TryGetValue("state", out string ExpectedState))
			{
				Assert.IsTrue(Values.TryGetValue("state", out object ReturnedState),
					"Expected state in OAuth authorization error response.");

				Assert.AreEqual(ExpectedState, ReturnedState);
			}

			return Values;
		}

		[TestMethod]
		public async Task Test_033_DuplicateTokenParameterIsRejected()
		{
			byte[] Encoded = Encoding.UTF8.GetBytes(
				"grant_type=password" +
				"&grant_type=client_credentials" +
				"&username=" + Uri.EscapeDataString(TestUserName) +
				"&password=" + Uri.EscapeDataString(TestPassword));
			CustomEncoding Request = new("application/x-www-form-urlencoded", Encoded);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				Request);

			AssertOAuthError(Response, "invalid_request", BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_034_TokenEndpointRejectsMultipleClientAuthenticationMethods()
		{
			using HttpClient Client = new();
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
				"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(TestUserName + ":" + TestPassword)));

			using FormUrlEncodedContent Content = new(new Dictionary<string, string>()
			{
				{ "grant_type", "client_credentials" },
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword }
			});

			using HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, Response.StatusCode);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceS256, "S256")]
		[DataRow(LoginMethod.CodePostWithPkceS256, "S256")]
		public async Task Test_035_S256PkceDowngradeProtection(LoginMethod Method, string MethodName)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);
			string CodeChallenge = CreateCodeChallenge(AuthorizationCode.CodeVerifier, MethodName);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", CodeChallenge }
				});

			OAuthError Error = AssertOAuthError(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code_verifier", Error.Description);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_036_CodeVerifierWithoutChallengeIsRejected(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", CreatePkceCodeVerifier() }
				});

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_037_BearerTokenInQueryStringIsRejected(LoginMethod Method)
		{
			TokenResult Token = await Login(Method);

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + ProtectedResource + "?access_token=" + Uri.EscapeDataString(Token.AccessToken)));

			OAuthError Error = AssertOAuthError(Response, null, UnauthorizedException.Code);
			AssertBearerChallenge(Error);
		}

		[TestMethod]
		public async Task Test_038_AuthorizationEndpointHasClickjackingProtection()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			using HttpClient Client = new();
			using HttpResponseMessage Response = await Client.GetAsync(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri),
				CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);

			bool FoundProtection = false;
			if (Response.Headers.TryGetValues("X-Frame-Options", out IEnumerable<string> XFrameOptions))
			{
				foreach (string Value in XFrameOptions)
				{
					if (!string.IsNullOrEmpty(Value))
						FoundProtection = true;
				}
			}

			if (Response.Headers.TryGetValues("Content-Security-Policy", out IEnumerable<string> CspValues))
			{
				foreach (string Value in CspValues)
				{
					if (Value.Contains("frame-ancestors", StringComparison.OrdinalIgnoreCase))
						FoundProtection = true;
				}
			}

			Assert.IsTrue(FoundProtection,
				"Expected X-Frame-Options or Content-Security-Policy frame-ancestors on the authorization page.");
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_039_AccessTokenIsSignedJwtWithExpiration(LoginMethod Method)
		{
			TokenResult Token = await Login(Method);

			IDictionary<string, object> Header = DecodeJwtPart(Token.AccessToken, 0);
			Assert.IsTrue(Header.TryGetValue("alg", out object Algorithm),
				"Expected JWT header to contain alg.");
			Assert.IsFalse(string.Equals("none", Algorithm as string, StringComparison.OrdinalIgnoreCase),
				"Expected signed JWT access token.");

			IDictionary<string, object> Payload = DecodeJwtPart(Token.AccessToken, 1);
			Assert.IsTrue(Payload.ContainsKey("sub"), "Expected JWT access token to contain sub.");
			AssertPositiveUnixTime(Required<object>(Payload, "exp"), "exp");
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		public async Task Test_040_AuthorizationCodeIssuesRefreshToken(LoginMethod Method)
		{
			TokenResult Token = await Login(Method);

			Assert.IsFalse(string.IsNullOrEmpty(Token.RefreshToken),
				"Expected authorization code grant to issue a refresh token.");
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		public async Task Test_041_RefreshTokenGrantReturnsAccessToken(LoginMethod Method)
		{
			TokenResult InitialToken = await Login(Method);
			Assert.IsFalse(string.IsNullOrEmpty(InitialToken.RefreshToken));

			TokenResult RefreshedToken = await RefreshAccessToken(InitialToken.RefreshToken, TestUserName);
			Assert.IsFalse(string.IsNullOrEmpty(RefreshedToken.AccessToken));
			await AssertHello(RefreshedToken.AccessToken, TestUserName);
		}

		private static Task<TokenResult> RefreshAccessToken(string RefreshToken,
			string ClientId)
		{
			return RefreshAccessToken(RefreshToken, ClientId, null);
		}

		private static async Task<TokenResult> RefreshAccessToken(string RefreshToken,
			string ClientId, string Scope)
		{
			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateRefreshTokenRequest(RefreshToken, ClientId, Scope));

			return AssertAccessTokenResponse(TokenResponse);
		}

		private static Dictionary<string, string> CreateRefreshTokenRequest(string RefreshToken,
			string ClientId)
		{
			return CreateRefreshTokenRequest(RefreshToken, ClientId, null);
		}

		private static Dictionary<string, string> CreateRefreshTokenRequest(string RefreshToken,
			string ClientId, string Scope)
		{
			Dictionary<string, string> Request = new()
			{
				{ "grant_type", "refresh_token" }
			};

			if (!string.IsNullOrEmpty(RefreshToken))
				Request["refresh_token"] = RefreshToken;

			if (!string.IsNullOrEmpty(ClientId))
				Request["client_id"] = ClientId;

			if (!string.IsNullOrEmpty(Scope))
				Request["scope"] = Scope;

			return Request;
		}

		[TestMethod]
		public async Task Test_042_RefreshTokenGrantRequiresRefreshToken()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "refresh_token" },
					{ "client_id", TestUserName }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_043_InvalidRefreshTokenIsRejected()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateRefreshTokenRequest("invalid-refresh-token", TestUserName));

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		public async Task Test_044_RefreshTokenIsBoundToClientId(LoginMethod Method)
		{
			TokenResult InitialToken = await Login(Method);
			Assert.IsFalse(string.IsNullOrEmpty(InitialToken.RefreshToken));

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateRefreshTokenRequest(InitialToken.RefreshToken, "Invalid User"));

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		public async Task Test_045_RefreshTokenRotationInvalidatesPreviousToken(LoginMethod Method)
		{
			TokenResult InitialToken = await Login(Method);
			Assert.IsFalse(string.IsNullOrEmpty(InitialToken.RefreshToken));

			TokenResult RefreshedToken = await RefreshAccessToken(InitialToken.RefreshToken, TestUserName);
			Assert.IsFalse(string.IsNullOrEmpty(RefreshedToken.RefreshToken),
				"Expected refresh token rotation to issue a replacement refresh token.");
			Assert.AreNotEqual(InitialToken.RefreshToken, RefreshedToken.RefreshToken,
				"Expected refresh token rotation to replace the previous refresh token.");

			ContentResponse ReuseResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateRefreshTokenRequest(InitialToken.RefreshToken, TestUserName));

			AssertOAuthError(ReuseResponse, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_046_GrantsThatShouldNotIssueRefreshTokens(LoginMethod Method)
		{
			AuthorizationResult Result = await Authorize(Method);

			Assert.IsTrue(Result.HasToken);
			Assert.IsTrue(string.IsNullOrEmpty(Result.RefreshToken));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		public async Task Test_047_RefreshTokenGrantSuccessfulResponseIsNotCacheable(LoginMethod Method)
		{
			TokenResult InitialToken = await Login(Method);
			Dictionary<string, string> Request = CreateRefreshTokenRequest(
				InitialToken.RefreshToken, TestUserName);

			using HttpClient Client = new();
			using FormUrlEncodedContent Content = new(Request);
			using HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);
			AssertNoStoreHeaders(Response);

			Dictionary<string, object> Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
			Assert.IsNotNull(Parsed);

			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "access_token")));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		public async Task Test_048_RefreshTokenGrantRejectsScopeEscalation(LoginMethod Method)
		{
			TokenResult InitialToken = await Login(Method);
			Dictionary<string, string> Request = CreateRefreshTokenRequest(
				InitialToken.RefreshToken, TestUserName);
			Request["scope"] = TestScopeOther;

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				Request);

			AssertOAuthError(Response, "invalid_scope", BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_049_DynamicClientRegistration_PublicClient()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Client", BaseUrl + CallbackResource);

			Assert.IsFalse(string.IsNullOrEmpty(Registration.ClientId));
			Assert.IsFalse(string.IsNullOrEmpty(Registration.RegistrationAccessToken));
			Assert.IsFalse(string.IsNullOrEmpty(Registration.RegistrationClientUri));
			Assert.IsTrue(Uri.TryCreate(Registration.RegistrationClientUri, UriKind.Absolute, out _),
				"Expected registration_client_uri to be an absolute URI.");
		}

		private class DynamicClientRegistrationResult
		{
			public string ClientId;
			public string ClientSecret;
			public string RegistrationAccessToken;
			public string RegistrationClientUri;
		}

		private static async Task<DynamicClientRegistrationResult> RegisterPublicClient(
			string ClientName, string RedirectUri)
		{
			Dictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath, null,
				CreatePublicClientRegistrationRequest(ClientName, RedirectUri),
				System.Net.HttpStatusCode.Created);

			return AssertClientRegistrationResponse(Response, null,
				ClientName, RedirectUri, "none", false);
		}

		private static Dictionary<string, object> CreatePublicClientRegistrationRequest(
			string ClientName, string RedirectUri)
		{
			return new Dictionary<string, object>()
			{
				{ "redirect_uris", new string[] { RedirectUri } },
				{ "client_name", ClientName },
				{ "grant_types", new string[] { "authorization_code", "refresh_token" } },
				{ "response_types", new string[] { "code" } },
				{ "token_endpoint_auth_method", "none" }
			};
		}

		private static DynamicClientRegistrationResult AssertClientRegistrationResponse(
			IDictionary<string, object> Response, string ExpectedClientId,
			string ExpectedClientName, string ExpectedRedirectUri,
			string ExpectedTokenEndpointAuthMethod, bool ExpectClientSecret)
		{
			string ClientId = Required<string>(Response, "client_id");
			Assert.IsFalse(string.IsNullOrEmpty(ClientId));

			string ClientSecret;

			if (ExpectClientSecret)
			{
				ClientSecret = Required<string>(Response, "client_secret");
				Assert.IsNotEmpty(ClientSecret, "Client secret cannot be empty.");
			}
			else
			{
				Assert.IsFalse(Response.ContainsKey("client_secret"),
					"A public client using token_endpoint_auth_method=none must not receive a client_secret.");

				ClientSecret = null;
			}

			if (!string.IsNullOrEmpty(ExpectedClientId))
				Assert.AreEqual(ExpectedClientId, ClientId);

			if (!string.IsNullOrEmpty(ExpectedClientName))
				Assert.AreEqual(ExpectedClientName, Required<string>(Response, "client_name"));

			if (!string.IsNullOrEmpty(ExpectedRedirectUri))
			{
				object[] RedirectUris = Required<string[]>(Response, "redirect_uris");
				Assert.Contains(ExpectedRedirectUri, RedirectUris);
			}

			Assert.Contains("authorization_code", Required<string[]>(Response, "grant_types"));
			Assert.Contains("refresh_token", Required<string[]>(Response, "grant_types"));
			Assert.Contains("code", Required<string[]>(Response, "response_types"));

			if (!string.IsNullOrEmpty(ExpectedTokenEndpointAuthMethod))
			{
				Assert.AreEqual(ExpectedTokenEndpointAuthMethod,
					Required<string>(Response, "token_endpoint_auth_method"));
			}

			string RegistrationAccessToken = Required<string>(Response,
				"registration_access_token");
			string RegistrationClientUri = Required<string>(Response,
				"registration_client_uri");

			Assert.IsFalse(string.IsNullOrEmpty(RegistrationAccessToken));
			Assert.IsFalse(string.IsNullOrEmpty(RegistrationClientUri));
			Assert.IsTrue(Uri.TryCreate(RegistrationClientUri, UriKind.Absolute, out _),
				"Expected registration_client_uri to be an absolute URI.");

			return new DynamicClientRegistrationResult()
			{
				ClientId = ClientId,
				ClientSecret = ClientSecret,
				RegistrationAccessToken = RegistrationAccessToken,
				RegistrationClientUri = RegistrationClientUri
			};
		}

		private static Task<Dictionary<string, object>> DoGet(string Uri,
			string AccessToken, System.Net.HttpStatusCode ExpectedStatusCode)
		{
			return DoRequest(HttpMethod.Get, Uri, AccessToken, null, ExpectedStatusCode, true);
		}

		private static Task<Dictionary<string, object>> DoPost(string Uri,
			string AccessToken, object Body, System.Net.HttpStatusCode ExpectedStatusCode)
		{
			return DoRequest(HttpMethod.Post, Uri, AccessToken, Body, ExpectedStatusCode, true);
		}

		private static Task<Dictionary<string, object>> DoPut(string Uri,
			string AccessToken, object Body, System.Net.HttpStatusCode ExpectedStatusCode)
		{
			return DoRequest(HttpMethod.Put, Uri, AccessToken, Body, ExpectedStatusCode, true);
		}

		private static async Task DoDelete(string Uri, string AccessToken,
			System.Net.HttpStatusCode ExpectedStatusCode)
		{
			await DoRequest(HttpMethod.Delete, Uri, AccessToken, null, ExpectedStatusCode, false);
		}

		private static async Task<Dictionary<string, object>> DoRequest(HttpMethod Method,
			string Uri, string AccessToken, object Body, System.Net.HttpStatusCode ExpectedStatusCode,
			bool ExpectResponse)
		{
			using HttpClient Client = new();
			using HttpRequestMessage Request = new()
			{
				Method = Method,
				RequestUri = new Uri(Uri)
			};

			if (Body is not null)
			{
				ContentResponse Encoded = await InternetContent.EncodeAsync(Body, Encoding.UTF8);
				Encoded.AssertOk();

				Request.Content = new ByteArrayContent(Encoded.Encoded);
				Request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(Encoded.ContentType);
			}

			if (!string.IsNullOrEmpty(AccessToken))
				Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

			using HttpResponseMessage Response = await Client.SendAsync(Request);

			Assert.AreEqual(ExpectedStatusCode, Response.StatusCode);
			AssertNoStoreHeaders(Response);

			if (!Response.IsSuccessStatusCode)
				ExpectResponse = true;

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);

			if (ExpectResponse)
			{
				Assert.IsNotEmpty(ResponseText, "Expected response body.");

				Dictionary<string, object> Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
				Assert.IsNotNull(Parsed);

				return Parsed;
			}
			else
			{
				Assert.IsEmpty(ResponseText, "Expected no response body.");
				return null;
			}
		}

		[TestMethod]
		public async Task Test_050_DynamicClientRegistrationCreatesConfidentialClient()
		{
			IDictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath, null,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "client_name", "Unit Test Confidential Client" },
					{ "grant_types", new string[] { "authorization_code", "refresh_token" } },
					{ "response_types", new string[] { "code" } },
					{ "token_endpoint_auth_method", "client_secret_basic" }
				},
				System.Net.HttpStatusCode.Created);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_id")));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_secret")));

			int ExpiresAt = Required<int>(Response, "client_secret_expires_at");

			if (ExpiresAt != 0)
			{
				int Now = (int)DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds;
				Assert.IsGreaterThan(Now, ExpiresAt);
			}

			Assert.AreEqual("client_secret_basic", Required<string>(Response, "token_endpoint_auth_method"));
		}

		[TestMethod]
		public async Task Test_051_DynamicClientRegistrationRejectsInvalidRedirectUri()
		{
			Dictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath, null,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource + "#fragment" } },
					{ "grant_types", new string[] { "authorization_code" } },
					{ "response_types", new string[] { "code" } }
				},
				System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("invalid_redirect_uri", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_052_DynamicClientRegistrationRejectsInconsistentGrantAndResponseTypes()
		{
			IDictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath, null,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "grant_types", new string[] { "implicit" } },
					{ "response_types", new string[] { "code" } }
				},
				System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("invalid_client_metadata", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_053_DynamicClientRegistrationRequiresJsonRequest()
		{
			await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath, null,
				new Dictionary<string, string>()
				{
					{ "redirect_uris", BaseUrl + CallbackResource },
				},
				System.Net.HttpStatusCode.BadRequest);
		}

		[TestMethod]
		public async Task Test_054_DynamicClientRegistrationAcceptsUnknownMetadata()
		{
			IDictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath, null,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "client_name", "Unit Test Client With Unknown Metadata" },
					{ "grant_types", new string[] { "authorization_code" } },
					{ "response_types", new string[] { "code" } },
					{ "unknown_metadata", "ignored" }
				},
				System.Net.HttpStatusCode.Created);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_id")));
			Assert.Contains(BaseUrl + CallbackResource, Required<string[]>(Response, "redirect_uris"));
		}

		[TestMethod]
		public async Task Test_055_DynamicClientRegistrationSuccessfulResponseIsNotCacheable()
		{
			Dictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath, null,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "client_name", "Unit Test Cache Client" },
					{ "grant_types", new string[] { "authorization_code" } },
					{ "response_types", new string[] { "code" } }
				},
				System.Net.HttpStatusCode.Created);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_id")));
		}

		[TestMethod]
		public async Task Test_056_DeviceAuthorizationResponseContainsRequiredValues()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);

			Assert.IsFalse(string.IsNullOrEmpty(Device.DeviceCode));
			Assert.IsFalse(string.IsNullOrEmpty(Device.UserCode));
			Assert.IsFalse(string.IsNullOrEmpty(Device.VerificationUri));
			Assert.IsGreaterThan(0, Device.ExpiresIn);
			Assert.IsGreaterThan(0, Device.Interval);

			if (!string.IsNullOrEmpty(Device.VerificationUriComplete))
			{
				Assert.IsTrue(Device.VerificationUriComplete.Contains(
					Uri.EscapeDataString(Device.UserCode), StringComparison.OrdinalIgnoreCase) ||
					Device.VerificationUriComplete.Contains(Device.UserCode, StringComparison.OrdinalIgnoreCase),
					"Expected verification_uri_complete to contain the user code or its escaped representation.");
			}
		}

		private static Task<DeviceAuthorizationResult> StartDeviceAuthorization(
			string ClientId)
		{
			return StartDeviceAuthorization(ClientId, null);
		}

		private static async Task<DeviceAuthorizationResult> StartDeviceAuthorization(
			string ClientId, string Scope)
		{
			Dictionary<string, string> Request = [];

			if (!string.IsNullOrEmpty(ClientId))
				Request["client_id"] = ClientId;

			if (!string.IsNullOrEmpty(Scope))
				Request["scope"] = Scope;

			using HttpClient Client = new();
			using FormUrlEncodedContent Content = new(Request);
			using HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath,
				Content, CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);
			AssertNoStoreHeaders(Response);

			Dictionary<string, object> Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
			Assert.IsNotNull(Parsed);

			DeviceAuthorizationResult Result = new()
			{
				DeviceCode = Required<string>(Parsed, "device_code"),
				UserCode = Required<string>(Parsed, "user_code"),
				VerificationUri = Required<string>(Parsed, "verification_uri"),
				ExpiresIn = Required<int>(Parsed, "expires_in"),
				Interval = 5
			};

			if (Parsed.TryGetValue("verification_uri_complete", out object Obj))
			{
				Assert.IsTrue(Obj is string);
				Result.VerificationUriComplete = (string)Obj;
			}

			if (Parsed.TryGetValue("interval", out Obj))
			{
				Assert.IsTrue(Obj is int);
				Result.Interval = (int)Obj;
			}

			return Result;
		}

		[TestMethod]
		public async Task Test_057_DeviceTokenPollingBeforeAuthorizationReturnsAuthorizationPending()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);

			Dictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, null,
				CreateDeviceTokenRequest(Device.DeviceCode, DeviceUserName),
				System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("authorization_pending", Required<string>(Error, "error"));
		}

		private static Dictionary<string, string> CreateDeviceTokenRequest(string DeviceCode,
			string ClientId)
		{
			Dictionary<string, string> Request = new()
			{
				{ "grant_type", OAuthDeviceAuthorizationResource.GrantType }
			};

			if (!string.IsNullOrEmpty(DeviceCode))
				Request["device_code"] = DeviceCode;

			if (!string.IsNullOrEmpty(ClientId))
				Request["client_id"] = ClientId;

			return Request;
		}

		[TestMethod]
		public async Task Test_058_DeviceTokenPollingRejectsInvalidClientId()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);

			Dictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, null,
				CreateDeviceTokenRequest(Device.DeviceCode, TestUserName),
				System.Net.HttpStatusCode.Forbidden);

			Assert.AreEqual("access_denied", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_059_DeviceTokenGrantRequiresDeviceCode()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", OAuthDeviceAuthorizationResource.GrantType },
					{ "client_id", TestUserName }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_060_DeviceTokenGrantRejectsInvalidDeviceCode()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateDeviceTokenRequest("invalid-device-code", DeviceUserName));

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_061_DeviceTokenGrantRequiresClientIdForPublicClient()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", OAuthDeviceAuthorizationResource.GrantType },
					{ "device_code", Device.DeviceCode }
				});

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_062_DeviceAuthorizationRejectsMissingClientId()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>());

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_063_DeviceFlowCompletesAfterUserVerification()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);
			await CompleteDeviceAuthorizationForm(Device, true, false, false, TestUserName, TestPassword);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateDeviceTokenRequest(Device.DeviceCode, DeviceUserName));

			TokenResult TokenResult = AssertAccessTokenResponse(TokenResponse);
			await AssertHello(TokenResult.AccessToken, DeviceUserName);
		}

		private static async Task<ContentResponse> CompleteDeviceAuthorizationForm(
			DeviceAuthorizationResult Device, bool Accept, bool Decline, bool AlreadyResponded,
			string UserName, string Password)
		{
			string VerificationUri = string.IsNullOrEmpty(Device.VerificationUriComplete) ?
				Device.VerificationUri : Device.VerificationUriComplete;

			ContentResponse VerificationResponse = await InternetContent.GetAsync(new Uri(VerificationUri));
			VerificationResponse.AssertOk();

			HtmlDocument HtmlDocument = VerificationResponse.Decoded as HtmlDocument;
			Assert.IsNotNull(HtmlDocument);

			Dictionary<string, string> FormPostback = [];
			Input[] InputFields = GetHtmlElements<Input>(HtmlDocument.Form);
			bool UserNameFieldFound = false;
			bool PasswordFieldFound = false;
			bool UserCodeFieldFound = false;
			bool AcceptFieldFound = false;
			bool DeclineFieldFound = false;

			foreach (Input Input in InputFields)
			{
				switch (Input["name"])
				{
					case "user_code":
						FormPostback["user_code"] = Device.UserCode;
						UserCodeFieldFound = true;
						break;

					case "Accept":
						FormPostback["Accept"] = CommonTypes.Encode(Accept);
						AcceptFieldFound = true;
						break;

					case "Decline":
						FormPostback["Decline"] = CommonTypes.Encode(Decline);
						DeclineFieldFound = true;
						break;

					case "UserName":
						FormPostback["UserName"] = UserName;
						UserNameFieldFound = true;
						break;

					case "Password":
						FormPostback["Password"] = Password;
						PasswordFieldFound = true;
						break;

					default:
						if (!FormPostback.ContainsKey(Input["name"]))
							FormPostback[Input["name"]] = Input["value"];
						break;
				}
			}

			if (AlreadyResponded)
			{
				Assert.IsFalse(AcceptFieldFound, "Expected Accept checkbox to be absent after user has already responded.");
				Assert.IsFalse(DeclineFieldFound, "Expected Decline checkbox to be absent after user has already responded.");
				Assert.IsFalse(UserNameFieldFound, "Expected UserName field to be absent after user has already responded.");
				Assert.IsFalse(PasswordFieldFound, "Expected Password field to be absent after user has already responded.");
				Assert.IsFalse(UserCodeFieldFound, "Expected UserCode field to be absent after user has already responded.");
				Assert.IsTrue(TryGetErrorMessage(HtmlDocument, out _), "Expected error message in form when user has already responded.");

				return VerificationResponse;
			}
			else
			{
				Assert.IsTrue(AcceptFieldFound, "Expected Accept checkbox to be present when user has not already responded.");
				Assert.IsTrue(DeclineFieldFound, "Expected Decline checkbox to be present when user has not already responded.");
				Assert.IsTrue(UserNameFieldFound, "Expected UserName field to be present when user has not already responded.");
				Assert.IsTrue(PasswordFieldFound, "Expected Password field to be present when user has not already responded.");
				Assert.IsTrue(UserCodeFieldFound, "Expected UserCode field to be present when user has not already responded.");
			}

			ContentResponse LoginResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath),
				FormPostback);

			if (LoginResponse.HasError)
				throw new LoginError(LoginResponse);

			if (LoginResponse.Decoded is HtmlDocument HtmlDocument2 &&
				TryGetErrorMessage(HtmlDocument2, out string ErrorMessage))
			{
				throw new LoginError(ErrorMessage);
			}

			return LoginResponse;
		}

		private static T[] GetHtmlElements<T>(IEnumerable<HtmlNode> HtmlNodes)
			where T : HtmlElement
		{
			ChunkedList<T> Found = [];
			ChunkedList<HtmlNode> ToProcess = [];
			ToProcess.AddRange(HtmlNodes);

			while (ToProcess.HasFirstItem)
			{
				HtmlNode N = ToProcess.RemoveFirst();
				if (N is not HtmlElement E)
					continue;

				if (N is T OfInterest)
					Found.Add(OfInterest);

				if (E.HasChildren)
					ToProcess.AddRange(E.Children);
			}

			return [.. Found];
		}

		[TestMethod]
		public async Task Test_064_DeviceTokenPollingTooFastReturnsSlowDown()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);

			await DoPost(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, null,
				CreateDeviceTokenRequest(Device.DeviceCode, DeviceUserName),
				System.Net.HttpStatusCode.BadRequest);

			IDictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, null,
				CreateDeviceTokenRequest(Device.DeviceCode, DeviceUserName),
				System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("slow_down", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_065_DeviceUserCodeIsSingleUse()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);
			await CompleteDeviceAuthorizationForm(Device, true, false, false, TestUserName, TestPassword);

			ContentResponse SecondVerificationResponse = await CompleteDeviceAuthorizationForm(
				Device, true, false, true, TestUserName, TestPassword);

			if (SecondVerificationResponse.HasError)
				return;

			if (SecondVerificationResponse.Decoded is HtmlDocument HtmlDocument &&
				TryGetErrorMessage(HtmlDocument, out string ErrorMessage))
			{
				Assert.IsFalse(string.IsNullOrEmpty(ErrorMessage));
				return;
			}

			Assert.Fail("Expected second use of the device user code to be rejected or to return an error form.");
		}

		[TestMethod]
		public async Task Test_066_DeviceVerificationRequiresUserCode()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(Device.VerificationUri),
				new Dictionary<string, string>()
				{
					{ "client_id", TestUserName },
					{ "client_secret", TestPassword }
				});

			AssertVerificationError(Response);
		}

		[TestMethod]
		public async Task Test_067_DeviceVerificationRejectsInvalidUserCode()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(Device.VerificationUri),
				new Dictionary<string, string>()
				{
					{ "user_code", "invalid-user-code" },
					{ "client_id", TestUserName },
					{ "client_secret", TestPassword }
				});

			AssertVerificationError(Response);
		}

		[TestMethod]
		public async Task Test_068_DeviceVerificationRejectsInvalidLogin()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);

			await Assert.ThrowsAsync<LoginError>(async () => await CompleteDeviceAuthorizationForm(
				Device, true, false, false, TestUserName, "Invalid Password"));
		}

		[TestMethod]
		public async Task Test_069_DeviceAuthorizationIgnoresUnknownParameter()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "client_id", DeviceUserName },
					{ "unknown_parameter", "ignored" }
				});

			Response.AssertOk();
			object Parsed = Response.Decoded;
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "device_code")));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "user_code")));
		}

		[TestMethod]
		public async Task Test_070_DeviceAuthorizationEmptyClientIdIsTreatedAsMissing()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "client_id", string.Empty }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_071_DuplicateDeviceAuthorizationParameterIsRejected()
		{
			using HttpClient Client = new();
			using StringContent Content = new(
				"client_id=" + Uri.EscapeDataString(TestUserName) +
				"&client_id=" + Uri.EscapeDataString("OtherUser"),
				Encoding.UTF8, "application/x-www-form-urlencoded");

			using HttpResponseMessage Response = await Client.PostAsync(
				new Uri(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath),
				Content, CancellationToken.None);

			Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, Response.StatusCode);
		}

		[TestMethod]
		public async Task Test_072_PasswordInvalidCredentials()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "password" },
					{ "username", TestUserName },
					{ "password", "Invalid Password" }
				});

			AssertOAuthError(Response, "invalid_grant", BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_073_BasicAuthenticationInvalidCredentials()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "client_credentials" }
				},
				new KeyValuePair<string, string>("Authorization", "Basic " +
					Convert.ToBase64String(Encoding.UTF8.GetBytes(TestUserName + ":Invalid Password"))));

			AssertOAuthError(Response, "invalid_client", UnauthorizedException.Code,
				ForbiddenException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_075_TokenPreservesScope(LoginMethod Method)
		{
			TokenResult Token = await Login(Method, TestScopeReadWrite);
			AssertScope(TestScopeReadWrite, Token.Scope);
			AssertJwtScope(Token.AccessToken, TestScopeReadWrite);
		}

		private static void AssertScope(string ExpectedScope, string ActualScope)
		{
			Assert.IsFalse(string.IsNullOrEmpty(ActualScope), "Missing scope.");

			HashSet<string> Expected = SplitScope(ExpectedScope);
			HashSet<string> Actual = SplitScope(ActualScope);

			Assert.HasCount(Expected.Count, Actual, "Unexpected number of scopes.");

			foreach (string Scope in Expected)
				Assert.Contains(Scope, Actual, "Missing scope: " + Scope);
		}

		private static void AssertJwtScope(string AccessToken, string ExpectedScope)
		{
			IDictionary<string, object> Payload = DecodeJwtPart(AccessToken, 1);
			AssertScope(ExpectedScope, Required<string>(Payload, "scope"));
		}

		private static HashSet<string> SplitScope(string Scope)
		{
			HashSet<string> Result = new(StringComparer.Ordinal);

			foreach (string Part in Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
				Result.Add(Part);

			return Result;
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		public async Task Test_076_RefreshTokenPreserveScope(LoginMethod Method)
		{
			TokenResult InitialToken = await Login(Method, TestScopeReadWrite);
			TokenResult RefreshedToken = await RefreshAccessToken(
				InitialToken.RefreshToken, TestUserName);

			AssertScope(TestScopeReadWrite, RefreshedToken.Scope);
			AssertJwtScope(RefreshedToken.AccessToken, TestScopeReadWrite);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		public async Task Test_077_RefreshTokenGrantCanReduceScope(LoginMethod Method)
		{
			TokenResult InitialToken = await Login(Method, TestScopeReadWrite);
			TokenResult RefreshedToken = await RefreshAccessToken(
				InitialToken.RefreshToken, TestUserName, TestScopeRead);

			AssertScope(TestScopeRead, RefreshedToken.Scope);
			AssertJwtScope(RefreshedToken.AccessToken, TestScopeRead);
		}

		[TestMethod]
		public async Task Test_078_DeviceAuthorizationGrantPreservesScope()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(
				DeviceUserName, TestScopeReadWrite);
			await CompleteDeviceAuthorizationForm(Device, true, false, false, TestUserName, TestPassword);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateDeviceTokenRequest(Device.DeviceCode, DeviceUserName));

			TokenResult Token = AssertAccessTokenResponse(TokenResponse);
			AssertScope(TestScopeReadWrite, Token.Scope);
			AssertJwtScope(Token.AccessToken, TestScopeReadWrite);
		}

		[TestMethod]
		public async Task Test_079_AuthorizationMalformedScope()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;
			string Scope = "read\"write";

			string ScopeParameter = string.IsNullOrEmpty(Scope) ? string.Empty :
				"&scope=" + Uri.EscapeDataString(Scope);

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
				ScopeParameter));

			AssertOAuthError(Response, "invalid_scope", BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_080_TokenMalformedScope()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "password" },
					{ "username", TestUserName },
					{ "password", TestPassword },
					{ "scope", "read\"write" }
				});

			AssertOAuthError(Response, "invalid_scope", BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_081_ClientCredentialsInvalid()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "client_credentials" },
					{ "client_id", TestUserName },
					{ "client_secret", "Invalid Password" }
				});

			AssertOAuthError(Response, "invalid_client",
				BadRequestException.Code, UnauthorizedException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_082_DeviceAuthorizationDeclineReportsAccessDenied()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(DeviceUserName);
			await CompleteDeviceAuthorizationForm(Device, false, true, false, TestUserName, TestPassword);

			IDictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, null,
				CreateDeviceTokenRequest(Device.DeviceCode, DeviceUserName),
				System.Net.HttpStatusCode.Forbidden);

			Assert.AreEqual("access_denied", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_083_ReadDynamicClientRegistration()
		{
			string RedirectUri = BaseUrl + CallbackResource;
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Read Client", RedirectUri);

			IDictionary<string, object> Response = await DoGet(
				Registration.RegistrationClientUri,
				Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);

			DynamicClientRegistration Read = AssertDynamicClientRegistration(Response);

			Assert.AreEqual(Registration.ClientId, Read.ClientId);
			Assert.AreEqual("Unit Test Read Client", Read.ClientName);
			Assert.Contains(RedirectUri, Read.RedirectUris);
			Assert.AreEqual("none", Read.TokenEndpointAuthMethod);
		}

		private static DynamicClientRegistration AssertDynamicClientRegistration(
			IDictionary<string, object> Response)
		{
			string ClientId = Required<string>(Response, "client_id");
			string ClientName = Required<string>(Response, "client_name");
			string[] RedirectUris = Required<string[]>(Response, "redirect_uris");
			string[] GrantTypes = Required<string[]>(Response, "grant_types");
			string[] ResponseTypes = Required<string[]>(Response, "response_types");
			string TokenEndpointAuthMethod = Required<string>(Response, "token_endpoint_auth_method");

			Assert.Contains("authorization_code", GrantTypes);
			Assert.Contains("refresh_token", GrantTypes);
			Assert.Contains("code", ResponseTypes);

			string RegistrationAccessToken = Required<string>(Response,
				"registration_access_token");
			string RegistrationClientUri = Required<string>(Response,
				"registration_client_uri");

			Assert.IsFalse(string.IsNullOrEmpty(RegistrationAccessToken));
			Assert.IsFalse(string.IsNullOrEmpty(RegistrationClientUri));
			Assert.IsTrue(Uri.TryCreate(RegistrationClientUri, UriKind.Absolute, out _),
				"Expected registration_client_uri to be an absolute URI.");

			return new DynamicClientRegistration()
			{
				ClientId = ClientId,
				ClientName = ClientName,
				RedirectUris = RedirectUris,
				GrantTypes = GrantTypes,
				ResponseTypes = ResponseTypes,
				TokenEndpointAuthMethod = TokenEndpointAuthMethod,
				RegistrationAccessToken = RegistrationAccessToken,
				RegistrationClientUri = RegistrationClientUri
			};
		}

		private class DynamicClientRegistration
		{
			public string ClientId;
			public string ClientName;
			public string[] RedirectUris;
			public string[] GrantTypes;
			public string[] ResponseTypes;
			public string TokenEndpointAuthMethod;
			public string RegistrationAccessToken;
			public string RegistrationClientUri;
		}

		[TestMethod]
		public async Task Test_084_UpdateDynamicClientRegistration()
		{
			string OriginalRedirectUri = BaseUrl + CallbackResource;
			string UpdatedRedirectUri = BaseUrl + "/UpdatedCallback";
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Update Client", OriginalRedirectUri);

			IDictionary<string, object> CurrentResponse = await DoGet(
				Registration.RegistrationClientUri,
				Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);
			DynamicClientRegistration Current = AssertDynamicClientRegistration(
				CurrentResponse);

			Assert.AreEqual(Registration.ClientId, Current.ClientId);
			Assert.AreEqual("Unit Test Update Client", Current.ClientName);
			Assert.Contains(OriginalRedirectUri, Current.RedirectUris);
			Assert.AreEqual("none", Current.TokenEndpointAuthMethod);

			Dictionary<string, object> UpdateRequest = CreatePublicClientRegistrationRequest(
				"Unit Test Updated Client", UpdatedRedirectUri);
			UpdateRequest["client_id"] = Registration.ClientId;

			IDictionary<string, object> UpdateResponse = await DoPut(
				Current.RegistrationClientUri, Current.RegistrationAccessToken,
				UpdateRequest, System.Net.HttpStatusCode.OK);

			DynamicClientRegistrationResult Updated = AssertClientRegistrationResponse(
				UpdateResponse, Registration.ClientId, "Unit Test Updated Client",
				UpdatedRedirectUri, "none", false);

			foreach (object RedirectUri in Required<string[]>(UpdateResponse, "redirect_uris"))
				Assert.AreNotEqual(OriginalRedirectUri, RedirectUri as string);

			IDictionary<string, object> RereadResponse = await DoGet(
				Updated.RegistrationClientUri, Updated.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);

			Current = AssertDynamicClientRegistration(RereadResponse);

			Assert.AreEqual(Registration.ClientId, Current.ClientId);
			Assert.AreEqual("Unit Test Updated Client", Current.ClientName);
			Assert.Contains(UpdatedRedirectUri, Current.RedirectUris);
			Assert.AreEqual("none", Current.TokenEndpointAuthMethod);
		}

		[TestMethod]
		public async Task Test_085_RegistrationUpdateRequiresAccessToken()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Update Auth Client", BaseUrl + CallbackResource);
			Dictionary<string, object> UpdateRequest = CreatePublicClientRegistrationRequest(
				"Unit Test Update Auth Client", BaseUrl + CallbackResource);
			UpdateRequest["client_id"] = Registration.ClientId;

			await DoPut(Registration.RegistrationClientUri, null, UpdateRequest,
				System.Net.HttpStatusCode.Unauthorized);

			await DoGet(Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);
		}

		[TestMethod]
		public async Task Test_086_RegistrationUpdateRequiresClientId()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Update Requires Client Id", BaseUrl + CallbackResource);

			Dictionary<string, object> UpdateRequest = CreatePublicClientRegistrationRequest(
				"Unit Test Update Requires Client Id", BaseUrl + CallbackResource);

			IDictionary<string, object> Error = await DoPut(
				Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				UpdateRequest, System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("invalid_client_metadata", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_087_RegistrationUpdateRejectsMismatchedClientId()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Mismatched Client Id", BaseUrl + CallbackResource);

			Dictionary<string, object> UpdateRequest = CreatePublicClientRegistrationRequest(
				"Unit Test Mismatched Client Id", BaseUrl + CallbackResource);
			UpdateRequest["client_id"] = "different-client-id";

			IDictionary<string, object> Error = await DoPut(
				Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				UpdateRequest, System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("invalid_client_metadata", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_088_RegistrationUpdateRejectsInvalidRedirectUri()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Invalid Redirect", BaseUrl + CallbackResource);

			Dictionary<string, object> UpdateRequest = CreatePublicClientRegistrationRequest(
				"Unit Test Invalid Redirect", BaseUrl + CallbackResource + "#fragment");
			UpdateRequest["client_id"] = Registration.ClientId;

			IDictionary<string, object> Error = await DoPut(
				Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				UpdateRequest, System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("invalid_redirect_uri", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_089_RegistrationUpdateRequiresJsonRequest()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Update Json", BaseUrl + CallbackResource);

			Dictionary<string, object> Error = await DoPut(
				Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				new Dictionary<string, string>()
				{
					{ "client_id", Registration.ClientId },
					{ "redirect_uris", BaseUrl + CallbackResource }
				},
				System.Net.HttpStatusCode.BadRequest);

			Assert.IsTrue(Error.ContainsKey("error"));
		}

		[TestMethod]
		public async Task Test_090_RegistrationDeleteRequiresAccessToken()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Delete Auth Client", BaseUrl + CallbackResource);

			await DoDelete(Registration.RegistrationClientUri, null,
				System.Net.HttpStatusCode.Unauthorized);

			await DoGet(Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);
		}

		[TestMethod]
		public async Task Test_091_RegistrationDeleteRejectsInvalidAccessToken()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Delete Invalid Token", BaseUrl + CallbackResource);

			await DoDelete(Registration.RegistrationClientUri, "invalid-registration-access-token",
				System.Net.HttpStatusCode.Unauthorized);

			await DoGet(Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);
		}

		[TestMethod]
		public async Task Test_092_DeleteRegistration()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Delete Client", BaseUrl + CallbackResource);

			await DoDelete(Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.NoContent);

			await DoGet(Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.Unauthorized);
		}

		[TestMethod]
		public async Task Test_093_RegistrationDeletedCannotBeUpdated()
		{
			DynamicClientRegistrationResult Registration = await RegisterPublicClient(
				"Unit Test Deleted Update Client", BaseUrl + CallbackResource);
			Dictionary<string, object> UpdateRequest = CreatePublicClientRegistrationRequest(
				"Unit Test Deleted Update Client", BaseUrl + CallbackResource);
			UpdateRequest["client_id"] = Registration.ClientId;

			await DoDelete(Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.NoContent);

			await DoPut(Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				UpdateRequest, System.Net.HttpStatusCode.Unauthorized);
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_094_CreateConfidentialClient(LoginMethod Method)
		{
			string RedirectUri = BaseUrl + CallbackResource;
			string TokenEndpointAuthMethod = GetTokenEdnpointAuthMethod(Method);
			DynamicClientRegistrationResult Registration = await RegisterConfidentialClient(
				"Unit Test Confidential Client", RedirectUri, TokenEndpointAuthMethod);

			Assert.IsFalse(string.IsNullOrEmpty(Registration.ClientId));
			Assert.IsFalse(string.IsNullOrEmpty(Registration.ClientSecret));

			IDictionary<string, object> Response = await DoGet(
				Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);
			DynamicClientRegistration Current = AssertDynamicClientRegistration(Response);

			Assert.AreEqual(Registration.ClientId, Current.ClientId);
			Assert.AreEqual("Unit Test Confidential Client", Current.ClientName);
			Assert.Contains(RedirectUri, Current.RedirectUris);
			Assert.Contains("client_credentials", Current.GrantTypes);
			Assert.AreEqual(TokenEndpointAuthMethod, Current.TokenEndpointAuthMethod);

			TokenResult Token = await Login(Method, Registration.ClientId,
				Registration.ClientSecret!);

			await AssertHello(Token.AccessToken, Registration.ClientId);
		}

		private static string GetTokenEdnpointAuthMethod(LoginMethod Method)
		{
			return Method switch
			{
				LoginMethod.ClientCredentials => "client_secret_post",
				LoginMethod.ClientCredentialsBasicAuth => "client_secret_basic",
				_ => throw new ArgumentException("Invalid login method for confidential client registration: " + Method.ToString(), nameof(Method))
			};
		}

		private static async Task<DynamicClientRegistrationResult> RegisterConfidentialClient(
			string ClientName, string RedirectUri, string TokenEndpointAuthMethod)
		{
			Dictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath, null,
				CreateConfidentialClientRegistrationRequest(ClientName, RedirectUri,
					TokenEndpointAuthMethod),
				System.Net.HttpStatusCode.Created);

			DynamicClientRegistrationResult Result = AssertClientRegistrationResponse(
				Response, null, ClientName, RedirectUri, TokenEndpointAuthMethod, true);

			Assert.Contains("client_credentials", Required<string[]>(Response, "grant_types"));

			int ExpiresAt = Required<int>(Response, "client_secret_expires_at");

			if (ExpiresAt != 0)
			{
				int Now = (int)DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds;
				Assert.IsGreaterThan(Now, ExpiresAt);
			}

			return Result;
		}

		private static Dictionary<string, object> CreateConfidentialClientRegistrationRequest(
			string ClientName, string RedirectUri, string TokenEndpointAuthMethod)
		{
			return new Dictionary<string, object>()
			{
				{ "redirect_uris", new string[] { RedirectUri } },
				{ "client_name", ClientName },
				{ "grant_types", new string[]
					{ "authorization_code", "refresh_token", "client_credentials" } },
				{ "response_types", new string[] { "code" } },
				{ "token_endpoint_auth_method", TokenEndpointAuthMethod }
			};
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_095_UpdateConfidentialClient(LoginMethod Method)
		{
			string TokenEndpointAuthMethod = GetTokenEdnpointAuthMethod(Method);
			string OriginalRedirectUri = BaseUrl + CallbackResource;
			string UpdatedRedirectUri = BaseUrl + "/UpdatedConfidentialCallback";
			DynamicClientRegistrationResult Registration = await RegisterConfidentialClient(
				"Unit Test Confidential Update Client", OriginalRedirectUri,
				TokenEndpointAuthMethod);
			string ClientSecret = Registration.ClientSecret!;

			Dictionary<string, object> UpdateRequest =
				CreateConfidentialClientRegistrationRequest(
					"Unit Test Updated Confidential Client", UpdatedRedirectUri,
					TokenEndpointAuthMethod);
			UpdateRequest["client_id"] = Registration.ClientId;

			IDictionary<string, object> UpdateResponse = await DoPut(
				Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				UpdateRequest, System.Net.HttpStatusCode.OK);
			DynamicClientRegistration Updated = AssertDynamicClientRegistration(UpdateResponse);

			Assert.AreEqual(Registration.ClientId, Updated.ClientId);
			Assert.AreEqual("Unit Test Updated Confidential Client", Updated.ClientName);
			Assert.Contains(UpdatedRedirectUri, Updated.RedirectUris);
			Assert.Contains("client_credentials", Updated.GrantTypes);
			Assert.AreEqual(TokenEndpointAuthMethod, Updated.TokenEndpointAuthMethod);

			foreach (string RedirectUri in Updated.RedirectUris)
				Assert.AreNotEqual(OriginalRedirectUri, RedirectUri);

			IDictionary<string, object> RereadResponse = await DoGet(
				Updated.RegistrationClientUri, Updated.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);
			DynamicClientRegistration Reread = AssertDynamicClientRegistration(RereadResponse);

			Assert.AreEqual(Registration.ClientId, Reread.ClientId);
			Assert.AreEqual("Unit Test Updated Confidential Client", Reread.ClientName);
			Assert.Contains(UpdatedRedirectUri, Reread.RedirectUris);
			Assert.AreEqual(TokenEndpointAuthMethod, Reread.TokenEndpointAuthMethod);

			TokenResult Token = await Login(Method, Registration.ClientId, ClientSecret);

			await AssertHello(Token.AccessToken, Registration.ClientId);
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_096_ChangeConfidentialClientPassword(LoginMethod Method)
		{
			string TokenEndpointAuthMethod = GetTokenEdnpointAuthMethod(Method);
			string RedirectUri = BaseUrl + CallbackResource;
			DynamicClientRegistrationResult Registration = await RegisterConfidentialClient(
				"Unit Test Confidential Password Client", RedirectUri,
				TokenEndpointAuthMethod);
			string PreviousClientSecret = Registration.ClientSecret!;
			string NewClientSecret = Guid.NewGuid().ToString();

			TokenResult InitialToken = await Login(Method,
				Registration.ClientId, PreviousClientSecret);

			await AssertHello(InitialToken.AccessToken, Registration.ClientId);

			Dictionary<string, object> UpdateRequest =
				CreateConfidentialClientRegistrationRequest(
					"Unit Test Confidential Password Client", RedirectUri,
					TokenEndpointAuthMethod);
			UpdateRequest["client_id"] = Registration.ClientId;
			UpdateRequest["client_secret"] = NewClientSecret;

			IDictionary<string, object> UpdateResponse = await DoPut(
				Registration.RegistrationClientUri, Registration.RegistrationAccessToken,
				UpdateRequest, System.Net.HttpStatusCode.OK);
			DynamicClientRegistration Updated = AssertDynamicClientRegistration(UpdateResponse);

			Assert.AreEqual(Registration.ClientId, Updated.ClientId);
			Assert.AreEqual(TokenEndpointAuthMethod, Updated.TokenEndpointAuthMethod);

			TokenResult Token = await Login(Method, Registration.ClientId, NewClientSecret);

			await AssertHello(Token.AccessToken, Registration.ClientId);

			LoginError Error = await Assert.ThrowsAsync<LoginError>(async () =>
				await Authorize(Method, Registration.ClientId, PreviousClientSecret,
				string.Empty));

			Assert.AreEqual("invalid_client", Error.ErrorCode, "Expected invalid_client error code.");
			Assert.IsTrue(
				Error.StatusCode == System.Net.HttpStatusCode.BadRequest ||
				Error.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
				Error.StatusCode == System.Net.HttpStatusCode.Forbidden);

			await DoGet(Updated.RegistrationClientUri, Updated.RegistrationAccessToken,
				System.Net.HttpStatusCode.OK);
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_097_DeleteConfidentialClient(LoginMethod Method)
		{
			string TokenEndpointAuthMethod = GetTokenEdnpointAuthMethod(Method);
			DynamicClientRegistrationResult Registration = await RegisterConfidentialClient(
				"Unit Test Confidential Delete Client", BaseUrl + CallbackResource,
				TokenEndpointAuthMethod);
			string ClientSecret = Registration.ClientSecret!;

			TokenResult Token = await Login(Method, Registration.ClientId, ClientSecret);
			await AssertHello(Token.AccessToken, Registration.ClientId);

			await DoDelete(Registration.RegistrationClientUri,
				Registration.RegistrationAccessToken, System.Net.HttpStatusCode.NoContent);

			await DoGet(Registration.RegistrationClientUri,
				Registration.RegistrationAccessToken, System.Net.HttpStatusCode.Unauthorized);

			LoginError Error = await Assert.ThrowsAsync<LoginError>(async () =>
				await Authorize(Method, Registration.ClientId, ClientSecret, string.Empty));

			Assert.AreEqual("invalid_client", Error.ErrorCode, "Expected invalid_client error code.");
			Assert.IsTrue(
				Error.StatusCode == System.Net.HttpStatusCode.BadRequest ||
				Error.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
				Error.StatusCode == System.Net.HttpStatusCode.Forbidden);

			ContentResponse HelloResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer " + Token.AccessToken));
			
			AssertOAuthError(HelloResponse, null, UnauthorizedException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_098_IntrospectionActiveAccessToken(LoginMethod Method)
		{
			TokenResult Token = await Login(Method, TestScopeReadWrite);

			IntrospectionHttpResponse Response = await IntrospectToken(
				Token.AccessToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);

			AssertActiveIntrospectionResponse(Response, Token.AccessToken,
				TestUserName, TestScopeReadWrite, true);
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_099_IntrospectionSupportsAdvertisedAuthenticationMethods(
			LoginMethod Method)
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeRead);

			IntrospectionHttpResponse Response = await IntrospectToken(
				Token.AccessToken, "access_token", Method, TestUserName, TestPassword);

			AssertActiveIntrospectionResponse(Response, Token.AccessToken,
				TestUserName, TestScopeRead, true);
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_100_IntrospectionActiveRefreshToken(LoginMethod Method)
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeReadWrite);

			IntrospectionHttpResponse Response = await IntrospectToken(
				Token.RefreshToken, "refresh_token", Method, TestUserName, TestPassword);

			AssertActiveIntrospectionResponse(Response, null,
				TestUserName, TestScopeReadWrite, false);
		}

		[TestMethod]
		public async Task Test_101_IntrospectionTokenTypeHintIsOptional()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeReadWrite);

			IntrospectionHttpResponse AccessTokenResponse = await IntrospectToken(
				Token.AccessToken, null, LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(AccessTokenResponse, Token.AccessToken,
				TestUserName, TestScopeReadWrite, true);

			IntrospectionHttpResponse RefreshTokenResponse = await IntrospectToken(
				Token.RefreshToken, null, LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(RefreshTokenResponse, null,
				TestUserName, TestScopeReadWrite, false);
		}

		[TestMethod]
		public async Task Test_102_IntrospectionWrongTokenTypeHintSearchesAllTokenTypes()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeReadWrite);

			IntrospectionHttpResponse AccessTokenResponse = await IntrospectToken(
				Token.AccessToken, "refresh_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(AccessTokenResponse, Token.AccessToken,
				TestUserName, TestScopeReadWrite, true);

			IntrospectionHttpResponse RefreshTokenResponse = await IntrospectToken(
				Token.RefreshToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(RefreshTokenResponse, null,
				TestUserName, TestScopeReadWrite, false);

			IntrospectionHttpResponse UnknownHintResponse = await IntrospectToken(
				Token.AccessToken, "unknown_token_type", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(UnknownHintResponse, Token.AccessToken,
				TestUserName, TestScopeReadWrite, true);
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_103_IntrospectionUnknownTokenReturnsInactive(LoginMethod Method)
		{
			IntrospectionHttpResponse Response = await IntrospectToken(
				"unknown-token-" + Guid.NewGuid().ToString(), "access_token", Method,
				TestUserName, TestPassword);

			AssertInactiveIntrospectionResponse(Response);
		}

		[TestMethod]
		public async Task Test_104_IntrospectionRequiresAuthentication()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeRead);

			IntrospectionHttpResponse Response = await IntrospectToken(
				Token.AccessToken, "access_token", null, null, null);

			Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, Response.StatusCode);
			AssertAuthenticationChallenge(Response, "Basic");
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_105_IntrospectionRejectsInvalidClientCredentials(
			LoginMethod Method)
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeRead);

			IntrospectionHttpResponse Response = await IntrospectToken(
				Token.AccessToken, "access_token", Method, TestUserName,
				"Invalid Password");

			Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, Response.StatusCode);

			if (Response.Body is not null)
				Assert.AreEqual("invalid_client", Required<string>(Response.Body, "error"));

			if (Method == LoginMethod.ClientCredentialsBasicAuth)
				AssertAuthenticationChallenge(Response, "Basic");
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_106_IntrospectionRequiresTokenParameter(LoginMethod Method)
		{
			IntrospectionHttpResponse Response = await IntrospectToken(
				null, "access_token", Method, TestUserName, TestPassword);

			AssertIntrospectionError(Response, System.Net.HttpStatusCode.BadRequest,
				"invalid_request");
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_107_IntrospectionEmptyTokenIsTreatedAsMissing(
			LoginMethod Method)
		{
			IntrospectionHttpResponse Response = await IntrospectToken(
				string.Empty, "access_token", Method, TestUserName, TestPassword);

			AssertIntrospectionError(Response, System.Net.HttpStatusCode.BadRequest,
				"invalid_request");
		}

		[TestMethod]
		public async Task Test_108_IntrospectionRejectsDuplicateTokenParameter()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeRead);
			string Form = "token=" + Uri.EscapeDataString(Token.AccessToken) +
				"&token=" + Uri.EscapeDataString("another-token") +
				"&token_type_hint=access_token";

			IntrospectionHttpResponse Response = await SendIntrospectionRequest(
				HttpMethod.Post,
				new StringContent(Form, Encoding.UTF8,
					"application/x-www-form-urlencoded"),
				null, LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);

			AssertIntrospectionError(Response, System.Net.HttpStatusCode.BadRequest,
				"invalid_request");
		}

		[TestMethod]
		public async Task Test_109_IntrospectionRequiresFormEncodedPost()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeRead);
			string Json = "{\"token\":\"" + Token.AccessToken + "\"}";

			IntrospectionHttpResponse Response = await SendIntrospectionRequest(
				HttpMethod.Post, new StringContent(Json, Encoding.UTF8, "application/json"),
				null, LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);

			Assert.IsTrue(
				Response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
				Response.StatusCode == System.Net.HttpStatusCode.UnsupportedMediaType,
				"Expected a non-form introspection request to be rejected.");
		}

		[TestMethod]
		public async Task Test_110_IntrospectionValidatesTokenTimeWindow()
		{
			DateTime Now = DateTime.UtcNow;
			string ValidToken = this.CreateIntrospectionTestToken(
				Now.AddHours(1), Now.AddMinutes(-1));
			string ExpiredToken = this.CreateIntrospectionTestToken(
				Now.AddMinutes(-1), Now.AddHours(-1));
			string NotYetValidToken = this.CreateIntrospectionTestToken(
				Now.AddHours(1), Now.AddMinutes(10));

			IntrospectionHttpResponse ValidResponse = await IntrospectToken(
				ValidToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(ValidResponse, ValidToken,
				TestUserName, TestScopeRead, true);

			IntrospectionHttpResponse ExpiredResponse = await IntrospectToken(
				ExpiredToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertInactiveIntrospectionResponse(ExpiredResponse);

			IntrospectionHttpResponse NotYetValidResponse = await IntrospectToken(
				NotYetValidToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertInactiveIntrospectionResponse(NotYetValidResponse);
		}

		[TestMethod]
		public async Task Test_111_IntrospectionRejectsInvalidTokenSignature()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeRead);
			string TamperedToken = TamperJwtSignature(Token.AccessToken);

			IntrospectionHttpResponse Response = await IntrospectToken(
				TamperedToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);

			AssertInactiveIntrospectionResponse(Response);
		}

		[TestMethod]
		public async Task Test_112_IntrospectionReportsDeprecatedAccessTokenInactive()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeRead);
			Assert.IsTrue(JwtToken.TryParse(Token.AccessToken, out JwtToken ParsedToken));

			JwtFactory.Deprecate(ParsedToken);

			IntrospectionHttpResponse Response = await IntrospectToken(
				Token.AccessToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);

			AssertInactiveIntrospectionResponse(Response);
		}

		[TestMethod]
		public async Task Test_113_IntrospectionReportsRotatedRefreshTokenInactive()
		{
			TokenResult InitialToken = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeReadWrite);

			IntrospectionHttpResponse InitialResponse = await IntrospectToken(
				InitialToken.RefreshToken, "refresh_token",
				LoginMethod.ClientCredentialsBasicAuth, TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(InitialResponse, null,
				TestUserName, TestScopeReadWrite, false);

			TokenResult RefreshedToken = await RefreshAccessToken(
				InitialToken.RefreshToken, TestUserName);

			IntrospectionHttpResponse PreviousResponse = await IntrospectToken(
				InitialToken.RefreshToken, "refresh_token",
				LoginMethod.ClientCredentialsBasicAuth, TestUserName, TestPassword);
			AssertInactiveIntrospectionResponse(PreviousResponse);

			IntrospectionHttpResponse ReplacementResponse = await IntrospectToken(
				RefreshedToken.RefreshToken, "refresh_token",
				LoginMethod.ClientCredentialsBasicAuth, TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(ReplacementResponse, null,
				TestUserName, TestScopeReadWrite, false);
		}

		[TestMethod]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		[DataRow(LoginMethod.ClientCredentials)]
		public async Task Test_114_IntrospectionReportsDeletedClientTokenInactive(
			LoginMethod Method)
		{
			string TokenEndpointAuthMethod = GetTokenEdnpointAuthMethod(Method);
			DynamicClientRegistrationResult Registration = await RegisterConfidentialClient(
				"Unit Test Introspection Delete Client", BaseUrl + CallbackResource,
				TokenEndpointAuthMethod);

			TokenResult Token = await Login(Method, Registration.ClientId,
				Registration.ClientSecret!);

			IntrospectionHttpResponse ActiveResponse = await IntrospectToken(
				Token.AccessToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(ActiveResponse, Token.AccessToken,
				Registration.ClientId, null, true);

			await DoDelete(Registration.RegistrationClientUri,
				Registration.RegistrationAccessToken, System.Net.HttpStatusCode.NoContent);

			IntrospectionHttpResponse InactiveResponse = await IntrospectToken(
				Token.AccessToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);
			AssertInactiveIntrospectionResponse(InactiveResponse);
		}

		[TestMethod]
		public async Task Test_115_IntrospectionValidatesTokenAudience()
		{
			DateTime Now = DateTime.UtcNow;
			string MatchingAudienceToken = this.CreateIntrospectionTestToken(
				Now.AddHours(1), Now.AddMinutes(-1), TestUserName);
			string OtherAudienceToken = this.CreateIntrospectionTestToken(
				Now.AddHours(1), Now.AddMinutes(-1), "other-resource-server");

			IntrospectionHttpResponse MatchingResponse = await IntrospectToken(
				MatchingAudienceToken, "access_token",
				LoginMethod.ClientCredentialsBasicAuth, TestUserName, TestPassword);
			AssertActiveIntrospectionResponse(MatchingResponse, MatchingAudienceToken,
				TestUserName, TestScopeRead, true);

			IntrospectionHttpResponse OtherResponse = await IntrospectToken(
				OtherAudienceToken, "access_token",
				LoginMethod.ClientCredentialsBasicAuth, TestUserName, TestPassword);
			AssertInactiveIntrospectionResponse(OtherResponse);
		}

		[TestMethod]
		public async Task Test_116_IntrospectionDeviceFlowAccessToken()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(
				DeviceUserName, TestScopeReadWrite);
			await CompleteDeviceAuthorizationForm(Device, true, false, false,
				TestUserName, TestPassword);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateDeviceTokenRequest(Device.DeviceCode, DeviceUserName));
			TokenResult Token = AssertAccessTokenResponse(TokenResponse);

			IntrospectionHttpResponse Response = await IntrospectToken(
				Token.AccessToken, "access_token", LoginMethod.ClientCredentialsBasicAuth,
				TestUserName, TestPassword);

			AssertActiveIntrospectionResponse(Response, Token.AccessToken,
				DeviceUserName, TestScopeReadWrite, true);
		}

		[TestMethod]
		public async Task Test_117_IntrospectionRejectsMultipleClientAuthenticationMethods()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256,
				TestScopeRead);
			Dictionary<string, string> Form = new()
			{
				{ "token", Token.AccessToken },
				{ "token_type_hint", "access_token" },
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword }
			};

			IntrospectionHttpResponse Response = await SendIntrospectionRequest(
				HttpMethod.Post, new FormUrlEncodedContent(Form), null,
				LoginMethod.ClientCredentialsBasicAuth, TestUserName, TestPassword);

			AssertIntrospectionError(Response, System.Net.HttpStatusCode.BadRequest,
				"invalid_request");
		}
		private class IntrospectionHttpResponse
		{
			public System.Net.HttpStatusCode StatusCode;
			public Dictionary<string, object> Body;
			public string ContentType;
			public string[] AuthenticationChallenges;
		}

		private static Task<IntrospectionHttpResponse> IntrospectToken(
			string Token, string TokenTypeHint, LoginMethod? AuthenticationMethod,
			string ClientId, string ClientSecret)
		{
			Dictionary<string, string> Form = [];

			if (Token is not null)
				Form["token"] = Token;

			if (!string.IsNullOrEmpty(TokenTypeHint))
				Form["token_type_hint"] = TokenTypeHint;

			if (AuthenticationMethod == LoginMethod.ClientCredentials)
			{
				if (ClientId is not null)
					Form["client_id"] = ClientId;

				if (ClientSecret is not null)
					Form["client_secret"] = ClientSecret;
			}
			else if (AuthenticationMethod.HasValue &&
				AuthenticationMethod.Value != LoginMethod.ClientCredentialsBasicAuth)
			{
				throw new ArgumentException("Unsupported introspection authentication method: " +
					AuthenticationMethod.Value.ToString(), nameof(AuthenticationMethod));
			}

			return SendIntrospectionRequest(HttpMethod.Post,
				new FormUrlEncodedContent(Form), null, AuthenticationMethod,
				ClientId, ClientSecret);
		}

		private static async Task<IntrospectionHttpResponse> SendIntrospectionRequest(
			HttpMethod Method, HttpContent Content, string QueryString,
			LoginMethod? AuthenticationMethod, string ClientId, string ClientSecret)
		{
			using HttpClient Client = new();
			using HttpRequestMessage Request = new()
			{
				Method = Method,
				RequestUri = new Uri(BaseUrl + OAuthIntrospectionResource.DefaultResourcePath +
					(QueryString ?? string.Empty)),
				Content = Content
			};

			Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			if (AuthenticationMethod == LoginMethod.ClientCredentialsBasicAuth)
			{
				Request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
					Convert.ToBase64String(Encoding.UTF8.GetBytes(
						(ClientId ?? string.Empty) + ":" + (ClientSecret ?? string.Empty))));
			}

			using HttpResponseMessage Response = await Client.SendAsync(Request,
				CancellationToken.None);
			string ResponseText = await Response.Content.ReadAsStringAsync(
				CancellationToken.None);
			Dictionary<string, object> Parsed = null;

			if (!string.IsNullOrEmpty(ResponseText))
			{
				Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
				Assert.IsNotNull(Parsed, ResponseText);
			}

			List<string> Challenges = [];
			foreach (AuthenticationHeaderValue Challenge in Response.Headers.WwwAuthenticate)
				Challenges.Add(Challenge.ToString());

			return new IntrospectionHttpResponse()
			{
				StatusCode = Response.StatusCode,
				Body = Parsed,
				ContentType = Response.Content.Headers.ContentType?.MediaType,
				AuthenticationChallenges = [.. Challenges]
			};
		}

		private static IDictionary<string, object> AssertActiveIntrospectionResponse(
			IntrospectionHttpResponse Response, string AccessToken, string ExpectedClientId,
			string ExpectedScope, bool IsAccessToken)
		{
			Assert.AreEqual(System.Net.HttpStatusCode.OK, Response.StatusCode);
			Assert.AreEqual("application/json", Response.ContentType);
			Assert.IsNotNull(Response.Body);
			Assert.IsTrue(Required<bool>(Response.Body, "active"));

			if (!string.IsNullOrEmpty(ExpectedScope) &&
				Response.Body.TryGetValue("scope", out object Scope))
			{
				Assert.IsTrue(Scope is string, "Introspection scope is not a string.");
				AssertScope(ExpectedScope, (string)Scope);
			}

			if (!string.IsNullOrEmpty(ExpectedClientId) &&
				Response.Body.TryGetValue("client_id", out object ClientId))
			{
				Assert.AreEqual(ExpectedClientId, ClientId as string);
			}

			if (Response.Body.TryGetValue("username", out object UserName))
			{
				Assert.IsTrue(UserName is string);
				Assert.IsFalse(string.IsNullOrEmpty((string)UserName));
			}

			if (IsAccessToken && Response.Body.TryGetValue("token_type", out object TokenType))
				Assert.AreEqual("Bearer", TokenType as string);

			if (!string.IsNullOrEmpty(AccessToken))
			{
				IDictionary<string, object> Payload = DecodeJwtPart(AccessToken, 1);

				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.Scope);
				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.ClientId);
				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.Subject);
				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.Audience);
				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.Issuer);
				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.ExpirationTime);
				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.IssueTime);
				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.NotBeforeTime);
				AssertIntrospectionClaimMatchesJwt(Response.Body, Payload, JwtClaims.JwtId);
			}

			return Response.Body;
		}

		private static void AssertIntrospectionClaimMatchesJwt(
			IDictionary<string, object> IntrospectionResponse,
			IDictionary<string, object> JwtPayload, string ClaimName)
		{
			if (!IntrospectionResponse.TryGetValue(ClaimName, out object Actual) ||
				!JwtPayload.TryGetValue(ClaimName, out object Expected))
			{
				return;
			}

			switch (ClaimName)
			{
				case JwtClaims.ExpirationTime:
				case JwtClaims.IssueTime:
				case JwtClaims.NotBeforeTime:
					Assert.AreEqual(ToInt64(Expected, ClaimName), ToInt64(Actual, ClaimName));
					break;

				case JwtClaims.Audience:
					AssertStringValuesEqual(Expected, Actual, ClaimName);
					break;

				case JwtClaims.Scope:
					AssertScope(Expected as string, Actual as string);
					break;

				default:
					Assert.AreEqual(Expected?.ToString(), Actual?.ToString(),
						"Unexpected introspection claim value: " + ClaimName);
					break;
			}
		}

		private static void AssertStringValuesEqual(object ExpectedValue,
			object ActualValue, string Name)
		{
			HashSet<string> Expected = ToStringValues(ExpectedValue);
			HashSet<string> Actual = ToStringValues(ActualValue);

			Assert.HasCount(Expected.Count, Actual,
				"Unexpected number of values in " + Name + ".");

			foreach (string Value in Expected)
				Assert.Contains(Value, Actual, "Missing value in " + Name + ": " + Value);
		}

		private static HashSet<string> ToStringValues(object Value)
		{
			HashSet<string> Result = new(StringComparer.Ordinal);

			if (Value is string s)
				Result.Add(s);
			else if (Value is IEnumerable Values)
			{
				foreach (object Item in Values)
				{
					if (Item is not null)
						Result.Add(Item.ToString());
				}
			}
			else if (Value is not null)
				Result.Add(Value.ToString());

			return Result;
		}

		private static void AssertInactiveIntrospectionResponse(
			IntrospectionHttpResponse Response)
		{
			Assert.AreEqual(System.Net.HttpStatusCode.OK, Response.StatusCode);
			Assert.AreEqual("application/json", Response.ContentType);
			Assert.IsNotNull(Response.Body);
			Assert.HasCount(1, Response.Body,
				"An inactive introspection response should not disclose token metadata.");
			Assert.IsFalse(Required<bool>(Response.Body, "active"));
		}

		private static void AssertIntrospectionError(IntrospectionHttpResponse Response,
			System.Net.HttpStatusCode ExpectedStatusCode, string ExpectedErrorCode)
		{
			Assert.AreEqual(ExpectedStatusCode, Response.StatusCode);
			Assert.AreEqual("application/json", Response.ContentType);
			Assert.IsNotNull(Response.Body);
			Assert.AreEqual(ExpectedErrorCode, Required<string>(Response.Body, "error"));

			if (Response.Body.TryGetValue("error_description", out object Description))
			{
				Assert.IsTrue(Description is string);
				Assert.IsFalse(string.IsNullOrEmpty((string)Description));
			}
		}

		private static void AssertAuthenticationChallenge(IntrospectionHttpResponse Response,
			string Scheme)
		{
			bool Found = false;

			foreach (string Challenge in Response.AuthenticationChallenges)
			{
				if (Challenge.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
				{
					Found = true;
					break;
				}
			}

			Assert.IsTrue(Found, "Expected " + Scheme + " authentication challenge.");
		}

		private string CreateIntrospectionTestToken(DateTime Expiration,
			DateTime NotBefore)
		{
			return this.CreateIntrospectionTestToken(Expiration, NotBefore, null);
		}

		private string CreateIntrospectionTestToken(DateTime Expiration,
			DateTime NotBefore, string Audience)
		{
			List<KeyValuePair<string, object>> Claims =
			[
				new KeyValuePair<string, object>(JwtClaims.Issuer, BaseUrl),
				new KeyValuePair<string, object>(JwtClaims.Subject, TestUserName),
				new KeyValuePair<string, object>(JwtClaims.ClientId, TestUserName),
				new KeyValuePair<string, object>(JwtClaims.ExpirationTime, Expiration),
				new KeyValuePair<string, object>(JwtClaims.NotBeforeTime, NotBefore),
				new KeyValuePair<string, object>(JwtClaims.IssueTime, DateTime.UtcNow.AddMinutes(-1)),
				new KeyValuePair<string, object>(JwtClaims.JwtId, Guid.NewGuid().ToString()),
				new KeyValuePair<string, object>(JwtClaims.Scope, TestScopeRead)
			];

			if (!string.IsNullOrEmpty(Audience))
				Claims.Add(new KeyValuePair<string, object>(JwtClaims.Audience, Audience));

			return this.jwtFactory.Create(Claims);
		}

		private static string TamperJwtSignature(string Token)
		{
			string[] Parts = Token.Split('.');
			Assert.HasCount(3, Parts);
			Assert.IsFalse(string.IsNullOrEmpty(Parts[2]));

			char First = Parts[2][0];
			Parts[2] = (First == 'A' ? 'B' : 'A') + Parts[2][1..];
			return string.Join('.', Parts);
		}

		private static OAuthError AssertOAuthError(ContentResponse Response,
			params int[] ExpectedStatusCodes)
		{
			return AssertOAuthError(Response, null, ExpectedStatusCodes);
		}

		private static OAuthError AssertOAuthError(ContentResponse Response,
			string ExpectedErrorCode, params int[] ExpectedStatusCodes)
		{
			Assert.IsTrue(Response.HasError);
			WebException Result = Response.Error as WebException;
			Assert.IsNotNull(Result);

			bool IsExpectedCode = false;

			foreach (int ExpectedStatusCode in ExpectedStatusCodes)
			{
				if ((int)Result.StatusCode == ExpectedStatusCode)
				{
					IsExpectedCode = true;
					break;
				}
			}

			Assert.IsTrue(IsExpectedCode, "Unexpected status code: " + (int)Result.StatusCode);

			OAuthError Error = new()
			{
				Headers = Result.Headers
			};

			if (Result.Content is Dictionary<string, object> ErrorObj)
			{
				if (!ErrorObj.TryGetValue("error", out object Obj))
					throw new Exception("Error code missing in response.");

				if (Obj is not string ErrorCode)
					throw new Exception("Error code is not a string.");
				else
					Error.Code = ErrorCode;

				if (!ErrorObj.TryGetValue("error_description", out Obj))
					throw new Exception("Error description missing in response.");

				if (Obj is not string ErrorDescription)
					throw new Exception("Error description is not a string.");
				else
					Error.Description = ErrorDescription;

				if (!string.IsNullOrEmpty(ExpectedErrorCode))
					Assert.AreEqual(ExpectedErrorCode, Error.Code, "Expected error code: " + ExpectedErrorCode);
			}
			else if (!string.IsNullOrEmpty(ExpectedErrorCode))
			{
				if (Result.Content is not null)
					throw new Exception("Invalid error response.");
				else
					throw new Exception("Missing error object body.");
			}

			return Error;
		}

		private class OAuthError
		{
			public string Code;
			public string Description;
			public HttpHeaders Headers;
		}

		private static void AssertPositiveUnixTime(object Value, string ClaimName)
		{
			long Timestamp = ToInt64(Value, ClaimName);
			long Now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			Assert.IsGreaterThan(Now, Timestamp, "Expected " + ClaimName + " to be a future UNIX timestamp.");
		}

		private static long ToInt64(object Value, string Name)
		{
			Assert.IsNotNull(Value, "Missing " + Name + ".");

			if (Value is string s)
			{
				Assert.IsTrue(long.TryParse(s, out long Parsed),
					"Expected numeric " + Name + ".");
				return Parsed;
			}

			string Error;

			try
			{
				return Convert.ToInt64(Value);
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			Assert.Fail("Expected numeric " + Name + ": " + Error);
			return 0;
		}

		private static IDictionary<string, object> DecodeJwtPart(string Token, int Part)
		{
			Assert.IsFalse(string.IsNullOrEmpty(Token), "Missing JWT.");
			string[] Parts = Token.Split('.');
			Assert.HasCount(3, Parts, "Expected JWT to contain three parts.");

			string Encoded = Parts[Part].Replace('-', '+').Replace('_', '/');
			switch (Encoded.Length % 4)
			{
				case 2:
					Encoded += "==";
					break;

				case 3:
					Encoded += "=";
					break;
			}

			byte[] Decoded = Convert.FromBase64String(Encoded);
			object Parsed = JSON.Parse(Encoding.UTF8.GetString(Decoded));
			IDictionary<string, object> Result = Parsed as IDictionary<string, object>;
			Assert.IsNotNull(Result);
			return Result;
		}

		private static Task<TokenResult> Login(LoginMethod Method)
		{
			return Login(Method, TestUserName, TestPassword, null);
		}

		private static Task<TokenResult> Login(LoginMethod Method, string Scope)
		{
			return Login(Method, TestUserName, TestPassword, Scope);
		}

		private static Task<TokenResult> Login(LoginMethod Method,
			string UserName, string Password)
		{
			return Login(Method, UserName, Password, null);
		}

		private static async Task<TokenResult> Login(LoginMethod Method,
			string UserName, string Password, string Scope)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method, UserName,
				Password, Scope);

			Assert.AreEqual(BaseUrl, AuthorizationCode.Issuer);

			if (AuthorizationCode.HasToken)
			{
				return new TokenResult()
				{
					AccessToken = AuthorizationCode.Token,
					RefreshToken = AuthorizationCode.RefreshToken,
					ExpiresIn = AuthorizationCode.ExpiresIn,
					Scope = Scope
				};
			}

			Assert.IsTrue(AuthorizationCode.HasCode);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateAuthorizationCodeTokenRequest(AuthorizationCode, UserName));

			return AssertAccessTokenResponse(TokenResponse);
		}

		private static TokenResult AssertAccessTokenResponse(ContentResponse TokenResponse)
		{
			if (TokenResponse.HasError)
				throw new LoginError(TokenResponse);

			object Parsed = JSON.Parse(Encoding.UTF8.GetString(TokenResponse.Encoded));
			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));

			string AccessToken = Required<string>(Parsed, "access_token");
			Assert.IsFalse(string.IsNullOrEmpty(AccessToken));

			string RefreshToken = Required<string>(Parsed, "refresh_token");
			Assert.IsFalse(string.IsNullOrEmpty(RefreshToken));

			int ExpiresIn = Required<int>(Parsed, "expires_in");
			Assert.IsGreaterThan(0, ExpiresIn, "Expected positive expires_in.");

			string Scope = null;
			if (Parsed is IDictionary<string, object> ParsedDictionary &&
				ParsedDictionary.TryGetValue("scope", out object ScopeObj))
			{
				Assert.IsTrue(ScopeObj is string, "scope is not a string.");
				Scope = (string)ScopeObj;
			}

			return new TokenResult()
			{
				AccessToken = AccessToken,
				RefreshToken = RefreshToken,
				Scope = Scope,
				ExpiresIn = ExpiresIn
			};
		}

		private class AuthorizationResult
		{
			public bool HasCode => !string.IsNullOrEmpty(this.Code);
			public bool HasToken => !string.IsNullOrEmpty(this.Token);
			public string Code;
			public string CodeVerifier;
			public string RedirectUri;
			public string Token;
			public string RefreshToken;
			public string Scope;
			public string Issuer;
			public int ExpiresIn;
		}

		private class TokenResult
		{
			public string AccessToken;
			public string RefreshToken;
			public string Scope;
			public int ExpiresIn;
		}

		private class DeviceAuthorizationResult
		{
			public string DeviceCode;
			public string UserCode;
			public string VerificationUri;
			public string VerificationUriComplete;
			public int ExpiresIn;
			public int Interval;
		}

		private static Task<AuthorizationResult> Authorize(LoginMethod Method)
		{
			return Authorize(Method, TestUserName, TestPassword, string.Empty);
		}

		private static Task<AuthorizationResult> Authorize(LoginMethod Method, string Scope)
		{
			return Authorize(Method, TestUserName, TestPassword, Scope);
		}

		private static Task<AuthorizationResult> Authorize(LoginMethod Method,
			string UserName, string Password)
		{
			return Authorize(Method, UserName, Password, string.Empty);
		}

		private static async Task<AuthorizationResult> Authorize(LoginMethod Method,
			string UserName, string Password, string Scope)
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;
			string AuthorizeUri = BaseUrl + OAuthAuthorizeResource.DefaultResourcePath;
			string CodeVerifier = null;
			string CodeChallenge;
			bool ExpectCode = false;
			bool ExpectToken = false;
			bool ExpectState = true;
			ContentResponse AuthorizeResponse;
			ContentResponse LoginResponse;
			Dictionary<string, string> FormPostback = new()
			{
				{ "redirect_uri", RedirectUri },
				{ "state", State }
			};

			if (!string.IsNullOrEmpty(UserName))
				FormPostback["client_id"] = UserName;

			if (!string.IsNullOrEmpty(Password))
				FormPostback["client_secret"] = Password;

			if (!string.IsNullOrEmpty(Scope))
				FormPostback["scope"] = Scope;

			string ScopeParameter = string.IsNullOrEmpty(Scope) ? string.Empty :
				"&scope=" + Uri.EscapeDataString(Scope);

			switch (Method)
			{
				case LoginMethod.CodeForm:
					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=code" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						ScopeParameter +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));
					ExpectCode = true;
					break;

				case LoginMethod.CodePost:
					Dictionary<string, string> Request = new()
					{
						{ "response_type", "code" },
						{ "state", State },
						{ "redirect_uri", RedirectUri }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					if (!string.IsNullOrEmpty(Scope))
						Request["scope"] = Scope;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(AuthorizeUri), Request);
					ExpectCode = true;
					break;

				case LoginMethod.CodeFormWithPkceDefault:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "plain");

					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=code" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						ScopeParameter +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
						"&code_challenge=" + Uri.EscapeDataString(CodeChallenge)));

					FormPostback["code_challenge"] = CodeChallenge;
					ExpectCode = true;
					break;

				case LoginMethod.CodeFormWithPkcePlain:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "plain");

					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=code" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						ScopeParameter +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
						"&code_challenge=" + Uri.EscapeDataString(CodeChallenge) +
						"&code_challenge_method=" + Uri.EscapeDataString("plain")));

					FormPostback["code_challenge"] = CodeChallenge;
					FormPostback["code_challenge_method"] = "plain";
					ExpectCode = true;
					break;

				case LoginMethod.CodeFormWithPkceS256:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "S256");

					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=code" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						ScopeParameter +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
						"&code_challenge=" + Uri.EscapeDataString(CodeChallenge) +
						"&code_challenge_method=" + Uri.EscapeDataString("S256")));

					FormPostback["code_challenge"] = CodeChallenge;
					FormPostback["code_challenge_method"] = "S256";
					ExpectCode = true;
					break;

				case LoginMethod.CodePostWithPkcePlain:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "plain");
					Request = new()
					{
						{ "response_type", "code" },
						{ "state", State },
						{ "redirect_uri", RedirectUri },
						{ "code_challenge", CodeChallenge },
						{ "code_challenge_method", "plain" }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					if (!string.IsNullOrEmpty(Scope))
						Request["scope"] = Scope;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(AuthorizeUri), Request);

					FormPostback["code_challenge"] = CodeChallenge;
					FormPostback["code_challenge_method"] = "plain";
					ExpectCode = true;
					break;

				case LoginMethod.CodePostWithPkceS256:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "S256");

					Request = new()
					{
						{ "response_type", "code" },
						{ "state", State },
						{ "redirect_uri", RedirectUri },
						{ "code_challenge", CodeChallenge },
						{ "code_challenge_method", "S256" }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					if (!string.IsNullOrEmpty(Scope))
						Request["scope"] = Scope;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(AuthorizeUri), Request);

					FormPostback["code_challenge"] = CodeChallenge;
					FormPostback["code_challenge_method"] = "S256";
					ExpectCode = true;
					break;

				case LoginMethod.ImplicitGet:
					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=token" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						ScopeParameter +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)),
						new KeyValuePair<string, string>("X-Context", UserName));

					if (AuthorizeResponse.HasError)
						throw new LoginError(AuthorizeResponse);

					FormPostback = null;
					ExpectToken = true;
					break;

				case LoginMethod.ImplicitPost:
					Request = new()
					{
						{ "response_type", "token" },
						{ "state", State },
						{ "redirect_uri", RedirectUri }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					if (!string.IsNullOrEmpty(Scope))
						Request["scope"] = Scope;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(AuthorizeUri), Request,
						new KeyValuePair<string, string>("X-Context", UserName));

					if (AuthorizeResponse.HasError)
						throw new LoginError(AuthorizeResponse);

					FormPostback = null;
					ExpectToken = true;
					break;

				case LoginMethod.Password:
					Request = new Dictionary<string, string>()
					{
						{ "grant_type", "password" }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["username"] = UserName;

					if (!string.IsNullOrEmpty(Password))
						Request["password"] = Password;

					if (!string.IsNullOrEmpty(Scope))
						Request["scope"] = Scope;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
						Request);

					FormPostback = null;
					ExpectToken = true;
					ExpectState = false;
					break;

				case LoginMethod.ClientCredentials:
					Request = new Dictionary<string, string>()
					{
						{ "grant_type", "client_credentials" }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					if (!string.IsNullOrEmpty(Password))
						Request["client_secret"] = Password;

					if (!string.IsNullOrEmpty(Scope))
						Request["scope"] = Scope;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
						Request);

					FormPostback = null;
					ExpectToken = true;
					ExpectState = false;
					break;

				case LoginMethod.ClientCredentialsBasicAuth:
					Request = new Dictionary<string, string>()
					{
						{ "grant_type", "client_credentials" }
					};

					if (!string.IsNullOrEmpty(Scope))
						Request["scope"] = Scope;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath), Request,
						new KeyValuePair<string, string>("Authorization", "Basic " +
							Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password))));

					FormPostback = null;
					ExpectToken = true;
					ExpectState = false;
					break;

				default:
					throw new Exception("Unknown code method: " + Method);
			}

			if (AuthorizeResponse.HasError)
				throw new LoginError(AuthorizeResponse);

			Dictionary<string, object> Response;

			if (AuthorizeResponse.Decoded is HtmlDocument HtmlDocument)
				Response = await CompleteAuthorizationForm(HtmlDocument, FormPostback);
			else
			{
				Assert.IsNull(FormPostback);
				LoginResponse = AuthorizeResponse;
				Response = LoginResponse.Decoded as Dictionary<string, object>;
				Assert.IsNotNull(Response);
			}

			if (ExpectState)
			{
				Assert.IsTrue(Response.TryGetValue("state", out object ReturnedState), "Missing state in response.");
				Assert.IsTrue(ReturnedState is string, "Returned state not a string.");
				Assert.AreEqual(State, ReturnedState);
			}

			AuthorizationResult Result = new()
			{
				CodeVerifier = CodeVerifier,
				RedirectUri = RedirectUri
			};

			if (Response.TryGetValue("iss", out object Issuer))
				Result.Issuer = Issuer as string;

			if (Response.TryGetValue("scope", out object ScopeObj))
				Result.Scope = ScopeObj as string;

			if (ExpectCode)
			{
				Assert.IsTrue(Response.TryGetValue("code", out object Code), "Response did not contain code.");
				Result.Code = Code as string;
				Assert.IsFalse(string.IsNullOrEmpty(Result.Code));
			}

			if (ExpectToken)
			{
				Assert.IsTrue(Response.TryGetValue("access_token", out object Token), "Response did not contain token.");
				Result.Token = Token as string;
				Assert.IsFalse(string.IsNullOrEmpty(Result.Token));

				Assert.IsTrue(Response.TryGetValue("token_type", out object TokenType), "Response did not contain token_type.");
				Assert.AreEqual("Bearer", TokenType as string);

				if (Response.TryGetValue("refresh_token", out object RefreshToken))
					Result.RefreshToken = RefreshToken as string;

				if (Response.TryGetValue("expires_in", out object ExpiresIn))
				{
					if (ExpiresIn is int i ||
						ExpiresIn is string s && int.TryParse(s, out i))
					{
						Assert.IsGreaterThan(0, i);
						Result.ExpiresIn = i;
					}
					else
						Assert.Fail("Invalid expires_in");
				}
			}

			return Result;
		}

		private static async Task<Dictionary<string, object>> CompleteAuthorizationForm(
			HtmlDocument HtmlDocument, Dictionary<string, string> FormPostback)
		{
			Assert.IsNotNull(FormPostback);

			foreach (Input Input in GetHtmlElements<Input>(HtmlDocument.Form))
			{
				if (Input["type"] != "hidden")
					continue;

				if (!FormPostback.ContainsKey(Input["name"]))
					FormPostback[Input["name"]] = Input["value"];
			}

			ContentResponse LoginResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthAuthorizeResource.DefaultResourcePath), FormPostback);

			if (LoginResponse.HasError)
				throw new LoginError(LoginResponse);

			if (LoginResponse.Decoded is HtmlDocument HtmlDocumentResponse &&
				TryGetErrorMessage(HtmlDocumentResponse, out string ErrorMessage))
			{
				throw new LoginError(ErrorMessage);
			}

			Dictionary<string, object> Response = LoginResponse.Decoded as Dictionary<string, object>;
			Assert.IsNotNull(Response);

			return Response;
		}

		private static bool TryGetErrorMessage(HtmlDocument HtmlDocument, out string ErrorMessage)
		{
			int i = HtmlDocument.HtmlText.IndexOf("id='errorMessage'>");
			if (i > 0)
			{
				i += 18;
				int j = HtmlDocument.HtmlText.IndexOf("</", i);
				if (j > i)
				{
					ErrorMessage = XML.DecodeString(HtmlDocument.HtmlText[i..j].Trim());
					return true;
				}
			}

			ErrorMessage = null;
			return false;
		}

		private static void AssertVerificationError(ContentResponse Response)
		{
			if (Response.HasError)
				return;

			if (Response.Decoded is HtmlDocument HtmlDocument &&
				TryGetErrorMessage(HtmlDocument, out string ErrorMessage))
			{
				Assert.IsFalse(string.IsNullOrEmpty(ErrorMessage));
				return;
			}

			if (Response.Decoded is IDictionary<string, object> Values &&
				Values.ContainsKey("error"))
			{
				return;
			}

			Assert.Fail("Expected device verification to return an error.");
		}

		private class LoginError : Exception
		{
			public LoginError(ContentResponse Response)
				: base(GetMessage(Response, out string ErrorCode2, out string Description2,
					out System.Net.HttpStatusCode? StatusCode2))
			{
				this.ErrorCode = ErrorCode2;
				this.Description = Description2;
				this.StatusCode = StatusCode2;
			}

			public LoginError(string Error)
				: base(Error)
			{
				this.ErrorCode = null;
				this.Description = null;
				this.StatusCode = null;
			}

			public string ErrorCode;
			public string Description;
			public System.Net.HttpStatusCode? StatusCode;

			private static string GetMessage(ContentResponse Response,
				out string ErrorCode, out string Description, out System.Net.HttpStatusCode? StatusCode)
			{
				if (!Response.HasError)
				{
					ErrorCode = null;
					Description = null;
					StatusCode = null;
					return "Unknown login error.";
				}

				if (Response.Error is not WebException Error)
				{
					ErrorCode = null;
					Description = null;
					StatusCode = null;
					return Response.Error.Message;
				}

				StatusCode = Error.StatusCode;

				if (Error.Content is HtmlDocument HtmlDocument &&
					TryGetErrorMessage(HtmlDocument, out string ErrorMessage))
				{
					ErrorCode = null;
					Description = null;
					return ErrorMessage;
				}

				if (Error.Content is IDictionary<string, object> Values &&
					Values.TryGetValue("error", out object Obj) &&
					Obj is string ErrorCode2 &&
					Values.TryGetValue("error_description", out Obj) &&
					Obj is string Description2)
				{
					ErrorCode = ErrorCode2;
					Description = Description2;
				}
				else
				{
					ErrorCode = null;
					Description = null;
				}

				return Response.Error.Message;
			}
		}

		private static string CreatePkceCodeVerifier()
		{
			byte[] RandomBytes = new byte[32];
			using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
			Rnd.GetBytes(RandomBytes);
			return Base64Url.Encode(RandomBytes);
		}

		private static string CreateCodeChallenge(string CodeVerifier, string Method)
		{
			return Method switch
			{
				"plain" => CodeVerifier,
				"S256" => Base64Url.Encode(Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(CodeVerifier))),
				_ => throw new Exception("Unknown code challenge method: " + Method),
			};
		}

		private static T Required<T>(object Dictionary, string Key)
		{
			object Result;

			if (Dictionary is IDictionary<string, object> Typed &&
				Typed.TryGetValue(Key, out object GenericValue))
			{
				Result = GenericValue;
			}
			else if (Dictionary is IDictionary Untyped && Untyped.Contains(Key))
				Result = Untyped[Key];
			else
				throw new Exception("Expected JSON object to contain key: " + Key);

			if (Result is null)
				throw new Exception("Property value is null: " + Key);

			if (Result is not T TypedResult)
			{
				Type ExpectedType = typeof(T);
				Type ResultType = Result.GetType();
				Type ExpectedElementType;
				Type ResultElementType;

				if (ExpectedType.IsArray && ResultType.IsArray &&
					(ExpectedElementType = ExpectedType.GetElementType()) !=
					(ResultElementType = ResultType.GetElementType()))
				{
					Array ResultArray = (Array)Result;
					int i, c = ResultArray.Length;
					Array TypedResultArray = Array.CreateInstance(ExpectedElementType, c);

					for (i = 0; i < c; i++)
					{
						object Element = ResultArray.GetValue(i)
							?? throw new Exception("Property value is null: " + Key + "[" + i.ToString() + "]");

						if (!ExpectedElementType.IsAssignableFrom(Element.GetType()))
							throw new Exception("Property value not of expected type: " + Key +
								"[" + i.ToString() + "] (" + Element.GetType().FullName + ")");

						TypedResultArray.SetValue(Element, i);
					}

					return (T)(object)TypedResultArray;
				}

				throw new Exception("Property value not of expected type: " + Key +
					" (" + Result.GetType().FullName + ")");
			}

			return TypedResult;
		}
	}
}
