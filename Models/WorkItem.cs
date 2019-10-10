using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TFSApi.Models
{
    public class WorkItem
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

            [DataMember(Name = "System.IterationPath")]
            public string SystemIterationPath { get; set; }

            [DataMember(Name = "System.IterationId")]
            public int SystemIterationId { get; set; }

            [DataMember(Name = "System.State")]
            public string SystemState { get; set; }


            [DataMember(Name = "Microsoft.VSTS.Common.BusinessValue")]
            public int MicrosoftVSTSCommonBusinessValue { get; set; }

            [DataMember(Name = "Microsoft.VSTS.Scheduling.StoryPoints")]
            public int MicrosoftVSTSSchedulingStoryPoints { get; set; }


        }

        public class rootObject
        {
            public int count { get; set; }

            public workItemList[] value { get; set; }
        }

        public class iterationDates {
            public attributes attributes { get; set; }
        }

        public class attributes
        {
            public string startDate { get; set; }
            public string finishDate { get; set; }
        }
    }
   
}