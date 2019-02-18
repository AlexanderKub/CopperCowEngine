namespace EngineCore
{
    class DebugTextureRender
    {
        public void CreateDebugTextureRenderer()
        {
            Engine.Instance.AddGameObject(
                "DebugTextureRenderer",
                null,
                new Renderer()
                {
                    Geometry = Primitives.Cube()
                }
             );
        }
    }
}
