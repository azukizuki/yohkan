using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using yohkan.runtime.scripts.interfaces;

namespace yohkan.runtime.scripts
{
    public partial class YohkanAssetProvider : IAssetExtender
    {
#if YOHKAN_ENABLE_UNITASK
        public async UniTask ExtendContainerAsync(CancellationToken cancellationToken, IAssetResolveEvent overrideResolveEvent = null) 
#else
        public async Task ExtendContainerAsync(CancellationToken cancellationToken, IAssetResolveEvent overrideResolveEvent = null) 
#endif
        {
            await ResolveInternalAsync(overrideResolveEvent ?? _resolveEvent, cancellationToken);
        }
    }
}
