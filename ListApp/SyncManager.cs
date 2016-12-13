using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ListApp {
	class SyncManager {
		//members
		private ProgressBar syncBar;
		private Label messagesLabel;
		private Button syncCancel;
		private MainWindow mainWindow;
		private BackgroundWorker itemWorker;
		private SyncList workingList;
		//constructors
		internal SyncManager(MainWindow mw, ProgressBar pb, Label ml, Button sc) {
			mainWindow = mw;
			syncBar = pb;
			messagesLabel = ml;
			syncCancel = sc;
			workingList = null;
			itemWorker = new BackgroundWorker();
			itemWorker.WorkerReportsProgress = true;
			itemWorker.WorkerSupportsCancellation = true;
			itemWorker.DoWork += ItemWorker_DoWork;
			itemWorker.ProgressChanged += ItemWorker_ProgressChanged;
			itemWorker.RunWorkerCompleted += ItemWorker_RunWorkerCompleted;
		}
		//methods
		internal void StartRefreshAllTask(SyncList sl) {
			if (!itemWorker.IsBusy) {
				this.workingList = sl;
				syncBar.Visibility = syncCancel.Visibility = System.Windows.Visibility.Visible;
				itemWorker.RunWorkerAsync();
			}
		}
		internal void CancelRefreshAllTask() {
			itemWorker.CancelAsync();
		}
		private void ItemWorker_DoWork(object sender, DoWorkEventArgs e) {
			BackgroundWorker bw = sender as BackgroundWorker;
			bw.ReportProgress(0, "Setting up...");
			workingList.PrepareRefresh();
			IEnumerable<SyncItem> itemEnum = workingList.Schema.CreateNewItems(workingList.Template);
			int done = 0;
			int count = workingList.Schema.GetItemCount();
			List<SyncItem> list = new List<SyncItem>();
			bw.ReportProgress(0, "Adding item #1 of " + count);
			foreach (SyncItem sli in itemEnum) {
				if (bw.CancellationPending) {
					e.Cancel = true;
					break;
				}
				list.Add(sli);
				done++;
				Console.WriteLine("Adding item #" + (done + 1) + " of " + count);
				bw.ReportProgress(done * 100 / count, "Adding item #" + (done + 1) + " of " + count);
			}
			e.Result = list;
		}
		private void ItemWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			syncBar.Value = e.ProgressPercentage;
			messagesLabel.Content = e.UserState as string;
		}
		private void ItemWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			syncBar.Visibility = syncCancel.Visibility = System.Windows.Visibility.Collapsed;
			messagesLabel.Content = "";
			if (e.Cancelled) {
				Console.WriteLine("Canceled");
			}
			else if (e.Error != null) {
				throw e.Error;
			}
			else {
				Console.WriteLine("Done");
				workingList.Items.Clear();
				workingList.Items.AddRange(e.Result as List<SyncItem>);
				mainWindow.Dispatcher.Invoke(mainWindow.Refresh);
			}
		}
	}
}
