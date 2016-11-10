using Nito.AsyncEx;
using PCLStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CountlySDK.Helpers
{
    internal class Storage
    {
        /// <summary>
        /// Countly folder
        /// </summary>
        private const string Folder = "countly";
        /// <summary>
        /// Settings file inside Countly folder
        /// </summary>
        private const string SettingsFile = "settings";
        private static Dictionary<string, string> _settings = new Dictionary<string, string>();
        private static readonly AsyncLock _settingsLock = new AsyncLock();
        private static readonly AsyncLock _fileLock = new AsyncLock();

        public static string GetValue(string key, string defaultvalue)
        {
            string value;

            //If the key exists, retrieve the value.
            string v;
            using (_settingsLock.Lock())
            {
                if (_settings.TryGetValue(key, out v))
                {
                    value = v;
                }
                else
                {
                    value = defaultvalue;
                }
            }

            return value;
        }

        public static bool SetValue(string key, string value)
        {
            bool valueChanged = false;

            string v;
            using (_settingsLock.Lock())
            {
                if (_settings.TryGetValue(key, out v))
                {
                    if (value != v)
                    {
                        _settings[key] = value;
                        valueChanged = true;
                    }
                }
                else
                {
                    _settings.Add(key, value);
                    valueChanged = true;
                }
            }

            if (valueChanged)
                WriteSettings();

            return valueChanged;
        }

        public static void RemoveValue(string key)
        {
            using (_settingsLock.Lock())
            {
                if (_settings.Remove(key))
                {
                    WriteSettings();
                }
            }
        }

        private static async void WriteSettings()
        {
            using (await _settingsLock.LockAsync())
            {
                await SaveToFile(SettingsFile, _settings);
            }
        }

        /// <summary>
        /// Needs to be called once on startup
        /// </summary>
        public static async Task ReadSettings()
        {
            using (await _settingsLock.LockAsync())
            {
                _settings = await LoadFromFile<Dictionary<string, string>>(SettingsFile) ?? new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Saves object into file
        /// </summary>
        /// <param name="filename">File to save to</param>
        /// <param name="objForSave">Object to save</param>
        public static async Task SaveToFile<T>(string path, T objForSave)
        {
            try
            {
                using (await _fileLock.LockAsync())
                {
                    var sessionSerializer = new DataContractSerializer(typeof(T));
                    var storageFolder = await GetFolder(Folder);
                    var storageFile = await storageFolder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);

                    using (var fileStream = await storageFile.OpenAsync(FileAccess.ReadAndWrite))
                    {
                        sessionSerializer.WriteObject(fileStream, objForSave);
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
                    var storageFolder = await GetFolder(Folder);
                    var storageFile = await storageFolder.GetFileAsync(path);
                    if (storageFile == null)
                        return default(T);

                    using (var fileStream = await storageFile.OpenAsync(FileAccess.Read))
                    {
                        var sessionSerializer = new DataContractSerializer(typeof(T));
                        return (T)sessionSerializer.ReadObject(fileStream);
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
            using (await _fileLock.LockAsync())
            {
                var storageFolder = await GetFolder(Folder);
                var sessionFile = await storageFolder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
                if (sessionFile != null)
                {
                    await sessionFile.DeleteAsync();
                }
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
