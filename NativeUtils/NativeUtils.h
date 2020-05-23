#pragma once
#include "ImageData.h"

using namespace System;
using namespace Collections::Generic;

namespace NativeUtilsNamespace {

	public ref class NativeUtils sealed
	{
	public:
		NativeUtils();

		ImageData^ load_hdr_image(String^ file_path);
	};
}
