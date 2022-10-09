using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

using static System.Environment;

namespace Automation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
	{
		private const string FileExplorerProcessName = "explorer";
		private string? status;

		public MainWindow()
		{
			InitializeComponent();

			DataContext = this;
		}

		public string? Status 
		{ 
			get
			{
				return status;
			}
			set
			{
				status = value;
				PropertyChanged?.Invoke(this, new  PropertyChangedEventArgs(nameof(Status)));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private AutomationElement? FindFileExplorer()
		{
			Condition windowCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, nameof(ControlType.Window), PropertyConditionFlags.IgnoreCase);

			return AutomationElement.RootElement
									.FindAll(TreeScope.Children, windowCondition)
							        .OfType<AutomationElement>()
							        .FirstOrDefault(a =>
									{
										using var process = Process.GetProcessById(a.Current.ProcessId);
										return Process.GetProcessById(a.Current.ProcessId).ProcessName == FileExplorerProcessName;
									});
		}

		private void AppendStatus(string line)
		{
			Status += NewLine + line;
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{			
			Status = null;
			automateButton.IsEnabled = false;

			var explorer = FindFileExplorer();

			if (explorer is null)
			{
				AppendStatus("fileExplorer not found");
				return;
			}

			AppendStatus("fileExplorer found");

			Condition ebCondition = new PropertyCondition(AutomationElement.NameProperty, "SearchEditBox", PropertyConditionFlags.IgnoreCase);
			var editBox = explorer.FindFirst(TreeScope.Descendants, ebCondition);


			//This event does not get raised so have to resort to using AddAutomationFocusChangedEventHandler
			//System.Windows.Automation.Automation.AddAutomationPropertyChangedEventHandler(editBox, TreeScope.Subtree, OnEditBoxFocusChanged, AutomationElement.HasKeyboardFocusProperty);

			System.Windows.Automation.Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);

			if (editBox is null)
			{
				AppendStatus("SearchEditBox not found");
				return;
			}

			AppendStatus("SearchEditBox found");

			editBox.SetFocus();

			AppendStatus("SearchEditBox focus set");
		}

		private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
		{			
			var automationElement = sender as AutomationElement;
			if(automationElement is null)
			{
				return;
			}
			
			if (automationElement.Current.Name == "Search box")
			{
				System.Windows.Automation.Automation.RemoveAllEventHandlers();
				AppendStatus($"event raised {automationElement.Current.Name}");

				if (automationElement.TryGetCurrentPattern(ValuePattern.Pattern, out object valuePattern))
				{
					((ValuePattern)valuePattern).SetValue("Search me");

					AppendStatus($"value set");
				}

				Dispatcher.BeginInvoke( () => automateButton.IsEnabled = true);
			}
		}
	}
}
