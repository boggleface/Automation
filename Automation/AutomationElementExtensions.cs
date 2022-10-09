using System.Threading.Tasks;
using System.Windows.Automation;

namespace Automation
{
	public static class AutomationElementExtensions
	{
		public static async Task<bool> SetFocusWithTimeout(this AutomationElement element, int timeout)
		{
			element.SetFocus();

			while (element.Current.HasKeyboardFocus)
			{
				await Task.Delay(25);
			}

			return true;
		}
	}
}
