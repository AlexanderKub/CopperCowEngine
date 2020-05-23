#pragma once

using namespace System;
using namespace Collections::Generic;

namespace NativeUtilsNamespace {

	public ref class ImageData sealed {

		List<float>^ data_;
		int width_ = 0;
		int height_ = 0;
		int components_ = 0;

	public:
		/// <summary>
		/// Data floats.
		/// </summary>
		property List<float>^ Data
		{
			List<float>^ get()
			{
				return data_;
			}
		}

		/// <summary>
		/// Image width.
		/// </summary>
		property int Width
		{
			int get()
			{
				return width_;
			}
		}

		/// <summary>
		/// Image height.
		/// </summary>
		property int Height
		{
			int get()
			{
				return height_;
			}
		}
		
		/// <summary>
		/// Image channels count.
		/// </summary>
		property int ChannelsCount
		{
			int get()
			{
				return components_;
			}
		}

		/// <summary>
		/// Creates instance of the image data.
		/// </summary>
		ImageData(const int w, const int h, const int channels_count)
		{
			data_ = gcnew List<float>(0);
			width_ = w;
			height_ = h;
			components_ = channels_count;
		}
	};

}