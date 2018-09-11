using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;

namespace CaseAnonymisation
{
    class serviceLayer : dataLayer
    {
        public static void ActiveCases(IOrganizationService _orgServ, string thread)
        {
            string ticket = string.Empty;
            string regarding = string.Empty;
        start:
            EntityCollection retrievedCases = GetActiveCases(_orgServ, thread);

            if (retrievedCases.Entities.Count != 0)
            {
                try
                {
                    foreach (Entity cas in retrievedCases.Entities)
                    {
                        ticket = cas["ticketnumber"].ToString();
                        DateTime createdOn = (DateTime)cas["createdon"];
                        var casId = (Guid)cas["incidentid"];

                        var updateCas = new Entity
                        {
                            Id = casId,
                            LogicalName = "incident"
                        };

                        if (cas["gcs_casetypes"] == null)
                        {
                            _orgServ.Delete("incident", casId);
                            continue;
                        }

                        regarding = cas["gcs_casetypes"].ToString();

                        updateCas["description"] =
                            "A case type of " + regarding + ", which was created on: " + createdOn;

                        updateCas["title"] =
                            "Anonymised Case: '" + regarding + "' Case created on: " + createdOn;

                        updateCas["gcs_resolutiondescription"] =
                            "A resolution case type of " + regarding + ", has been resolved.";

                        activityUpdate(_orgServ, casId, ticket);

                        _orgServ.Update(updateCas);

                        Console.WriteLine(ticket + " updated. \n");
                    }
                    retrievedCases.Entities.Clear();
                }
                catch (FaultException<OrganizationServiceFault> e)
                {
                    Console.WriteLine("Exception: occured on Case: " + ticket + "\n" + e.Message);
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
            string regarding = string.Empty;
        start:
            EntityCollection retrievedResCases = GetResolvedCases(_orgServ, thread);

            if (retrievedResCases.Entities.Count != 0)
            {
                try
                {
                    foreach (Entity cas in retrievedResCases.Entities)
                    {
                        var statusReason = cas.GetAttributeValue<OptionSetValue>("statuscode").Value;
                        var dateClosed = cas.GetAttributeValue<DateTime>("gcs_dateresolved");
                        var rootCause = cas.GetAttributeValue<EntityReference>("ccrm_rootcauseid");
                        var resoCode = cas.GetAttributeValue<EntityReference>("gcs_resolutioncode");
                        ticket = cas["ticketnumber"].ToString();
                        DateTime createdOn = (DateTime)cas["createdon"];

                        var resCasId = (Guid)cas["incidentid"];

                        var updateResCas = new Entity
                        {
                            Id = resCasId,
                            LogicalName = "incident"
                        };


                        if (cas["gcs_casetypes"] == null)
                        {
                            _orgServ.Delete("incident", resCasId);
                        }

                        regarding = cas["gcs_casetypes"].ToString();

                        OpenResolvedCase(_orgServ, resCasId, ticket);


                        updateResCas["description"] =
                            "A case type of " + regarding + ", which was created on: " + createdOn;

                        updateResCas["title"] =
                            "Anonymised Case: " + regarding + " Case created on: " + createdOn;

                        updateResCas["gcs_resolutiondescription"] =
                            "A resolution case type of " + regarding + ", has been resolved.";

                        if (dateClosed == default(DateTime))
                        {
                            dateClosed = DateTime.Now;
                        }

                        updateResCas["gcs_dateresolved"] = dateClosed.Date;
                        updateResCas["ccrm_rootcauseid"] = rootCause;
                        updateResCas["gcs_resolutioncode"] = resoCode;
                        updateResCas["gcs_resolutiondescription"] = "Anonymised Closure";

                        activityUpdate(_orgServ, resCasId, ticket);

                        _orgServ.Update(updateResCas);

                        CloseCase(_orgContext, _orgServ, statusReason, cas.LogicalName, resCasId, ticket);

                        Console.WriteLine(ticket + " updated. \n");
                    }
                }
                catch (FaultException<OrganizationServiceFault> e)
                {
                    Console.WriteLine("Exception: occured on Case: " + ticket + "\n");
                    Console.WriteLine(e.Message);
                }
                retrievedResCases.Entities.Clear();
            }
            else
            {
                return;
            }
            goto start;
        }

        public static void OpenResolvedCase(IOrganizationService _orgServ, Guid resCasId, string ticket)
        {
            try
            {
                SetStateRequest state = new SetStateRequest();
                state.EntityMoniker = new EntityReference("incident", resCasId);
                state.State = new OptionSetValue(0);
                state.Status = new OptionSetValue(1);
                SetStateResponse stateSet = (SetStateResponse)_orgServ.Execute(state);
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                Console.WriteLine("Exception: occured on Case: " + ticket + "\n" + e.Message);
            }
        }

        public static void CloseCase(OrganizationServiceContext orgContext, IOrganizationService _orgServ, int statusReason, string logiName, Guid incId, string ticket)
        {
            if (orgContext != default(OrganizationServiceContext))
            {

                var incidentResolution = new Entity("incidentresolution");
                incidentResolution["subject"] = "Anonymised Incident Resolved";
                incidentResolution["incidentid"] = new EntityReference(Incident.EntityLogicalName, incId);
                incidentResolution["description"] = "Test Case";
                incidentResolution["timespent"] = 60;

                var closeIncidentRequest = new CloseIncidentRequest
                {
                    IncidentResolution = incidentResolution,
                    Status = new OptionSetValue(statusReason)
                };

                try
                {
                    _orgServ.Execute(closeIncidentRequest);
                }
                catch (Exception ex)
                {
                    string errorMsg = ticket + ex.StackTrace;
                }
                
            }
        }
       
        public static void activityUpdate(IOrganizationService _orgServ, Guid CaseId, string ticket)
        {
            OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);
            EntityCollection retrievedActivities = GetActivities(_orgServ, CaseId);

            foreach (Entity entity in retrievedActivities.Entities)
            {
                var logicalName = entity["activitytypecode"].ToString();
                var typeCode = entity["activitytypecode"].ToString();
                var actId = (Guid)entity["activityid"];

                var updateAct = new Entity
                {
                    Id = actId,
                    LogicalName = logicalName
                };

                #region
                if (logicalName == "email")
                {
                    SetStateRequest setStateRequest = new SetStateRequest()
                    {
                        EntityMoniker = new EntityReference
                        {
                            Id = actId,
                            LogicalName = logicalName,
                        },
                        Status = new OptionSetValue(1),
                        State = new OptionSetValue(0)
                    };
                    _orgServ.Execute(setStateRequest);
                }
                #endregion

                updateAct["description"] = "Anonymised activity description. " + typeCode;
                updateAct["subject"] = "Activity type: " + typeCode;


                _orgServ.Update(updateAct);
            }
            Console.WriteLine("Activities for " + ticket + " have been updated \n");

            retrievedActivities.Entities.Clear();
        }
    }
}
