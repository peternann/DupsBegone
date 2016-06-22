using System;
using System.Text;

namespace DupsBegone
{
	public class FileSystemItem
	{
		public string name { get; }
		protected FolderItem parentFolder;

		public FileSystemItem()
		{
		}

		public FileSystemItem( string name, FolderItem parentFolder )
		{
			this.name = name;
			this.parentFolder = parentFolder;
		}

		public string getFullPath() {
			if ( parentFolder == null ) {
				return name;
			} else {
				return (parentFolder.getFullPath() + "/" + name);
			}
		}
	}
}

