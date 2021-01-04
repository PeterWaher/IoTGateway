using System;
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
		private readonly PhongIntensity ambient;
		private Vector3 sourcePosition;
		private readonly float sourceDiffuseRed;
		private readonly float sourceDiffuseGreen;
		private readonly float sourceDiffuseBlue;
		private readonly float sourceDiffuseAlpha;
		private readonly float sourceSpecularRed;
		private readonly float sourceSpecularGreen;
		private readonly float sourceSpecularBlue;
		private readonly float sourceSpecularAlpha;
		private readonly float ambientRed;
		private readonly float ambientGreen;
		private readonly float ambientBlue;
		private readonly float ambientAlpha;
		private readonly float ambientReflectionConstant;
		private readonly float diffuseReflectionConstant;
		private readonly float specularReflectionConstant;
		private readonly float shininess;
		private readonly int nrSources;
		private readonly bool hasSpecularReflectionConstant;
		private readonly bool singleSource;
		private readonly bool opaque;

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
			this.ambient = Ambient;
			this.sources = LightSources;

			this.ambientReflectionConstant = Material.AmbientReflectionConstant;
			this.diffuseReflectionConstant = Material.DiffuseReflectionConstant;
			this.specularReflectionConstant = Material.SpecularReflectionConstant;
			this.hasSpecularReflectionConstant = this.specularReflectionConstant != 0;
			this.shininess = Material.Shininess;

			this.nrSources = LightSources.Length;
			this.singleSource = this.nrSources == 1;

			if (this.singleSource)
			{
				this.source = LightSources[0];
				this.sourcePosition = this.source.Position;

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

			this.ambientRed = this.ambientReflectionConstant * Ambient.Red;
			this.ambientGreen = this.ambientReflectionConstant * Ambient.Green;
			this.ambientBlue = this.ambientReflectionConstant * Ambient.Blue;
			this.ambientAlpha = this.ambientReflectionConstant * Ambient.Alpha;

			if (this.ambientAlpha < 255 ||
				this.sourceDiffuseAlpha < 255 ||
				this.sourceSpecularAlpha < 255)
			{
				this.opaque = false;
			}
			else
			{
				this.opaque = true;
			
				foreach (PhongLightSource Source in this.sources)
				{
					if (!Source.Opaque)
					{
						this.opaque = false;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets a color for a position.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Z">Z-coordinate.</param>
		/// <param name="Normal">Surface normal vector.</param>
		/// <param name="Canvas">Current canvas.</param>
		/// <returns>Color</returns>
		public SKColor GetColor(float X, float Y, float Z, Vector3 Normal, Canvas3D Canvas)
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

			if (this.singleSource)
			{
				L = Vector3.Normalize(this.sourcePosition - P);
				d = Vector3.Dot(L, Normal);

				Red = this.ambientRed;
				Green = this.ambientGreen;
				Blue = this.ambientBlue;
				Alpha = this.ambientAlpha;

				if (d >= 0)
				{
					d *= this.diffuseReflectionConstant;

					if (this.hasSpecularReflectionConstant)
					{
						R = 2 * d * Normal - L;
						V = Vector3.Normalize(Canvas.ViewerPosition - P);
						d2 = Math.Abs(Vector3.Dot(R, V));
						d2 = this.specularReflectionConstant * (float)Math.Pow(d2, this.shininess);

						Red += d2 * this.sourceSpecularRed;
						Green += d2 * this.sourceSpecularGreen;
						Blue += d2 * this.sourceSpecularBlue;
						Alpha += d2 * this.sourceSpecularAlpha;
					}

					Red += d * this.sourceDiffuseRed;
					Green += d * this.sourceDiffuseGreen;
					Blue += d * this.sourceDiffuseBlue;
					Alpha += d * this.sourceDiffuseAlpha;
				}
			}
			else
			{
				PhongLightSource Source;
				PhongIntensity Specular;
				PhongIntensity Diffuse;
				int j;

				Red = Green = Blue = Alpha = 0;

				if (this.hasSpecularReflectionConstant)
					V = Vector3.Normalize(Canvas.ViewerPosition - P);
				else
					V = Vector3.Zero;

				for (j = 0; j < this.nrSources; j++)
				{
					Source = this.sources[j];
					Specular = Source.Specular;
					Diffuse = Source.Diffuse;

					L = Vector3.Normalize(Source.Position - P);
					d = Vector3.Dot(L, Normal);

					Red += this.ambientRed;
					Green += this.ambientGreen;
					Blue += this.ambientBlue;
					Alpha += this.ambientAlpha;

					if (d >= 0)
					{
						d *= this.diffuseReflectionConstant;

						if (this.hasSpecularReflectionConstant)
						{
							R = 2 * d * Normal - L;
							d2 = Math.Abs(Vector3.Dot(R, V));
							d2 = this.specularReflectionConstant * (float)Math.Pow(d2, this.shininess);

							Red += d2 * Specular.Red;
							Green += d2 * Specular.Green;
							Blue += d2 * Specular.Blue;
							Alpha += d2 * Specular.Alpha;
						}

						Red += d * Diffuse.Red;
						Green += d * Diffuse.Green;
						Blue += d * Diffuse.Blue;
						Alpha += d * Diffuse.Alpha;
					}
				}
			}

			byte R2, G2, B2, A2;
			int Rest = 0;
			int k;

			if (Red < 0)
				R2 = 0;
			else if (Red > 255)
			{
				Rest = (int)(Red - 254.5f);
				R2 = 255;
			}
			else
				R2 = (byte)(Red + 0.5f);

			if (Green < 0)
				G2 = 0;
			else if (Green > 255)
			{
				Rest += (int)(Green - 254.5f);
				G2 = 255;
			}
			else
				G2 = (byte)(Green + 0.5f);

			if (Blue < 0)
				B2 = 0;
			else if (Blue > 255)
			{
				Rest += (int)(Blue - 254.5f);
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
		/// <param name="Canvas">Current canvas.</param>
		public void GetColors(float[] X, float[] Y, float[] Z, Vector3[] Normals, int N,
			SKColor[] Colors, Canvas3D Canvas)
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

			if (this.singleSource)
			{
				for (i = 0; i < N; i++)
				{
					P.X = X[i];
					P.Y = Y[i];
					P.Z = Z[i];
					Normal = Normals[i];

					L = Vector3.Normalize(this.sourcePosition - P);
					d = Vector3.Dot(L, Normal);

					Red = this.ambientRed;
					Green = this.ambientGreen;
					Blue = this.ambientBlue;
					Alpha = this.ambientAlpha;

					if (d >= 0)
					{
						d *= this.diffuseReflectionConstant;

						if (this.hasSpecularReflectionConstant)
						{
							R = 2 * d * Normal - L;
							V = Vector3.Normalize(Canvas.ViewerPosition - P);
							d2 = Math.Abs(Vector3.Dot(R, V));
							d2 = this.specularReflectionConstant * (float)Math.Pow(d2, this.shininess);

							Red += d2 * this.sourceSpecularRed;
							Green += d2 * this.sourceSpecularGreen;
							Blue += d2 * this.sourceSpecularBlue;
							Alpha += d2 * this.sourceSpecularAlpha;
						}

						Red += d * this.sourceDiffuseRed;
						Green += d * this.sourceDiffuseGreen;
						Blue += d * this.sourceDiffuseBlue;
						Alpha += d * this.sourceDiffuseAlpha;
					}

					Rest = 0;

					if (Red < 0)
						R2 = 0;
					else if (Red > 255)
					{
						Rest = (int)(Red - 254.5f);
						R2 = 255;
					}
					else
						R2 = (byte)(Red + 0.5f);

					if (Green < 0)
						G2 = 0;
					else if (Green > 255)
					{
						Rest += (int)(Green - 254.5f);
						G2 = 255;
					}
					else
						G2 = (byte)(Green + 0.5f);

					if (Blue < 0)
						B2 = 0;
					else if (Blue > 255)
					{
						Rest += (int)(Blue - 254.5f);
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

					Colors[i] = new SKColor(R2, G2, B2, A2);
				}
			}
			else
			{
				PhongLightSource Source;
				PhongIntensity Specular;
				PhongIntensity Diffuse;
				int j;

				for (i = 0; i < N; i++)
				{
					P.X = X[i];
					P.Y = Y[i];
					P.Z = Z[i];
					Normal = Normals[i];

					Red = Green = Blue = Alpha = 0;

					if (this.hasSpecularReflectionConstant)
						V = Vector3.Normalize(Canvas.ViewerPosition - P);
					else
						V = Vector3.Zero;

					for (j = 0; j < this.nrSources; j++)
					{
						Source = this.sources[j];
						Specular = Source.Specular;
						Diffuse = Source.Diffuse;

						L = Vector3.Normalize(Source.Position - P);
						d = Vector3.Dot(L, Normal);

						Red += this.ambientRed;
						Green += this.ambientGreen;
						Blue += this.ambientBlue;
						Alpha += this.ambientAlpha;

						if (d >= 0)
						{
							d *= this.diffuseReflectionConstant;

							if (this.hasSpecularReflectionConstant)
							{
								R = 2 * d * Normal - L;
								d2 = Math.Abs(Vector3.Dot(R, V));
								d2 = this.specularReflectionConstant * (float)Math.Pow(d2, this.shininess);

								Red += d2 * Specular.Red;
								Green += d2 * Specular.Green;
								Blue += d2 * Specular.Blue;
								Alpha += d2 * Specular.Alpha;
							}

							Red += d * Diffuse.Red;
							Green += d * Diffuse.Green;
							Blue += d * Diffuse.Blue;
							Alpha += d * Diffuse.Alpha;
						}
					}

					Rest = 0;

					if (Red < 0)
						R2 = 0;
					else if (Red > 255)
					{
						Rest = (int)(Red - 254.5f);
						R2 = 255;
					}
					else
						R2 = (byte)(Red + 0.5f);

					if (Green < 0)
						G2 = 0;
					else if (Green > 255)
					{
						Rest += (int)(Green - 254.5f);
						G2 = 255;
					}
					else
						G2 = (byte)(Green + 0.5f);

					if (Blue < 0)
						B2 = 0;
					else if (Blue > 255)
					{
						Rest += (int)(Blue - 254.5f);
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

					Colors[i] = new SKColor(R2, G2, B2, A2);
				}
			}
		}

		/// <summary>
		/// If shader is 100% opaque.
		/// </summary>
		public bool Opaque => this.opaque;

		/// <summary>
		/// Exports shader specifics to script.
		/// </summary>
		/// <returns>Exports the shader to parsable script.</returns>
		public string ToScript()
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append("PhongShader(PhongMaterial(");
			sb.Append(Expression.ToString(this.ambientReflectionConstant));
			sb.Append(',');
			sb.Append(Expression.ToString(this.diffuseReflectionConstant));
			sb.Append(',');
			sb.Append(Expression.ToString(this.specularReflectionConstant));
			sb.Append(',');
			sb.Append(Expression.ToString(this.shininess));
			sb.Append("),");
			sb.Append(Canvas3D.ToString(this.ambient));
			sb.Append(",[");

			foreach (PhongLightSource Source in this.sources)
			{
				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append("PhongLightSource(");
				sb.Append(Canvas3D.ToString(Source.Diffuse));
				sb.Append(',');
				sb.Append(Canvas3D.ToString(Source.Specular));
				sb.Append(',');
				sb.Append(Canvas3D.ToString(Source.Position));
				sb.Append(')');
			}

			sb.Append("])");

			return sb.ToString();
		}

	}
}
