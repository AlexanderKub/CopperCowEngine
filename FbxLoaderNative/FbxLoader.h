#pragma once


//#define FBXSDK_NEW_API
#include <fbxsdk.h>
#include "Scene.h"

using namespace System;

namespace FbxNative {

	public ref class FbxLoader
	{
		FbxManager				*fbxManager;
		FbxImporter				*fbxImporter;
		FbxScene				*fbxScene;
		FbxGeometryConverter	*fbxGConv;
		FbxTime::EMode			timeMode;

		void IterateChildren(FbxNode *fbxNode, FbxScene *fbxScene, Scene ^scene, int parentIndex, int depth);
		void HandleMesh(Scene ^scene, Node ^node, FbxNode *fbxNode);
		void HandleSkinning(Mesh ^nodeMesh, Scene ^scene, Node ^node, FbxNode *fbxNode, Matrix4x4^ meshTransform, array<Vector4> ^skinIndices, array<Vector4>	^skinWeights);
		void HandleCamera(Scene ^scene, Node ^node, FbxNode *fbxNode);
		void HandleLight(Scene ^scene, Node ^node, FbxNode *fbxNode);
		void HandleMaterial(MeshSubset ^sg, FbxSurfaceMaterial *material);
		void GetNormalForVertex(MeshVertex *vertex, FbxMesh *fbxMesh, int vertexIdCount, int ctrlPointId);
		void GetTextureForVertex(MeshVertex *vertex, FbxMesh *fbxMesh, int vertexIdCount, int vertexId);
		void GetColorForVertex(MeshVertex *vertex, FbxMesh *fbxMesh, int vertexIdCount, int vertexId);

		void GetCustomProperties(Node ^node, FbxNode *fbxNode);


		bool ImportAnimation = false;
		bool ImportGeometry = true;

	public:
		FbxLoader();


		Scene^ LoadScene(String^ filePath);

	};

}