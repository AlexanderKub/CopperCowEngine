using System.Runtime.CompilerServices;

namespace CopperCowEngine.JobSystem
{
    public interface IJobParallelFor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Execute(int i);
    }
}
