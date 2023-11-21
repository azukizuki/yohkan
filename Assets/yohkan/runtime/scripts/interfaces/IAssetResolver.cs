using System.Threading;
#if YOHKAN_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace yohkan.runtime.scripts.interfaces
{
    public interface IAssetResolver
    {
        #if YOHKAN_ENABLE_UNITASK
        UniTask ResolveAsync(CancellationToken cancellationToken);

        #else
        Task ResolveAsync(CancellationToken cancellationToken);
        #endif
    }
}
