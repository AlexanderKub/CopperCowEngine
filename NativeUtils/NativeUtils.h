#pragma once
#include "ImageData.h"

using namespace System;
using namespace System::Collections::Generic;

namespace NativeUtilsNS {

	public ref class NativeUtils
	{
	public:
		NativeUtils();

		ImageData^ LoadHDRImage(String^ filePath);
	};
}
