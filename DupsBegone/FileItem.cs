using System;

namespace DupsBegone
{
	public class FileItem : FileSystemItem
	{
		public ulong  size { get; set; }


		private FileItem()
		{
		}

		public FileItem( string name, FolderItem parentFolder ) : base(name, parentFolder)
		{
		}

	}
}

