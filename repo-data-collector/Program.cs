using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace repo_data_collector
{
    class Worker
    {
        public static string OAuthKey = "";

        public static BlockingCollection<string> Repositories = new BlockingCollection<string>(new ConcurrentQueue<string>());  

        public static ConcurrentDictionary<string, string> RepositoryStatistics = new ConcurrentDictionary<string, string>();
  
        private WebClient WebClient { get; set; }

        public Worker()
        {
            this.WebClient = new WebClient();
        }

        public void Run()
        {
            while (!Repositories.IsCompleted)
            {
                string repository;
                if (Repositories.TryTake(out repository))
                {
                    RepositoryStatistics.TryAdd(repository, this.GetStatistics(repository));
                }
            }
        }

        public string GetStatistics(string repository)
        {
            var url = $"https://api.github.com/repos/{repository}&token={OAuthKey}";
            Console.WriteLine($"Fetching url: {url}");

            string result;
            try
            {
                result = this.WebClient.DownloadString(url);
            }
            catch (WebException)
            {
                result = "failed to fetch results";
            }

            return result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Need a CSV.");
                Environment.Exit(1);
            }

            Console.WriteLine("Paste a valid GitHub OAuth access token (or else everything will fail.):");
            string oAuthKey = Console.ReadLine();

            if (oAuthKey == null || oAuthKey.Length != 40)
            {
                Console.WriteLine("That doesn't look like a valid access token. Exiting.");
                Environment.Exit(1);
            }

            Worker.OAuthKey = oAuthKey;

            Console.WriteLine("Creating pool of 10 worker threads.");
            List<Thread> threadPool = new List<Thread>();
            for (int i = 0; i < 10; i++)
            {
                Worker worker = new Worker();
                Thread workerThread = new Thread(new ThreadStart(worker.Run));
                workerThread.Start();
                threadPool.Add(workerThread);
            }

            Console.WriteLine("Adding work from CSV.");
            using (TextFieldParser parser = new TextFieldParser(args[0]))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    Worker.Repositories.Add(fields[0]);
                }
            }

            Console.WriteLine("Completed adding work.");
            Worker.Repositories.CompleteAdding();

            foreach (Thread workerThread in threadPool)
            {
                workerThread.Join();
            }

            Console.WriteLine("Work is completed.");

            foreach (KeyValuePair<string, string> keyValuePair in Worker.RepositoryStatistics)
            {
                Console.WriteLine("Result for {0}: {1}", keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
