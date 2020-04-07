using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperCowEngine.AssetsManagement
{
    public partial class AssetsManager
    {
        public void CubeMapPrerender(string path, string outputName)
        {
            RenderBackend.CubeMapPrerender(path, outputName);
        }

        public void BrdfIntegrate(string outputName)
        {
            RenderBackend.BrdfIntegrate(outputName);
        }
    }
}
