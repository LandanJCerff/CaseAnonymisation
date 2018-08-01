using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CaseAnonymisation
{
    class serviceLayer : dataLayer
    {
        public static void ActiveCases(IOrganizationService _orgServ, string thread)
        {
            OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);
            string ticket = string.Empty;
        start:
            EntityCollection retrievedCases = GetActiveCases(_orgServ, thread);

            if (retrievedCases.Entities.Count != 0)
            {
                try
                {                   
                    for (int i = 0; i < retrievedCases.Entities.Count; i++)
                    {
                        Incident myCase = (Incident)retrievedCases[i];
                        ticket = myCase.TicketNumber;

                        Console.Write("Case No.: " + ticket + " Updating... \n");
                        myCase.Description = "XXXX XXXXXXXX, XXXX XXXX.";
                        myCase.Title = "XXXXX XX. XXX XXXXX XX XXXX.";
                        myCase.gcs_resolutiondescription = "XXXX XXXXXXX! XX XXXXX XXX; X XXXX XXX.";


                        _orgContext.Attach(myCase);
                        _orgContext.UpdateObject(myCase);
                        _orgContext.SaveChanges();

                        Console.WriteLine("\n Case " + ticket + " updated. \n");

                        activityUpdate(_orgServ, myCase.Id);
                    }
                    _orgContext?.Dispose();
                    retrievedCases.Entities.Clear();
                }
                catch (FaultException<OrganizationServiceFault> e)
                {                   
                    Console.WriteLine("Exception: occured on Case: " + ticket + "\n");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                return;
            }                       
                goto start;           
        }

        public static void ResolvedCases(IOrganizationService _orgServ, string thread)
        {
            OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);
            string ticket = string.Empty;
        start:
            EntityCollection retrievedCases = GetResolvedCases(_orgServ, thread);

            if (retrievedCases.Entities.Count != 0)
            {
                try
                {                   
                    for (int r = 0; r < retrievedCases.Entities.Count; r++)
                    {
                        Incident myCase = (Incident)retrievedCases[r];
                        int? statusReason = (int?)myCase.StatusCode.Value;

                        ticket = myCase.TicketNumber;
                        Console.Write("Resolved Case No.: " + ticket + " Updating \n");
                        myCase.Description = "XXXX XXXXXXXX, XXXX XXXX.";
                        myCase.Title = "XXXXX XX. XXX XXXXX XX XXXX.";
                        myCase.gcs_resolutiondescription = "XXXX XXXXXXX! XX XXXXX XXX; X XXXX XXX.";
                        myCase.StateCode = 0;
                        myCase.StatusCode = (incident_statuscode)1;

                        _orgServ.Update(myCase);

                        Entity incidentResolution = new Entity("incidentresolution");
                        incidentResolution.Attributes.Add("incidentid", new EntityReference("incident", (Guid)myCase.Id));
                        Console.WriteLine("\n Resolved Case " + ticket + " updated. \n");

                        var closeIncidentRequest = new CloseIncidentRequest
                        {
                            IncidentResolution = incidentResolution,
                            Status = new OptionSetValue((int)statusReason),
                        };

                        activityUpdate(_orgServ, myCase.Id);
                        CloseIncidentResponse closeResponse = (CloseIncidentResponse)_orgServ.Execute(closeIncidentRequest);
                    }
                }
                catch (FaultException<OrganizationServiceFault> e)
                {
                    Console.WriteLine("Exception: occured on Case: " + ticket + "\n");
                    Console.WriteLine(e.Message);
                }
                _orgContext?.Dispose();
                retrievedCases.Entities.Clear();
            }
            else
            {
                return; 
            }
            goto start;
        }

        public static void activityUpdate(IOrganizationService _orgServ, Guid CaseId)
        {
            OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);
            EntityCollection retrievedActivities = GetActivities(_orgServ, CaseId);

            foreach (Entity entity in retrievedActivities.Entities)
            {
                string logicalName = entity.Attributes["activitytypecode"].ToString();
                Console.Write("Updating Activity Type: " + logicalName.ToString() + ", \n");
                Guid emailid = (Guid)entity.Attributes["activityid"];
                Entity activity = new Entity();

                if (logicalName == "email")
                {
                    SetStateRequest setStateRequest = new SetStateRequest()
                    {
                        EntityMoniker = new EntityReference
                        {
                            Id = emailid,
                            LogicalName = logicalName,
                        },
                        Status = new OptionSetValue(1),
                        State = new OptionSetValue(0)
                    };
                    _orgServ.Execute(setStateRequest);
                    goto Update;
                }
            Update:
                activity.LogicalName = logicalName;
                activity.Id = new Guid(entity.Attributes["activityid"].ToString());
                activity.Attributes["description"] = "XXXX XXXX XXXX XXX";
                activity.Attributes["subject"] = "XXXX XXXX XXXX XXXX";

                _orgContext.Attach(activity);
                _orgContext.UpdateObject(activity);
            }
            _orgContext.SaveChanges();

            _orgContext?.Dispose();
            retrievedActivities.Entities.Clear();
        }
    }
}
