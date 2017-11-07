using System;
using System.Collections.Generic;
using System.IO;
using Mono.Unix;
using Mono.Unix.Native;
using System.Collections.Concurrent;
using System.Threading;

namespace DupsBegone
{
	public class DupFinder
	{
//		private string[] pathsToScan;


		List<FolderItem> c00SuppliedPathsToScan = new List<FolderItem>();

		// Currently this DOES NOT need to be 'Concurrent', as we have only a single folder walking thread.
		// But that might change...
		ConcurrentQueue<FolderItem> c01FoldersToRecurseQueue = new ConcurrentQueue<FolderItem>();
		Thread folderRecurseThread;

		ConcurrentQueue<FolderItem> c02AllFoldersFoundQueue = new ConcurrentQueue<FolderItem>();

		// A dictionary, hashed by Folder-File count strings like:
		//  "4-23-AB89" = 4 folder, 23 files, and a hash of all constituent numbers.
		// Which contains lists of folders with that same pattern;
		GroupedFolderSet c03FoldersHashedByItemCounts = new GroupedFolderSet();
		// An ordered queue of _potential_ duplicate folders, based on crude file/folder count data above:
		// Note that currently this is not ready for use until the end of above processing. Because
		// a) It needs to be ordered, and b) We want to know about all potential dups befor triggering UI...
		ConcurrentQueue<FolderItem> c04PotentialDupFoldersByItemCounts = new ConcurrentQueue<FolderItem>();

		public int PotentialMatchingFoldsToScanCount  = 0;
		public int PotentialMatchingFoldsScannedCount = 0;
		public int PotentialMatchingFoldsFoundCount   = 0;


		GroupedFolderSet c05PotentialDupFoldersByFileSizeHash = new GroupedFolderSet();

//		// We keep a collection of terminal/leaf folders (i.e. Containing no files)
//		// for the duplicate folder algorithm later:
//		ICollection<FolderItem> foldersTerminals = new List<FolderItem>();



//		Thread scanSizesThread;


		private const byte DT_DIR = 0x04;
		private const byte DT_REG = 0x08;


		/// <summary>
		/// Private default constructor. Must supply paths to process!
		/// </summary>
		private DupFinder()
		{
		}

		public DupFinder(string[] pathsToScan)
		{
			foreach (string s in pathsToScan) {
				c00SuppliedPathsToScan.Add(new FolderItem(s, null));
			}
		}

		public long getFoldersFoundCount()
		{
			return c02AllFoldersFoundQueue.Count + c01FoldersToRecurseQueue.Count;
		}

