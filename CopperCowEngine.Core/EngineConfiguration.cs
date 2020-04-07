using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopperCowEngine.Rendering;

namespace CopperCowEngine.Core
{
    public abstract class RenderOption
    {
        internal abstract IRenderBackend Create();
    }

    public class RenderingOption<T> : RenderOption where T : IRenderBackend
    {
        private readonly RenderingConfiguration _configuration;
        private readonly object[] _parameters;

        public RenderingOption(RenderingConfiguration configuration, params object[] parameters)
        {
            _configuration = configuration;
            _parameters = parameters;
        }

        internal override IRenderBackend Create()
        {
            var backend = Activator.CreateInstance<T>();
            backend.Initialize(_configuration, _parameters);
            return backend;
        }
    }

    public struct EngineConfiguration
    {
        public RenderOption Rendering;
    }
}
