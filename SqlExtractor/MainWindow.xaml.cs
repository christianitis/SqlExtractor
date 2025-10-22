using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Win32;
using SqlExtractor.Lib;
using System.Diagnostics;
using System.IO;
using MdXaml;
using Microsoft.Extensions.DependencyInjection;
using SqlExtractor.Lib.TreeItem;
using SqlExtractor.Lib.TreeItem.Impl;

namespace SqlExtractor;

public partial class MainWindow : Window, INotifyPropertyChanged
{
	public static string[] TransferDataEnabledTypes { get; } =
		["Server", "Database", "Schema", "Table", null];

	public event PropertyChangedEventHandler? PropertyChanged;

	public SqlObjectTree InputTree { get; } = new();
	public SqlObjectTree OutputTree { get; } = new();
	private AbstractSqlTreeItem? _selectedItem;

	public AbstractSqlTreeItem? SelectedItem
	{
		get => _selectedItem;
		private set
		{
			_selectedItem = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
		}
	}

	public MainWindow()
	{
		InitializeComponent();
		DataContext = this;
		AddButton.Content = ">>>";
		RemoveButton.Content = "<<<";

		InputTreeView.ItemsSource = InputTree.Items;
		OutputTreeView.ItemsSource = OutputTree.Items;
	}

	private static bool ShouldTransferWithDataCheckboxBeEnabled(AbstractSqlTreeItem? item)
	{
		if (item == null) return false;
		//if (item.Urn == null && item.Name == "Table") return true;
		//return item.Urn != null && TransferDataEnabledTypes.Contains(item.Urn.Type);
		return true;
	}

	private async void AddConnectionButton_Click(object sender, RoutedEventArgs e)
	{
		var dialog = new NewConnectionDialog();
		dialog.ShowDialog();
		if (dialog.DialogResult is not true) return;

		var host = dialog.Host;
		var user = dialog.User;
		var password = dialog.Password;

		StatusProgress.IsIndeterminate = true;

		try
		{
			StatusText.Text = $"Connecting to {host}";

			var serverConnection = new ServerConnection
			{
				LoginSecure = false,
				Login = user,
				Password = password,
				ServerInstance = host
			};

			var item = new ServerSqlTreeItem("Loading...");
			item.InitializeFromDatabaseWithChildren(new Server(serverConnection));
			InputTree.Items.Add(item);
			
			SavedConnections.SaveConnectionString(serverConnection.ConnectionString);
			StatusText.Text = $"Connected to {host}";
		}
		catch (Exception exception)
		{
			MessageBox.Show(exception.Message, "Error While Connecting To Database", MessageBoxButton.OK,
				MessageBoxImage.Error);
		}

		StatusProgress.IsIndeterminate = false;
	}

	private void RemoveConnectionButton_Click(object sender, RoutedEventArgs e)
	{
		//if (InputTreeView.SelectedItem is not AbstractSqlTreeItem item || item.Urn?.Type != "Server") return;
		//InputTree.RemoveItem(item);
	}

	private void AddButton_Click(object sender, RoutedEventArgs e)
	{
		if (InputTreeView.SelectedItem is not AbstractSqlTreeItem item) return;
		AddItemToOutput(item);
	}

	private void RemoveButton_Click(object sender, RoutedEventArgs e)
	{
		if (OutputTreeView.SelectedItem is not AbstractSqlTreeItem item) return;
		//OutputTree.RemoveItem(item);
	}

	private void AddItemToOutput(AbstractSqlTreeItem item)
	{
		//OutputTree.AddItem(item);
		if (AddDependenciesCheckbox.IsChecked is true)
		{
			HandleDependencies(item);
		}
	}

	private void SourceItemClicked(object sender, MouseButtonEventArgs e)
	{
		if (sender is not TextBlock textBlock)
		{
			TransferDataCheckBox.IsEnabled = false;
			return;
		}

		SelectedItem = textBlock.Tag as AbstractSqlTreeItem ?? textBlock.DataContext as AbstractSqlTreeItem;

		if (e.ClickCount < 2) return;
		if (SelectedItem == null) return;
		AddItemToOutput(SelectedItem);
	}

