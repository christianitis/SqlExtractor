using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using SqlExtractor.Lib.TreeItem;

namespace SqlExtractor;

public partial class MergeConflictResolutionDialog : Window, INotifyPropertyChanged
{
	private Urn _pathToHere;

	public Urn PathToHere
	{
		get => _pathToHere;
		set => SetField(ref _pathToHere, value);
	}

	private IUrnPart _oldItem;

	public IUrnPart OldItem
	{
		get => _oldItem;
		set => SetField(ref _oldItem, value);
	}

	private IUrnPart _newItem;

	public IUrnPart NewItem
	{
		get => _newItem;
		set => SetField(ref _newItem, value);
	}

	public enum Resolution
	{
		UseOld,
		UseNew,
		RenameOld,
		RenameNew,
		Cancel
	}
	public (IUrnPart? objectToUse, Resolution resolution) Result { get; private set; }
	
	private string OldVersionText => $"Old version: {PathToHere.GetAttribute("Name")}.{OldItem.Name}";
	private string NewVersionText => $"New version: {PathToHere.GetAttribute("Name")}.{NewItem.Name}";

	public MergeConflictResolutionDialog(Urn pathToHere, IUrnPart oldItem, IUrnPart newItem)
	{
		PathToHere = pathToHere;
		OldItem = oldItem;
		NewItem = newItem;
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}