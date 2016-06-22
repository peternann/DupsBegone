using System;
using System.Collections.Generic;

namespace DupsBegone
{
	public class FolderItem : FileSystemItem
	{

		private ICollection<FileItem>   localFiles = null;
		private ICollection<FolderItem> localFolds = null;

		// Used to store TOTAL counts of files/folders in this folder AND all sub-folders:
		public ulong totalFoldsRecursive = 0;
		public ulong totalFilesRecursive = 0;

		// We also store an XOR hash of file/folder numbers:
		public ulong hashFFCounts = 0;

		private FolderItem()
		{
		}

		public FolderItem( string name, FolderItem parentFolder )
			: base( name, parentFolder )
		{
		}

		public void addItem( FileItem fi )
		{
			if ( localFiles == null )
				localFiles = new LinkedList<FileItem>();

			localFiles.Add(fi);
		}

		public void addItem( FolderItem fi )
		{
			if ( localFolds == null )
				localFolds = new LinkedList<FolderItem>();

			localFolds.Add(fi);
		}

		/// <summary>
		/// Rolls up total counts. This overload should only get called from 'leaf' folder:
		/// </summary>
		public void rollUp() {

			// Initiliase total counts to local folder/file counts. (We haven't explored any children yet)
			totalFoldsRecursive = (localFolds == null) ? 0 : (ulong)localFolds.Count;
			totalFilesRecursive = (localFiles == null) ? 0 : (ulong)localFiles.Count;

			// Similarly with our hash:
			hashFFCounts = 0;
			hashFFCounts ^= totalFilesRecursive << (int)( totalFilesRecursive & 3 );
			hashFFCounts ^= totalFoldsRecursive << (int)( totalFoldsRecursive & 3 ) << 10;

			// Recurse up through parent folders adding counts and the NEW hash:
			if ( parentFolder != null ) {
				parentFolder.rollUp( totalFoldsRecursive, totalFilesRecursive, hashFFCounts );
			}

		}

		/// <summary>
		/// Rolls up total counts.
		/// Called from sub-dirs telling us total file/folder counts from that branch.
		/// </summary>
		/// <param name="numFolds">Number of folders from sub-dir.</param>
		/// <param name="numFiles">Number of files from sub-dir.</param>
		public void rollUp( ulong numFolds, ulong numFiles, ulong xorHash )
		{
			totalFoldsRecursive += numFolds;
			totalFilesRecursive += numFiles;

			// And update XOR hash with child's hash:
			hashFFCounts ^= xorHash;

			// Recurse up through parent folders adding new counts and hash value:
			if ( parentFolder != null ) {
				parentFolder.rollUp( numFolds, numFiles, xorHash );
			}
			
		}
	}
}

