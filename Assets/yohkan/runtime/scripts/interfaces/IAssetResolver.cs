using System;
using System.Threading;
using System.Threading.Tasks;

namespace yohkan.runtime.scripts.interfaces
{
    public interface IAssetResolver
    {
        Task ResolveAsync(Func<long,Task<bool>> askDownloadConfirmEvent,Action<float> onUpdateDownloadProgressEvent, CancellationToken cancellationToken);

    }
}
