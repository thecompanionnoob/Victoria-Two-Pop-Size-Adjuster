using HanumanInstitute.MvvmDialogs.Avalonia;
namespace Vic2PopSizeAdjuster;

/// <summary>
/// Maps view models to views.
/// </summary>
public class ViewLocator : ViewLocatorBase
{
    /// <inheritdoc />
    protected override string GetViewName(object viewModel) => viewModel.GetType().FullName!.Replace("ViewModel", "");
}