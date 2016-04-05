using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ListApp {
	public partial class AddItemDialog : Window {
		//methods
		private MainWindow mainWindow; //TODO need the reference?
		//constructors
		public AddItemDialog(MainWindow mw) {
			mainWindow = mw;
			InitializeComponent();
		}
		//methods
		internal ListItem ShowAndGetItem(List<ItemTemplateItem> template) {
			if(Owner == null) {
				Owner = mainWindow;
			}
			ShowDialog();
			if (DialogResult.Value) {
				//TODO implement
				return null;
			}
			else {
				return null;
			}
		}
		//WPF
		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			//TODO perform validation
			DialogResult = true;
		}
	}
}
