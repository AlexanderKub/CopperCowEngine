using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Core;
using CopperCowEngine.Rendering;
using CopperCowEngine.Rendering.D3D11;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using SharpDX;

namespace PureProject
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var game = new Game();
        }

        private static void PreRenderTools()
        {
            var path = "C:\\Repos\\CopperCowEngine\\RawContent\\Mt-Washington-Cave-Room_Ref.hdr";
            //path = "C:\\Repos\\CopperCowEngine\\RawContent\\Tokyo_BigSight_3k.hdr";
            AssetsManager.GetManager().CubeMapPrerender(path, "House");
        }
    }
}
