using System;
using Gtk;

namespace DupsBegone
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("DupsBegone starts...");
				
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}
