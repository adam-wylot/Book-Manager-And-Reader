using System.Windows.Input;

namespace BookReader.ViewModel;

internal class WelcomeViewModel : BaseViewModel
{
    public ICommand CreateRepositoryCommand { get; }
    public ICommand OpenRepositoryCommand { get; }


    public WelcomeViewModel(ICommand createCommand, ICommand openCommand)
    {
        CreateRepositoryCommand = createCommand;
        OpenRepositoryCommand = openCommand;
    }
}