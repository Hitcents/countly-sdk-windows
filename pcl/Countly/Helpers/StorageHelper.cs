using Newtonsoft.Json;
using Nito.AsyncEx;
using PCLStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CountlySDK.Helpers
{
    internal class Storage
    {
        /// <summary>
        /// Countly folder
        /// </summary>
        private const string Folder = "countly";
        private static readonly AsyncLock _fileLock = new AsyncLock();

        /// <summary>
        /// Saves object into file
        /// </summary>
        /// <param name="filename">File to save to</param>
        /// <param name="obj">Object to save</param>
        public static async Task SaveToFile<T>(string path, T obj)
        {
            try
            {
                using (await _fileLock.LockAsync())
                {
                    var folder = await GetFolder(Folder);
                    var file = await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);

                    using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite))
                    using (var writer = new StreamWriter(stream))
                    using (var json = new JsonTextWriter(writer))
                    {
                        var serializer = JsonSerializer.Create(Api.JsonSettings);
                        serializer.Serialize(json, obj);
                    }
                }
            }
            catch (Exception exc)
            {
                Countly.Log(exc);
            }
        }

        /// <summary>
        /// Load object from file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filename">Filename to load from</param>
        /// <returns>Object from file</returns>
        public static async Task<T> LoadFromFile<T>(string path) where T : class
        {
            try
            {
                using (await _fileLock.LockAsync())
                {
                    var folder = await GetFolder(Folder);
                    var file = await folder.GetFileAsync(path);

                    using (var fileStream = await file.OpenAsync(FileAccess.Read))
                    using (var reader = new StreamReader(fileStream))
                    using (var json = new JsonTextReader(reader))
                    {
                        var serializer = JsonSerializer.Create(Api.JsonSettings);
                        return (T)serializer.Deserialize(json, typeof(T));
                    }
                }
            }
            catch (FileNotFoundException) { }
            catch (Exception exc)
            {
                Countly.Log(exc);
            }

            return default(T);
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="path">Filename to delete</param>
        public static async Task DeleteFile(string path)
        {
            try
            {
                using (await _fileLock.LockAsync())
                {
                    var folder = await GetFolder(Folder);
                    var file = await folder.GetFileAsync(path);
                    await file.DeleteAsync();
                }
            }
            catch (FileNotFoundException) { }
            catch (Exception exc)
            {
                Countly.Log(exc);
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
