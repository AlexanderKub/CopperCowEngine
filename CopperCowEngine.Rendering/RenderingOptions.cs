using System;
using System.Runtime.CompilerServices;
using CopperCowEngine.Core;

[assembly:InternalsVisibleTo("CopperCowEngine.Engine")]

namespace CopperCowEngine.Rendering
{
    public abstract class RenderOptions
    {
        public RenderingConfiguration Configuration { get; internal set; }
        internal abstract IRenderBackend Create(IEngineLoopProvider loopProvider, IScriptEngine scriptEngine);
    }

    public class RenderingOptions<T> : RenderOptions where T : IRenderBackend
    {
        private readonly object[] _parameters;

        public RenderingOptions(RenderingConfiguration configuration, params object[] parameters)
        {
            Configuration = configuration;
            _parameters = parameters;
        }

        internal override IRenderBackend Create(IEngineLoopProvider loopProvider, IScriptEngine scriptEngine)
        {
            var backend = Activator.CreateInstance<T>();
            ValidateConfiguration(Configuration);
            backend.Initialize(Configuration, loopProvider, scriptEngine, _parameters);
            return backend;
        }

        private static void ValidateConfiguration(RenderingConfiguration config)
        {
            if (!config.EnableHdr && !config.PostProcessing.Equals(PostProcessingConfiguration.Disabled))
            {
                Debug.Log("Settings Validation", "Postprocessing not supported when HDR is disabled!");
            }
        }
    }
}
