#pragma once

using namespace System;


namespace FbxNative {

	public ref class MaterialRef
	{
	public:
		String ^ Name;

		/// <summary>
		/// Base texture path.
		/// </summary>
		String^	Texture;
	};

}
