using EngineCore;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EngineCore.ECS.Components
{
    // Thread safe singleton. Reference: CLR via C# by Jeffry Richter
    /*
    private static readonly Object s_lock = new Object();
    private static SingletonFrameScene instance = null;

    public static SingletonFrameScene Instance {
        get {
            if (instance != null) return instance;
            Monitor.Enter(s_lock);
            SingletonFrameScene temp = new SingletonFrameScene();
            Interlocked.Exchange(ref instance, temp);
            Monitor.Exit(s_lock);
            return instance;
        }
    }
    */

    internal sealed class SingletonFrameScene : ISingletonEntityComponent
    {
        public StandardFrameData FrameData;
        
        public SingletonFrameScene()
        {
            FrameData = new StandardFrameData();
        }
    }
}
