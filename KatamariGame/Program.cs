using EngineCore;

namespace KatamariGame
{
    class Program
    {
        static void Main(string[] args) {
            Engine game = new Game("Katamari");
            game.Run();
        }
    }
}
