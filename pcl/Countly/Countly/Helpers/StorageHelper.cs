using PCLStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CountlySDK.Helpers
{
    internal class Storage
    {
        /// <summary>
        /// Countly folder
        /// </summary>
        private const string folder = "countly";

        public static TValue GetValue<TValue>(string key, TValue defaultvalue)
        {
            TValue value = defaultvalue;

            // If the key exists, retrieve the value.
            //if (Settings.Values.ContainsKey(key))
            //{
            //    value = (TValue)Settings.Values[key];
            //}
            //// Otherwise, use the default value.
            //else
            //{
            //    value = defaultvalue;
            //}

            return value;
        }

        public static bool SetValue(string key, object value)
        {
            bool valueChanged = false;

            //if (Settings.Values.ContainsKey(key))
            //{
            //    if (Settings.Values[key] != value)
            //    {
            //        Settings.Values[key] = value;
            //        valueChanged = true;
            //    }
            //}
            //else
            //{
            //    Settings.Values.Add(key, value);
            //    valueChanged = true;
            //}

            return valueChanged;
        }

        public static void RemoveValue(string key)
        {
            //if (Settings.Values.ContainsKey(key))
            //{
            //    Settings.Values.Remove(key);
            //}
        }

        /// <summary>
        /// Saves object into file
        /// </summary>
        /// <param name="filename">File to save to</param>
        /// <param name="objForSave">Object to save</param>
        public static async Task SaveToFile<T>(string path, object objForSave)
        {
            try
            {
                var sessionSerializer = new DataContractSerializer(typeof(T));
                MemoryStream sessionData = new MemoryStream();
                sessionSerializer.WriteObject(sessionData, objForSave);
                sessionData.Seek(0, SeekOrigin.Begin);

                await SaveStream(sessionData, path);
            }
            catch
            { }
        }

        /// <summary>
        /// Saves stream into file
        /// </summary>
        /// <param name="stream">stream to save</param>
        /// <param name="file">filename</param>
        private static async Task SaveStream(Stream stream, string file)
        {
            try
            {
                var storageFolder = await GetFolder(folder);

                var storageFile = await GetFile(file);

                using (Stream fileStream = await storageFile.OpenAsync(FileAccess.ReadAndWrite))
                {
                    await stream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                    fileStream.Dispose();
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Load object from file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filename">Filename to load from</param>
        /// <returns>Object from file</returns>
        public static async Task<T> LoadFromFile<T>(string path) where T : class
        {
            T t = null;

            try
            {
                Stream stream = await LoadStream(path);

                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var sessionSerializer = new DataContractSerializer(typeof(T));
                        T obj = (T)sessionSerializer.ReadObject(reader.BaseStream);

                        t = obj;
                    }
                }
            }
            catch
            { }

            if (t != null)
            {
                return t;
            }
            else if (!path.EndsWith(".backup"))
            {
                return await LoadFromFile<T>(path + ".backup");
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads stream from file
        /// </summary>
        /// <param name="path">filename</param>
        /// <returns>stream</returns>
        private static async Task<Stream> LoadStream(string path)
        {
            try
            {
                bool isFileExists = await FileExists(path);

                if (!isFileExists) return null;

                var storageFolder = await GetFolder(folder);

                var storageFile = await storageFolder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);

                if (storageFile == null)
                {
                    throw new Exception();
                }
                
                using (StreamReader reader = new StreamReader(await storageFile.OpenAsync(FileAccess.Read)))
                {
                    MemoryStream memoryStream = new MemoryStream();

                    reader.BaseStream.CopyTo(memoryStream);

                    memoryStream.Position = 0;

                    return memoryStream;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks whether there is a file in storage
        /// </summary>
        /// <param name="path">filename</param>
        /// <returns>true if file exists, false otherwise</returns>
        private static async Task<bool> FileExists(string path)
        {
            var storageFolder = await GetFolder(folder);

            var files = await storageFolder.GetFilesAsync();

            foreach (var file in files)
            {
                if (file.Name == path)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get StorageFile object from specified path
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns>StorageFile object</returns>
        private static async Task<IFile> GetFile(string path)
        {
            var storageFolder = await GetFolder(folder);

            var storageFile = await storageFolder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);

            return storageFile;
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="path">Filename to delete</param>
        public static async Task DeleteFile(string path)
        {
            var storageFolder = await GetFolder(folder);

            var sessionFile = await storageFolder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);

            if (sessionFile != null)
            {
                await sessionFile.DeleteAsync();
            }
        }

        /// <summary>
        /// Get StorageFolder object from specified path
        /// </summary>
        /// <param name="folder">folder path</param>
        /// <returns>StorageFolder object</returns>
        private static Task<IFolder> GetFolder(string folder)
        {
            return FileSystem.Current.LocalStorage.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
        }
    }
}
