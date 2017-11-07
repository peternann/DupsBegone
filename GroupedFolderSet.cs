using System;
using System.Collections.Generic;
using System.Text;

namespace DupsBegone
{
	public class GroupedFolderSet : Dictionary<string, List<FolderItem>>
	{
		public GroupedFolderSet() : base()
		{
		}

		public int AddFolder( string hash, FolderItem folder )
		{
			List<FolderItem> folderList;
			if ( this.TryGetValue(hash, out folderList) ) {
				folderList.Add(folder);
			} else {
				folderList = new List<FolderItem>();
				folderList.Add(folder);
				this.Add(hash, folderList);
			}
			return folderList.Count;
		}

		/// <summary>
		/// Return a form of this object with non-duplicates removed.
		/// </summary>
		/// <returns>The non dups removed.</returns>
		public GroupedFolderSet WithNonDupsRemoved()
		{
			var gfs = new GroupedFolderSet();
			foreach (KeyValuePair<string,List<FolderItem>> kvp in this) {
				if ( kvp.Value.Count > 1 )
					gfs.Add(kvp.Key, kvp.Value);
			}
			return gfs;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			List<FolderItem> folderList;
			foreach (KeyValuePair<string,List<FolderItem>> kvp in this) {
				folderList = kvp.Value;
				sb.AppendLine("Group:" + kvp.Key );
				foreach (FolderItem fi in folderList) {
					sb.AppendLine("..Item Path:" + fi.getFullPath());
				}
			}

			return sb.ToString();
		}
			
	}
}

