#pragma once

using namespace System;
using namespace System::Numerics;

namespace FbxNative {

	public ref class Node
	{
	public:
		property String^ Name;

		/// <summary>
		/// Parent index in scene. Zero value means root node.
		/// </summary>
		property int ParentIndex;

		/// <summary>
		/// Scene mesh index. Negative value means no mesh reference.
		/// </summary>
		property int MeshIndex;

		/// <summary>
		/// Scene animation track index.
		/// Negative value means no animation track.
		/// </summary>
		property int TrackIndex;

		/// <summary>
		/// Node transform
		/// </summary>
		property Matrix4x4 Transform;

		/// <summary>
		/// Global matrix of "bind-posed" node.
		/// For nodes that do not affect skinning this value is always Matrix.Identity.
		/// </summary>
		property Matrix4x4 BindPose;

		/// <summary>
		/// Tag object. This value will not be serialized.
		/// </summary>
		property Object^ Tag;


		/// <summary>
		/// Creates instance of the node.
		/// </summary>
		Node() {
			MeshIndex = -1;
			ParentIndex = -1;
			TrackIndex = -1;
		}
	};

}
