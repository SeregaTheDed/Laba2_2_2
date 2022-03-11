using System.Xml.Serialization;

class Program
{
    static private Queue<FileInfo> files = new Queue<FileInfo>();

    static int RecursyEndBool = 0;
    static int SchetEndBool = 0;
    readonly static int DesiredNumberOfThreads = Environment.ProcessorCount / 2;

    static void Main(string[] args)
    {
        string catalog = @"D:\Torrents";
        //XmlSerializer xmlSerializer = new XmlSerializer(typeof(string));
        //using (var stream = new StreamReader("file.xml"))
        //{
        //    catalog = (string)xmlSerializer.Deserialize(stream);
        //}
        DirectoryInfo directoryInfo = new DirectoryInfo(catalog);
        //ThreadPool.QueueUserWorkItem(Obolochka, directoryInfo, true);
        for (int i = 0; i < DesiredNumberOfThreads; i++)
        {
            ThreadPool.QueueUserWorkItem(Obolochka, directoryInfo, true);
        }
        //ThreadPool.QueueUserWorkItem(DequeueFilesQueue);
        for (int i = 0; i < DesiredNumberOfThreads; i++)
        {
            ThreadPool.QueueUserWorkItem(DequeueFilesQueue);
        }
        while (SchetEndBool != DesiredNumberOfThreads && 
            RecursyEndBool != DesiredNumberOfThreads)
        {
            Thread.Sleep(1000);
        }
        //Thread.Sleep(1000);
        Console.WriteLine("------------------Программа завершена------------------");
    }
    static void Obolochka(DirectoryInfo directoryInfo)
    {
        EnqueueFilesQueue(directoryInfo);
        RecursyEndBool++;
        //Console.WriteLine("Рекурсия окончена");
    }


    static void EnqueueFilesQueue(DirectoryInfo directoryInfo)
    {
        foreach (var file in directoryInfo.GetFiles())
        {
            lock (files)
            {
                files.Enqueue(file);
            }
            //Console.WriteLine(file.Name);
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
                if (RecursyEndBool != 0 && files.Count == 0)
                {
                    SchetEndBool++;
                    //Console.WriteLine("Счет окончен");
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

