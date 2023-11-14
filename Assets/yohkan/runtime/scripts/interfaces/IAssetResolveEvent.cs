using System.Threading.Tasks;

namespace yohkan.runtime.scripts.interfaces
{
    public interface IAssetResolveEvent
    {
        Task<bool> AskDownloadConfirm(long downloadFileSizeByte);
        void OnUpdateDownloadProgress(float normalizedProgress);
    }
}
