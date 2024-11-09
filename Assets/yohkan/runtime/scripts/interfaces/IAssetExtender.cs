using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace yohkan.runtime.scripts.interfaces
{
    public interface IAssetExtender
    {
#if YOHKAN_ENABLE_UNITASK
        UniTask ExtendContainerAsync(CancellationToken cancellationToken, IAssetResolveEvent overrideResolveEvent = null);
#else
        Task ExtendContainerAsync(CancellationToken cancellationToken, IAssetResolveEvent overrideResolveEvent = null);
#endif
    }
}
