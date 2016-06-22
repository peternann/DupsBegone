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

		// A dictionary, hashed by Folder-File count strings like "4-23",
		//  containing lists of folders with that same pattern;
		Dictionary<string, List<FolderItem>> HashedFolderListByCounts = new Dictionary<string, List<FolderItem>>();


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
			// (Simple disk access is not improved with multi-threading, so we don't)
			recurseFolders();
			TotalFoldersFound = getFoldersFoundCount();

			findPotentialIdenticalFoldersByItemCounts();
			// Potential Identical folders now in HashedFolderListByCounts.
			//TODO: Look closer at duplicates.

			/// ...

			// Now scan through the folder list looking for:
			// a) Folders that might be identical, based on number and size of files,
			// b) Files that might be identical based on file size.
			scanSizesThread = new Thread( delegate() {scanFolderAndFileSizes();});
			scanSizesThread.Start();


		}


		/// <summary>
		/// This method is used initially to get the full folder list as fast as possible in a single thread.
		/// Only after this method completes do we start a closer look at folder contents
		/// </summary>
		public void recurseFolders()
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

			List<FolderItem> folderList;
			foreach (FolderItem fi in foldersRecursed) {
				string countHash = String.Format("{0:D}-{1:D}-{2:X}",
					fi.totalFoldsRecursive, fi.totalFilesRecursive, fi.hashFFCounts);
				
				if ( HashedFolderListByCounts.TryGetValue(countHash,out folderList) ) {
					folderList.Add(fi);
					// List already exists. That means we found a duplicate:
					++PotentialMatchingFoldsFoundCount;
					// We never count the first one (as it wasn't a dupplicate yet).
					// But count it now we found a duplicate:
					if ( folderList.Count == 2 )
						++PotentialMatchingFoldsFoundCount;
				} else {
					folderList = new List<FolderItem>();
					folderList.Add(fi);
					HashedFolderListByCounts.Add(countHash, folderList);
				}
				++PotentialMatchingFoldsScannedCount;
			}

			// Now loop through the summary dictionary looking for matches:
			var filteredList = new Dictionary<string, List<FolderItem>>();
			LOG.d( "Found total of " + HashedFolderListByCounts.Count  + " Folder count signatures. Looking for dups..." );
			foreach (KeyValuePair<string, List<FolderItem>> kvp in HashedFolderListByCounts) {
				folderList = kvp.Value;
				if ( folderList.Count > 1 ) {
					LOG.d("Found " + folderList.Count + " folders with pattern: " + kvp.Key);
					filteredList.Add(kvp.Key, folderList);
				}
			}

			// Assign the filtered list to the global:
			HashedFolderListByCounts = filteredList;

		}

		public void scanFolderAndFileSizes()
		{
			FolderItem folderItem;
			UnixDirectoryInfo di;
			UnixFileSystemInfo[] folderItems;
			uint totalDirs;
			uint totalFiles;
			ulong totalFilesSize;
			ulong sizeHash;
			string hashString;

			while (foldersRecursed.TryDequeue(out folderItem)) {
				di = new UnixDirectoryInfo(folderItem.getFullPath());
				folderItems = di.GetFileSystemEntries();
				totalDirs = totalFiles = 0;
				totalFilesSize = sizeHash = 0;
				foreach (UnixFileSystemInfo fsItem in folderItems) {
					if ( fsItem.IsDirectory ) {
						++totalDirs;
					} else {
						++totalFiles;
						totalFilesSize += (ulong)fsItem.Length;
						// A simple, fast, order-independent hash of the set of file sizes:
						// (Who knows if it's any good...)
						sizeHash ^= (ulong)fsItem.Length << (int)(fsItem.Length >> 4 & 0x1F);   // 0x550 = (binary)010101010101

						//TODO: Store _file_ path indexed against the file size, for comparison in File Comparison thread.
					}
				}
				hashString = String.Format("{0:X}-{1:D}-{2:D}-{3:D}", sizeHash, totalFilesSize, totalFiles, totalDirs);
				LOG.d("Folder Hash string: " + hashString);
				//TODO: Store the path indexed by the hash, for potential folder matches.
				// Maybe we should store file info here too (while we have it)
			}

		}



	}
}

