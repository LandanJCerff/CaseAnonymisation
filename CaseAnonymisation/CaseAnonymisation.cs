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
  
            FileStream ostrm;
            StreamWriter writer;
            ostrm = new FileStream("C:/CaseAnonApplication/CaseAnonymisation/Logs/Logs.txt", FileMode.OpenOrCreate, FileAccess.Write);
            writer = new StreamWriter(ostrm);

            TextWriter oldOut = Console.Out;
           
            if (args.Length > 0)
            {
                AuthenticationCredentials authCredentials = new AuthenticationCredentials();
                Uri organizationUri = new Uri("https://" + env + ".api.crm4.dynamics.com/XRMServices/2011/Organization.svc");
                authCredentials.ClientCredentials.UserName.UserName = ""; 
                authCredentials.ClientCredentials.UserName.Password = ""; 
                ClientCredentials credentials = authCredentials.ClientCredentials;
                OrganizationServiceProxy proxy = new OrganizationServiceProxy(organizationUri, null, credentials, null);
                proxy.Timeout = TimeSpan.FromMinutes(5000);
                proxy.EnableProxyTypes();
                IOrganizationService _orgServ = (IOrganizationService)proxy;
                OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);


                Thread t1 = new Thread(() => mainThread1(_orgServ, "false"));          
                Thread t2 = new Thread(() => mainThread2(_orgServ, "false"));
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
            mainThread6(_orgServ, thread);
        }       
       
        static void mainThread2(IOrganizationService _orgServ, string thread)
        {
            Thread.Sleep(300000);
            Console.WriteLine("thread 2 initiated");
           ResolvedCases(_orgServ, "false");

        }
        
        static void mainThread3(IOrganizationService _orgServ, string thread)
        {
            Console.WriteLine("thread 3 initiated");            
            ResolvedCases(_orgServ, "true");
        }
    }
}

