using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;

namespace CaseAnonymisation
{
    class CaseAnonymisationClass : serviceLayer
    {
        public static int count = 0;

        static void Main(string[] args)
        {
            string env = args[0];
           // string env = "";

           FileStream ostrm;
            StreamWriter writer;
            ostrm = new FileStream("C:/CaseAnonApplication/CaseAnonymisation/Logs/Logs.txt", FileMode.OpenOrCreate, FileAccess.Write);
            writer = new StreamWriter(ostrm);

            TextWriter oldOut = Console.Out;

            FileStream exStream;
            StreamWriter exWrite;
            exStream = new FileStream("C:/CaseAnonApplication/CaseAnonymisation/Logs/Exceptions.txt", FileMode.OpenOrCreate, FileAccess.Write);
            exWrite = new StreamWriter(exStream);

            if (args.Length > 0)
            {
                AuthenticationCredentials authCredentials = new AuthenticationCredentials();
                Uri organizationUri = new Uri("https://shgl-" + env + ".api.crm4.dynamics.com/XRMServices/2011/Organization.svc");
                authCredentials.ClientCredentials.UserName.UserName = ""; 
                authCredentials.ClientCredentials.UserName.Password = ""; 
                ClientCredentials credentials = authCredentials.ClientCredentials;
                OrganizationServiceProxy proxy = new OrganizationServiceProxy(organizationUri, null, credentials, null);
                proxy.Timeout = TimeSpan.FromMinutes(5000);
                proxy.EnableProxyTypes();
                IOrganizationService _orgServ = (IOrganizationService)proxy;
                OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);


                Thread t1 = new Thread(() => mainThread1(_orgServ, "false"));
               // Thread t2 = new Thread(() => mainThread2(_orgServ, "true"));
               // Thread t3 = new Thread(() => mainThread3(_orgServ, "false"));
               // Thread t4 = new Thread(() => mainThread4(_orgServ, "true"));

                Thread t5 = new Thread(() => mainThread5(_orgServ, "false"));
                Thread t6 = new Thread(() => mainThread6(_orgServ, "true"));
               // Thread t7 = new Thread(() => mainThread7(_orgServ, "false"));
               // Thread t8 = new Thread(() => mainThread8(_orgServ, "true"));

                try
                {
                    Console.SetOut(writer);
                    Console.Write("Anonymisation Started... \n");

                    t1.Start();
                   // t2.Start();
                    //t3.Start();
                    //t4.Start();
                    
                    t5.Start();
                   // t6.Start();
                    //t7.Start();
                    //t8.Start();
                }

                catch (FaultException<OrganizationServiceFault> e)
                {
                   // Console.SetOut(exWrite);
                    Console.WriteLine("Cannot open Logs.txt for writing");
                    Console.WriteLine(e.Message);
                    Console.Write("Exception: {0} \n Exception occured ", e.ToString());

                    Console.SetOut(oldOut);
                     writer.Close();
                     ostrm.Close();
                     exStream.Close();
                     exWrite.Close();
                    throw new InvalidPluginExecutionException(e.Message);
                    throw;
                }
            }
        }

        static void mainThread1(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 1 initiated");
            ActiveCases(_orgServ, "false");
            mainThread6(_orgServ, thread);

        }
        static void mainThread2(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 2 initiated");
            ActiveCases(_orgServ, "true");
        }
        static void mainThread3(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 3 initiated");
            ActiveCases(_orgServ, "false");

        }
        static void mainThread4(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 4 initiated");
            ActiveCases(_orgServ, "true");
        }
        static void mainThread5(IOrganizationService _orgServ, string thread)
        {
            Thread.Sleep(300000);
            Console.WriteLine("thread 5 initiated");
           ResolvedCases(_orgServ, "false");

        }
        static void mainThread6(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 6 initiated");            
            ResolvedCases(_orgServ, "true");
        }
        static void mainThread7(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 7 initiated");
            ResolvedCases(_orgServ, "false");

        }
        static void mainThread8(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 8 initiated");
            ResolvedCases(_orgServ, "true");
        }
    }
}

