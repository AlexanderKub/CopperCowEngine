#pragma once

using namespace System;
using namespace System::Collections::Generic;

#include "Node.h"
#include "Mesh.h"
#include "MaterialRef.h"

namespace FbxNative {

	public ref class Scene {

		List<Node^>^		nodes = gcnew List<Node^>(0);
		List<Mesh^>^		meshes = gcnew List<Mesh^>(0);
		List<MaterialRef^>^	materials = gcnew List<MaterialRef^>();

		//int firstFrame = 0;
		//int lastFrame = 0;
		//int trackCount = 0;
		//array<Matrix, 2>^ animData = nullptr;

	public:
		/// <summary>
		/// List of scene nodes
		/// </summary>
		property IList<Node^>^ Nodes {
			IList<Node^>^ get() {
				return nodes;
			}
		}


		/// <summary>
		/// List of scene meshes.
		/// </summary>
		property IList<Mesh^>^ Meshes {
			IList<Mesh^>^ get() {
				return meshes;
			}
		}


		property IList<MaterialRef^>^ Materials {
			IList<MaterialRef^>^ get() {
				return materials;
			}
		}

	};

}