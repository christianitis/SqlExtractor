using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SqlExtractor.Lib;

namespace SqlExtractor;

public partial class OutputToFolderWindow : Window
{
	private const string DefaultFileTextBoxText = "No file selected.";

	public enum OutputType
	{
		OneFilePerObject,
		OneFilePerType,
		OneFile
	}

	public OutputType? CurrentOutputType { get; set; }


	public OutputToFolderWindow(SqlObjectTree tree)
	{
		InitializeComponent();
		FileTextBox.Text = DefaultFileTextBoxText;
	}

	private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
	{
		var fileDialog = new OpenFolderDialog();
		if (fileDialog.ShowDialog() is true)
		{
			FileTextBox.Text = fileDialog.FolderName;
		}

		ShouldExportButtonBeEnabled();
	}

	private void ExportButton_OnClick(object sender, RoutedEventArgs e)
	{
		var task = new Task(() =>
		{
			Dispatcher.Invoke(() => StatusProgress.IsIndeterminate = true);
			
			Dispatcher.Invoke(() => StatusProgress.IsIndeterminate = false);
		});
		
		task.Start();
	}

	private MessageBoxResult ShowErrorDialog(Exception e)
	{
		return MessageBox.Show($"An error occurred while generating scripts:\n{e.Message}", "Script Generation Error",
			MessageBoxButton.OKCancel, MessageBoxImage.Error);
	}

	private void OutputRadioButton_OnClick(object sender, RoutedEventArgs e)
	{
		if (sender is not RadioButton { CommandParameter: OutputType outputType }) return;
		CurrentOutputType = outputType;
		ShouldExportButtonBeEnabled();
	}

	private void ShouldExportButtonBeEnabled()
	{
		ExportButton.IsEnabled = CurrentOutputType is not null && FileTextBox.Text is not DefaultFileTextBoxText;
	}
}