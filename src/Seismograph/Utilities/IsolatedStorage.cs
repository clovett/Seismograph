﻿// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Seismograph
{

    /// <summary>
    /// Isolated storage file helper class
    /// </summary>
    /// <typeparam name="T">Data type to serialize/deserialize</typeparam>
    public class IsolatedStorage<T>
    {
        CacheFolder cacheFolder;

        public IsolatedStorage(CacheFolder cacheFolder)
        {
            this.cacheFolder = cacheFolder;
        }

        /// <summary>
        /// Loads data from a file asynchronously.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <returns>Deserialized data object</returns>
        public async Task<T> LoadFromFileAsync(string fileName)
        {
            T loadedFile = default(T);

            try
            {
                StorageFile storageFile = await cacheFolder.LoadFileAsync(fileName);
                if (storageFile != null)
                {
                    using (Stream myFileStream = await storageFile.OpenStreamForReadAsync())
                    {
                        // Call the Deserialize method and cast to the object type.
                        return LoadFromStream(myFileStream);
                    }
                }
            }
            catch
            {
                // silently rebuild data file if it got corrupted.
            }
            return loadedFile;
        }

        public T LoadFromStream(Stream s)
        {
            // Call the Deserialize method and cast to the object type.
            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            return (T)mySerializer.Deserialize(s);
        }

        /// <summary>
        /// Saves data to a file.
        /// </summary>
        /// <param name="fileName">Name of the file to write to</param>
        /// <param name="data">The data to save</param>
        public async Task SaveToFileAsync(string fileName, T data)
        {
            using (var stream = await cacheFolder.SaveFileAsync(fileName))
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(T));
                mySerializer.Serialize(stream.AsStreamForWrite(), data);
            }
        }

    }
}
