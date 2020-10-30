using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SkiaSharp;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// The Phong Shader uses the Phong Reflection model to generate colors.
	/// https://en.wikipedia.org/wiki/Phong_reflection_model
	/// </summary>
	public class PhongShader : I3DShader
	{
		private readonly PhongLightSource[] sources;
		private readonly PhongLightSource source;
		private Vector3 sourcePosition;
		private Vector3 viewerPosition;
		private readonly float sourceDiffuseRed;
		private readonly float sourceDiffuseGreen;
		private readonly float sourceDiffuseBlue;
		private readonly float sourceDiffuseAlpha;
		private readonly float sourceSpecularRed;
		private readonly float sourceSpecularGreen;
		private readonly float sourceSpecularBlue;
		private readonly float sourceSpecularAlpha;
		private readonly float ambientRedFront;
		private readonly float ambientGreenFront;
		private readonly float ambientBlueFront;
		private readonly float ambientAlphaFront;
		private readonly float ambientRedBack;
		private readonly float ambientGreenBack;
		private readonly float ambientBlueBack;
		private readonly float ambientAlphaBack;
		private readonly float ambientReflectionConstantFront;
		private readonly float diffuseReflectionConstantFront;
		private readonly float specularReflectionConstantFront;
		private readonly float shininessFront;
		private readonly float ambientReflectionConstantBack;
		private readonly float diffuseReflectionConstantBack;
		private readonly float specularReflectionConstantBack;
		private readonly float shininessBack;
		private readonly int nrSources;

		/// <summary>
		/// The Phong Shader uses the Phong Reflection model to generate colors.
		/// https://en.wikipedia.org/wiki/Phong_reflection_model
		/// </summary>
		/// <param name="Material">Surface material.</param>
		/// <param name="Ambient">Ambient intensity.</param>
		/// <param name="LightSources">Light sources.</param>
		public PhongShader(PhongMaterial Material, PhongIntensity Ambient,
			params PhongLightSource[] LightSources)
		{
			this.ambientReflectionConstantFront = Material.AmbientReflectionConstantFront;
			this.diffuseReflectionConstantFront = Material.DiffuseReflectionConstantFront;
			this.specularReflectionConstantFront = Material.SpecularReflectionConstantFront;
			this.shininessFront = Material.ShininessFront;
			this.ambientReflectionConstantBack = Material.AmbientReflectionConstantBack;
			this.diffuseReflectionConstantBack = -Material.DiffuseReflectionConstantBack;
			this.specularReflectionConstantBack = Material.SpecularReflectionConstantBack;
			this.shininessBack = Material.ShininessBack;

			this.sources = LightSources;
			this.nrSources = LightSources.Length;
			if (this.nrSources == 1)
			{
				this.source = LightSources[0];
				this.sourcePosition = this.source.TransformedPosition;

				PhongIntensity I = this.source.Diffuse;

				this.sourceDiffuseRed = I.Red;
				this.sourceDiffuseGreen = I.Green;
				this.sourceDiffuseBlue = I.Blue;
				this.sourceDiffuseAlpha = I.Alpha;

				I = this.source.Specular;

				this.sourceSpecularRed = I.Red;
				this.sourceSpecularGreen = I.Green;
				this.sourceSpecularBlue = I.Blue;
				this.sourceSpecularAlpha = I.Alpha;
			}

			this.ambientRedFront = this.ambientReflectionConstantFront * Ambient.Red;
			this.ambientGreenFront = this.ambientReflectionConstantFront * Ambient.Green;
			this.ambientBlueFront = this.ambientReflectionConstantFront * Ambient.Blue;
			this.ambientAlphaFront = this.ambientReflectionConstantFront * Ambient.Alpha;
			this.ambientRedBack = this.ambientReflectionConstantBack * Ambient.Red;
			this.ambientGreenBack = this.ambientReflectionConstantBack * Ambient.Green;
			this.ambientBlueBack = this.ambientReflectionConstantBack * Ambient.Blue;
			this.ambientAlphaBack = this.ambientReflectionConstantBack * Ambient.Alpha;
		}

		/// <summary>
		/// Gets a color for a position.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Z">Z-coordinate.</param>
		/// <param name="Normal">Surface normal vector.</param>
		/// <returns>Color</returns>
		public SKColor GetColor(float X, float Y, float Z, Vector3 Normal)
		{
			Vector3 L;
			Vector3 R;
			Vector3 V;
			Vector3 P = new Vector3(X, Y, Z);
			float d, d2;
			float Red;
			float Green;
			float Blue;
			float Alpha;

			V = Vector3.Normalize(this.viewerPosition - P);

			if (this.nrSources == 1)
			{
				L = Vector3.Normalize(this.sourcePosition - P);
				d = Vector3.Dot(L, Normal);
				R = 2 * d * Normal - L;
				d2 = Math.Abs(Vector3.Dot(R, V));

				if (Vector3.Dot(Normal, V) >= 0)
				{
					d *= this.diffuseReflectionConstantFront;
					d2 = this.specularReflectionConstantFront * (float)Math.Pow(d2, this.shininessFront);

					Red = this.ambientRedFront;
					Green = this.ambientGreenFront;
					Blue = this.ambientBlueFront;
					Alpha = this.ambientAlphaFront;
				}
				else
				{
					d *= this.diffuseReflectionConstantBack;
					d2 = this.specularReflectionConstantBack * (float)Math.Pow(d2, this.shininessBack);

					Red = this.ambientRedBack;
					Green = this.ambientGreenBack;
					Blue = this.ambientBlueBack;
					Alpha = this.ambientAlphaBack;
				}

				Red += d * this.sourceDiffuseRed;
				Green += d * this.sourceDiffuseGreen;
				Blue += d * this.sourceDiffuseBlue;
				Alpha += d * this.sourceDiffuseAlpha;

				Red += d2 * this.sourceSpecularRed;
				Green += d2 * this.sourceSpecularGreen;
				Blue += d2 * this.sourceSpecularBlue;
				Alpha += d2 * this.sourceSpecularAlpha;
			}
			else
			{
				Vector3 P2;
				PhongLightSource Source;
				PhongIntensity I;
				int j;
				bool Front = Vector3.Dot(Normal, V) >= 0;

				if (Front)
				{
					Red = this.ambientRedFront;
					Green = this.ambientGreenFront;
					Blue = this.ambientBlueFront;
					Alpha = this.ambientAlphaFront;
				}
				else
				{
					Red = this.ambientRedBack;
					Green = this.ambientGreenBack;
					Blue = this.ambientBlueBack;
					Alpha = this.ambientAlphaBack;
				}

				for (j = 0; j < this.nrSources; j++)
				{
					Source = this.sources[j];
					P2 = Source.TransformedPosition;

					L = Vector3.Normalize(P2 - P);
					d = Vector3.Dot(L, Normal);
					R = 2 * d * Normal - P2;
					d2 = Math.Abs(Vector3.Dot(R, V));

					if (Front)
					{
						d *= this.diffuseReflectionConstantFront;
						d2 = this.specularReflectionConstantFront * (float)Math.Pow(d2, this.shininessFront);
					}
					else
					{
						d *= this.diffuseReflectionConstantBack;
						d2 = this.specularReflectionConstantBack * (float)Math.Pow(d2, this.shininessBack);
					}

					I = Source.Diffuse;

					Red += d * I.Red;
					Green += d * I.Green;
					Blue += d * I.Blue;
					Alpha += d * I.Alpha;

					I = Source.Specular;

					Red += d2 * I.Red;
					Green += d2 * I.Green;
					Blue += d2 * I.Blue;
					Alpha += d2 * I.Alpha;
				}
			}

			byte R2, G2, B2, A2;
			int Rest = 0;
			int k;

			if (Red < 0)
				R2 = 0;
			else if (Red > 255)
			{
				Rest = (int)(Red - 255 + 0.5f);
				R2 = 255;
			}
			else
				R2 = (byte)(Red + 0.5f);

			if (Green < 0)
				G2 = 0;
			else if (Green > 255)
			{
				Rest += (int)(Green - 255 + 0.5f);
				G2 = 255;
			}
			else
				G2 = (byte)(Green + 0.5f);

			if (Blue < 0)
				B2 = 0;
			else if (Blue > 255)
			{
				Rest += (int)(Blue - 255 + 0.5f);
				B2 = 255;
			}
			else
				B2 = (byte)(Blue + 0.5f);

			if (Alpha < 0)
				A2 = 0;
			else if (Alpha > 255)
				A2 = 255;
			else
				A2 = (byte)(Alpha + 0.5f);

			if (Rest > 0)
			{
				Rest /= 2;

				if (R2 < 255)
				{
					k = R2 + Rest;
					if (k > 255)
						R2 = 255;
					else
						R2 = (byte)k;
				}

				if (G2 < 255)
				{
					k = G2 + Rest;
					if (k > 255)
						G2 = 255;
					else
						G2 = (byte)k;
				}

				if (B2 < 255)
				{
					k = B2 + Rest;
					if (k > 255)
						B2 = 255;
					else
						B2 = (byte)k;
				}
			}

			return new SKColor(R2, G2, B2, A2);
		}

		/// <summary>
		/// Gets an array of colors.
		/// </summary>
		/// <param name="X">X-coordinates.</param>
		/// <param name="Y">Y-coordinates.</param>
		/// <param name="Z">Z-coordinates.</param>
		/// <param name="Normals">Normal vectors.</param>
		/// <param name="N">Number of coordinates.</param>
		/// <param name="Colors">Where color values will be stored.</param>
		public void GetColors(float[] X, float[] Y, float[] Z, Vector3[] Normals, int N, SKColor[] Colors)
		{
			int i;
			Vector3 L;
			Vector3 R;
			Vector3 V;
			Vector3 P = new Vector3();
			Vector3 Normal;
			float d, d2;
			float Red;
			float Green;
			float Blue;
			float Alpha;
			byte R2, G2, B2, A2;
			int Rest;
			int k;

			if (this.nrSources == 1)
			{
				for (i = 0; i < N; i++)
				{
					P.X = X[i];
					P.Y = Y[i];
					P.Z = Z[i];
					L = Vector3.Normalize(this.sourcePosition - P);
					d = Vector3.Dot(L, Normal = Normals[i]);
					R = 2 * d * Normal - L;
					V = Vector3.Normalize(this.viewerPosition - P);
					d2 = Math.Abs(Vector3.Dot(R, V));

					if (Vector3.Dot(Normal, V) >= 0)
					{
						d *= this.diffuseReflectionConstantFront;
						d2 = this.specularReflectionConstantFront * (float)Math.Pow(d2, this.shininessFront);

						Red = this.ambientRedFront;
						Green = this.ambientGreenFront;
						Blue = this.ambientBlueFront;
						Alpha = this.ambientAlphaFront;
					}
					else
					{
						d *= this.diffuseReflectionConstantBack;
						d2 = this.specularReflectionConstantBack * (float)Math.Pow(d2, this.shininessBack);

						Red = this.ambientRedBack;
						Green = this.ambientGreenBack;
						Blue = this.ambientBlueBack;
						Alpha = this.ambientAlphaBack;
					}

					Red += d * this.sourceDiffuseRed;
					Green += d * this.sourceDiffuseGreen;
					Blue += d * this.sourceDiffuseBlue;
					Alpha += d * this.sourceDiffuseAlpha;

					Red += d2 * this.sourceSpecularRed;
					Green += d2 * this.sourceSpecularGreen;
					Blue += d2 * this.sourceSpecularBlue;
					Alpha += d2 * this.sourceSpecularAlpha;

					Rest = 0;

					if (Red < 0)
						R2 = 0;
					else if (Red > 255)
					{
						Rest = (int)(Red - 255 + 0.5f);
						R2 = 255;
					}
					else
						R2 = (byte)(Red + 0.5f);

					if (Green < 0)
						G2 = 0;
					else if (Green > 255)
					{
						Rest += (int)(Green - 255 + 0.5f);
						G2 = 255;
					}
					else
						G2 = (byte)(Green + 0.5f);

					if (Blue < 0)
						B2 = 0;
					else if (Blue > 255)
					{
						Rest += (int)(Blue - 255 + 0.5f);
						B2 = 255;
					}
					else
						B2 = (byte)(Blue + 0.5f);

					if (Alpha < 0)
						A2 = 0;
					else if (Alpha > 255)
						A2 = 255;
					else
						A2 = (byte)(Alpha + 0.5f);

					if (Rest > 0)
					{
						Rest /= 2;

						if (R2 < 255)
						{
							k = R2 + Rest;
							if (k > 255)
								R2 = 255;
							else
								R2 = (byte)k;
						}

						if (G2 < 255)
						{
							k = G2 + Rest;
							if (k > 255)
								G2 = 255;
							else
								G2 = (byte)k;
						}

						if (B2 < 255)
						{
							k = B2 + Rest;
							if (k > 255)
								B2 = 255;
							else
								B2 = (byte)k;
						}
					}

					Colors[i] = new SKColor(R2, B2, G2, A2);
				}
			}
			else
			{
				Vector3 P2;
				PhongLightSource Source;
				PhongIntensity I;
				bool Front;
				float DiffuseC, SpecularC, ShininessC;
				int j;

				for (i = 0; i < N; i++)
				{
					P.X = X[i];
					P.Y = Y[i];
					P.Z = Z[i];
					Normal = Normals[i];
					V = Vector3.Normalize(this.viewerPosition - P);

					if (Front = (Vector3.Dot(Normal, V) >= 0))
					{
						Red = this.ambientRedFront;
						Green = this.ambientGreenFront;
						Blue = this.ambientBlueFront;
						Alpha = this.ambientAlphaFront;
						DiffuseC = this.diffuseReflectionConstantFront;
						SpecularC = this.specularReflectionConstantFront;
						ShininessC = this.shininessFront;
					}
					else
					{
						Red = this.ambientRedBack;
						Green = this.ambientGreenBack;
						Blue = this.ambientBlueBack;
						Alpha = this.ambientAlphaBack;
						DiffuseC = this.diffuseReflectionConstantBack;
						SpecularC = this.specularReflectionConstantBack;
						ShininessC = this.shininessBack;
					}

					for (j = 0; j < this.nrSources; j++)
					{
						Source = this.sources[j];
						P2 = Source.TransformedPosition;

						L = Vector3.Normalize(P2 - P);
						d = Vector3.Dot(L, Normal);
						R = 2 * d * Normal - P2;
						d *= DiffuseC;
						d2 = SpecularC * (float)Math.Pow(Math.Abs(Vector3.Dot(R, V)), ShininessC);

						I = Source.Diffuse;

						Red += d * I.Red;
						Green += d * I.Green;
						Blue += d * I.Blue;
						Alpha += d * I.Alpha;

						I = Source.Specular;

						Red += d2 * I.Red;
						Green += d2 * I.Green;
						Blue += d2 * I.Blue;
						Alpha += d2 * I.Alpha;
					}

					Rest = 0;

					if (Red < 0)
						R2 = 0;
					else if (Red > 255)
					{
						Rest = (int)(Red - 255 + 0.5f);
						R2 = 255;
					}
					else
						R2 = (byte)(Red + 0.5f);

					if (Green < 0)
						G2 = 0;
					else if (Green > 255)
					{
						Rest += (int)(Green - 255 + 0.5f);
						G2 = 255;
					}
					else
						G2 = (byte)(Green + 0.5f);

					if (Blue < 0)
						B2 = 0;
					else if (Blue > 255)
					{
						Rest += (int)(Blue - 255 + 0.5f);
						B2 = 255;
					}
					else
						B2 = (byte)(Blue + 0.5f);

					if (Alpha < 0)
						A2 = 0;
					else if (Alpha > 255)
						A2 = 255;
					else
						A2 = (byte)(Alpha + 0.5f);

					if (Rest > 0)
					{
						Rest /= 2;

						if (R2 < 255)
						{
							k = R2 + Rest;
							if (k > 255)
								R2 = 255;
							else
								R2 = (byte)k;
						}

						if (G2 < 255)
						{
							k = G2 + Rest;
							if (k > 255)
								G2 = 255;
							else
								G2 = (byte)k;
						}

						if (B2 < 255)
						{
							k = B2 + Rest;
							if (k > 255)
								B2 = 255;
							else
								B2 = (byte)k;
						}
					}

					Colors[i] = new SKColor(R2, B2, G2, A2);
				}
			}
		}

		/// <summary>
		/// Transforms any coordinates according to current settings in <paramref name="Canvas"/>.
		/// </summary>
		/// <param name="Canvas">3D Canvas</param>
		public void Transform(Canvas3D Canvas)
		{
			this.viewerPosition = Canvas.ViewerPosition;

			if (this.nrSources == 1)
			{
				this.source.Transform(Canvas);
				this.sourcePosition = this.source.TransformedPosition;
			}
			else
			{
				foreach (PhongLightSource Source in this.sources)
					Source.Transform(Canvas);
			}
		}

	}
}
