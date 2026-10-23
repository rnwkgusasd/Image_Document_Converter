using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IDC.Models;

public partial class ConvertItemRow : ObservableObject
{
    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private string fileName;

    [ObservableProperty]
    private FileInfo fileInfo;

    [ObservableProperty]
    private Uri iconUri;
}
