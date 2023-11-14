using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace yohkan.runtime.scripts.debug
{
    public static class YohkanLogger
    {
        [Conditional("ENABLE_YOHKAN_LOG")]
        public static void Log(object o)
        {
            Debug.Log(o);
        }
        
        [Conditional("ENABLE_YOHKAN_LOG")]
        public static void LogWarning(object o)
        {
            Debug.LogWarning(o);
        }

        [Conditional("ENABLE_YOHKAN_LOG")]
        public static void LogError(object o)
        {
            Debug.LogError(o);
        }
    }
}
