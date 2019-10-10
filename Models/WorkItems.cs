using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFSApi.Models
{
    public class WorkItems
    {
        public class RootObject
        {
            public List<WorkItemId> workItems { get; set; }
        }

        public class WorkItemId
        {
            public int id { get; set; }
        }




        public class workItemList
        {
            public int id { get; set; }
            public int rev { get; set; }

            public field fields { get; set; }
        }

        [DataContract]
        public class field
        {
            [DataMember(Name = "System.WorkItemType")]
            public string SystemWorkItemType { get; set; }

            [DataMember(Name = "System.State")]
            public string SystemState { get; set; }


            [DataMember(Name = "Microsoft.VSTS.Common.BusinessValue")]
            public int MicrosoftVSTSCommonBusinessValue { get; set; }

            [DataMember(Name = "Microsoft.VSTS.Scheduling.StoryPoints")]
            public int MicrosoftVSTSSchedulingStoryPoints { get; set; }


        }

        public class main
        {
            public int count { get; set; }

            public workItemList[] value { get; set; }
        }

       
    }

   
}