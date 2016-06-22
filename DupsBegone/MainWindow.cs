﻿using System;
using Gtk;
using DupsBegone;

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
		if ( "Go!".Equals(buttonGoStop.Label) ) {
			string[] foldersToScan = textviewFoldersToScan.Buffer.Text.Split('\n');
			vboxRunStatus.Visible = true;
			dupFinder = new DupFinder(foldersToScan);
			dupFinder.startInBackground();

			GLib.Timeout.Add(100, new GLib.TimeoutHandler(update_status));
			buttonGoStop.Label = "Stop";
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