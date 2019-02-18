#pragma once

using namespace SharpDX;
using namespace SharpDX::Mathematics;


namespace FbxNative {

	ref class Mesh;


	public value class MeshTriangle
	{
	public:

		int		Index0;
		int		Index1;
		int		Index2;
		int		MaterialIndex;

		MeshTriangle(int i0, int i1, int i2, int mtrlId) {
			Index0 = i0;
			Index1 = i1;
			Index2 = i2;
			MaterialIndex = mtrlId;
		}

		bool IsDegenerate();

		Vector3 ComputeNormal(Mesh^ mesh);
		Vector3 Centroid(Mesh mesh);
	};

}
