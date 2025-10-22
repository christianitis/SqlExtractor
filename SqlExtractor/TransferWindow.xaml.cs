using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using SqlExtractor.Lib;


namespace SqlExtractor;

public partial class TransferWindow : Window
{
	public SqlObjectTree Tree;

	public record ErrorMessageItem(string ObjectName, string Error);

	public TransferWindow(SqlObjectTree tree)
	{
		InitializeComponent();
		Tree = tree;
		HostComboBox.ItemsSource = SavedConnections.GetAllConnectionStrings().AsBuilders().Select(it => it.DataSource);
	}

	private void HostComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count == 0 || e.AddedItems[0] is not string host) return;
		if (SavedConnections.GetAllConnectionStrings().AsBuilders()
			    .FirstOrDefault(it => it.DataSource == host) is not { } existingConnection) return;
		UserTextBox.Text = existingConnection.UserID;
		PasswordBox.Password = existingConnection.Password;
	}

	private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
	{
		// var targetConnection = SavedConnections.GetAllConnectionStrings().AsBuilders()
		// 	.FirstOrDefault(it => it.DataSource == HostComboBox.Text) ?? new SqlConnectionStringBuilder
		// {
		// 	DataSource = HostComboBox.Text,
		// 	UserID = UserTextBox.Text,
		// 	Password = PasswordBox.Password,
		// 	TrustServerCertificate = true
		// };
		// var transfer = new Transfer(Tree);
		// transfer.StatusChanged += (status, obj) => StatusText.Text = status switch
		// {
		// 	Transfer.Status.Initializing => "Initializing...",
		// 	Transfer.Status.ScriptingObject => $"Scripting {obj}",
		// 	Transfer.Status.ScriptingJob => $"Scripting {obj}"
		// };
		// transfer.OnError += (item, exception) => ErrorsListBox.Items.Add(item.Name + ": " + exception.Message);
		// StatusProgress.IsIndeterminate = true;
		//
		// StatusProgress.IsIndeterminate = false;
		// StatusText.Text = "Done!";
	}
}