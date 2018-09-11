using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;
using System.IO;

namespace CaseAnonymisation
{
    class CaseAnonymisationClass : serviceLayer
    {
        public static int count = 0;

        static void Main(string[] args)
        {
            string env = args[0];
            //string env = "sandbox1";
            IOrganizationService _orgServ = null;
            IOrganizationService _orgServRes = null;

            System.IO.FileStream ostrm;
             StreamWriter writer;
             ostrm = new FileStream("C:/CaseAnonApplication/CaseAnonymisation/Logs/Logs.txt", FileMode.OpenOrCreate, FileAccess.Write);
             writer = new StreamWriter(ostrm);

             TextWriter oldOut = Console.Out;   

            if (args.Length > 0)
            {
                ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = "";
                clientCredentials.UserName.Password = "";

                Console.WriteLine("credentials set");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                _orgServ  = (IOrganizationService)new OrganizationServiceProxy(
                    new Uri("https://" + env + ".api.crm4.dynamics.com/XRMServices/2011/Organization.svc"),
                    null, clientCredentials,null);
                _orgServRes = (IOrganizationService)new OrganizationServiceProxy(
                    new Uri("https://" + env + ".api.crm4.dynamics.com/XRMServices/2011/Organization.svc"),
                    null, clientCredentials, null);

                Thread t1 = new Thread(() => mainThread1(_orgServ, "false"));
                Thread t2 = new Thread(() => mainThread2(_orgServRes, "false"));
                Thread t3 = new Thread(() => mainThread3(_orgServ, "true"));

                try
                {
                    Console.SetOut(writer);
                    Console.Write("Anonymisation Started... \n");

                   t1.Start();
                   t2.Start();
                }

                catch (FaultException<OrganizationServiceFault> e)
                {
                    Console.WriteLine("Cannot open Logs.txt for writing");
                    Console.WriteLine(e.Message);
                    Console.Write("Exception: {0} \n Exception occured ", e.ToString());

                    Console.SetOut(oldOut);
                    writer.Close();
                    ostrm.Close();
                    
                    throw new InvalidPluginExecutionException(e.Message);
                    throw;
                }
            }
        }

        static void mainThread1(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 1 initiated");
            ActiveCases(_orgServ, "false");
            mainThread3(_orgServ, thread);
        }
       
        static void mainThread2(IOrganizationService _orgServRes, string thread)
        {
            Thread.Sleep(10000);
            Console.WriteLine("thread 2 initiated");
           ResolvedCases(_orgServRes, "false");

        }
        static void mainThread3(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 3 initiated");            
            ResolvedCases(_orgServ, "true");
        }       
    }
}

