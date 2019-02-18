using EngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PongGame
{
    class Program
    {
        static void Main(string[] args) {
            Engine game = new Game("PongGame");
            game.Run();
        }
    }
}
