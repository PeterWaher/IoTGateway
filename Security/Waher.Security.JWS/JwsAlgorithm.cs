using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Security.JWS
{
	/// <summary>
	/// Abstract base class for JWS algorithm.
	/// </summary>
	public abstract class JwsAlgorithm : IJwsAlgorithm
	{
		/// <summary>
		/// Short name for algorithm.
		/// </summary>
		public abstract string Name
		{
			get;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// Signs data.
		/// </summary>
		/// <param name="Header">Properties to include in the header.</param>
		/// <param name="Payload">Properties to include in the payload.</param>
		/// <param name="HeaderString">Resulting encoded header string.</param>
		/// <param name="PayloadString">Resulting encoded payload string.</param>
		/// <param name="Signature">Generated signature.</param>
		public virtual void Sign(IEnumerable<KeyValuePair<string, object>> Header,
			IEnumerable<KeyValuePair<string, object>> Payload, out string HeaderString,
			out string PayloadString, out string Signature)
		{
			string HeaderJson = JSON.Encode(Header, null, new KeyValuePair<string, object>("alg", this.Name));
			byte[] HeaderBin = Encoding.UTF8.GetBytes(HeaderJson);
			HeaderString = Base64Url.Encode(HeaderBin);

			string PayloadJson = JSON.Encode(Payload, null);
			byte[] PayloadBin = Encoding.UTF8.GetBytes(PayloadJson);
			PayloadString = Base64Url.Encode(PayloadBin);

			Signature = this.Sign(HeaderString, PayloadString);
		}

		/// <summary>
		/// Signs data.
		/// </summary>
		/// <param name="HeaderEncoded">Encoded properties to include in the header.</param>
		/// <param name="PayloadEncoded">Encoded properties to include in the payload.</param>
		/// <returns>Signature</returns>
		public abstract string Sign(string HeaderEncoded, string PayloadEncoded);

		/// <summary>
		/// Checks if a signature is valid.
		/// </summary>
		/// <param name="HeaderEncoded">Encoded properties to include in the header.</param>
		/// <param name="PayloadEncoded">Encoded properties to include in the payload.</param>
		/// <param name="SignatureEncoded">Encoded signature.</param>
		/// <returns>If the signature is valid.</returns>
		public virtual bool IsValid(string HeaderEncoded, string PayloadEncoded, string SignatureEncoded)
		{
			return this.Sign(HeaderEncoded, PayloadEncoded) == SignatureEncoded;
		}

		/// <summary>
		/// Gets the JWS algoritm that corresponds to a given algorithm name.
		/// </summary>
		/// <param name="Name">Algorithm name.</param>
		/// <param name="Algorithm">Algorithm object, if found.</param>
		/// <returns>If an algorithm with the given name was found.</returns>
		public static bool TryGetAlgorithm(string Name, out IJwsAlgorithm Algorithm)
		{
			lock (algorithms)
			{
				if (!initialized)
				{
					foreach (Type T in Types.GetTypesImplementingInterface(typeof(IJwsAlgorithm)))
					{
						try
						{
							Algorithm = (IJwsAlgorithm)Activator.CreateInstance(T);

							if (algorithms.ContainsKey(Algorithm.Name))
								Log.Warning("JWS algorithm with name " + Algorithm.Name + " already registered.");
							else
								algorithms[Algorithm.Name] = Algorithm;
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					if (!registered)
					{
						Types.OnInvalidated += Types_OnInvalidated;
						registered = true;
					}

					initialized = true;
				}

				return algorithms.TryGetValue(Name, out Algorithm);
			}
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			lock (algorithms)
			{
				algorithms.Clear();
				initialized = false;
			}
		}

		private static readonly Dictionary<string, IJwsAlgorithm> algorithms = new Dictionary<string, IJwsAlgorithm>();
		private static bool initialized = false;
		private static bool registered = false;
	}
}
