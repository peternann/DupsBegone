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
		private string[] pathsToScan;

		// Currently this DOES NOT need to be 'Concurrent', as we have only a single folder walking thread.
		// But that might change...
		ConcurrentQueue<FolderItem> foldersToRecurse = new ConcurrentQueue<FolderItem>();
		Thread folderRecurseThread;

		ConcurrentQueue<FolderItem> foldersRecursed = new ConcurrentQueue<FolderItem>();

		// A dictionary, hashed by Folder-File count strings like "4-23-AB89",
		//  containing lists of folders with that same pattern;
		Dictionary<string, List<FolderItem>> HashedFolderListByCounts = new Dictionary<string, List<FolderItem>>();

		// An ordered queue of _potential_ duplicate folders, based on crude file/folder count data:
		ConcurrentQueue<FolderItem> potentialDupFoldersByCounts = new ConcurrentQueue<FolderItem>();

		public int PotentialMatchingFoldsToScanCount  = 0;
		public int PotentialMatchingFoldsScannedCount = 0;
		public int PotentialMatchingFoldsFoundCount   = 0;

		// We keep a collection of terminal/leaf folders (i.e. Containing no files)
		// for the duplicate folder algorithm later:
		ICollection<FolderItem> foldersTerminals = new List<FolderItem>();


		public long TotalFoldersFound = 0;

		Thread scanSizesThread;


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
			this.pathsToScan = pathsToScan;
		}

		public long getFoldersFoundCount()
		{
			if (TotalFoldersFound == 0)
				return (foldersRecursed.Count + foldersToRecurse.Count);
			else
				return TotalFoldersFound;
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

			FolderItem folderItem;

			TotalFoldersFound = 0;

			try
			{
				foreach ( string folderPath in pathsToScan ) {
					folderItem = new FolderItem( folderPath, null );
					foldersToRecurse.Enqueue( folderItem );
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
			// This method also calculates recursive file/folder counts and a crude hash for each folder that
			//  helps to identify _potential_ groups of identical folders.
			fastFindAllFolders();
			TotalFoldersFound = getFoldersFoundCount();

			// As stated above, the fastFindAllFolders() method calculated a crude count/hash per folder.
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
			while (foldersToRecurse.TryDequeue(out currentFolder)) {

				toScan = new UnixDirectoryInfo(currentFolder.getFullPath());
				dentries = toScan.GetEntries();

				numFoldersHere = 0;
				foreach (Mono.Unix.Native.Dirent de in dentries) {
					//LOG.d("Type: " + de.d_type + " " + UnixPath.Combine(root.FullName, de.d_name));
					if ( (de.d_type ^ DT_DIR) == 0 ) {
						newFolderItem = new FolderItem(de.d_name, currentFolder);
						foldersToRecurse.Enqueue( newFolderItem );
						//LOG.d("Enqueue folder...");
						currentFolder.addItem( newFolderItem );
						++numFoldersHere;
					} else {
						currentFolder.addItem( new FileItem(de.d_name,currentFolder) );
					}
				}

				currentFolder.rollUp();

//				if ( numFoldersHere == 0 ) {
//					// For 'leaf' folders, roll-up total File/Folder counts through parent folders:
//					// Note that for efficiency, we only trigger this at leaf folders.
//					currentFolder.rollUpTotalCounts();
//				}

				// Put the folder we have just scanned into the (has been) 'Recursed' queue:
				foldersRecursed.Enqueue(currentFolder);

				if ( numFoldersHere == 0 ) {
					//TODO: Current folder should be stored with a hash lookup containing the folder and file counts
					//  like "4-3" or something like that. - A folder can only be identical to another if they both have
					// the same sub-folder AND file count.
					foldersTerminals.Add(currentFolder);
				}

				//LOG.d("Folders recursed: " + foldersRecursed.Count + ", To recurse: " + foldersToRecurse.Count);
			}

			LOG.d("Failed to Dequeue any more folders. Returning.");
			return;


		}

		private void findPotentialIdenticalFoldersByItemCounts()
		{
			PotentialMatchingFoldsToScanCount = foldersRecursed.Count;

			Dictionary<string, List<FolderItem>> duplicatesOnly = new Dictionary<string, List<FolderItem>>();

			var dupKeyList = new List<string>();

			// Create a summary dictionary, containing lists of items with matching (crude) hashes:
			// This loop has no disk access, so is very fast:
			List<FolderItem> folderList;
			foreach (FolderItem fi in foldersRecursed) {
				string countHash = fi.getCountHash();
				
				if ( HashedFolderListByCounts.TryGetValue(countHash,out folderList) ) {
					folderList.Add(fi);
					// List already exists. That means we found a duplicate:
					++PotentialMatchingFoldsFoundCount;
					// We never count the first one (as it wasn't a duplicate yet).
					// But count it now we found a duplicate:
					if ( folderList.Count == 2 ) {
						++PotentialMatchingFoldsFoundCount;
						// Also now store this list in a DuplicatesOnly dictionary:
						duplicatesOnly.Add(countHash, folderList);
						dupKeyList.Add(countHash);
					}
				} else {
					folderList = new List<FolderItem>();
					folderList.Add(fi);
					HashedFolderListByCounts.Add(countHash, folderList);
				}
				++PotentialMatchingFoldsScannedCount;
			}

			LOG.d( "Found total of " + PotentialMatchingFoldsFoundCount  + " potential duplicate folders" );
			LOG.d( "Over " + duplicatesOnly.Count  + " duplicate groups" );

			// Assign the filtered list to the global:
			HashedFolderListByCounts = duplicatesOnly;


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
			foreach (string key in dupKeyList) {
				folderList = HashedFolderListByCounts[key];
				foreach(FolderItem fi in folderList) {
					potentialDupFoldersByCounts.Enqueue(fi);
				}
			}

			LOG.d("Potential dup folders:");
			foreach (FolderItem fi in potentialDupFoldersByCounts) {
				LOG.d(fi.getCountHash() + " : " + fi.getFullPath());
			}

		}

		public void filterPotentialIdenticalFoldersByFileSizeHash()
		{
			GroupedFolderSet gfs = new GroupedFolderSet();

			foreach (KeyValuePair<string, List<FolderItem>> kvp in HashedFolderListByCounts) {
				foreach (FolderItem fi in kvp.Value) {
					long hash = fi.getFileSizesHash();
					gfs.AddFolder(String.Format("{0:D}", hash), fi);
				}
			}

			gfs = gfs.WithNonDupsRemoved();

			LOG.d("New Dup Folder list via file size hashing:\n" + gfs.ToString());

		}

		public void scanFolderAndFileSizes()
		{
			FolderItem folderItem;
//			UnixDirectoryInfo di;
//			UnixFileSystemInfo[] folderItems;
//			uint totalDirs;
//			uint totalFiles;
//			ulong totalFilesSize;
			long sizeHash;
//			string hashString;

			while (foldersRecursed.TryDequeue(out folderItem)) {
				sizeHash = folderItem.getFileSizesHash();
			}

		}



	}
}

