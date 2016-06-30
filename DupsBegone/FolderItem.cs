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
//		public ulong totalFoldsRecursive = 0;
//		public ulong totalFilesRecursive = 0;

		// We also store an XOR hash of file/folder numbers:
//		public ulong hashFFCounts = 0;

		private ABHash ffcHash;   // File and Folder Count Hash.

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


		public ABHash getFileAndFolderCountHash()
		{
			// Initilise this.ffcHash with local counts:
			ffcHash = new ABHash(
				(localFiles == null) ? 0 : localFiles.Count,
				(localFolds == null) ? 0 : localFolds.Count
			);
			// Then add in hashes from all child folders:
			// Will recurse into all children (But no extra disk access):
			if ( this.localFolds != null ) {
				foreach (FolderItem fi in this.localFolds) {
					ffcHash.Add(fi.getFileAndFolderCountHash());
				}
			}

			return ffcHash;
		}

		public string getCountHashString() {
			return ffcHash.ToString();
//			return String.Format("{0:D}-{1:D}-{2:X}",
//				totalFoldsRecursive, totalFilesRecursive, hashFFCounts);
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

