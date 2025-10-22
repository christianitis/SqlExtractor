using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlExtractor.Lib.TreeItem;

namespace SqlExtractor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	public static IHost? AppHost { get; private set; }

	public App()
	{
		
	}
	
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
	}
}