	private void HandleDependencies(AbstractSqlTreeItem item)
	{
		/*
		if (SqlObjectTree.NonObjectTypes.Contains(item.Urn?.Type))
		{
			return;
		}

		var urns = (item.Urn is null ? item.Children.Select(it => it.Urn!).ToArray() : [item.Urn!])
			.Where(urn => urn.Type != "Job").ToArray();
		if (urns.IsNullOrEmpty())
		{
			return;
		}

		var walker = new DependencyWalker(new Server(item.OriginalServer));
		var dependencies = walker
			.WalkDependencies(walker.DiscoverDependencies(urns, DependencyType.Parents))
			.DistinctBy(it => it.Urn)
			.Where(it => it.Urn.Type != "UnresolvedEntity");
		var allInputObjects = InputTree.Objects.ToArray();
		var allOutputObjects = OutputTree.Objects.ToArray();
		foreach (var dependency in dependencies)
		{
			if (allOutputObjects.Any(obj => obj.Urn == dependency.Urn)) continue;
			Dispatcher.Invoke(() =>
			{
				var objectToAdd = (AbstractSqlTreeItem)allInputObjects
					.Where(obj => obj.OriginalServer.ConnectionString == item.OriginalServer.ConnectionString)
					.First(obj => obj.Name == dependency.Urn.GetAttribute("Name"))
					.Clone();
				//objectToAdd.BackgroundColor = new SolidColorBrush(Color.FromRgb(0, 150, 255));

				OutputTree.AddItem(objectToAdd);
			});
		}
		*/
	}

	private void TargetItemClicked(object sender, MouseButtonEventArgs e)
	{
		try
		{
			if (sender is not TextBlock textBlock)
			{
				TransferDataCheckBox.IsEnabled = false;
				return;
			}

			SelectedItem = textBlock.Tag as AbstractSqlTreeItem ?? textBlock.DataContext as AbstractSqlTreeItem;
			TransferDataCheckBox.IsEnabled = ShouldTransferWithDataCheckboxBeEnabled(SelectedItem);
			//TransferDataCheckBox.IsChecked = SelectedItem?.WithData;

			if (e.ClickCount < 2) return;
			if (SelectedItem == null) return;
			//OutputTree.RemoveItem(SelectedItem);
		}
		catch (Exception exception)
		{
			MessageBox.Show(exception.Message, $"Error: {exception.Message}", MessageBoxButton.OK,
				MessageBoxImage.Error);
		}
	}

	private void GenerateScriptButton_OnClick(object sender, RoutedEventArgs e)
	{
		var dialog = new TransferWindow(OutputTree);
		dialog.ShowDialog();
	}

	private async void Load_OnClick(object sender, RoutedEventArgs e)
	{
		try
		{
			var list = new List<AbstractSqlTreeItem>();
			await Task.Run(() =>
			{
				var dialog = new OpenFileDialog
				{
					Filter = "SQL Extractor Profiles (*.profile.xml)|*.profile.xml",
					DefaultExt = ".profile.xml",
					Title = "Save Profile"
				};
				if (dialog.ShowDialog() is not true) return;
				var serializer = new XmlSerializer(typeof(SqlObjectTree));
				using var stream = dialog.OpenFile();
				list.AddRange(((SqlObjectTree)serializer.Deserialize(stream)).Items);
			});
			foreach (var AbstractSqlTreeItem in list)
			{
				//OutputTree.AddItem(AbstractSqlTreeItem);
			}
		}
		catch (Exception exception)
		{
			MessageBox.Show(exception.Message, "Error While Loading File", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private void Save_OnClick(object sender, RoutedEventArgs e)
	{
		var dialog = new SaveFileDialog
		{
			Filter = "SQL Extractor Profiles (*.profile.xml)|*.profile.xml",
			DefaultExt = ".profile.xml",
			Title = "Save Profile"
		};
		if (dialog.ShowDialog() is not true) return;
		var serializer = new XmlSerializer(typeof(SqlObjectTree));
		using var stream = dialog.OpenFile();
		serializer.Serialize(stream, OutputTree);
	}

	private void TransferDataCheckBox_OnClick(object sender, RoutedEventArgs e)
	{
		if (SelectedItem is not { } item) return;
		//SelectedItem.WithData = TransferDataCheckBox.IsChecked ?? false;
		foreach (var child in SelectedItem.Children
			         .RecursiveSelect(ShouldTransferWithDataCheckboxBeEnabled))
		{
			//child.WithData = TransferDataCheckBox.IsChecked ?? false;
		}
	}

	private async void OutputToFolderButton_OnClick(object sender, RoutedEventArgs e)
	{
		var outputToFolderWindow = new OutputToFolderWindow(OutputTree);
		outputToFolderWindow.ShowDialog();
	}

	private void Manual_OnClick(object sender, RoutedEventArgs e)
	{
		var engine = new Markdown();
		using var markdown = System.Reflection.Assembly.GetExecutingAssembly()
			.GetManifestResourceStream("SqlExtractor.README.md");
		using var streamReader = new StreamReader(markdown);
		var document = engine.Transform(streamReader.ReadToEnd());
		var reader = new FlowDocumentReader { Document = document };
		var window = new Window
		{
			Content = reader,
			Title = "User Manual"
		};
		window.Show();
	}

	private void GitHub_OnClick(object sender, RoutedEventArgs e)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = "https://github.com/Element-Logic-FL/SQLExtractor",
			UseShellExecute = true
		});
	}
}