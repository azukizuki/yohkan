using System.Collections.Generic;
using System.Linq;

namespace yohkan.runtime.scripts.debug
{
#if ENABLE_YOHKAN_ANALYZER || UNITY_EDITOR

    public static class YohkanReserveAnalyzer
    {
        public class ReserveAnalyzerEventArgs
        {
            public ReserveAnalyzerEventArgs(IEnumerable<string> addresses)
            {
                Address = addresses;
            }
            public IEnumerable<string> Address { get; }
        }

        public delegate void ReserveAnalyzeEventHandler(ReserveAnalyzerEventArgs args);
        public static event ReserveAnalyzeEventHandler ReserveAnalyzeEvent;
        
        public static void PushReservedInfo(IEnumerable<string> address)
        {
            ReserveAnalyzeEvent?.Invoke(new ReserveAnalyzerEventArgs(address));
        }

    }
#endif
}
