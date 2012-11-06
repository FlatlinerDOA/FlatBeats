namespace FlatBeats.DataModel.Services
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Text;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;
    using Flatliner.Functional;

    public sealed class AsyncIsolatedStorage : IAsyncStorage
    {
        private readonly object syncRoot = new object();

        public static readonly IAsyncStorage Instance = new AsyncIsolatedStorage();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool Exists(string file)
        {
            if (file == null)
            {
                return false;
            }

            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return storage.FileExists(file);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="text"></param>
        private void Save(string file, string text)
        {
            if (file == null)
            {
                return;
            }

            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                this.CreateFolderForFile(storage, file);

                using (var stream = new IsolatedStorageFileStream(file, FileMode.Create, storage))
                {
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(text);
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
        }

        public IObservable<PortableUnit> SaveJsonAsync<T>(string file, T data) where T : class
        {
            return ObservableEx.DeferredStart(
                () =>
                {
                    lock (this.syncRoot)
                    {
                        using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            this.CreateFolderForFile(storage, file);
                            using (IsolatedStorageFileStream fileStream = storage.CreateFile(file))
                            {
                                Json<T>.Instance.SerializeToStream(data, fileStream);
                            }
                        }
                    }
                }, 
                Scheduler.ThreadPool);
        }

        public IObservable<PortableUnit> SaveStringAsync(string file, string text)
        {
            return ObservableEx.DeferredStart(() => this.Save(file, text), Scheduler.ThreadPool);
        }

        public IObservable<T> LoadJsonAsync<T>(string filePath) where T : class
        {
            return ObservableEx.DeferredStart(() =>
            {
                lock (this.syncRoot)
                {
                    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!storage.FileExists(filePath))
                        {
                            return default(T);
                        }

                        using (
                            var input = new IsolatedStorageFileStream(
                                filePath, FileMode.Open, FileAccess.Read, FileShare.Read, storage))
                        {
                            return Json<T>.Instance.DeserializeFromStream(input);
                        }
                    }
                }
            }, 
            Scheduler.ThreadPool);
        }

        public IObservable<string> LoadStringAsync(string file)
        {
            return ObservableEx.DeferredStart(() => this.Load(file), Scheduler.ThreadPool);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string Load(string file)
        {
            if (file == null)
            {
                return null;
            }

            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.FileExists(file))
                {
                    return null;
                }

                using (var stream = new IsolatedStorageFileStream(file, FileMode.Open, storage))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        private const int ChunkSize = 4096;

        public void Save(string file, Stream data)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                this.CreateFolderForFile(storage, file);
                using (IsolatedStorageFileStream fileStream = storage.CreateFile(file))
                {
                    byte[] bytes = new byte[ChunkSize];
                    int byteCount;
                    while ((byteCount = data.Read(bytes, 0, ChunkSize)) > 0)
                    {
                        fileStream.Write(bytes, 0, byteCount);
                    }

                    fileStream.Flush();
                    fileStream.Close();
                }
            }
        }

        public void Delete(string file)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storage.DeleteFile(file);
            }
        }

        private void CreateFolderForFile(IsolatedStorageFile storage, string filePath)
        {
            var folder = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }
            
            storage.CreateDirectory(folder);
        }

        public Stream ReadStream(string filePath)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.FileExists(filePath))
                {
                    return null;
                }

                using (var input = new IsolatedStorageFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, storage))
                {
                    var output = new MemoryStream();
                    input.CopyTo(output);
                    input.Close();
                    output.Position = 0;
                    return output;
                }
            }
        }
    }
}
