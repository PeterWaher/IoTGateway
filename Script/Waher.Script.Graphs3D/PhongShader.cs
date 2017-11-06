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
		private PhongLightSource[] sources;
		private PhongLightSource source;
		private Vector3 sourcePosition;
		private float sourceDiffuseRed;
		private float sourceDiffuseGreen;
		private float sourceDiffuseBlue;
		private float sourceSpecularRed;
		private float sourceSpecularGreen;
		private float sourceSpecularBlue;
		private float ambientReflectionConstant;
		private float diffuseReflectionConstant;
		private float specularReflectionConstant;
		private float shininess;
		private float ambientRed;
		private float ambientGreen;
		private float ambientBlue;
		private int nrSources;

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
			this.ambientReflectionConstant = Material.AmbientReflectionConstant;
			this.diffuseReflectionConstant = Material.DiffuseReflectionConstant;
			this.specularReflectionConstant = Material.SpecularReflectionConstant;
			this.shininess = Material.Shininess;

			this.sources = LightSources;
			this.nrSources = LightSources.Length;
			if (this.nrSources == 1)
			{
				this.source = LightSources[0];
				this.sourcePosition = this.source.Position;

				PhongIntensity I = this.source.Diffuse;

				this.sourceDiffuseRed = I.Red;
				this.sourceDiffuseGreen = I.Green;
				this.sourceDiffuseBlue = I.Blue;

				I = this.source.Specular;

				this.sourceSpecularRed = I.Red;
				this.sourceSpecularGreen = I.Green;
				this.sourceSpecularBlue = I.Blue;
			}

			this.ambientRed = this.ambientReflectionConstant * Ambient.Red;
			this.ambientGreen = this.ambientReflectionConstant * Ambient.Green;
			this.ambientBlue = this.ambientReflectionConstant * Ambient.Blue;
		}

		private static readonly Vector3 V = new Vector3(0, 0, -1);  // Viewer

		public SKColor GetColor(float X, float Y, float Z, Vector3 Normal)
		{
			Vector3 L;
			Vector3 R;
			float d;
			float Red = this.ambientRed;
			float Green = this.ambientGreen;
			float Blue = this.ambientBlue;

			if (this.nrSources == 1)
			{
				L = Vector3.Normalize(this.sourcePosition - new Vector3(X, Y, Z));
				d = Vector3.Dot(L, Normal);
				R = 2 * d * Normal - this.sourcePosition;
				d *= this.diffuseReflectionConstant;

				Red += d * this.sourceDiffuseRed;
				Green += d * this.sourceDiffuseGreen;
				Blue += d * this.sourceDiffuseBlue;

				d = this.specularReflectionConstant * (float)Math.Pow(Vector3.Dot(R, V), this.shininess);

				Red += d * this.sourceSpecularRed;
				Green += d * this.sourceSpecularGreen;
				Blue += d * this.sourceSpecularBlue;
			}
			else
			{
				Vector3 P, P2;
				PhongLightSource Source;
				PhongIntensity I;
				int j;

				P = new Vector3(X, Y, Z);

				for (j = 0; j < this.nrSources; j++)
				{
					Source = this.sources[j];
					P2 = Source.Position;

					L = Vector3.Normalize(P2 - P);
					d = Vector3.Dot(L, Normal);
					R = 2 * d * Normal - P2;
					d *= this.diffuseReflectionConstant;

					I = Source.Diffuse;

					Red += d * I.Red;
					Green += d * I.Green;
					Blue += d * I.Blue;

					d = this.specularReflectionConstant * (float)Math.Pow(Vector3.Dot(R, V), this.shininess);

					I = Source.Specular;

					Red += d * I.Red;
					Green += d * I.Green;
					Blue += d * I.Blue;
				}
			}

			return new SKColor(
				Red > 255 ? (byte)255 : (byte)(Red + 0.5f),
				Green > 255 ? (byte)255 : (byte)(Green + 0.5f),
				Blue > 255 ? (byte)255 : (byte)(Blue + 0.5f));
		}

		public void GetColors(float[] X, float[] Y, float[] Z, Vector3[] Normals, int N, SKColor[] Colors)
		{
			int i;
			Vector3 L;
			Vector3 R;
			Vector3 Normal;
			float d;
			float Red = this.ambientRed;
			float Green = this.ambientGreen;
			float Blue = this.ambientBlue;

			for (i = 0; i < N; i++)
			{
				Red = this.ambientRed;
				Green = this.ambientGreen;
				Blue = this.ambientBlue;

				if (this.nrSources == 1)
				{
					L = Vector3.Normalize(this.sourcePosition - new Vector3(X[i], Y[i], Z[i]));
					d = Vector3.Dot(L, Normal = Normals[i]);
					R = 2 * d * Normal - this.sourcePosition;
					d *= this.diffuseReflectionConstant;

					Red += d * this.sourceDiffuseRed;
					Green += d * this.sourceDiffuseGreen;
					Blue += d * this.sourceDiffuseBlue;

					d = this.specularReflectionConstant * (float)Math.Pow(Vector3.Dot(R, V), this.shininess);

					Red += d * this.sourceSpecularRed;
					Green += d * this.sourceSpecularGreen;
					Blue += d * this.sourceSpecularBlue;
				}
				else
				{
					Vector3 P, P2;
					PhongLightSource Source;
					PhongIntensity I;
					int j;

					P = new Vector3(X[i], Y[i], Z[i]);

					for (j = 0; j < this.nrSources; j++)
					{
						Source = this.sources[j];
						P2 = Source.Position;

						L = Vector3.Normalize(P2 - P);
						d = Vector3.Dot(L, Normal = Normals[i]);
						R = 2 * d * Normal - P2;
						d *= this.diffuseReflectionConstant;

						I = Source.Diffuse;

						Red += d * I.Red;
						Green += d * I.Green;
						Blue += d * I.Blue;

						d = this.specularReflectionConstant * (float)Math.Pow(Vector3.Dot(R, V), this.shininess);

						I = Source.Specular;

						Red += d * I.Red;
						Green += d * I.Green;
						Blue += d * I.Blue;
					}
				}

				Colors[i] = new SKColor(
					Red > 255 ? (byte)255 : (byte)(Red + 0.5f),
					Green > 255 ? (byte)255 : (byte)(Green + 0.5f),
					Blue > 255 ? (byte)255 : (byte)(Blue + 0.5f));
			}
		}

	}
}
