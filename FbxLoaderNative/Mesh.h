#pragma once
#include "MeshVertex.h"
#include "MeshTriangle.h"
#include "MeshSubset.h"

using namespace System;
using namespace System::Collections::Generic;

namespace FbxNative {

	public ref class Mesh
	{
		List<MeshVertex>^	vertices;
		List<MeshTriangle>^	triangles;
		List<MeshSubset>^	subsets;

		bool isSkinned;

	public:
		property List<MeshVertex>^		Vertices { List<MeshVertex>^		get() { return vertices; };		private: void set(List<MeshVertex>^ val) { vertices = val; }; }
		property List<MeshTriangle>^	Triangles { List<MeshTriangle>^	get() { return triangles; };	private: void set(List<MeshTriangle>^ val) { triangles = val; }; }
		property List<MeshSubset>^		Subsets { List<MeshSubset>^		get() { return subsets; };		private: void set(List<MeshSubset>^ val) { subsets = val; }; }
		property int					TriangleCount { int get() { return Triangles->Count; } }
		property int					VertexCount { int get() { return Vertices->Count; } }
		property int					IndexCount { int get() { return TriangleCount * 3; } }

		property bool				IsSkinned { bool get() { return isSkinned; }; private: void set(bool val) { isSkinned = val; }; }


		/// <summary>
		/// Mesh constructor
		/// </summary>
		Mesh()
		{
			Vertices = gcnew List<MeshVertex>(0);
			Triangles = gcnew List<MeshTriangle>(0);
			Subsets = gcnew List<MeshSubset>(0);
		}

	};

}
