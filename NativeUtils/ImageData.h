#pragma once

using namespace System;
using namespace System::Collections::Generic;

namespace NativeUtilsNS {

	public ref class ImageData {

		List<float>^ data;
		int width = 0;
		int height = 0;

	public:
		/// <summary>
		/// Data floats.
		/// </summary>
		property List<float>^ Data {
			List<float>^ get() {
				return data;
			}
		}

		/// <summary>
		/// Image width.
		/// </summary>
		property int Width {
			int get() {
				return width;
			}
		}

		/// <summary>
		/// Image height.
		/// </summary>
		property int Height {
			int get() {
				return height;
			}
		}

		/// <summary>
		/// Creates instance of the image data.
		/// </summary>
		ImageData(int w, int h) {
			data = gcnew List<float>(0);
			width = w;
			height = h;
		}
	};

}