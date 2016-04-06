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
			CreateElements(template);
			ShowDialog();
			if (DialogResult.Value) {
				Console.WriteLine("OK");
				//TODO implement
				foreach(ItemTemplateItem iti in template) {
					object field = FindName(iti.Name + "_ui");
				}
				return null;
			}
			else {
				return null;
			}
		}
		private void CreateElements(List<ItemTemplateItem> template, ListItem li = null) {
			contentPanel.Children.Clear();
			for (int i = 0; i < template.Count; i++) {
				ItemTemplateItem iti = template[i];
				UIElement uie = null;
				switch (iti.Type) {
					case ItemType.BASIC:
					case ItemType.DATE:
						uie = new TextBlock();
						if(li != null) {
							(uie as TextBlock).Text = li[i].GetValue().ToString();
						}
						break;
					case ItemType.ENUM:
						ComboBox cb = new ComboBox();
						cb.ItemsSource = iti.Metadata as string[];
						cb.SelectedIndex = 0;
						uie = cb;
						break;
					case ItemType.IMAGE:

						break;
				}
				uie.SetValue(NameProperty, iti.Name + "_ui");
				switch (iti.Type) {
					case ItemType.BASIC:
					case ItemType.DATE:
					case ItemType.ENUM:
						StackPanel sp = new StackPanel();
						sp.Orientation = Orientation.Horizontal;
						Label l = new Label();
						l.Content = iti.Name + ": ";
						sp.Children.Add(l);
						sp.Children.Add(uie);
						contentPanel.Children.Add(sp);
						break;
					case ItemType.IMAGE:


						break;
				}
			}
		}
		private bool IsValidInput() {
			//TODO perform validation
			foreach(UIElement uie in contentPanel.Children) {

			}
		}
		//WPF
		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			
			DialogResult = IsValidInput();
		}
	}
}
