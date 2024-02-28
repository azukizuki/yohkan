using System.Threading;
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
        UniTask<bool> AskDownloadConfirmAsync(long downloadFileSizeByte,CancellationToken cancellationToken);
        #else
        Task<bool> AskDownloadConfirmAsync(long downloadFileSizeByte, CancellationToken cancellationToken);
        #endif

        #if YOHKAN_ENABLE_UNITASK
        UniTask OnStartDownloadAsync(CancellationToken cancellationToken);
        #else
        Task OnStartDownloadAsync(CancellationToken cancellationToken);
        #endif
        
        #if YOHKAN_ENABLE_UNITASK
        UniTask OnEndDownloadAsync(CancellationToken cancellationToken);
        #else
        Task OnEndDownloadAsync(CancellationToken cancellationToken);
        #endif
        
        void OnUpdateDownloadProgress(float normalizedProgress);
    }
}
