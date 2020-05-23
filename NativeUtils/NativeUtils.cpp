#include "NativeUtils.h"
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"

using namespace Runtime::InteropServices;

NativeUtilsNamespace::NativeUtils::NativeUtils()
{
	Console::WriteLine("NativeUtils Loaded");
}

NativeUtilsNamespace::ImageData^ NativeUtilsNamespace::NativeUtils::load_hdr_image(String^ file_path)
{
	const auto file_name = static_cast<const char*>(Marshal::StringToHGlobalAnsi(file_path).ToPointer());

	stbi_set_flip_vertically_on_load(false);
	int width, height, nr_components;
	const auto data = stbi_loadf(file_name, &width, &height, &nr_components, 0);

	if (data)
	{
		const auto n = width * height * nr_components;
		auto result = gcnew ImageData(width, height, nr_components);
		for (auto i = 0; i < n; i++)
		{
			result->Data->Add(data[i]);
		}
		stbi_image_free(data);
		return result;
	}
	else
	{
		Console::WriteLine("Failed to load HDR image.");
	}
	return nullptr;
}

