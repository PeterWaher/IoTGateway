using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Security.ACME;

namespace Waher.Utility.Acme
{
	/// <summary>
	/// Helps you create certificates using the Automatic Certificate 
	/// Management Environment (ACME) v2 protocol.
	/// 
	/// Command line switches:
	/// 
	/// -dir URI              URI to the ACME directory resource to use.
	///                       If not provided, the default Let's Encrypt
	///                       ACME v2 directory will be used:
	///                       https://acme-v02.api.letsencrypt.org/directory
	/// -le                   Uses the Let's Encrypt ACME v2 directory:
	///                       https://acme-v02.api.letsencrypt.org/directory
	/// -let                  Uses the Let's Encrypt ACME v2 staging directory:
	///                       https://acme-staging-v02.api.letsencrypt.org/directory
	/// -ce EMAIL             Adds EMAIL to the list of contact e-mail addresses
	///                       when creating an account. Can be used multiple
	///                       times.
	/// -cu URI               Adds URI to the list of contact URIs when creating
	///                       an account. Can be used multiple times.
	/// -a                    You agree to the terms of service agreement. This
	///                       might be required if you want to be able to create
	///                       an account.
	/// -dns DOMAIN           Adds DOMAIN to the list of domain names when creating
	///                       an order for a new certificate. Can be used multiple
	///                       times.
	/// -nb TIMESTAMP         Generated certificate will not be valid before
	///                       TIMESTAMP.
	/// -na TIMESTAMP         Generated certificate will not be valid after
	///                       TIMESTAMP.
	/// -nk                   Generates a new account key.
	/// -v                    Verbose mode.
	/// -?                    Help.
	/// </summary>
	class Program
	{
		static int Main(string[] args)
		{
			Uri Directory = null;
			List<string> ContactURLs = null;
			List<string> DomainNames = null;
			DateTime? NotBefore = null;
			DateTime? NotAfter = null;
			string s;
			int i = 0;
			int c = args.Length;
			bool Help = false;
			bool Verbose = false;
			bool TermsOfServiceAgreed = false;
			bool NewKey = false;

			try
			{
				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-dir":
							if (i >= c)
								throw new Exception("Missing directory URI.");

							if (Directory == null)
								Directory = new Uri(args[i++]);
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-le":
							if (Directory == null)
								Directory = new Uri("https://acme-v02.api.letsencrypt.org/directory");
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-let":
							if (Directory == null)
								Directory = new Uri("https://acme-staging-v02.api.letsencrypt.org/directory");
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-ce":
							if (i >= c)
								throw new Exception("Missing contact e-mail.");

							if (ContactURLs == null)
								ContactURLs = new List<string>();

							ContactURLs.Add("mailto:" + args[i++]);
							break;

						case "-cu":
							if (i >= c)
								throw new Exception("Missing contact URI.");

							if (ContactURLs == null)
								ContactURLs = new List<string>();

							ContactURLs.Add(args[i++]);
							break;

						case "-dns":
							if (i >= c)
								throw new Exception("Missing domain name.");

							if (DomainNames == null)
								DomainNames = new List<string>();

							DomainNames.Add(args[i++]);
							break;

						case "-na":
							if (i >= c)
								throw new Exception("Missing timestamp.");

							if (DateTime.TryParse(args[i++], out DateTime TP))
								NotAfter = TP;
							else
								throw new Exception("Invalid timestamp: " + args[i - 1]);
							break;

						case "-nb":
							if (i >= c)
								throw new Exception("Missing timestamp.");

							if (DateTime.TryParse(args[i++], out TP))
								NotBefore = TP;
							else
								throw new Exception("Invalid timestamp: " + args[i - 1]);
							break;

						case "-?":
							Help = true;
							break;

						case "-v":
							Verbose = true;
							break;

						case "-a":
							TermsOfServiceAgreed = true;
							break;

						case "-nk":
							NewKey = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
				{
					Console.Out.WriteLine("Helps you create certificates using the Automatic Certificate");
					Console.Out.WriteLine("Management Environment (ACME) v2 protocol.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-dir URI              URI to the ACME directory resource to use.");
					Console.Out.WriteLine("                      If not provided, the default Let's Encrypt");
					Console.Out.WriteLine("                      ACME v2 directory will be used:");
					Console.Out.WriteLine("                      https://acme-v02.api.letsencrypt.org/directory");
					Console.Out.WriteLine("-le                   Uses the Let's Encrypt ACME v2 directory:");
					Console.Out.WriteLine("                      https://acme-v02.api.letsencrypt.org/directory");
					Console.Out.WriteLine("-let                  Uses the Let's Encrypt ACME v2 staging directory:");
					Console.Out.WriteLine("                      https://acme-staging-v02.api.letsencrypt.org/directory");
					Console.Out.WriteLine("-ce EMAIL             Adds EMAIL to the list of contact e-mail addresses");
					Console.Out.WriteLine("                      when creating an account. Can be used multiple");
					Console.Out.WriteLine("                      times.");
					Console.Out.WriteLine("-cu URI               Adds URI to the list of contact URIs when creating");
					Console.Out.WriteLine("                      an account. Can be used multiple times.");
					Console.Out.WriteLine("-a                    You agree to the terms of service agreement. This");
					Console.Out.WriteLine("                      might be required if you want to be able to create");
					Console.Out.WriteLine("                      an account.");
					Console.Out.WriteLine("-dns DOMAIN           Adds DOMAIN to the list of domain names when creating");
					Console.Out.WriteLine("                      an order for a new certificate. Can be used multiple");
					Console.Out.WriteLine("                      times.");
					Console.Out.WriteLine("-nk                   Generates a new account key.");
					Console.Out.WriteLine("-v                    Verbose mode.");
					Console.Out.WriteLine("-?                    Help.");
					return 0;
				}

				if (Verbose)
					Log.Register(new ConsoleEventSink(false));

				if (Directory == null)
					Directory = new Uri("https://acme-v02.api.letsencrypt.org/directory");

				Process(Directory, ContactURLs?.ToArray(), TermsOfServiceAgreed, NewKey,
					DomainNames?.ToArray(), NotBefore, NotAfter).Wait();

				Console.Out.WriteLine("Press ENTER to continue."); // TODO: Remove
				Console.In.ReadLine();  // TODO: Remove
				return 0;
			}
			catch (Exception ex)
			{
				if (Verbose)
					Log.Critical(ex);
				else
					Console.Out.WriteLine(ex.Message);

				Console.Out.WriteLine("Press ENTER to continue."); // TODO: Remove
				Console.In.ReadLine();  // TODO: Remove
				return -1;
			}
		}

