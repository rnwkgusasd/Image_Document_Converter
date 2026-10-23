using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IDC.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IDC.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Uri ICON_FILE = new("pack://application:,,,/Resources/Icons/icon_file.png", UriKind.Absolute);
    private readonly Uri ICON_PDF = new("pack://application:,,,/Resources/Icons/icon_pdf.png", UriKind.Absolute);
    private readonly Uri ICON_JPG = new("pack://application:,,,/Resources/Icons/icon_jpg.png", UriKind.Absolute);
    private readonly Uri ICON_PNG = new("pack://application:,,,/Resources/Icons/icon_png.png", UriKind.Absolute);

    public MainViewModel()
    {
        
    }

    [ObservableProperty]
    private bool isPreviewVisible = true;

    [ObservableProperty]
    private ImageSource? selectedImage = null;

    private void UploadFiles(IEnumerable<string> files)
    {
        if (files == null || !files.Any()) return;

        foreach (string filePath in files)
        {
            FileInfo fileInfo = new(filePath);

            var iconUri = fileInfo.Extension switch
            {
                ".jpg" => ICON_JPG,
                ".png" => ICON_PNG,
                ".pdf" => ICON_PDF,
                _ => ICON_FILE
            };

            ItemList.Add(new ConvertItemRow() { FileName = fileInfo.Name, FileInfo = fileInfo, IconUri = iconUri });
        }
    }

    [RelayCommand]
    private void PreviewToggle()
    {
        IsPreviewVisible = !IsPreviewVisible;
    }

    [RelayCommand]
    private void ItemClick(object param)
    {
        if(param is ConvertItemRow selectedItem)
        {
            foreach (var item in ItemList) item.IsSelected = false;

            selectedItem.IsSelected = true;

            var fileInfo = selectedItem.FileInfo;

            if (fileInfo.Exists)
            {
                if(fileInfo.Extension != ".pdf" || fileInfo.Extension != "")
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fileInfo.FullName, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        SelectedImage = bitmap;
                    }
                    catch (Exception ex)
                    {
                        // 로딩 실패 예외 처리 (로그 등)
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }
        }
    }

    [RelayCommand]
    private void FileDrop(List<string> filePaths)
    {
        UploadFiles(filePaths);
    }

    [RelayCommand]
    private void OpenItems()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "PDF Files|*.pdf|PNG Image Files|*.png|JPG Image Files|*.jpg|All Files|*.*"
        };

        if(dialog.ShowDialog() == DialogResult.OK)
        {
            var files = dialog.FileNames;

            if(files.Any(x => !x.EndsWith(".pdf") && !x.EndsWith(".png") && !x.EndsWith(".jpg")))
            {
                System.Windows.MessageBox.Show("형식에 맞지 않는 파일이 선택되었습니다.", "파일 선택 에러", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UploadFiles(files);
        }
    }

    [RelayCommand]
    private void ClearItemList()
    {
        SelectedImage = null;
        ItemList.Clear();
        OnPropertyChanged(nameof(ItemList));
    }

    public ObservableCollection<ConvertItemRow> ItemList { get; set; } = new ObservableCollection<ConvertItemRow>();
}