		public void startInBackground()
		{

//			try
//			{
//				var txtFiles = Directory.EnumerateFileSystemEntries(pathsToScan[0]);
//
//				foreach (string item in txtFiles)
//				{
//					LOG.d("Got FileSystem item: \"" + item + "\"" );
//					string fileName = item.Substring(item.Length + 1);
//				}
//			}

			try
			{
				foreach ( FolderItem fi in c00SuppliedPathsToScan ) {
					c01FoldersToRecurseQueue.Enqueue( fi );
				}

				folderRecurseThread = new Thread( delegate() {findThemDups();});
				folderRecurseThread.Start();

			
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		public void stop()
		{
			folderRecurseThread.Abort();
		}

		/// <summary>
		/// Finds the them dups.
		/// Main function orchestrating the steps in finding duplicates.
		/// </summary>
		private void findThemDups()
		{
			// Find the complete folder list as fast as possible, without competing background threads,
			//  so that we can formulate a rough ETA based on total folders:
			// (Simple disk access is probably not improved with multi-threading, so we don't)
			fastFindAllFolders();

			// Now we have the basic folder tree structures completely in memory, recurse from the top again generating
			// a crude folder-file count hash for each folder:
			// This helps to identify _potential_ groups of identical folders.
			foreach (FolderItem fi in c00SuppliedPathsToScan) {
				fi.getFileAndFolderCountHash();
			}

			// As stated above, we now have a crude count/hash per folder.
			// We now scan these looking for hash matches:
			findPotentialIdenticalFoldersByItemCounts();

			filterPotentialIdenticalFoldersByFileSizeHash();

			/// ...

			// Now scan through the folder list looking for:
			// a) Folders that might be identical, based on number and size of files,
			// b) Files that might be identical based on file size.
//			scanSizesThread = new Thread( delegate() {scanFolderAndFileSizes();} );
//			scanSizesThread.Start();

//			scanFolderAndFileSizes();


		}


		/// <summary>
		/// This method is used initially to get the full folder list as fast as possible in a single thread.
		/// Only after this method completes do we start a closer look at folder contents
		/// </summary>
		public void fastFindAllFolders()
		{
			// Pre-allocate all possible variables used. Faster?
			FolderItem currentFolder, newFolderItem;
			UnixDirectoryInfo toScan;
			Mono.Unix.Native.Dirent[] dentries;
			uint numFoldersHere;

			// This loop essentially implements a non-recursive breadth-first search,
			// storing folders found in a breadth-ordered queue.
			// It is open to multi-threading, but is not threaded as yet.
			while (c01FoldersToRecurseQueue.TryDequeue(out currentFolder)) {

				toScan = new UnixDirectoryInfo(currentFolder.getFullPath());
				dentries = toScan.GetEntries();

				numFoldersHere = 0;
				foreach (Mono.Unix.Native.Dirent de in dentries) {
					//LOG.d("Type: " + de.d_type + " " + UnixPath.Combine(root.FullName, de.d_name));
					if ( (de.d_type ^ DT_DIR) == 0 ) {
						newFolderItem = new FolderItem(de.d_name, currentFolder);
						c01FoldersToRecurseQueue.Enqueue( newFolderItem );
						//LOG.d("Enqueue folder...");
						currentFolder.addItem( newFolderItem );
						++numFoldersHere;
					} else {
						currentFolder.addItem( new FileItem(de.d_name,currentFolder) );
					}
				}

//				currentFolder.rollUp();

//				if ( numFoldersHere == 0 ) {
//					// For 'leaf' folders, roll-up total File/Folder counts through parent folders:
//					// Note that for efficiency, we only trigger this at leaf folders.
//					currentFolder.rollUpTotalCounts();
//				}

				// Put the folder we have just scanned into the (has been) 'Recursed' queue:
				c02AllFoldersFoundQueue.Enqueue(currentFolder);

//				if ( numFoldersHere == 0 ) {
//					//TODO: Current folder should be stored with a hash lookup containing the folder and file counts
//					//  like "4-3" or something like that. - A folder can only be identical to another if they both have
//					// the same sub-folder AND file count.
//					foldersTerminals.Add(currentFolder);
//				}

				//LOG.d("Folders recursed: " + c02AllFoldersFoundQueue.Count + ", To recurse: " + c01FoldersToRecurseQueue.Count);
			}

			LOG.d("Failed to Dequeue any more folders. Returning.");
			return;


		}

		private void findPotentialIdenticalFoldersByItemCounts()
		{
			PotentialMatchingFoldsToScanCount = c02AllFoldersFoundQueue.Count;

//			Dictionary<string, List<FolderItem>> duplicatesOnly = new Dictionary<string, List<FolderItem>>();

			var dupKeyList = new List<string>();

			// Create a summary dictionary, containing lists of items with matching (crude) hashes:
			// This loop has no disk access, so is very fast:
			foreach (FolderItem fi in c02AllFoldersFoundQueue) {
				string countHash = fi.getCountHashString();

				int num = c03FoldersHashedByItemCounts.AddFolder(countHash, fi);

				if ( num > 1 ) {
					++PotentialMatchingFoldsFoundCount;
					if ( num == 2 ) ++PotentialMatchingFoldsFoundCount;
				}

				++PotentialMatchingFoldsScannedCount;
			}

			c03FoldersHashedByItemCounts = c03FoldersHashedByItemCounts.WithNonDupsRemoved();

			LOG.d( "Found total of " + PotentialMatchingFoldsFoundCount  + " potential duplicate folders" );
			LOG.d( "Over " + c03FoldersHashedByItemCounts.Count  + " duplicate groups" );


			// Sort the keylist, based on most folders in each dup, then most files:
			dupKeyList.Sort( delegate(string s1, string s2) {
				// Hash strings look like "0-21-AB89" - 0=numFolds, 21=numFiles
				string[] s1a = s1.Split('-'), s2a = s2.Split('-');
				int numFolds1 = Int32.Parse(s1a[0]);
				int numFolds2 = Int32.Parse(s2a[0]);
			
				if ( numFolds2 > numFolds1 )
					return +1;
				else if ( numFolds2 < numFolds1 )
					return -1;
				else {
					int numFiles1 = Int32.Parse(s1a[1]);
					int numFiles2 = Int32.Parse(s2a[1]);
					if ( numFiles2 > numFiles1 )
						return +1;
					else if ( numFiles2 < numFiles1 )
						return -1;
					else
						return 0;
				}
			} );


			// Now create an ordered (grouped) list of potential duplicate folders, for which we should take a closer look:
			List<FolderItem> folderList;
			foreach (string key in dupKeyList) {
				folderList = c03FoldersHashedByItemCounts[key];
				foreach(FolderItem fi in folderList) {
					c04PotentialDupFoldersByItemCounts.Enqueue(fi);
				}
			}

			LOG.d("Potential dup folders:");
			foreach (FolderItem fi in c04PotentialDupFoldersByItemCounts) {
				LOG.d(fi.getCountHashString() + " : " + fi.getFullPath());
			}

		}

		public void filterPotentialIdenticalFoldersByFileSizeHash()
		{
			int n = 0;
			foreach (KeyValuePair<string, List<FolderItem>> kvp in c03FoldersHashedByItemCounts) {
				foreach (FolderItem fi in kvp.Value) {
					++n;
				}
			}

			PotentialMatchingFoldsToScanCount  = n;
			PotentialMatchingFoldsScannedCount = 0;

			int groupSet = 0;

			// Create a new Grouped Folder set with more detailed hashes based on file sizes (not just counts):
			foreach (KeyValuePair<string, List<FolderItem>> kvp in c03FoldersHashedByItemCounts) {
				GroupedFolderSet gfs = new GroupedFolderSet();
				foreach (FolderItem fi in kvp.Value) {
					// .getFileSizesHash() hits the disk to find individual file sizes, and hashes them,
					// And recurses to the bottom of the file tree as needed:
					long hash = fi.getFileSizesHash();
					gfs.AddFolder(String.Format("{0:X}", hash), fi);
					++PotentialMatchingFoldsScannedCount;
				}
				// At the end of the above loop we have processed one 'group' of potentially duplicate folders.
				// We can ditch the non-duplicates now:
				gfs = gfs.WithNonDupsRemoved();
				if ( gfs.Count > 0 )
					LOG.d("Potential group(s) based on file sizes:\n" + gfs.ToString());
				// And add the remaining dups to the next queue:
				foreach (var x in gfs) {
					// Use of groupSet avoids occasional hash clash problems:
					c05PotentialDupFoldersByFileSizeHash.Add(x.Key + "-" + ++groupSet, x.Value);
				}
			}

//			// Remove 'Groups' that have only 1 item:
//			gfs = gfs.WithNonDupsRemoved();
			LOG.d("New Dup Folder list via file size hashing:\n" + c05PotentialDupFoldersByFileSizeHash.ToString());


		}

//		public void scanFolderAndFileSizes()
//		{
//			FolderItem folderItem;
//			long sizeHash;
//
//			while (c02AllFoldersFoundQueue.TryDequeue(out folderItem)) {
//				sizeHash = folderItem.getFileSizesHash();
//			}
//
//		}



	}
}

