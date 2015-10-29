using Orleans;
using OrleansGeo.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansGeo.TestSilo
{
    class Program
    {

        static async Task RunTests()
        {
            var grain = GrainClient.GrainFactory.GetGrain<IItemGrain>("item1");
            await grain.SetValueAndPosition("bar", new Position(52.0999542, 1.0969174), null);

            var search = GrainClient.GrainFactory.GetGrain<ISearchGrain>(0);
            var results = await search.Search(new Position(52.0999542, 1.0969222), 1000);

            Debug.Assert(results.Length == 1, "no reults returned");
            Debug.Assert(results[0].Key == "item1", "did not get item1");

            await grain.SetValueAndPosition("baz", new Position(51.0999542, 1.0969174), null);

            var results2 = await search.Search(new Position(52.0999542, 1.0969222), 1000);

            Debug.Assert(results2.Length == 0, "unexpected results found");

        }


        static void Main(string[] args)
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });

            Orleans.GrainClient.Initialize("DevTestClientConfiguration.xml");
            
            RunTests().Wait();

            Console.WriteLine("Tests passed!.\nPress Enter to terminate...");

            Console.ReadLine();

            hostDomain.DoCallBack(ShutdownSilo);
        }

        static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper(args);

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        static void ShutdownSilo()
        {
            if (hostWrapper != null)
            {
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }

        private static OrleansHostWrapper hostWrapper;
    }
}
