using System;
using Gtk;
using DupsBegone;
using System.Reflection;

public partial class MainWindow: Gtk.Window
{
	DupFinder dupFinder;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnButtonGoStopClicked(object sender, EventArgs e)
	{

		var rect = this.textviewFoldersToScan.Allocation;
		PropertyInfo[] properties = rect.GetType().GetProperties();
		var sb = new System.Text.StringBuilder();
		foreach (PropertyInfo pi in properties)
		{
			sb.Append(
				string.Format("Name: {0} | Value: {1}\n", 
					pi.Name, 
					pi.GetValue(rect, null)
				) 
			);
		}
		LOG.d(sb);

		if ( "Go!".Equals(buttonGoStop.Label) ) {
			string[] foldersToScan = textviewFoldersToScan.Buffer.Text.Split('\n');
			textviewFoldersToScan.HeightRequest = 5;


			vboxRunStatus.Visible = true;
			dupFinder = new DupFinder(foldersToScan);
			dupFinder.startInBackground();

			GLib.Timeout.Add(100, new GLib.TimeoutHandler(update_status));
			buttonGoStop.Label = "Stop";

			UIQueries window = new UIQueries (); 
			window.ShowAll();
			window.newEmptyFolderFound("foo");
			window.newEmptyFolderFound("bar");

		} else {
			dupFinder.stop();
		}
	}

	public bool update_status ()
	{

		lblFoldersFoundCount.Text = dupFinder.getFoldersFoundCount().ToString();
		// returning true means that the timeout routine should be invoked
		// again after the timeout period expires.   Returning false would
		// terminate the timeout.

		if ( dupFinder.PotentialMatchingFoldsToScanCount > 0 ) {
			lblPotentialMatchingFoldsCount.Text = dupFinder.PotentialMatchingFoldsFoundCount.ToString();
			progressbarPotentialMatchingFolds.Fraction = 
				(float)dupFinder.PotentialMatchingFoldsScannedCount / dupFinder.PotentialMatchingFoldsToScanCount;
		}

		return true;
	}
}
