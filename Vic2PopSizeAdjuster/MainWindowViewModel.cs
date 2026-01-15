using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;
using System;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using System.Reactive;

namespace Vic2PopSizeAdjuster;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    public ObservableCollection<string> Paths { get; private set; } = new();    

    private string _popType = string.Empty;
    public string PopTypeToMult
    {
        get { return _popType; }
        set { this.RaiseAndSetIfChanged(ref _popType, value); }
    }
    private string _multText = string.Empty;
    public string MultiplierText
    {
        get { return _multText; }
        set { this.RaiseAndSetIfChanged(ref _multText, value); }
    }


    private string _errorText = string.Empty;
    public string ErrorText
    {
        get => _errorText;
        set => this.RaiseAndSetIfChanged(ref _errorText,  value);
    }

    public ReactiveCommand<Unit, Unit> SubmitCommand { get; }


    public MainWindowViewModel(IDialogService dialogService)
    {
        IObservable<bool> isInputValid = this.WhenAnyValue(
                        x => x.PopTypeToMult,
                        x => !string.IsNullOrWhiteSpace(x)
                        );

        this._dialogService = dialogService;

        OpenFileCommand = ReactiveCommand.Create(OpenFileAsync);
        OpenFilesCommand = ReactiveCommand.Create(OpenFilesAsync);     
        SubmitCommand = ReactiveCommand.Create(() =>
        {
            string pathToFile = Paths[0];

            bool leave = false;
            string text = null;

            if(float.TryParse(MultiplierText, out float multiplier))
            {

                try
                {
                    StreamReader reader = new StreamReader(pathToFile);
                    text = reader.ReadToEnd();
                    reader.Close();
                }
                catch(UnauthorizedAccessException)
                {
                    ErrorText = "ERROR: access denied to file at given path. (or it is a directory)";
                    leave = true;
                }

                if(!leave)
                {


                    string[] lines = text.Split(Environment.NewLine);

                    bool insideProv = false, insidePop = false, waitingForSize = false;

                    string popType = "none";

                    for(int i = 0; i < lines.Length; i++)
                    {
                        string[] splitByComment = lines[i].Split('#');
                        string[] splitByWhitespace = splitByComment[0].Split(null);
                        for(int x = 0; x < splitByWhitespace.Length; x++)
                        {
                            if(insideProv)
                            {
                                if(insidePop)
                                {
                                    if(splitByWhitespace[x] == "}")
                                    {
                                        waitingForSize = false;
                                        insidePop = false;
                                    }
                                    else if(waitingForSize && int.TryParse(splitByWhitespace[x].Trim(), out int value))
                                    {
                                        if(popType == PopTypeToMult || PopTypeToMult == "All")
                                        {
                                            int finalValue = (int)Math.Round(value * multiplier);
                                            splitByWhitespace[x] = finalValue.ToString();
                                        }
                                        waitingForSize = false;
                                    }
                                    else if(splitByWhitespace[x] == "size")
                                    {
                                        waitingForSize = true;
                                    }
                                }
                                else if(splitByWhitespace[x] == "}")
                                {
                                    insideProv = false;
                                }
                                else if(!string.IsNullOrEmpty(splitByWhitespace[x]))
                                {
                                    popType = splitByWhitespace[x];
                                    insidePop = true;
                                }
                            }
                            else if(int.TryParse(splitByWhitespace[x], out int result))
                            {
                                insideProv = true;  
                            }
                        }
                        splitByComment[0] = string.Join(" ", splitByWhitespace);
                        lines[i] = string.Join("#", splitByComment);
                    }


                    text = string.Join("\n", lines);

                    File.WriteAllText(pathToFile, text);
                    
                }
                
            }
            else
            {
                ErrorText = "ERROR: multiplier provided is not a number!";
            }
            
        }, isInputValid); 
    }

    public ICommand OpenFileCommand { get; }
    public ICommand OpenFilesCommand { get; }

    private async Task OpenFileAsync()
    {
        var settings = GetSettings(false);
        var result = await _dialogService.ShowOpenFileDialogAsync(this, settings);
        Paths.Clear();
        if (result?.Path != null)
        {
            Paths.Add(result.Path.LocalPath);
        }
    }

    private async Task OpenFilesAsync()
    {
        var settings = GetSettings(true);
        var result = await _dialogService.ShowOpenFilesDialogAsync(this, settings);
        Paths.Clear();
        foreach (var item in result)
        {
            Paths.Add(item?.Path?.LocalPath ?? string.Empty);
        }
    }

    private OpenFileDialogSettings GetSettings(bool multiple) => new()
    {
        Title = multiple ? "Open multiple files" : "Open single file",
        SuggestedStartLocation = new DesktopDialogStorageFolder(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!),
        Filters = new List<FileFilter>()
        {
            new(
                "Text Documents",
                new[]
                {
                    "txt", "md"
                }),
            new(
                "Binaries",
                new[]
                {
                    ".exe", ".dll"
                }),
            new("All Files", "*")
        }
    };
}
