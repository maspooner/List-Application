using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ListApp {
	public partial class EditLayoutDialog : Window {
		//members
		private MainWindow mainWindow;
		private Dictionary<string, FrameworkElement> register;
		//constructors
		public EditLayoutDialog(MainWindow mw) {
			InitializeComponent();
			this.mainWindow = mw;
			register = new Dictionary<string, FrameworkElement>();
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
				DockPanel comp = new DockPanel();
				Grid.SetColumn(comp, iti.X);
				Grid.SetRow(comp, iti.Y);
				Grid.SetColumnSpan(comp, iti.Width);
				Grid.SetRowSpan(comp, iti.Height);
				
				Label c = new Label();
				c.HorizontalContentAlignment = HorizontalAlignment.Center;
				c.VerticalContentAlignment = VerticalAlignment.Center;
				c.Content = iti.Name + "\n(" + iti.Type + ")";
				c.Background = System.Windows.Media.Brushes.LightGray;

				Label r = new Label();
				DockPanel.SetDock(r, Dock.Right);
				r.Cursor = System.Windows.Input.Cursors.SizeWE;
				r.Width = 5;
				r.BorderThickness = new Thickness(0, 0, 3, 0);
				r.Background = System.Windows.Media.Brushes.Aqua;

				comp.Children.Add(r);
				comp.Children.Add(c);

				Console.WriteLine(iti.X + " " + iti.Y);
				layoutContent.Children.Add(comp);
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
