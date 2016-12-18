using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ListApp {
	enum SyncTask { ONE, NEW, UPDATE, ALL }
	class SyncManager {
		//members
		private ProgressBar syncBar;
		private Label messagesLabel;
		private Button syncCancel;
		private MainWindow mainWindow;
		private BackgroundWorker itemWorker;
		private SyncList workingList;
		private int listId;
		private SyncTask taskType;
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
			taskType = SyncTask.NEW;
			listId = -1;
		}
		//properties
		internal bool HasJob { get { return itemWorker.IsBusy; } }
		//methods
		internal bool StartRefreshAllTask(SyncList sl, int shownList) {
			return StartTask(sl, SyncTask.ALL, null, shownList);
		}
		internal bool StartRefreshOneTask(SyncList sl, SyncItem si, int shownList) {
			return StartTask(sl, SyncTask.ONE, si, shownList);
		}
		internal bool StartRefreshNewTask(SyncList sl, int shownList) {
			return StartTask(sl, SyncTask.NEW, null, shownList);
		}
		internal bool StartRefreshCurrentTask(SyncList sl, int shownList) {
			return StartTask(sl, SyncTask.UPDATE, null, shownList);
		}
		private bool StartTask(SyncList sl, SyncTask type, object argument, int shownList) {
			if (!itemWorker.IsBusy) {
				this.workingList = sl;
				listId = shownList;
				sl.SetObservable(false);
				taskType = type;
				syncBar.Visibility = syncCancel.Visibility = System.Windows.Visibility.Visible;
				itemWorker.RunWorkerAsync(argument);
				return true;
			}
			return false;
		}
		internal void CancelTask() {
			itemWorker.CancelAsync();
			workingList.SetObservable(true);
		}
		private void ItemWorker_DoWork(object sender, DoWorkEventArgs e) {
			BackgroundWorker bw = sender as BackgroundWorker;
			ISchema sch = workingList.Schema;
			bw.ReportProgress(0, "Setting up...");
			sch.PrepareRefresh(taskType, workingList, workingList.SchemaParams);
			bw.ReportProgress(0, "Adding item #1 of " + sch.GetTotalProgress());
			int xyz = 0; //TODO remove
			while (sch.GetProgress() < sch.GetTotalProgress() && (++xyz) < 10) {
				if (bw.CancellationPending) {
					e.Cancel = true;
					break;
				}
				switch (taskType) {
					case SyncTask.ALL: sch.RefreshAll(workingList); break;
					case SyncTask.NEW: sch.FindNew(workingList); break;
					case SyncTask.UPDATE: sch.RefreshCurrent(workingList); break;
					case SyncTask.ONE: sch.RefreshOne(workingList, e.Argument as SyncItem); break;
					default: throw new NotImplementedException();
				}
				Console.WriteLine("Adding item #" + sch.GetProgress() + " of " + sch.GetTotalProgress());
				bw.ReportProgress(sch.GetProgress() * 100 / sch.GetTotalProgress(),
					"Adding item #" + sch.GetProgress() + " of " + sch.GetTotalProgress());
			}
			sch.FinishRefresh();
		}
		private void ItemWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			syncBar.Value = e.ProgressPercentage;
			messagesLabel.Content = e.UserState as string;
		}
		private void ItemWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			syncBar.Visibility = syncCancel.Visibility = System.Windows.Visibility.Collapsed;
			workingList.SetObservable(true);
			messagesLabel.Content = "";
			if (e.Cancelled) {
				Console.WriteLine("Canceled");
			}
			else if (e.Error != null) {
				throw e.Error; //TODO handle gracefully
			}
			else {
				Console.WriteLine("Done");
				mainWindow.Dispatcher.Invoke(delegate {
					mainWindow.SyncCompleted_Callback(listId);
				});
			}
		}
	}
}
