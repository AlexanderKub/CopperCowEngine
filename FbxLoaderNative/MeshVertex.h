#pragma once

using namespace System::Numerics;

namespace FbxNative {

	public value class MeshVertex
	{
	public:
		/// <summary>
		/// XYZ position
		/// </summary>
		Vector3		Position;

		/// <summary>
		/// Tangential vector. Depends on Texture coordinates from TexCoord0
		/// </summary>
		Vector3		Tangent;

		/// <summary>
		/// Binormal vector. Depends on Texture coordinates from TexCoord0
		/// </summary>
		Vector3		Binormal;

		/// <summary>
		/// Normal vector.
		/// </summary>
		Vector3		Normal;

		/// <summary>
		/// Texture coordinates.
		/// </summary>
		Vector2		TexCoord0;

		/// <summary>
		/// Additional texture coordinates.
		/// </summary>
		Vector2		TexCoord1;

		/// <summary>
		/// Primary vertex color.
		/// </summary>
		Vector4		Color0; // Color

		/// <summary>
		/// Secondary vertex color.
		/// </summary>
		Vector4		Color1; // Color

		/// <summary>
		/// Four component skin vertices.
		/// </summary>
		Vector4		SkinIndices; //Int4

		/// <summary>
		/// Four component skin weights.
		/// </summary>
		Vector4		SkinWeights;
	};

}