		private static async Task Process(Uri Directory, string[] ContactURLs, bool TermsOfServiceAgreed,
			bool NewKey, string[] DomainNames, DateTime? NotBefore, DateTime? NotAfter)
		{
			using (AcmeClient Client = new AcmeClient(Directory))
			{
				Log.Informational("Connecting to directory.",
					new KeyValuePair<string, object>("URL", Directory.ToString()));

				AcmeDirectory AcmeDirectory = await Client.GetDirectory();

				if (AcmeDirectory.ExternalAccountRequired)
					Log.Warning("An external account is required.");

				if (AcmeDirectory.TermsOfService != null)
				{
					Log.Informational("Terms of service available.",
						new KeyValuePair<string, object>("URL", AcmeDirectory.TermsOfService.ToString()));
				}

				if (AcmeDirectory.Website != null)
				{
					Log.Informational("Web site available.",
						new KeyValuePair<string, object>("URL", AcmeDirectory.Website.ToString()));
				}


				Log.Informational("Getting account.");

				AcmeAccount Account;

				try
				{
					Account = await Client.GetAccount();

					Log.Informational("Account found.",
						new KeyValuePair<string, object>("Created", Account.CreatedAt),
						new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
						new KeyValuePair<string, object>("Status", Account.Status),
						new KeyValuePair<string, object>("Contact", Account.Contact));

					if (ContactURLs != null && !AreEqual(Account.Contact, ContactURLs))
					{
						Log.Informational("Updating contact URIs in account.");

						Account = await Account.Update(ContactURLs);

						Log.Informational("Account updated.",
							new KeyValuePair<string, object>("Created", Account.CreatedAt),
							new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
							new KeyValuePair<string, object>("Status", Account.Status),
							new KeyValuePair<string, object>("Contact", Account.Contact));
					}
				}
				catch (AcmeAccountDoesNotExistException)
				{
					Log.Warning("Account not found. Creating account.",
						new KeyValuePair<string, object>("Contact", ContactURLs),
						new KeyValuePair<string, object>("TermsOfServiceAgreed", TermsOfServiceAgreed));

					Account = await Client.CreateAccount(ContactURLs, TermsOfServiceAgreed);

					Log.Informational("Account created.",
						new KeyValuePair<string, object>("Created", Account.CreatedAt),
						new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
						new KeyValuePair<string, object>("Status", Account.Status),
						new KeyValuePair<string, object>("Contact", Account.Contact));
				}

				if (NewKey)
				{
					Log.Informational("Generating new key.");

					await Account.NewKey();

					Log.Informational("New key generated.");
				}


				if (DomainNames != null)
				{
					Log.Informational("Creating order.");

					AcmeOrder Order = await Account.OrderCertificate(DomainNames, NotBefore, NotAfter);

					Log.Informational("Order created.",
						new KeyValuePair<string, object>("Status", Order.Status),
						new KeyValuePair<string, object>("Expires", Order.Expires),
						new KeyValuePair<string, object>("NotBefore", Order.NotBefore),
						new KeyValuePair<string, object>("NotAfter", Order.NotAfter),
						new KeyValuePair<string, object>("Identifiers", Order.Identifiers));

					//Order.Authorizations;
					//Order.AuthorizationUris;
				}
			}
		}

		public static bool AreEqual(string[] A1, string[] A2)
		{
			int i, c;

			if (A1 == null ^ A2 == null)
				return false;

			if (A1 == null)
				return true;

			c = A1.Length;
			if (A2.Length != c)
				return false;

			for (i = 0; i < c; i++)
			{
				if (A1[i] != A2[i])
					return false;
			}

			return true;
		}

	}
}
