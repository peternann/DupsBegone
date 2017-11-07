using System;

namespace DupsBegone
{
	public partial class UIQueries : Gtk.Window
	{
		public UIQueries() :
			base(Gtk.WindowType.Toplevel)
		{
			this.Build();
		}

		public void newEmptyFolderFound( string path )
		{

//			var hb = new global::Gtk.HBox ();
//			hb.Name = "hbox";
//			hb.Spacing = 6;

			global::Gtk.Image trashIcon = new global::Gtk.Image( global::Gdk.Pixbuf.LoadFromResource ("DupsBegone.trashcan_16x16.gif") );
			trashIcon.Name = "trashcan";
			trashIcon.Visible = true;

//			var lbl = new global::Gtk.Label();
//			lbl.Name = "label";
//			lbl.LabelProp = path;
//
//			hb.Add(trashIcon);
//			hb.Add(lbl);
//
//			hb.Visible = trashIcon.Visible = lbl.Visible = true;
//
//			this.vboxDeleteEmptyFolders.Add (hb);

			this.vboxDeleteEmptyFolders.Add(trashIcon);

		}
	}
}

