using System.Diagnostics;
using System.Security.AccessControl;
using System.Xml.Serialization;

class Program
{
    static private Queue<FileInfo> files = new Queue<FileInfo>();

    static int CurrentTraversingCount = 0;
    static int CurrentSchetCount = 0;

    static void Main(string[] args)
    {
        string catalog = @"D:\bomb";//D:\bomb
        //string catalog = @"C:\Users\shtan\OneDrive\Рабочий стол\лаба3\лаба 3 задание 4";

        DirectoryInfo directoryInfo = new DirectoryInfo(catalog);
        CurrentTraversingCount++;

        ThreadPool.QueueUserWorkItem(EnqueueFilesQueue, directoryInfo, true);
        for (int i = 0; i < Environment.ProcessorCount - 1; i++)
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


    static void EnqueueFilesQueue(DirectoryInfo directoryInfo)
    {
        Queue<DirectoryInfo> queue = new Queue<DirectoryInfo>();
        queue.Enqueue(directoryInfo);
        while (queue.Count != 0)
        {
            //https://translated.turbopages.org/proxy_u/en-ru.ru.baae2c3f-623d2764-5dd05cde-74722d776562/https/stackoverflow.com/questions/265953/how-can-you-easily-check-if-access-is-denied-for-a-file-in-net
            //https://stackoverflow.com/questions/3507862/duplicate-getaccessrules-filesystemaccessrule-entries
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
            catch
            {
                Console.WriteLine(directoryInfo + "Доступ запрещен");
            }

            foreach (var directory in queue.Dequeue().GetDirectories())
            {
                queue.Enqueue(directory);
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
            if (file.Length > 2147483647)
            {
                Console.WriteLine($"{file.Name} - Размер слишком большой");
                continue;
            }
            else if (file.Length <= 2)
            {
                Console.WriteLine($"{file.Name} - Размер слишком мал");
            }
            else
            {
                var bytes = File.ReadAllBytes(file.FullName);
                Console.WriteLine($"{file.Name} - {bytes[0] + bytes[1] + bytes[^1]}");
            }
        }


    }

}

