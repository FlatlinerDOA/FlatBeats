namespace FlatBeats.DataModel
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Text;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;

    public static class Storage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool Exists(string file)
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
        private static void Save(string file, string text)
        {
            if (file == null)
            {
                return;
            }

            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                CreateFolderForFile(storage, file);

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

        public static IObservable<Unit> SaveJsonAsync<T>(string file, T data) where T : class
        {
            return ObservableEx.DeferredStart(
                () =>
                {
                    var json = Json<T>.Serialize(data);
                    Save(file, json);
                }, Scheduler.ThreadPool);
        }

        public static IObservable<Unit> SaveStringAsync(string file, string text)
        {
            return ObservableEx.DeferredStart(() => Save(file, text), Scheduler.ThreadPool);
        }

        public static IObservable<T> LoadJsonAsync<T>(string file) where T : class
        {
            return ObservableEx.DeferredStart(() =>
            {
                var json = Load(file);
                return Json<T>.Deserialize(json);
            }, Scheduler.ThreadPool);
        }

        public static IObservable<string> LoadStringAsync(string file)
        {
            return ObservableEx.DeferredStart(() => Load(file), Scheduler.ThreadPool);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string Load(string file)
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

        public static void Save(string file, Stream data)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                CreateFolderForFile(storage, file);
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

        public static void Delete(string file)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storage.DeleteFile(file);
            }
        }

        private static void CreateFolderForFile(IsolatedStorageFile storage, string filePath)
        {
            var folder = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }
            
            storage.CreateDirectory(folder);
        }

        public static Stream ReadStream(string filePath)
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
