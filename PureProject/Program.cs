using CopperCowEngine.Rendering.D3D11.Editor;

namespace PureProject
{
    internal class Program
    {
        private static int[] _source = new[] {88, 1, 2, 3, 4, 5};

        private static void Main(string[] args)
        {
            var game = new Game();
        }

        private static void PreRenderTools()
        {
            var path = "C:\\Repos\\CopperCowEngine\\RawContent\\Mt-Washington-Cave-Room_Ref.hdr";
            //path = "C:\\Repos\\CopperCowEngine\\RawContent\\Tokyo_BigSight_3k.hdr";
            D3D11AssetsImporter.CubeMapPrerender(path, "House");
        }
    }
}
