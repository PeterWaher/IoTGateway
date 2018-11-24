using System;
using System.Security.Cryptography;

namespace Waher.Utility.Csp
{
	class Program
	{
		static void Main(string[] Arguments)
		{
			try
			{
				CspProviderFlags Flags = CspProviderFlags.UseMachineKeyStore;
				CspParameters Csp;
				RSAParameters Parameters;
				int i, c = Arguments.Length;
				string s;
				string Key = null;
				int KeySize = 4096;

				if (c == 0)
				{
					c = 1;
					Arguments = new string[] { "-?" };
				}

				for (i = 0; i < c; i++)
				{
					s = Arguments[i];
					switch (s.ToLower())
					{
						case "-?":
							Console.Out.WriteLine("Command-line arguments:");
							Console.Out.WriteLine();
							Console.Out.WriteLine("-?     Shows this help.");
							Console.Out.WriteLine("-key NAME     Provides a name of a key.");
							Console.Out.WriteLine("-size SIZE    Provides a key size of the currently named key.");
							Console.Out.WriteLine("-currentuser  Uses the current user key store.");
							Console.Out.WriteLine("-cu           Same as -currentuser.");
							Console.Out.WriteLine("-machine      Uses the current machine key store.");
							Console.Out.WriteLine("-m            Same as -machine.");
							Console.Out.WriteLine("-default      Uses the default key store.");
							Console.Out.WriteLine("-d            Same as -default.");
							Console.Out.WriteLine("-ephemeral    Used for creating an ephemeral key.");
							Console.Out.WriteLine("-e            Same as -ephemeral.");
							Console.Out.WriteLine("-create       Creates a key with the given name and size.");
							Console.Out.WriteLine("-delete       Deletes the currently named key.");
							Console.Out.WriteLine("-get          Gets the currently named key.");
							break;

						case "-currentuser":
						case "-cu":
							Flags = CspProviderFlags.UseUserProtectedKey;
							break;

						case "-machine":
						case "-m":
							Flags = CspProviderFlags.UseMachineKeyStore;
							break;

						case "-default":
						case "-d":
							Flags = CspProviderFlags.UseDefaultKeyContainer;
							break;

						case "-ephemeral":
						case "-e":
							Flags = CspProviderFlags.CreateEphemeralKey;
							break;

						case "-key":
							if (++i >= c)
								throw new Exception("Expected key name.");

							Key = Arguments[i];
							break;

						case "-size":
							if (++i >= c)
								throw new Exception("Expected key size.");

							if (!int.TryParse(Arguments[i], out int j) || j <= 0)
								throw new Exception("Invalid key size.");

							KeySize = j;
							break;

						case "-create":
							Csp = new CspParameters()
							{
								KeyContainerName = Key,
								Flags = Flags
							};

							using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(KeySize, Csp))
							{
								Console.Out.WriteLine("Key created.");
							}
							break;

						case "-delete":
							Csp = new CspParameters()
							{
								KeyContainerName = Key,
								Flags = Flags | CspProviderFlags.UseExistingKey
							};

							using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(KeySize, Csp))
							{
								RSA.PersistKeyInCsp = false;    // Deletes key.
								RSA.Clear();
							}

							Console.Out.WriteLine("Key deleted.");
							break;

						case "-get":
							Csp = new CspParameters()
							{
								KeyContainerName = Key,
								Flags = Flags | CspProviderFlags.UseExistingKey
							};

							using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(KeySize, Csp))
							{
								Parameters = RSA.ExportParameters(true);

								Console.Out.WriteLine("Public:");
								Console.Out.WriteLine("n: " + Convert.ToBase64String(Parameters.Modulus));
								Console.Out.WriteLine("e: " + Convert.ToBase64String(Parameters.Exponent));

								Console.Out.WriteLine("Private:");
								Console.Out.WriteLine("p: " + Convert.ToBase64String(Parameters.P));
								Console.Out.WriteLine("q: " + Convert.ToBase64String(Parameters.Q));
								Console.Out.WriteLine("d: " + Convert.ToBase64String(Parameters.D));
								Console.Out.WriteLine("dP: " + Convert.ToBase64String(Parameters.DP));
								Console.Out.WriteLine("dQ: " + Convert.ToBase64String(Parameters.DQ));
								Console.Out.WriteLine("qinv: " + Convert.ToBase64String(Parameters.InverseQ));
							}
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
			}
		}
	}
}
