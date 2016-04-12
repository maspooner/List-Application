using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ListApp {
	public partial class EditLayoutDialog : Window {
		//members
		private MainWindow mainWindow;
		//constructors
		public EditLayoutDialog(MainWindow mw) {
			InitializeComponent();
			this.mainWindow = mw;
		}
		//methods
		internal List<ItemTemplateItem> ShowAndGetTemplate(List<ItemTemplateItem> template) {
			Owner = mainWindow;
			CreateElements(template);
			ShowDialog();
			return null; //TODO
		}
		private void CreateElements(List<ItemTemplateItem> template) {
			for(int i = 0; i < MList.FIELD_GRID_WIDTH; i++) {
				ColumnDefinition cd = new ColumnDefinition();
				cd.Width = new GridLength(0, GridUnitType.Star);
				contentPanel.ColumnDefinitions.Add(cd);
			}
			int maxHeight = 0;
			foreach (ItemTemplateItem iti in template) {
				if (iti.Y > maxHeight) {
					maxHeight = iti.Y;
				}
			}
			maxHeight += 5;
			for (int i = 0; i < maxHeight; i++) {
				RowDefinition rd = new RowDefinition();
				rd.Height = new GridLength(0, GridUnitType.Star); //TODO always equal width
				contentPanel.RowDefinitions.Add(rd);
			}

			//TODO
		}
		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}
	}
}
