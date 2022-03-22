using System.Diagnostics;
using System.Xml.Serialization;

class Program
{
    static private Queue<FileInfo> files = new Queue<FileInfo>();

    static int CurrentTraversingCount = 0;
    static int CurrentSchetCount = 0;

    static void Main(string[] args)
    {
        string catalog = @"D:\bomb\MyBomber";//D:\bomb
        //string catalog = @"C:\Users\shtan\OneDrive\Рабочий стол\лаба3\лаба 3 задание 4";

        DirectoryInfo directoryInfo = new DirectoryInfo(catalog);
        CurrentTraversingCount++;

        ThreadPool.QueueUserWorkItem(Obolochka, directoryInfo, true);
        for (int i = 0; i < Environment.ProcessorCount-1; i++)
        {
            ThreadPool.QueueUserWorkItem(DequeueFilesQueue);
        }

        while (CurrentSchetCount != 0 ||
            CurrentTraversingCount != 0)
        {
            Thread.Sleep(1000);
            //Console.WriteLine("Обходов:" + CurrentTraversingCount);
            //Console.WriteLine("Счет" + CurrentSchetCount);
        }
        //Thread.Sleep(1000);
        Console.WriteLine("------------------Программа завершена------------------");
    }
    static void Obolochka(DirectoryInfo directoryInfo)
    {
        EnqueueFilesQueue(directoryInfo);
    }


    static void EnqueueFilesQueue(DirectoryInfo directoryInfo)
    {
        Queue<DirectoryInfo> queue = new Queue<DirectoryInfo>();
        queue.Enqueue(directoryInfo);
        while (queue.Count != 0)
        {
            try
            {
                foreach (var file in queue.Peek().GetFiles())
                {
                    lock (files)
                    {
                        files.Enqueue(file);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(directoryInfo + "Доступ запрещен");
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            try
            {
                foreach (var directory in queue.Dequeue().GetDirectories())
                {
                    try
                    {
                        queue.Enqueue(directory);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }
        CurrentTraversingCount--;
        
    }

    static void DequeueFilesQueue(object p)
    {
        CurrentSchetCount++;
        while (true)
        {
            
            FileInfo file;
            lock (files)
            {
                if (CurrentTraversingCount == 0 && files.Count == 0)
                {
                    CurrentSchetCount--;
                    return; 
                }
                if (files.Count == 0)
                    continue;
                file = files.Dequeue();
            }
            try
            {
                var bytes = File.ReadAllBytes(file.FullName);
                if (bytes.Length > 2)
                {
                    Console.WriteLine($"{file.Name} - {bytes[0] + bytes[1] + bytes[^1]}");
                }
                else
                {
                    Console.WriteLine($"{file.Name} - Размер слишком мал");
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"{file.Name} - Размер слишком большой");
            }
            
            
        }
        
    }
}

