﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Seismograph
{
    /// <summary>
    /// This class manages cached files in a folder 
    /// </summary>
    public class CacheFolder
    {
        private string folder;
        private StorageFolder storageFolder;
        private Dictionary<string, StorageFile> files = new Dictionary<string, StorageFile>(StringComparer.OrdinalIgnoreCase);

        public CacheFolder(string folder)
        {                        
            this.folder = folder;
        }

        private async Task PopulateCache()
        {
            var appData = ApplicationData.Current;
            var localSettings = appData.LocalSettings;
            var localFolder = appData.LocalFolder;

            try
            {
                storageFolder = await localFolder.GetFolderAsync(this.folder);
            }
            catch
            {
                storageFolder = null;
            }
            if (storageFolder == null)
            {
                storageFolder = await localFolder.CreateFolderAsync(this.folder);
            }
        
            foreach (StorageFile file in await this.storageFolder.GetFilesAsync()) 
            {
                files[file.Name] = file;
            }
        }

        /// <summary>
        /// Find out if a given file is in the cache.  
        /// </summary>
        /// <param name="filename">The name of the file to find</param>
        /// <returns>Returns true if the file is in the cache</returns>
        public bool FileExists(string filename) 
        {
            return files.ContainsKey(filename);
        }

        /// <summary>
        /// Download the given Uri and add it to the cache (replacing any existing file).
        /// The cached file is given the file name based on the last segment of the Uri.  For example,
        /// http://www.lovettsoftware.com/videos/DgmlTestModel.mp4 would be stored in the file "DgmlTestModel.mp4"
        /// </summary>
        /// <param name="uri">The Uri to download</param>
        /// <returns>The local cached file</returns>
        public async Task<StorageFile> DownloadFileAsync(Uri uri)
        {
            await PopulateCache();
            string name = uri.Segments[uri.Segments.Length-1];            
            StorageFile storageFile = await StorageFile.CreateStreamedFileFromUriAsync(name, uri, null);
            files[storageFile.Name] = storageFile;
            return storageFile;
        }


        /// <summary>
        /// Load the given file from the cache
        /// </summary>
        /// <param name="fileName">The local name of the file to load</param>
        /// <returns>The local cached file or null if it is not in the cache</returns>
        public async Task<StorageFile> LoadFileAsync(string fileName)
        {
            await PopulateCache();
            if (files.ContainsKey(fileName))
            {
                return files[fileName];
            }
            return null;
        }


        /// <summary>
        /// Save the given file to the cache using the given stream for data.
        /// </summary>
        /// <param name="fileName">The local name of the file to load</param>
        /// <returns>The stream to write to</returns>
        public async Task<IRandomAccessStream> SaveFileAsync(string fileName)
        {
            await PopulateCache();
            
            StorageFile storageFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);            
            files[storageFile.Name] = storageFile;
            return fileStream;
        }
    }
}
