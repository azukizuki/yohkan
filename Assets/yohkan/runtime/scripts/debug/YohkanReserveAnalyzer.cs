using System.Collections.Generic;
using System.Linq;

namespace yohkan.runtime.scripts.debug
{
#if ENABLE_YOHKAN_ANALYZER || UNITY_EDITOR

    public static class YohkanReserveAnalyzer
    {
        public static IReadOnlyCollection<string[]> ReservedInfo => _queue;
        private static Queue<string[]> _queue = new();
        
        
        public static void PushReservedInfo(IEnumerable<string> address)
        {
            _queue.Enqueue(address.ToArray());
        }

        public static void Clear()
        {
            _queue.Clear();
        }

    }
#endif
}
