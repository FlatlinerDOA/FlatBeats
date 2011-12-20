using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.DataModel.Data
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;

    using Microsoft.Phone.Reactive;

    public class JsonFileRepository<T> : IAsyncRepository<T> where T : class
    {
        private readonly Func<T, string> getKeyFromItem;

        private readonly ManualResetEvent syncLock = new ManualResetEvent(true);

        private readonly DataContractJsonSerializer serializer;

        private bool directoryExists;

        private readonly string folderPath;

        /// <summary>
        /// Initializes a new instance of the JsonFileRepository class.
        /// </summary>
        public JsonFileRepository(Func<T, string> getKeyFromItem)
            : this(getKeyFromItem, typeof(T).Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonFileRepository class.
        /// </summary>
        public JsonFileRepository(Func<T, string> getKeyFromItem, string folderPath)
        {
            this.folderPath = folderPath;
            this.getKeyFromItem = getKeyFromItem;
            this.serializer = new DataContractJsonSerializer(typeof(T));
            
        }

        private void EnsureDirectoryExistsAsync(IsolatedStorageFile storage)
        {
            if (directoryExists)
            {
                return;
            }

            directoryExists = true;
            storage.CreateDirectory(this.folderPath);
        }

        public IObservable<Unit> SaveAsync(T item)
        {
            if (item == null)
            {
                return Observable.Return(new Unit());
            }

            return Observable.CreateWithDisposable<Unit>(
               observer => Scheduler.ThreadPool.Schedule(() =>
                {
                    try
                    {
                        var key = this.getKeyFromItem(item);
                        var filePath = this.GetFilePath(key);
                        using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            this.EnsureDirectoryExistsAsync(storage);
                            using (var stream = new IsolatedStorageFileStream(filePath, FileMode.Create, storage))
                            {
                                serializer.WriteObject(stream, item);
                                stream.Flush();
                                stream.Close();
                            }
                        }

                        observer.OnNext(new Unit());
                        observer.OnCompleted();
                    }
                    catch (IOException error)
                    {
                        observer.OnError(error);
                    }
                })).Wait(this.syncLock, TimeSpan.FromSeconds(5));
        }

        private string GetFilePath(string key)
        {
            var filePath = Path.Combine(this.folderPath, key);
            if (!filePath.EndsWith(".json"))
            {
                filePath += ".json";
            }

            return filePath;
        }

        public IObservable<Unit> DeleteAsync(T item)
        {
            return Observable.Defer(
            () =>
            {
                var key = this.getKeyFromItem(item);
                var filePath = this.GetFilePath(key);
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    this.EnsureDirectoryExistsAsync(storage);
                    if (storage.FileExists(filePath))
                    {
                        storage.DeleteFile(filePath);
                    }
                }

                return Observable.Return(new Unit());
            }).Wait(this.syncLock, TimeSpan.FromSeconds(5));
        }

        public IObservable<T> GetAsync(string key)
        {
            if (key == null)
            {
                return Observable.Empty<T>();
            }

            return Observable.Defer<T>(
                () =>
                {
                    var filePath = this.GetFilePath(key);
                    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        this.EnsureDirectoryExistsAsync(storage);
                        if (!storage.FileExists(filePath))
                        {
                            return null;
                        }

                        using (var stream = new IsolatedStorageFileStream(filePath, FileMode.Open, storage))
                        {
                            return Observable.Return(this.serializer.ReadObject(stream) as T);
                        }
                    }
                }).Wait(this.syncLock, TimeSpan.FromSeconds(5)).Where(t => t != null);
        }

        public IObservable<T> GetAllAsync()
        {
            return this.GetAll().ToObservable(Scheduler.ThreadPool).Wait(this.syncLock, TimeSpan.FromSeconds(5)).Where(t => t != null);
        }

        private IEnumerable<T> GetAll()
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                this.EnsureDirectoryExistsAsync(storage);

                foreach (var fileName in storage.GetFileNames(folderPath))
                {
                    using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Open, storage))
                    {
                        var item = this.serializer.ReadObject(stream) as T;
                        if (item != null)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }
}
