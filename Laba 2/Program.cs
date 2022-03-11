using System.Xml.Serialization;

class Program
{
    static private Queue<FileInfo> files = new Queue<FileInfo>();

    static int CurrentRecursyCount = 0;
    static int CurrentSchetCount = 0;
    readonly static int DesiredNumberOfThreads = Environment.ProcessorCount / 2;

    static void Main(string[] args)
    {
        string catalog = @"D:\bomb";

        DirectoryInfo directoryInfo = new DirectoryInfo(catalog);
        CurrentRecursyCount++;

        ThreadPool.QueueUserWorkItem(Obolochka, directoryInfo, true);
        for (int i = 0; i < DesiredNumberOfThreads; i++)
        {
            ThreadPool.QueueUserWorkItem(DequeueFilesQueue);
        }

        while (CurrentSchetCount != 0 ||
            CurrentRecursyCount != 0)
        {
            Thread.Sleep(1000);
            Console.WriteLine("Рекурсий:" + CurrentRecursyCount);
            Console.WriteLine("Счет" + CurrentSchetCount);
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
        try
        {
            foreach (var file in directoryInfo.GetFiles())
            {
                lock (files)
                {
                    files.Enqueue(file);
                }
            }
            foreach (var directory in directoryInfo.GetDirectories())
            //!!!Насколько корректно для каждого узла дерева создавать отдельный поток
            {
                ThreadPool.QueueUserWorkItem(EnqueueFilesQueue, directory, true);
                CurrentRecursyCount++;
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine(directoryInfo + "Доступ запрещен");
        }
        catch(Exception e) { Console.WriteLine("FLAG!!!" + e); }
        finally
        {
            CurrentRecursyCount--;
        }
        
        
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

