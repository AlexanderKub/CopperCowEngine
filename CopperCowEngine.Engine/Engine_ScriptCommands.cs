namespace CopperCowEngine.Engine
{
    public partial class Engine
    {
        private void CreateScriptCommands()
        {
            ScriptEngine.RegisterFunction("quit", this, "QuitCommand");
            ScriptEngine.RegisterFunction("bloom", this, "SwitchBloom");
            ScriptEngine.RegisterFunction("hdr", this, "SwitchHdr");
            ScriptEngine.RegisterFunction("msaa", this, "SwitchMsaa");
        }

        private void QuitCommand()
        {
            Quit();
        }
        
        private void SwitchBloom()
        {
            var config = Configuration;
            var renderingConfig = config.Rendering.Configuration;
            renderingConfig.PostProcessing.Bloom.Enable = !renderingConfig.PostProcessing.Bloom.Enable;

            config.Rendering.Configuration = renderingConfig;
            SwitchConfiguration(config);
        }

        private void SwitchHdr()
        {
            var config = Configuration;
            var renderingConfig = config.Rendering.Configuration;
            renderingConfig.EnableHdr = !config.Rendering.Configuration.EnableHdr;

            config.Rendering.Configuration = renderingConfig;
            SwitchConfiguration(config);
        }

        private void SwitchMsaa(int enable)
        {
            var config = Configuration;
            var renderingConfig = config.Rendering.Configuration;
            renderingConfig.EnableMsaa = enable <= 0 ? Rendering.MsaaEnabled.Off : (enable == 4 ? Rendering.MsaaEnabled.X4 : Rendering.MsaaEnabled.X8);

            config.Rendering.Configuration = renderingConfig;
            SwitchConfiguration(config);
        }

        private void SwitchConfiguration(EngineConfiguration configuration)
        {
            Configuration = configuration;
            _renderBackend.SwitchConfiguration(Configuration.Rendering.Configuration);
        }
    }
}
