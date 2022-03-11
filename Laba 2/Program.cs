using System.Xml.Serialization;

class Program
{
    static private Queue<FileInfo> files = new Queue<FileInfo>();

    static object RecursyEnd = new();
    static bool RecursyEndBool = false;
    static bool SchetEndBool = false;

    static void Main(string[] args)
    {
        string catalog = @"D:\Torrents";
        //XmlSerializer xmlSerializer = new XmlSerializer(typeof(string));
        //using (var stream = new StreamReader("file.xml"))
        //{
        //    catalog = (string)xmlSerializer.Deserialize(stream);
        //}
        DirectoryInfo directoryInfo = new DirectoryInfo(catalog);
        ThreadPool.QueueUserWorkItem(Obolochka, directoryInfo, true);
        ThreadPool.QueueUserWorkItem(DequeueFilesQueue);
        //for (int i = 0; i < Environment.ProcessorCount-1; i++)
        //{
        //    ThreadPool.QueueUserWorkItem(DequeueFilesQueue);
        //}
        while (!SchetEndBool && !RecursyEndBool)
        {
            Thread.Sleep(1000);
        }
        //Thread.Sleep(1000);
        Console.WriteLine("------------------Программа завершена------------------");
    }
    static void Obolochka(DirectoryInfo directoryInfo)
    {
        lock (RecursyEnd)
        {
            EnqueueFilesQueue(directoryInfo);
        }
        RecursyEndBool = true;
        Console.WriteLine("Рекурсия окончена");
    }


    static void EnqueueFilesQueue(DirectoryInfo directoryInfo)
    {
        foreach (var file in directoryInfo.GetFiles())
        {
            lock (files)
            {
                files.Enqueue(file);
            }
            Console.WriteLine(file.Name + "++++++++++++++++++++++++++++++++++++++++++++");
        }
        foreach (var directory in directoryInfo.GetDirectories())
        {
            EnqueueFilesQueue(directory);
        }
    }

    static void DequeueFilesQueue(object p)
    {
        while (true)
        {
            
            FileInfo file;
            lock (files)
            {
                if (RecursyEndBool && files.Count == 0)
                {
                    SchetEndBool = true;
                    Console.WriteLine("Счет окончен");
                    return; 
                }
                if (files.Count == 0)
                    continue;
                file = files.Dequeue();
            }
            var result = file.Length;
            Console.WriteLine($"{file.Name}----------------------------------------------");
            
        }
        
    }
}

