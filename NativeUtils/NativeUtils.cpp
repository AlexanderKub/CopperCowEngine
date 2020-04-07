#include "NativeUtils.h"
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"

using namespace Runtime::InteropServices;

NativeUtilsNS::NativeUtils::NativeUtils()
{
	Console::WriteLine("NativeUtils {0}", 1);
}

NativeUtilsNS::ImageData^ NativeUtilsNS::NativeUtils::LoadHDRImage(String^ filePath) {
	const char* fileName = (const char*)(Marshal::StringToHGlobalAnsi(filePath)).ToPointer();

	stbi_set_flip_vertically_on_load(false);
	int width, height, nrComponents;
	float *data = stbi_loadf(fileName, &width, &height, &nrComponents, 0);
	unsigned int hdrTexture;
	if (data)
	{
		int n = width * height * nrComponents;
		ImageData^ result = gcnew ImageData(width, height);
		for (size_t i = 0; i < n; i++)
		{
			result->Data->Add(data[i]);
		}
		return result;
		stbi_image_free(data);
	}
	else
	{
		Console::WriteLine("Failed to load HDR image.");
	}
	return nullptr;
}

