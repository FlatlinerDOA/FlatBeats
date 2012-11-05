namespace Flatliner.Phone.ViewModels
{
    using Microsoft.Phone.Tasks;

    public interface IChooserViewModel<TResult> where TResult : TaskEventArgs
    {      
        void ProcessResult(TResult result);
    }
}
