using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Module maintaining available SASL mechanisms.
	/// </summary>
	public class SaslModule : IModule
	{
		private static IAuthenticationMechanism[] mechanisms = Array.Empty<IAuthenticationMechanism>();

		/// <summary>
		/// Starts the module.
		/// </summary>
		public async Task Start()
		{
			mechanisms = await GetMechanisms();
			Types.OnInvalidated += Types_OnInvalidated;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			Types.OnInvalidated -= Types_OnInvalidated;
			mechanisms = Array.Empty<IAuthenticationMechanism>();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Available SASL mechanisms.
		/// </summary>
		public static IAuthenticationMechanism[] Mechanisms => mechanisms;

		private static async void Types_OnInvalidated(object Sender, EventArgs e)
		{
			try
			{
				mechanisms = await GetMechanisms();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private static async Task<IAuthenticationMechanism[]> GetMechanisms()
		{
			Dictionary<string, bool> MechanismsFound = new Dictionary<string, bool>();
			List<IAuthenticationMechanism> Result = new List<IAuthenticationMechanism>();
			ConstructorInfo CI;
			IAuthenticationMechanism Mechanism;
			Type[] MechanismTypes = Types.GetTypesImplementingInterface(typeof(IAuthenticationMechanism));

			foreach (Type T in MechanismTypes)
			{
				if (T.IsAbstract)
					continue;

				CI = T.GetConstructor(Types.NoTypes);
				if (CI is null)
					continue;

				try
				{
					Mechanism = (IAuthenticationMechanism)CI.Invoke(Types.NoParameters);

					if (MechanismsFound.ContainsKey(Mechanism.Name))
						throw new Exception("Authentication mechanism collision." + T.FullName + ": " + Mechanism.Name);

					await Mechanism.Initialize();

					MechanismsFound[Mechanism.Name] = true;
					Result.Add(Mechanism);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			Result.Sort((m1, m2) => m2.Weight - m1.Weight);

			return Result.ToArray();
		}
	}
}
