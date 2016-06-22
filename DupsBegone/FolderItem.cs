using System;
using System.Collections.Generic;
using Mono.Unix;

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

		private long hashByFileSizes = -1;

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
		/// Rolls up total counts.
		/// </summary>
		public void rollUp() {

			// Initiliase total counts to local folder/file counts. (We haven't explored any children yet)
			totalFoldsRecursive = (localFolds == null) ? 0 : (ulong)localFolds.Count;
			totalFilesRecursive = (localFiles == null) ? 0 : (ulong)localFiles.Count;

			// Similarly with our File/Folder count hash:
			hashFFCounts = 0;
			hashFFCounts ^= totalFilesRecursive << (int)( totalFilesRecursive & 3 );
			hashFFCounts ^= totalFoldsRecursive << (int)( totalFoldsRecursive & 3 ) << 10;

			// Recurse up through parent folders adding counts and the NEW hash:
			if ( parentFolder != null ) {
				// It says "total*" here, but really they are just the current local counts inside this immediate method:
				parentFolder.rollUp( totalFoldsRecursive, totalFilesRecursive, hashFFCounts );
			}

		}

		/// <summary>
		/// Rolls up total counts.
		/// Called from sub-dirs telling us total file/folder counts from that branch.
		/// </summary>
		/// <param name="numFolds">Number of folders from sub-dir.</param>
		/// <param name="numFiles">Number of files from sub-dir.</param>
		protected void rollUp( ulong numFolds, ulong numFiles, ulong xorHash )
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

		public string getCountHash() {
			return String.Format("{0:D}-{1:D}-{2:X}",
				totalFoldsRecursive, totalFilesRecursive, hashFFCounts);
		}

		/// <summary>
		/// Gets the file sizes hash.
		/// May take some time to run on high-level folders, so the intention is to call it only on qualified folders, likely to be matches.
		/// </summary>
		/// <returns>The file sizes hash.</returns>
		public long getFileSizesHash() {

			if ( hashByFileSizes != -1 ) {
				return hashByFileSizes;
			} else {
				UnixDirectoryInfo di = new UnixDirectoryInfo(this.getFullPath());
				UnixFileSystemInfo[] folderItems = di.GetFileSystemEntries();
				long hash = 0;
				long totalSize = 0;
				foreach (UnixFileSystemInfo fsItem in folderItems) {
					if ( fsItem.IsDirectory ) {
						;
					} else {
						// A simple, fast, order-independent hash of the set of file sizes:
						// (Who knows if it's any good...)
						// Some online info suggests simply adding...
						hash ^= (long)fsItem.Length << (int)((fsItem.Length >> 2) & 0x15);   // 0x15 = (binary)10101
						totalSize += fsItem.Length;
						//TODO: Store _file_ path indexed against the file size, for comparison in File Comparison thread.
					}
				}

				// Special case: For folders with an even number of files of the same size (usually 2) the hash
				// comes out at zero. For this case, use the total size instead:
				// This is still a valid hash for the local folder contents, as long as we are consistent:
				// Some online info suggests simply adding to avoid repeats 'nulling' with xor.
				if ( hash == 0 ) {
					hash = totalSize;
				}

				// The Hash is not complete and true for this folder unless it incorporates all child folders, so recurse down:
				if ( localFolds != null ) {
					foreach (FolderItem fi in localFolds) {
						hash ^= fi.getFileSizesHash();
					}
				}

				return hash;
			}

			//hashString = String.Format("{0:X}-{1:D}-{2:D}-{3:D}", hash, totalFilesSize, totalFiles, totalDirs);
			//return hashString;
		}
	}
}

