#if YOHKAN_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace yohkan.runtime.scripts.interfaces
{
    public interface IAssetResolveEvent
    {
        #if YOHKAN_ENABLE_UNITASK
        UniTask<bool> AskDownloadConfirm(long downloadFileSizeByte);
        #else
        Task<bool> AskDownloadConfirm(long downloadFileSizeByte);
        #endif        
        void OnUpdateDownloadProgress(float normalizedProgress);
    }
}
