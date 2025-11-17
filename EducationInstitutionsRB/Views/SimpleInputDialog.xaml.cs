using Microsoft.UI.Xaml.Controls;

namespace EducationInstitutionsRB.Views;

public sealed partial class SimpleInputDialog : ContentDialog
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string InputText { get; set; }

    public SimpleInputDialog(string title, string message, string defaultValue = "")
    {
        this.InitializeComponent();
        Title = title;
        Message = message;
        InputText = defaultValue;
    }
}