using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeartWatch
{
    class Program
    {
        private static HttpListener listener;
        private static Task listenerTask;
        private static CancellationTokenSource token;

        static void Main(string[] args)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8880/");
            listener.Prefixes.Add("http://127.0.0.1:8880/");
            listener.Prefixes.Add("http://DanielsComputer:8880/");
            listener.Prefixes.Add("http://192.168.188.23:8880/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            Console.WriteLine("Creating Listener Thread");
            listener.Start();
            token = new CancellationTokenSource();
            listenerTask = new Task(startListener, token.Token, token.Token);
            listenerTask.Start();
            Console.WriteLine("Started Listener Thread");


            Console.WriteLine();
            Console.ReadLine();
            token.Cancel();
        }

        private static void startListener(object state)
        {
            var t = (CancellationToken)state;
            if (t.IsCancellationRequested)
                return;

            listener.BeginGetContext((r) => ListenerCallback(r, t), listener);
        }

        private static void ListenerCallback(IAsyncResult result, CancellationToken t)
        {
            var context = listener.EndGetContext(result);
            if (context.Request.HttpMethod == "POST")
            {
                if (context.Request.InputStream != null)
                    using (var sr = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                    {
                        var data_text = sr.ReadToEnd();
                        Console.WriteLine(data_text);
                        Console.Title = $"HeartRate:{data_text}";
                    }
                else
                {
                    Console.WriteLine("Got a null stream");
                }
            }

            context.Response.Close();
            startListener(t);
        }
    }
}
