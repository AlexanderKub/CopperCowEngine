using System;

namespace CopperCowEngine.Rendering
{
    public struct MotionBlurSettings
    {
        public bool Enable;

        public MotionBlurSettings(bool enable)
        {
            Enable = enable;
        }
    }

    public struct BloomSettings
    {
        public bool Enable;

        public BloomSettings(bool enable)
        {
            Enable = enable;
        }
    }

    public struct DofBlurSettings
    {
        public bool Enable;

        public DofBlurSettings(bool enable)
        {
            Enable = enable;
        }
    }

    public struct PostProcessingConfiguration
    {
        public MotionBlurSettings MotionBlur;

        public BloomSettings Bloom;

        public DofBlurSettings DofBlur;

        public static PostProcessingConfiguration Disabled = new PostProcessingConfiguration();
    }
}