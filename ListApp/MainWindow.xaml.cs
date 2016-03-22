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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ListApp {
	public class CodeControl : DependencyObject {
		public static readonly DependencyProperty CodeProperty = DependencyProperty.Register("Code", typeof(string), typeof(CodeControl), 
			new PropertyMetadata("default", OnCodeChange));
		public string Code {
			get { return (string) GetValue(CodeProperty); }
			set { SetValue(CodeProperty, value); }
		}
		public static void OnCodeChange(DependencyObject source, DependencyPropertyChangedEventArgs e) {
			Console.WriteLine("CHANGE");
		}
	}
	public partial class MainWindow : Window {
		
		public MainWindow() {
			InitializeComponent();
		}
	}
}
