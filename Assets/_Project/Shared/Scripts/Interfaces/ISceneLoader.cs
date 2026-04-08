namespace WreckTogether.Shared
{
    using System.Threading.Tasks;

    public interface ISceneLoader
    {
        Task LoadSceneAsync(string sceneName);
        Task UnloadSceneAsync(string sceneName);
    }
}
