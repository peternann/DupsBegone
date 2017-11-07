
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	
	private global::Gtk.VBox vbox1;
	
	private global::Gtk.MenuBar menubar1;
	
	private global::Gtk.Frame frame1;
	
	private global::Gtk.Alignment GtkAlignment;
	
	private global::Gtk.TextView textviewFoldersToScan;
	
	private global::Gtk.Label GtkLabel1;
	
	private global::Gtk.Button buttonGoStop;
	
	private global::Gtk.VBox vboxRunStatus;
	
	private global::Gtk.HBox hbox1;
	
	private global::Gtk.Label label2;
	
	private global::Gtk.Label lblFoldersFoundCount;
	
	private global::Gtk.HBox hbox2;
	
	private global::Gtk.Label label3;
	
	private global::Gtk.Label lblPotentialMatchingFoldsCount;
	
	private global::Gtk.ProgressBar progressbarPotentialMatchingFolds;

	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("DupsBegone!");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 6;
		// Container child vbox1.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><menubar name='menubar1'/></ui>");
		this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar1")));
		this.menubar1.Name = "menubar1";
		this.vbox1.Add (this.menubar1);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.menubar1]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.frame1 = new global::Gtk.Frame ();
		this.frame1.Name = "frame1";
		this.frame1.ShadowType = ((global::Gtk.ShadowType)(0));
		// Container child frame1.Gtk.Container+ContainerChild
		this.GtkAlignment = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
		this.GtkAlignment.Name = "GtkAlignment";
		this.GtkAlignment.LeftPadding = ((uint)(12));
		// Container child GtkAlignment.Gtk.Container+ContainerChild
		this.textviewFoldersToScan = new global::Gtk.TextView ();
		this.textviewFoldersToScan.Buffer.Text = "/NAS/N/OurPhotos_BACKUP\n/NAS/N/OurPhotos-OldAndMessy";
		this.textviewFoldersToScan.CanFocus = true;
		this.textviewFoldersToScan.Name = "textviewFoldersToScan";
		this.GtkAlignment.Add (this.textviewFoldersToScan);
		this.frame1.Add (this.GtkAlignment);
		this.GtkLabel1 = new global::Gtk.Label ();
		this.GtkLabel1.Name = "GtkLabel1";
		this.GtkLabel1.Xalign = 0F;
		this.GtkLabel1.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Folder(s) to scan:</b>");
		this.GtkLabel1.UseMarkup = true;
		this.frame1.LabelWidget = this.GtkLabel1;
		this.vbox1.Add (this.frame1);
		global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.frame1]));
		w5.Position = 1;
		// Container child vbox1.Gtk.Box+BoxChild
		this.buttonGoStop = new global::Gtk.Button ();
		this.buttonGoStop.WidthRequest = 10;
		this.buttonGoStop.CanFocus = true;
		this.buttonGoStop.Name = "buttonGoStop";
		this.buttonGoStop.UseUnderline = true;
		this.buttonGoStop.Label = global::Mono.Unix.Catalog.GetString ("Go!");
		this.vbox1.Add (this.buttonGoStop);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.buttonGoStop]));
		w6.Position = 2;
		w6.Expand = false;
		w6.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.vboxRunStatus = new global::Gtk.VBox ();
		this.vboxRunStatus.Name = "vboxRunStatus";
		this.vboxRunStatus.Spacing = 6;
		// Container child vboxRunStatus.Gtk.Box+BoxChild
		this.hbox1 = new global::Gtk.HBox ();
		this.hbox1.Name = "hbox1";
		this.hbox1.Spacing = 6;
		// Container child hbox1.Gtk.Box+BoxChild
		this.label2 = new global::Gtk.Label ();
		this.label2.Name = "label2";
		this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Fast-scan folders:");
		this.hbox1.Add (this.label2);
		global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label2]));
		w7.Position = 0;
		w7.Expand = false;
		w7.Fill = false;
		// Container child hbox1.Gtk.Box+BoxChild
		this.lblFoldersFoundCount = new global::Gtk.Label ();
		this.lblFoldersFoundCount.Name = "lblFoldersFoundCount";
		this.lblFoldersFoundCount.LabelProp = global::Mono.Unix.Catalog.GetString ("0");
		this.hbox1.Add (this.lblFoldersFoundCount);
		global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.lblFoldersFoundCount]));
		w8.Position = 1;
		w8.Expand = false;
		w8.Fill = false;
		this.vboxRunStatus.Add (this.hbox1);
		global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vboxRunStatus [this.hbox1]));
		w9.Position = 0;
		w9.Expand = false;
		w9.Fill = false;
		// Container child vboxRunStatus.Gtk.Box+BoxChild
		this.hbox2 = new global::Gtk.HBox ();
		this.hbox2.Name = "hbox2";
		this.hbox2.Spacing = 6;
		// Container child hbox2.Gtk.Box+BoxChild
		this.label3 = new global::Gtk.Label ();
		this.label3.Name = "label3";
		this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("Shallow-scan potential matching folders:");
		this.hbox2.Add (this.label3);
		global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.label3]));
		w10.Position = 0;
		w10.Expand = false;
		w10.Fill = false;
		// Container child hbox2.Gtk.Box+BoxChild
		this.lblPotentialMatchingFoldsCount = new global::Gtk.Label ();
		this.lblPotentialMatchingFoldsCount.Name = "lblPotentialMatchingFoldsCount";
		this.lblPotentialMatchingFoldsCount.LabelProp = global::Mono.Unix.Catalog.GetString ("0");
		this.hbox2.Add (this.lblPotentialMatchingFoldsCount);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.lblPotentialMatchingFoldsCount]));
		w11.Position = 1;
		w11.Expand = false;
		w11.Fill = false;
		// Container child hbox2.Gtk.Box+BoxChild
		this.progressbarPotentialMatchingFolds = new global::Gtk.ProgressBar ();
		this.progressbarPotentialMatchingFolds.Name = "progressbarPotentialMatchingFolds";
		this.hbox2.Add (this.progressbarPotentialMatchingFolds);
		global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.progressbarPotentialMatchingFolds]));
		w12.Position = 2;
		this.vboxRunStatus.Add (this.hbox2);
		global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vboxRunStatus [this.hbox2]));
		w13.Position = 1;
		w13.Expand = false;
		w13.Fill = false;
		this.vbox1.Add (this.vboxRunStatus);
		global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.vboxRunStatus]));
		w14.Position = 3;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 474;
		this.DefaultHeight = 354;
		this.vboxRunStatus.Hide ();
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.buttonGoStop.Clicked += new global::System.EventHandler (this.OnButtonGoStopClicked);
	}
}