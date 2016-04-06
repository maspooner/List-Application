using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
						uie = new TextBox();
						if(li != null) {
							(uie as TextBox).Text = li[i].GetValue().ToString();
						}
						break;
					case ItemType.DATE:
						DatePicker dp = new DatePicker();
						dp.SelectedDate = li == null ? DateTime.Today : (DateTime)li[i].GetValue();
						uie = dp;
						break;
					case ItemType.ENUM:
						ComboBox cb = new ComboBox();
						cb.ItemsSource = iti.Metadata as string[];
						cb.SelectedIndex = li == null ? 0 : (int)li[i].GetValue();
						uie = cb;
						break;
					case ItemType.IMAGE:
						uie = new Image();
						if(li != null) {
							(uie as Image).Source = (li[i] as ImageField).GetBitmap();
						}
						break;
				}
				uie.SetValue(NameProperty, iti.Name + "_ui");
				switch (iti.Type) {
					case ItemType.BASIC:
					case ItemType.DATE:
					case ItemType.ENUM:
						DockPanel dp = new DockPanel();
						Label l = new Label();
						l.Content = iti.Name + ": ";
						dp.Children.Add(l);
						DockPanel.SetDock(l, Dock.Left);
						dp.Children.Add(uie);
						DockPanel.SetDock(uie, Dock.Right);
						contentPanel.Children.Add(dp);
						break;
					case ItemType.IMAGE:
						//TODO format

						break;
				}
			}
		}
		private bool IsValidInput() {
			foreach(UIElement uie in contentPanel.Children) {
				if(uie is DockPanel) {
					UIElement comp = (uie as DockPanel).Children[1];
					if(comp is TextBox) {
						if ((comp as TextBox).Text.Equals("")) {
							return false;
						}
					}
					else if(comp is DatePicker) {
						if((comp as DatePicker).SelectedDate == null) {
							return false;
						}
					}
					//combo box don't care
				}
				else { //image
					//TODO
				}
			}
			return true;
		}
		//WPF
		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			if (IsValidInput()) {
				DialogResult = true;
			}
			else {

			}
		}
	}
}
