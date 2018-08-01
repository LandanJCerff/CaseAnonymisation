using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseAnonymisation
{
    class dataLayer
    {      
        public static EntityCollection GetActivities(IOrganizationService _orgServ, Guid CaseId)
        {
            string activityFetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
            <entity name='activitypointer'>
            <attribute name='activityid'/>
            <attribute name='activitytypecode'/>
            <attribute name='statecode'/>
            <attribute name='statuscode'/>
            <filter type='and'>
                <condition attribute='regardingobjectid' operator='eq' uitype='incident' value='" + CaseId.ToString() + @"' />
            </filter>
            </entity>
            </fetch>";
            EntityCollection activities = _orgServ.RetrieveMultiple(new FetchExpression(activityFetch));
            return activities;
        }

        public static EntityCollection GetResolvedCases(IOrganizationService _orgServ, string thread)
        {
            EntityCollection cases = new EntityCollection();
            string caseFetch = @"<fetch version='1.0' count='500' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='incident'>
                <attribute name='ticketnumber'/>    
                <attribute name='incidentid'/>
	            <attribute name='statecode'/>
	            <attribute name='statuscode'/>
                <order attribute='createdon' descending='" + thread + @"'/>
                <filter type='and'>
                  <condition attribute='title' operator='ne' value='XXXXX XX. XXX XXXXX XX XXXX.'/>        
                    <condition attribute='statecode' operator='in'>
                    <value>2</value>
                    <value>1</value>
                    </condition>                
                    </filter>
              </entity>
            </fetch>";
            cases = _orgServ.RetrieveMultiple(new FetchExpression(caseFetch));

            if (cases.Entities.Count >= 1)
            {
                return cases;
            }
            else
            {
                return null;
            }
        }

        public static EntityCollection GetActiveCases(IOrganizationService _orgServ, string thread)
        {
           // EntityCollection activeCases = new EntityCollection();
            string activeCaseFetch = @"<fetch version='1.0' count='500' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='incident'>
                <attribute name='ticketnumber'/>    
                <attribute name='incidentid'/>
	            <attribute name='statecode'/>
	            <attribute name='statuscode'/>
                <order attribute='createdon' descending='" + thread + @"'/>
                <filter type='and'>
                  <condition attribute='title' operator='ne' value='XXXXX XX. XXX XXXXX XX XXXX.'/>          
                    <condition attribute='statecode' value='0' operator='eq'/>             
                    </filter>
              </entity>
            </fetch>";
            FetchExpression f = new FetchExpression(activeCaseFetch);
            EntityCollection activeCases = _orgServ.RetrieveMultiple(f);
            //cases = _orgServ.RetrieveMultiple(new FetchExpression(caseFetch));

            if (activeCases.Entities != null)
            {
                return activeCases;
            }
            else
            {
                return null;
            }  
        }
    }
}
