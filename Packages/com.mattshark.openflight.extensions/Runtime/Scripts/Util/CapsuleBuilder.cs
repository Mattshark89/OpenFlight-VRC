// ref: https://behreajj.medium.com/making-a-capsule-mesh-via-script-in-five-3d-environments-c2214abf02db
using UnityEngine;

namespace PrimitiveGenerator
{
	public class CapsuleBuilder
	{
		public static Mesh Build(float height, float radius, int latitudes, int longitudes, int rings)
		{
			latitudes = latitudes % 2 != 0 ? latitudes + 1 : latitudes;

			int halfLats = latitudes / 2;

			// vertext index offsets
			int vOffsetNorthHemi = longitudes;
			int vOffsetNorthEquator = vOffsetNorthHemi + (longitudes + 1) * (halfLats - 1);
			int vOffsetCylinder = vOffsetNorthEquator + (longitudes + 1);
			int vOffsetSouthEquator = (rings > 0) ? vOffsetCylinder + (longitudes + 1) * rings : vOffsetCylinder;
			int vOffsetSouthHemi = vOffsetSouthEquator + (longitudes + 1);
			int vOffsetSouthPolar = vOffsetSouthHemi + (longitudes + 1) * (halfLats - 2);
			int vOffsetSouthCap = vOffsetSouthPolar + (longitudes + 1);

			int vCount = vOffsetSouthCap + longitudes;
			var vertices = new Vector3[vCount];
			var uvs = new Vector2[vCount];
			var normals = new Vector3[vCount];

			float toTheta = 2f * Mathf.PI / longitudes;
			float toPhi = Mathf.PI / latitudes;
			float toTexHorizontal = 1f / longitudes;
			float toTexVertical = 1f / halfLats;

			float vtAspectRatio = 1f / 3f;
			float vtAspectNorth = 1f - vtAspectRatio;
			float vtAspectSouth = vtAspectRatio;

			var thetaCartesian = new Vector2[longitudes];
			var rhoThetaCartesian = new Vector2[longitudes];
			var sTexCache = new float[longitudes + 1];

			// Polar vertices
			for (int lon = 0; lon < longitudes; ++lon)
			{
				float lf = lon;
				float sTexPolar = 1f - ((lf + .5f) * toTexHorizontal);
				float cosTheta = Mathf.Cos(lf * toTheta);
				float sinTheta = Mathf.Sin(lf * toTheta);

				thetaCartesian[lon] = new Vector2(cosTheta, sinTheta);
				rhoThetaCartesian[lon] = radius * new Vector2(cosTheta, sinTheta);

				// North
				vertices[lon] = new Vector3(0f, height / 2f + radius, 0f);
				uvs[lon] = new Vector2(sTexPolar, 1f);
				normals[lon] = new Vector3(0f, 1f, 0f);

				// South
				int i = vOffsetSouthCap + lon;
				vertices[i] = new Vector3(0f, -(height / 2f + radius), 0f);
				uvs[i] = new Vector2(sTexPolar, 0f);
				normals[i] = new Vector3(0f, -1f, 0f);
			}

			// Equatorial vertices
			for (int lon = 0; lon <= longitudes; ++lon)
			{
				float sTex = 1f - lon * toTexHorizontal;
				sTexCache[lon] = sTex;

				int ll = lon % longitudes;
				Vector2 tc = thetaCartesian[ll];
				Vector2 rtc = rhoThetaCartesian[ll];

				// North equator
				int idxn = vOffsetNorthEquator + lon;
				vertices[idxn] = new Vector3(rtc.x, height / 2f, -rtc.y);
				uvs[idxn] = new Vector2(sTex, vtAspectNorth);
				normals[idxn] = new Vector3(tc.x, 0f, -tc.y);

				// South equator
				int idxs = vOffsetSouthEquator + lon;
				vertices[idxs] = new Vector3(rtc.x, -height / 2f, -rtc.y);
				uvs[idxs] = new Vector2(sTex, vtAspectSouth);
				normals[idxs] = new Vector3(tc.x, 0f, -tc.y);
			}

			// Hemisphere vertices
			for (int lat = 0; lat < halfLats - 1; ++lat)
			{
				float phi = (lat + 1f) * toPhi;
				float cosPhiSouth = Mathf.Cos(phi);
				float sinPhiSouth = Mathf.Sin(phi);
				float cosPhiNorth = sinPhiSouth;
				float sinPhiNorth = -cosPhiSouth;

				float rhoCosPhiNorth = radius * cosPhiNorth;
				float rhoSinPhiNorth = radius * sinPhiNorth;
				float zOffsetNorth = height / 2f - rhoSinPhiNorth;
				float rhoCosPhiSouth = radius * cosPhiSouth;
				float rhoSinPhiSouth = radius * sinPhiSouth;
				float zOffsetSouth = -height / 2f - rhoSinPhiSouth;

				float tTexFac = (lat + 1f) * toTexVertical;
				float cmplTexFac = 1f - tTexFac;
				float tTexNorth = cmplTexFac + vtAspectNorth * tTexFac;
				float tTexSouth = cmplTexFac * vtAspectSouth;

				int vCurrentLatNorth = vOffsetNorthHemi + (lat * (longitudes + 1));
				int vCurrentLatSouth = vOffsetSouthHemi + (lat * (longitudes + 1));

				for (int lon = 0; lon <= longitudes; ++lon)
				{
					float sTex = sTexCache[lon];
					Vector2 tc = thetaCartesian[lon % longitudes];

					// North hemisphere
					int idxn = vCurrentLatNorth + lon;
					vertices[idxn] = new Vector3(rhoCosPhiNorth * tc.x, zOffsetNorth, -rhoCosPhiNorth * tc.y);
					uvs[idxn] = new Vector2(sTex, tTexNorth);
					normals[idxn] = new Vector3(cosPhiNorth * tc.x, -sinPhiNorth, -cosPhiNorth * tc.y);

					// South hemisphere
					int idxs = vCurrentLatSouth + lon;
					vertices[idxs] = new Vector3(rhoCosPhiSouth * tc.x, zOffsetSouth, -rhoCosPhiSouth * tc.y);
					uvs[idxs] = new Vector2(sTex, tTexSouth);
					normals[idxs] = new Vector3(cosPhiSouth * tc.x, -sinPhiSouth, -cosPhiSouth * tc.y);
				}
			}

			// Cylinder vertices
			if (rings > 0)
			{
				float toFac = 1f / (rings + 1);
				int index = vOffsetCylinder;

				for (int h = 1; h <= rings; ++h)
				{
					float fac = h * toFac;
					float cmplFac = 1f - fac;
					float tTex = cmplFac * vtAspectNorth + fac * vtAspectSouth;
					float z = (height / 2f) - height * fac;

					for (int lon = 0; lon <= longitudes; ++lon)
					{
						var tc = thetaCartesian[lon % longitudes];
						var rtc = rhoThetaCartesian[lon % longitudes];
						float sTex = sTexCache[lon];

						vertices[index] = new Vector3(rtc.x, z, -rtc.y);
						uvs[index] = new Vector2(sTex, tTex);
						normals[index] = new Vector3(tc.x, 0f, -tc.y);
						++index;
					}
				}
			}

			// Triangle indices
			int long3 = longitudes * 3;
			int long6 = longitudes * 6;
			int hemiLong = (halfLats - 1) * long6;

			int tOffsetNorthHemi = long3;
			int tOffsetCylinder = tOffsetNorthHemi + hemiLong;
			int tOffsetSouthHemi = tOffsetCylinder + (rings + 1) * long6;
			int tOffsetSouthCap = tOffsetSouthHemi + hemiLong;

			int triCount = tOffsetSouthCap + long3;
			var triangles = new int[triCount];

			// Polar caps
			for (int i = 0, k = 0, m = tOffsetSouthCap; i < longitudes; ++i, k += 3, m += 3)
			{
				// North
				triangles[k] = i;
				triangles[k + 1] = vOffsetNorthHemi + i;
				triangles[k + 2] = vOffsetNorthHemi + i + 1;
				// South
				triangles[m] = vOffsetSouthCap + i;
				triangles[m + 1] = vOffsetSouthPolar + i + 1;
				triangles[m + 2] = vOffsetSouthPolar + i;
			}

			// Hemispheres
			for (int i = 0, k = tOffsetNorthHemi, m = tOffsetSouthHemi; i < (halfLats - 1); ++i)
			{
				int vCurrentLatNorth = vOffsetNorthHemi + (i * (longitudes + 1));
				int vNextLatNorth = vCurrentLatNorth + (longitudes + 1);
				int vCurrentLatSouth = vOffsetSouthEquator + (i * (longitudes + 1));
				int vNextLatSouth = vCurrentLatSouth + (longitudes + 1);

				for (int j = 0; j < longitudes; ++j, k += 6, m += 6)
				{
					// North
					int n00 = vCurrentLatNorth + j;
					int n01 = vNextLatNorth + j;
					int n11 = vNextLatNorth + j + 1;
					int n10 = vCurrentLatNorth + j + 1;

					triangles[k] = n00;
					triangles[k + 1] = n11;
					triangles[k + 2] = n10;

					triangles[k + 3] = n00;
					triangles[k + 4] = n01;
					triangles[k + 5] = n11;

					// South
					int s00 = vCurrentLatSouth + j;
					int s01 = vNextLatSouth + j;
					int s11 = vNextLatSouth + j + 1;
					int s10 = vCurrentLatSouth + j + 1;

					triangles[m] = s00;
					triangles[m + 1] = s11;
					triangles[m + 2] = s10;

					triangles[m + 3] = s00;
					triangles[m + 4] = s01;
					triangles[m + 5] = s11;
				}
			}

			// Cylinder
			for (int i = 0, k = tOffsetCylinder; i <= rings; ++i)
			{
				int vCurrentLat = vOffsetNorthEquator + i * (longitudes + 1);
				int vNextLat = vCurrentLat + (longitudes + 1);

				for (int j = 0; j < longitudes; ++j, k += 6)
				{
					int cy00 = vCurrentLat + j;
					int cy01 = vNextLat + j;
					int cy11 = vNextLat + j + 1;
					int cy10 = vCurrentLat + j + 1;

					triangles[k] = cy00;
					triangles[k + 1] = cy11;
					triangles[k + 2] = cy10;
					triangles[k + 3] = cy00;
					triangles[k + 4] = cy01;
					triangles[k + 5] = cy11;
				}
			}

			Mesh mesh = new Mesh();
			mesh.name = "Capsule";
			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.normals = normals;
			mesh.triangles = triangles;

			mesh.Optimize();
			mesh.RecalculateTangents();

			return mesh;
		}
	}
}
