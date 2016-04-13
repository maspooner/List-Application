using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
			Utils.SetupContentGrid(layoutContent, template);
			//TODO
			foreach (ItemTemplateItem iti in template) {
				Label l = new Label();
				l.HorizontalContentAlignment = HorizontalAlignment.Center;
				l.VerticalContentAlignment = VerticalAlignment.Center;
				l.Content = iti.Name + "\n(" + iti.Type + ")";
				l.Background = System.Windows.Media.Brushes.LightGray;
				l.BorderThickness = new Thickness(3);
				Grid.SetColumn(l, iti.X);
				Grid.SetRow(l, iti.Y);
				Grid.SetColumnSpan(l, iti.Width);
				Grid.SetRowSpan(l, iti.Height);
				Console.WriteLine(iti.X + " " + iti.Y);
				layoutContent.Children.Add(l);
			}
			Console.WriteLine(layoutContent.ColumnDefinitions.Count);
			Console.WriteLine(layoutContent.RowDefinitions.Count);
		}

		private void LayoutContent_SizeChanged(object sender, SizeChangedEventArgs e) {
			double width = layoutContent.ColumnDefinitions[0].ActualWidth;
			foreach (RowDefinition rd in layoutContent.RowDefinitions) {
				rd.MinHeight = width;
			}
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}
	}
}
