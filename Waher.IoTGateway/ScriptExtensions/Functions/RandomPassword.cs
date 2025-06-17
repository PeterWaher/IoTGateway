using System;
using Waher.Content;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Functions.Vectors;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Statistics.Functions;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Creates a random password.
	/// </summary>
	public class RandomPassword : FunctionZeroVariables
	{
		/// <summary>
		/// Creates a random password.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public RandomPassword(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(RandomPassword);

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => false;

		/// <summary>
		/// Creates a random password.
		/// </summary>
		/// <param name="NrBytes">Number of random bytes.</param>
		/// <param name="NrBuckets">Number of buckets to use to remove passwords that are not sufficiently distributed
		/// along available bytes. If zero, no passwords are removed.</param>
		/// <returns>Random password</returns>
		public static string CreateRandomPassword(int NrBytes, int NrBuckets)
		{
			if (NrBytes < 16)
				throw new ArgumentOutOfRangeException(nameof(NrBytes), "Must be at least 16.");

			if (NrBuckets < 0)
				throw new ArgumentOutOfRangeException(nameof(NrBuckets), "Must be non-negative.");

			byte[] Proposal;
			double[] H;
			double MinH;

			do
			{
				Proposal = Gateway.NextBytes(NrBytes);

				if (NrBuckets > 1)
				{
					H = Histogram.Compute(Proposal, NrBuckets, 0, 256);
					MinH = Min.CalcMin(H, null);
				}
				else MinH = 1;
			}
			while (MinH == 0);

			return Base64Url.Encode(Proposal);
		}

		/// <summary>
		/// Creates a random password having approximately 255 bits of entropy.
		/// </summary>
		/// <returns>Random password</returns>
		public static string CreateRandomPassword()
		{
			return CreateRandomPassword(32, 12);

			/////////////////////////////////////////////////////////////////////
			//
			// This condition approximately removes 1 in 2 generated passwords, 
			// if NrBytes=32, NrBuckets=12, reducing the strength from 256 bits to 255 bits.
			//
			// In a test of 100'000 randomly generated passwords, 42'973 passed
			// (~43%), 57027 failed (~57%).
			//
			// Sample script:
			//
			//     NrPass:=0;
			//     NrFail:=0;
			//     foreach x in 1..100000 do
			//     (
			//         Bin:=Base64Decode(Base64Encode(Gateway.NextBytes(32)));
			//         H:=Histogram([foreach x in Bin : x],0,256,12);
			//         if (Min(H[1])>0) then NrPass++ else NrFail++
			//     );
			//     {"NrPass":NrPass,"NrFail":NrFail}
			//
			/////////////////////////////////////////////////////////////////////
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return new StringValue(CreateRandomPassword(32, 12));
		}
	}
}
