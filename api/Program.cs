using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Webapi_starter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .Build();
            
            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel(o =>
                {
                    o.UseHttps(
                        Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).FullName, "SSLCertificate.pfx"),
                        "Passw0rd");
                })
                .UseUrls("http://127.0.0.1:5000", "https://127.0.0.1:44396")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
