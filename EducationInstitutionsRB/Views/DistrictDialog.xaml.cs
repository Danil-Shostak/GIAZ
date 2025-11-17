using EducationInstitutionsRB.Models;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace EducationInstitutionsRB.Views;

public sealed partial class DistrictDialog : ContentDialog
{
    public District District { get; set; }
    public List<Region> Regions { get; set; }

    public DistrictDialog(District district, string title, List<Region> regions)
    {
        this.InitializeComponent();
        District = district;
        Regions = regions;
        DialogTitle.Text = title;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Валидация
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(District.Name))
            errors.Add("Название района");

        if (District.RegionId == 0)
            errors.Add("Область");

        if (errors.Any())
        {
            args.Cancel = true;
        }
    }
}