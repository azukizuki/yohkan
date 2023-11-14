using System;
using System.Threading;
using System.Threading.Tasks;

namespace yohkan.runtime.scripts.interfaces
{
    public interface IAssetResolver
    {
        Task ResolveAsync(CancellationToken cancellationToken);

    }
}
