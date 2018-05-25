using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
	/// -nk                   Generates a new account key.
	/// -dns DOMAIN           Adds DOMAIN to the list of domain names when creating
	///                       an order for a new certificate. Can be used multiple
	///                       times.
	/// -nb TIMESTAMP         Generated certificate will not be valid before
	///                       TIMESTAMP.
	/// -na TIMESTAMP         Generated certificate will not be valid after
	///                       TIMESTAMP.
	/// -http ROOTFOLDER      Allows the application to respond to HTTP challenges
	///                       by storing temporary files under the corresponding ACME
	///                       challenge response folder /.well-known/acme-challenge
	/// -pi MS                Polling Interval, in milliseconds. Default value is
	///                       5000.
	/// -ks BITS              Certificate key size, in bits. Default is 4096.
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
			string HttpRootFolder = null;
			string s;
			int? PollingIngerval = null;
			int? KeySize = null;
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

						case "-http":
							if (i >= c)
								throw new Exception("Missing HTTP root folder.");

							if (HttpRootFolder == null)
								HttpRootFolder = args[i++];
							else
								throw new Exception("Only one HTTP Root Folder allowed.");
							break;

						case "-pi":
							if (i >= c)
								throw new Exception("Missing polling interval.");

							if (!int.TryParse(args[i++], out int j) || j <= 0)
								throw new Exception("Invalid polling interval.");

							if (PollingIngerval.HasValue)
								throw new Exception("Only one polling interval allowed.");
							else
								PollingIngerval = j;
							break;

						case "-ks":
							if (i >= c)
								throw new Exception("Missing key size.");

							if (!int.TryParse(args[i++], out j) || j <= 0)
								throw new Exception("Invalid key size.");

							if (KeySize.HasValue)
								throw new Exception("Only one key size allowed.");
							else
								KeySize = j;
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
					Console.Out.WriteLine("-nk                   Generates a new account key.");
					Console.Out.WriteLine("-dns DOMAIN           Adds DOMAIN to the list of domain names when creating");
					Console.Out.WriteLine("                      an order for a new certificate. Can be used multiple");
					Console.Out.WriteLine("                      times.");
					Console.Out.WriteLine("-nb TIMESTAMP         Generated certificate will not be valid before");
					Console.Out.WriteLine("                      TIMESTAMP.");
					Console.Out.WriteLine("-na TIMESTAMP         Generated certificate will not be valid after");
					Console.Out.WriteLine("                      TIMESTAMP.");
					Console.Out.WriteLine("-http ROOTFOLDER      Allows the application to respond to HTTP challenges");
					Console.Out.WriteLine("                      by storing temporary files under the corresponding ACME");
					Console.Out.WriteLine("                      challenge response folder /.well-known/acme-challenge");
					Console.Out.WriteLine("-pi MS                Polling Interval, in milliseconds. Default value is");
					Console.Out.WriteLine("                      5000.");
					Console.Out.WriteLine("-ks BITS              Certificate key size, in bits. Default is 4096.");
					Console.Out.WriteLine("-v                    Verbose mode.");
					Console.Out.WriteLine("-?                    Help.");
					return 0;
				}

				if (Verbose)
					Log.Register(new ConsoleEventSink(false));

				if (Directory == null)
					Directory = new Uri("https://acme-v02.api.letsencrypt.org/directory");

				if (!PollingIngerval.HasValue)
					PollingIngerval = 5000;

				if (!KeySize.HasValue)
					KeySize = 4096;

				Process(Verbose, Directory, ContactURLs?.ToArray(), TermsOfServiceAgreed, NewKey,
					DomainNames?.ToArray(), NotBefore, NotAfter, HttpRootFolder, PollingIngerval.Value,
					KeySize.Value).Wait();

				Console.Out.WriteLine("Press ENTER to continue."); // TODO: Remove
				Console.In.ReadLine();  // TODO: Remove
				return 0;
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);

				if (Verbose)
					Log.Error(ex.Message);
				else
					Console.Out.WriteLine(ex.Message);

				Console.Out.WriteLine("Press ENTER to continue."); // TODO: Remove
				Console.In.ReadLine();  // TODO: Remove
				return -1;
			}
		}

		private static async Task Process(bool Verbose, Uri Directory, string[] ContactURLs, bool TermsOfServiceAgreed,
			bool NewKey, string[] DomainNames, DateTime? NotBefore, DateTime? NotAfter, string HttpRootFolder, int PollingInterval,
			int KeySize)
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
					if (!string.IsNullOrEmpty(HttpRootFolder))
					{
						CheckExists(HttpRootFolder);
						HttpRootFolder = Path.Combine(HttpRootFolder, ".well-known");
						CheckExists(HttpRootFolder);
						HttpRootFolder = Path.Combine(HttpRootFolder, "acme-challenge");
						CheckExists(HttpRootFolder);
					}

					Log.Informational("Creating order.");

					AcmeOrder Order = await Account.OrderCertificate(DomainNames, NotBefore, NotAfter);

					Log.Informational("Order created.",
						new KeyValuePair<string, object>("Status", Order.Status),
						new KeyValuePair<string, object>("Expires", Order.Expires),
						new KeyValuePair<string, object>("NotBefore", Order.NotBefore),
						new KeyValuePair<string, object>("NotAfter", Order.NotAfter),
						new KeyValuePair<string, object>("Identifiers", Order.Identifiers));

					List<string> FileNames = null;

					try
					{
						foreach (AcmeAuthorization Authorization in await Order.GetAuthorizations())
						{
							Log.Informational("Processing authorization.",
								new KeyValuePair<string, object>("Type", Authorization.Type),
								new KeyValuePair<string, object>("Value", Authorization.Value),
								new KeyValuePair<string, object>("Expires", Authorization.Status),
								new KeyValuePair<string, object>("Expires", Authorization.Expires),
								new KeyValuePair<string, object>("Wildcard", Authorization.Wildcard));

							AcmeChallenge Challenge;
							bool Manual = true;
							int Index = 1;
							int NrChallenges = Authorization.Challenges.Length;
							string s;

							for (Index = 1; Index <= NrChallenges; Index++)
							{
								Challenge = Authorization.Challenges[Index - 1];

								if (Challenge is AcmeHttpChallenge HttpChallenge)
								{
									Log.Informational(Index.ToString() + ") HTTP challenge.",
										new KeyValuePair<string, object>("Resource", HttpChallenge.ResourceName),
										new KeyValuePair<string, object>("Response", HttpChallenge.KeyAuthorization),
										new KeyValuePair<string, object>("Content-Type", "application/octet-stream"));

									if (!string.IsNullOrEmpty(HttpRootFolder))
									{
										string FileName = Path.Combine(HttpRootFolder, HttpChallenge.Token);
										File.WriteAllBytes(FileName, Encoding.ASCII.GetBytes(HttpChallenge.KeyAuthorization));

										if (FileNames == null)
											FileNames = new List<string>();

										FileNames.Add(FileName);

										Log.Informational("Acknowleding challenge.");

										Challenge = await HttpChallenge.AcknowledgeChallenge();

										Log.Informational("Challenge acknowledged.",
											new KeyValuePair<string, object>("", Challenge.Status));

										Manual = false;
									}
									else if (!Verbose)
									{
										Console.Out.WriteLine(Index.ToString() + ") HTTP challenge.");
										Console.Out.WriteLine("Resource: " + HttpChallenge.ResourceName);
										Console.Out.WriteLine("Response: " + HttpChallenge.KeyAuthorization);
										Console.Out.WriteLine("Content-Type: " + "application/octet-stream");
									}
								}
								else if (Challenge is AcmeDnsChallenge DnsChallenge)
								{
									Log.Informational(Index.ToString() + ") DNS challenge.",
										new KeyValuePair<string, object>("Domain", DnsChallenge.ValidationDomainNamePrefix + Authorization.Value),
										new KeyValuePair<string, object>("TXT Record", DnsChallenge.KeyAuthorization));

									if (!Verbose)
									{
										Console.Out.WriteLine(Index.ToString() + ") DNS challenge.");
										Console.Out.WriteLine("Domain: " + DnsChallenge.ValidationDomainNamePrefix + Authorization.Value);
										Console.Out.WriteLine("TXT Record: " + DnsChallenge.KeyAuthorization);
									}
								}
							}

							if (Manual)
							{
								Console.Out.WriteLine();
								Console.Out.WriteLine("No automated method found to respond to any of the authorization challenges. " +
									"You can respond to a challenge manually. After configuring the corresponding " +
									"resource, enter the number of the corresponding challenge and press ENTER to acknowledge it.");

								do
								{
									Console.Out.Write("Challenge to acknowledge: ");
									s = Console.In.ReadLine();
								}
								while (!int.TryParse(s, out Index) || Index <= 0 || Index > NrChallenges);

								Log.Informational("Acknowleding challenge.");

								Challenge = await Authorization.Challenges[Index - 1].AcknowledgeChallenge();

								Log.Informational("Challenge acknowledged.",
									new KeyValuePair<string, object>("", Challenge.Status));
							}

							AcmeAuthorization Authorization2 = Authorization;

							do
							{
								Log.Informational("Waiting to poll authorization status.",
									new KeyValuePair<string, object>("ms", PollingInterval));

								System.Threading.Thread.Sleep(PollingInterval);

								Log.Informational("Polling authorization.");

								Authorization2 = await Authorization2.Poll();

								Log.Informational("Authorization polled.",
									new KeyValuePair<string, object>("Type", Authorization2.Type),
									new KeyValuePair<string, object>("Value", Authorization2.Value),
									new KeyValuePair<string, object>("Expires", Authorization2.Status),
									new KeyValuePair<string, object>("Expires", Authorization2.Expires),
									new KeyValuePair<string, object>("Wildcard", Authorization2.Wildcard));
							}
							while (Authorization2.Status == AcmeAuthorizationStatus.pending);

							if (Authorization2.Status != AcmeAuthorizationStatus.valid)
							{
								switch (Authorization2.Status)
								{
									case AcmeAuthorizationStatus.deactivated:
										throw new Exception("Authorization deactivated.");

									case AcmeAuthorizationStatus.expired:
										throw new Exception("Authorization expired.");

									case AcmeAuthorizationStatus.invalid:
										throw new Exception("Authorization invalid.");

									case AcmeAuthorizationStatus.revoked:
										throw new Exception("Authorization revoked.");
								}
							}
						}

						using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(KeySize))
						{
							Log.Informational("Finalizing order.");

							Order = await Order.FinalizeOrder(new CertificateRequest(new RsaSha256(RSA))
							{
								CommonName = DomainNames[0],
								SubjectAlternativeNames = DomainNames
							});

							Log.Informational("Order finalized.",
								new KeyValuePair<string, object>("Status", Order.Status),
								new KeyValuePair<string, object>("Expires", Order.Expires),
								new KeyValuePair<string, object>("NotBefore", Order.NotBefore),
								new KeyValuePair<string, object>("NotAfter", Order.NotAfter),
								new KeyValuePair<string, object>("Identifiers", Order.Identifiers));
						}
					}
					finally
					{
						if (FileNames != null)
						{
							foreach (string FileName in FileNames)
								File.Delete(FileName);
						}
					}
				}
			}
		}

		private static void CheckExists(string Folder)
		{
			if (!Directory.Exists(Folder))
			{
				Log.Informational("Creating folder.",
					new KeyValuePair<string, object>("Folder", Folder));

				Directory.CreateDirectory(Folder);

				Log.Informational("Folder created.",
					new KeyValuePair<string, object>("Folder", Folder));
			}
		}

		private static bool AreEqual(string[] A1, string[] A2)
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
