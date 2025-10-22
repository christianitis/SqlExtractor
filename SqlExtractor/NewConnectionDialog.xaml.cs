using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SqlExtractor.Lib;

namespace SqlExtractor;

public partial class NewConnectionDialog : Window
{
	public string User => UserTextBox.Text;
	public string Password => PasswordBox.Password;
	public string Host => HostComboBox.Text;

	public NewConnectionDialog()
	{
		InitializeComponent();
		DataContext = this;
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

	private void CancelButton_OnClick(object sender, RoutedEventArgs e)
	{
		DialogResult = false;
		Close();
	}
	private void OkButton_OnClick(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
		Close();
	}
}