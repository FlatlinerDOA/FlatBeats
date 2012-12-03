////namespace FlatBeats.DataModel.Data
////{
////    using System;
////    using System.Collections.Generic;
////    using System.IO;
////    using System.IO.IsolatedStorage;
////    using System.Threading;

////    using Flatliner.Functional;
////    using Flatliner.Phone;

////    using Microsoft.Phone.Reactive;

////    using ProtoBuf;

////    public sealed class ProtobufFileRepository<T> : IAsyncRepository<T> where T : class
////    {
////        private readonly Func<T, string> getKeyFromItem;

////        private readonly ManualResetEvent syncLock = new ManualResetEvent(true);

////        private bool directoryExists;

////        private readonly string folderPath;

////        private const PrefixStyle Prefix = PrefixStyle.Fixed32;

////        /// <summary>
////        /// Initializes a new instance of the JsonFileRepository class.
////        /// </summary>
////        public ProtobufFileRepository(Func<T, string> getKeyFromItem)
////            : this(getKeyFromItem, typeof(T).Name)
////        {
////        }

////        /// <summary>
////        /// Initializes a new instance of the JsonFileRepository class.
////        /// </summary>
////        public ProtobufFileRepository(Func<T, string> getKeyFromItem, string folderPath)
////        {
////            this.folderPath = folderPath;
////            this.getKeyFromItem = getKeyFromItem;
////        }

////        private void EnsureDirectoryExistsAsync(IsolatedStorageFile storage)
////        {
////            if (directoryExists)
////            {
////                return;
////            }

////            directoryExists = true;
////            storage.CreateDirectory(this.folderPath);
////        }

////        public IObservable<PortableUnit> SaveAsync(T item)
////        {
////            if (item == null)
////            {
////                return ObservableEx.SingleUnit();
////            }

////            return Observable.CreateWithDisposable<PortableUnit>(
////               observer => Scheduler.ThreadPool.Schedule(() =>
////                {
////                    try
////                    {
////                        var key = this.getKeyFromItem(item);
////                        var filePath = this.GetFilePath(key);
////                        using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
////                        {
////                            this.EnsureDirectoryExistsAsync(storage);
////                            using (var stream = new IsolatedStorageFileStream(filePath, FileMode.Create, storage))
////                            {
////                                Serializer.SerializeWithLengthPrefix(stream, item, Prefix);
////                                stream.Flush();
////                                stream.Close();
////                            }
////                        }

////                        observer.OnNext(ObservableEx.Unit);
////                        observer.OnCompleted();
////                    }
////                    catch (IOException error)
////                    {
////                        observer.OnError(error);
////                    }
////                })).Wait(this.syncLock, TimeSpan.FromSeconds(5));
////        }

////        private string GetFilePath(string key)
////        {
////            var filePath = Path.Combine(this.folderPath, key);
////            if (!filePath.EndsWith(".pb"))
////            {
////                filePath += ".pb";
////            }

////            return filePath;
////        }

////        public IObservable<PortableUnit> DeleteAsync(T item)
////        {
////            return Observable.Defer(
////            () =>
////            {
////                var key = this.getKeyFromItem(item);
////                var filePath = this.GetFilePath(key);
////                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
////                {
////                    this.EnsureDirectoryExistsAsync(storage);
////                    if (storage.FileExists(filePath))
////                    {
////                        storage.DeleteFile(filePath);
////                    }
////                }

////                return ObservableEx.SingleUnit();
////            }).Wait(this.syncLock, TimeSpan.FromSeconds(5));
////        }

////        public IObservable<T> GetAsync(string key)
////        {
////            if (key == null)
////            {
////                return Observable.Empty<T>();
////            }

////            return Observable.Defer(
////                () =>
////                {
////                    var filePath = this.GetFilePath(key);
////                    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
////                    {
////                        this.EnsureDirectoryExistsAsync(storage);
////                        if (!storage.FileExists(filePath))
////                        {
////                            return null;
////                        }

////                        using (var stream = new IsolatedStorageFileStream(filePath, FileMode.Open, storage))
////                        {
////                            return Observable.Return(Serializer.DeserializeWithLengthPrefix<T>(stream, Prefix));
////                        }
////                    }
////                }).Wait(this.syncLock, TimeSpan.FromSeconds(5)).Where(t => t != null);
////        }

////        public IObservable<T> GetAllAsync()
////        {
////            return this.GetAll().ToObservable(Scheduler.ThreadPool).Wait(this.syncLock, TimeSpan.FromSeconds(5)).Where(t => t != null);
////        }

////        private IEnumerable<T> GetAll()
////        {
////            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
////            {
////                this.EnsureDirectoryExistsAsync(storage);

////                foreach (var fileName in storage.GetFileNames(folderPath))
////                {
////                    using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Open, storage))
////                    {
////                        var item = Serializer.DeserializeWithLengthPrefix<T>(stream, Prefix);
////                        if (item != null)
////                        {
////                            yield return item;
////                        }
////                    }
////                }
////            }
////        }
////    }
////}
