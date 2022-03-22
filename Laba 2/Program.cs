using System.Xml.Serialization;

class Program
{
    static private Queue<FileInfo> files = new Queue<FileInfo>();

    static int CurrentRecursyCount = 0;
    static int CurrentSchetCount = 0;

    static void Main(string[] args)
    {
        string catalog = @"D:\bomb\MyBomber";//D:\bomb
        //string catalog = @"C:\Users\shtan\OneDrive\Рабочий стол\лаба3\лаба 3 задание 4";

        DirectoryInfo directoryInfo = new DirectoryInfo(catalog);
        CurrentRecursyCount++;

        ThreadPool.QueueUserWorkItem(Obolochka, directoryInfo, true);
        for (int i = 0; i < Environment.ProcessorCount-1; i++)
        {
            ThreadPool.QueueUserWorkItem(DequeueFilesQueue);
        }

        while (CurrentSchetCount != 0 ||
            CurrentRecursyCount != 0)
        {
            Thread.Sleep(1000);
            //Console.WriteLine("Рекурсий:" + CurrentRecursyCount);
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
                foreach (var directory in queue.Dequeue().GetDirectories())
                {
                    queue.Enqueue(directory);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(directoryInfo + "Доступ запрещен");
            }
            catch (Exception e) { Console.WriteLine("FLAG!!!" + e); }
        }
        CurrentRecursyCount--;
        
    }

    static void DequeueFilesQueue(object p)
    {
        CurrentSchetCount++;
        while (true)
        {
            
            FileInfo file;
            lock (files)
            {
                if (CurrentRecursyCount == 0 && files.Count == 0)
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

