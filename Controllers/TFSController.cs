using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Http;
using System.Collections.Generic;
using TFSApi.Models;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using LoginDecryption;
using System.Linq;
using static TFSApi.Models.WorkItem;
using System.IO;
using System.Text;
using System.Security.Claims;
using System.Web;

namespace TFSApi.Controllers
{
    // [BasicAuthentication]

    public class TFSController : ApiController
    {

        static string jsonFilePath = @"C:\Users\CGI\Documents\visual studio 2015\Projects\TFSApi\TFSApi\TreservaTFSURL.json";

        static UrlHelper deserializedURL = JsonConvert.DeserializeObject<UrlHelper>(File.ReadAllText(jsonFilePath));



        //Base URL For TFS
        string TFSUrl = deserializedURL.TFSBaseUrl;

        int targetFeaturePoint = 0;

        int closedFeaturePoint = 0;

        int removedFeaturePoint = 0;

        int inProgressFeaturePoint = 0;

        int newFeaturePoint = 0;

        int targetStoryPoint = 0;

        int closedStoryPoint = 0;

        int resolvedStoryPoint = 0;

        int activeStoryPoint = 0;

        int newStoryPoint = 0;

        int bugRaised = 0;

        int bugClosed = 0;

        [Route("api/TFS/GetAllTeamName")]
        // [Authorize]
        public string GetAllTeamName()
        {
            try
            {

                string teamNameUrl = deserializedURL.teamNameUrl;

                string releaseUrl = deserializedURL.ReleaseUrl;

                string newTeamNameUrl = string.Format("{0}{1}", TFSUrl, teamNameUrl);

                string newreleaseUrl = String.Format("{0}{1}", TFSUrl, releaseUrl);

                var teamName = BasicAuthentication.client.DownloadString(newTeamNameUrl);

                var release = BasicAuthentication.client.DownloadString(newreleaseUrl);

                var teamAndRelease = "[" + teamName + "," + release + "]";

                return teamAndRelease;
            }
            catch (NullReferenceException)
            {
                return "Please Login";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

      //  [Authorize]
        [Route("api/TFS/GetTeamProject/{teamName}")]

        public string GetTeamProject(string teamName)
        {
            try
            {
                // string teamProjectUrl = "ProjectOne/"+teamName+"/_apis/Work/TeamSettings/TeamFieldValues?api-version=2.0";

                string teamProjectUrl = deserializedURL.TeamProjectUrl;

                string newteamProjectUrl = string.Format("{0}{1}", TFSUrl, teamProjectUrl);

                dynamic projects = JsonConvert.DeserializeObject(BasicAuthentication.client.DownloadString(newteamProjectUrl));

                int counter = 0;

                foreach (var TeamList in projects.value[0].children)
                {
                    foreach (var TeamDatas in TeamList)
                    {
                        foreach (var TeamData in TeamDatas)
                        {

                            if (TeamData.ToString() == teamName || counter != 0)
                            {
                                counter++;

                                var type = TeamData.GetType();

                                var pros = type.ToString();

                                if (pros == "Newtonsoft.Json.Linq.JArray")
                                {
                                    projects = TeamData.ToString();
                                }

                            }

                        }

                    }
                    counter = 0;
                }

                return projects;
            }
            catch (NullReferenceException)
            {
                return "Please Login";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]

        [Route("api/TFS/login")]

        public async Task<string> login(HttpRequestMessage request)
        {
            try
            {
                var jObject = await request.Content.ReadAsAsync<JObject>();

                var item = JsonConvert.DeserializeObject<userLogin>(jObject.ToString());

                var userName = item.UserName;

                var password = item.Password;

                var decryptedUserName = Logindecryption.DecryptStringAES(userName);

                var decryptedPassword = Logindecryption.DecryptStringAES(password);

                bool isValidUser = BasicAuthentication.IsAuthorizedUser(decryptedUserName, decryptedPassword);

                return isValidUser.ToString();
            }

            catch (Exception e)
            {
                return e.ToString();
            }

        }

        // [Authorize]
        [HttpGet]

        [Route("api/TFS/GetReleases/{releaseId}")]

        public string GetReleases(int releaseId)
        {
            try
            {
                string releaseUrl = "ProjectOne/apis/release/releases/" + releaseId + "?api-version=3.0-preview";

                string newreleaseUrl = string.Format("{0}{1}", TFSUrl, releaseUrl);

                var releaseDetails = BasicAuthentication.client.DownloadString(newreleaseUrl);

                return releaseDetails;

            }
            catch (NullReferenceException)
            {
                return "Please Login";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        //[Authorize]
        [HttpGet]

        [Route("api/TFS/post/{AreaPath}")]

        public string post(string AreaPath)
        {
            Dictionary<string, Dictionary<string, string>> iterationdata = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> li;
            string iterationName = "";
            //  byte[] data = Convert.FromBase64String(AreaPath);
            //string decodedString = Encoding.UTF8.GetString(data);
            string uri = deserializedURL.WIQLUrl;
            BasicAuthentication.client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            var query = new { query = "Select [System.Id],[System.Title] from WorkItems where [System.AreaPath] = '" + AreaPath + "' and ([System.WorkItemType] ='Feature' OR [System.WorkItemType] ='User Story' OR [System.WorkItemType] ='Bug')" };
            string serializedQuery = JsonConvert.SerializeObject(query);
            string json = BasicAuthentication.client.UploadString(uri, "POST", serializedQuery).ToString();
            var deserializedWorkItems = JsonConvert.DeserializeObject<RootObject>(json);
            var workItemId = deserializedWorkItems.workItems.Select(p => p.id).ToList();
            var splitItemIds = workItemId.Select((x, i) => new { Index = i, Value = x }) // spliting ids into 200 
             .GroupBy(x => x.Index / 200)
             .Select(x => x.Select(v => v.Value).ToList())
             .ToList();
            foreach (var ids in splitItemIds)
            {
                string combindedWorkItemId = string.Join(",", ids.ToArray());
                string workItemDetailsUrl = String.Format("{0}{1}", TFSUrl, "_apis/wit/workitems?ids=" + combindedWorkItemId + "&$expand=all&api-version=2.0");
                var workItemDetails = BasicAuthentication.client.DownloadString(workItemDetailsUrl);
                rootObject lists = JsonConvert.DeserializeObject<rootObject>(workItemDetails);
                var groupByItertaion = lists.value.Where(t => t.fields != null).GroupBy(t => t.fields.SystemIterationPath);
                var groupByiterationPath = groupByItertaion.Select(t => new { path = t.Key });
                //  var iterationPathandId= groupByItertaion.Where(t => t.Key != null).Select(g => new { iterationPath = g.OrderBy(a => a.fields.SystemIterationPath), iterationId = g.OrderBy(a => a.fields.SystemIterationId) }).ToList();
                foreach (var iterationPath in groupByiterationPath)
                {
                    li = new Dictionary<string, string>();
                    iterationName = iterationPath.path.ToString();
                    BasicAuthentication.client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    var newQuery = new { Path = iterationName };
                    string newserializedQuery = JsonConvert.SerializeObject(newQuery);
                    string iterationUrl = deserializedURL.IterationUrl;
                    string newIterationUrl = string.Format("{0}{1}", TFSUrl, iterationUrl);
                    string newjson = BasicAuthentication.client.UploadString(newIterationUrl, "POST", newserializedQuery).ToString();
                    dynamic deserializediterationdata = JsonConvert.DeserializeObject<iterationDates>(newjson);
                    DateTime startDate = Convert.ToDateTime(deserializediterationdata.attributes.startDate);
                    DateTime finishDate = Convert.ToDateTime(deserializediterationdata.attributes.finishDate);
                    startDate = startDate.Date;
                    finishDate = finishDate.Date;
                    li.Add("startDate", startDate.ToString("dd/MM/yyyy"));
                    li.Add("finishDate", finishDate.ToString("dd/MM/yyyy"));
                    li.Add("itertaionName", iterationName);
                    var iterationfeatureData = groupByItertaion.Where(t => t.Key == iterationName)
                                           .Select(g => new { tfp = g.Sum(a => a.fields.MicrosoftVSTSCommonBusinessValue),
                                           closedFP = g.Sum(a => a.fields.SystemState == "Done" ? a.fields.MicrosoftVSTSCommonBusinessValue : 0) }).ToList();
                    var iterationbugData = groupByItertaion.Where(t => t.Key == iterationName).
                                           Select(g => new { BugRaised = g.Sum(a => a.fields.SystemWorkItemType == "Bug" ? 1 : 0),
                                           BugClosed = g.Sum(a => a.fields.SystemState == "Done" ? a.fields.SystemWorkItemType == "Bug" ?1 :0: 0) }).ToList();
                    var iterationuserStoriesData = groupByItertaion.Where(t => t.Key == iterationName).
                                                   Select(g => new { TargetUs = g.Sum(a => a.fields.MicrosoftVSTSSchedulingStoryPoints),
                                                   closedUs = g.Sum(a => a.fields.SystemState == "Done" ? a.fields.MicrosoftVSTSSchedulingStoryPoints : 0) }).ToList();
                    if (iterationfeatureData.Count > 0)
                    {
                        li.Add("iterationtargetFeaturePoint", iterationfeatureData[0].tfp.ToString());
                        li.Add("iterationclosedFeaturePoint", iterationfeatureData[0].closedFP.ToString());
                    }
                    if (iterationbugData.Count > 0)
                    {
                        li.Add("iterationbugRaised", iterationbugData[0].BugRaised.ToString());
                        li.Add("iterationbugClosed", iterationbugData[0].BugClosed.ToString());
                    }
                    if (iterationuserStoriesData.Count > 0)
                    {
                        li.Add("iterationuserStoriesData", iterationuserStoriesData[0].TargetUs.ToString());
                        li.Add("iterationclosedStoryPoint", iterationuserStoriesData[0].closedUs.ToString());
                    }
                    iterationdata.Add(iterationName, li);
                }
                var groupByWorkItems = lists.value.Where(t => t.fields != null).GroupBy(t => t.fields.SystemWorkItemType);
                var featureData = groupByWorkItems.Where(t => t.Key == "Feature").
                                  Select(g => new { TargetFP = g.Sum(a => a.fields.MicrosoftVSTSCommonBusinessValue),
                                  closedFP = g.Sum(a => a.fields.SystemState == "Done" ? a.fields.MicrosoftVSTSCommonBusinessValue : 0) }).ToList();
                var bugData = groupByWorkItems.Where(t => t.Key == "Bug").
                              Select(g => new { BugRaised = g.Sum(a => 1),
                              BugClosed = g.Sum(a => a.fields.SystemState == "Done" ? 1 : 0) }).ToList();
                var userStoriesData = groupByWorkItems.Where(t => t.Key == "User Story").
                                      Select(g => new { TargetUs = g.Sum(a => a.fields.MicrosoftVSTSSchedulingStoryPoints),
                                      closedUs = g.Sum(a => a.fields.SystemState == "Done" ? a.fields.MicrosoftVSTSSchedulingStoryPoints : 0) }).ToList();
                if (featureData.Count > 0)
                {
                    targetFeaturePoint += featureData[0].TargetFP;
                    closedFeaturePoint += featureData[0].closedFP;
                }
                if (bugData.Count > 0)
                {
                    bugRaised += bugData[0].BugRaised;
                    bugClosed += bugData[0].BugClosed;
                }
                if (userStoriesData.Count > 0)
                {
                    targetStoryPoint += userStoriesData[0].TargetUs;
                    closedStoryPoint += userStoriesData[0].closedUs;
                }
            }
            Dictionary<string, int> ExpectedData = new Dictionary<string, int>();
            ExpectedData.Add("TargetFeaturePoint", targetFeaturePoint);
            ExpectedData.Add("ClosedFeaturePoint", closedFeaturePoint);
            ExpectedData.Add("TargetStoryPoint", targetStoryPoint);
            ExpectedData.Add("ClosedStoryPoint", closedStoryPoint);
            ExpectedData.Add("BugRaised", bugRaised);
            ExpectedData.Add("BugClosed", bugClosed);
            var ExpectedjsonByIteration = JsonConvert.SerializeObject(iterationdata);
            var Expectedjson = JsonConvert.SerializeObject(ExpectedData);
            Expectedjson = "[" + Expectedjson + "," + ExpectedjsonByIteration + "]";
            return Expectedjson;
        }


        [HttpGet]
        [Route("api/TFS/GetUserClaims")]
        public string GetUserClaims()
        {
            string id;
            id = User.Identity.Name;

            var identity = User.Identity as ClaimsIdentity;

            var identityClaims = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identityClaims.Claims;
            string userName = identityClaims.FindFirst("UserName").Value;
            return userName;
        }

        [HttpGet]
        //   [Authorize]
        [Route("api/TFS/GetAllIterations")]
        public string GetAllIterations()
        {
            try
            {
                string currentIterationName = "";
                string iterationUrl = deserializedURL.IterationUrl;
                string newIterationUrl = string.Format("{0}{1}", TFSUrl, iterationUrl);
                string currenIterationUrl = string.Format("{0}{1}", TFSUrl, deserializedURL.CurrentIterationUrl);
                var iteration = (BasicAuthentication.client.DownloadString(newIterationUrl));

                dynamic currentIteration = JsonConvert.DeserializeObject(BasicAuthentication.client.DownloadString(currenIterationUrl));
                foreach (var current in currentIteration.value)
                {
                    currentIterationName = current.name;
                }
                JObject rss = JObject.Parse(iteration);
                JObject channel = (JObject)rss;
                channel.Property("value").AddAfterSelf(new JProperty("currentIteration", currentIterationName));
                return rss.ToString();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/TFS/WorkItemsByIteration/{iterationPath}/{AreaPath}")]
        public string WorkItemsByIteration(string iterationPath, string AreaPath)
        {
            byte[] data = Convert.FromBase64String(iterationPath);
            string decodediterationPath = Encoding.UTF8.GetString(data);
            byte[] data1 = Convert.FromBase64String(AreaPath);
            string decodedAreaPath = Encoding.UTF8.GetString(data1);
            string wiqlUrl = deserializedURL.WIQLUrl;
            BasicAuthentication.client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            var query = new { query = "Select [System.Id],[System.Title] from WorkItems where ([System.AreaPath] = '" + decodedAreaPath + "' and [System.IterationPath] = '" + decodediterationPath + "' and ([System.WorkItemType] ='Feature' OR [System.WorkItemType] ='User Story' OR [System.WorkItemType] ='Bug'))" };
            string serializedQuery = JsonConvert.SerializeObject(query);
            string json = BasicAuthentication.client.UploadString(wiqlUrl, "POST", serializedQuery).ToString();
            var deserializedWorkItems = JsonConvert.DeserializeObject<RootObject>(json);
            var workItemId = deserializedWorkItems.workItems.Select(p => p.id).ToList();
            var splitItemIds = workItemId.Select((x, i) => new { Index = i, Value = x })
             .GroupBy(x => x.Index / 200)
             .Select(x => x.Select(v => v.Value).ToList())
             .ToList();
            foreach (var ids in splitItemIds)
            {
                string combindedWorkItemId = string.Join(",", ids.ToArray());
                string workItemDetailsUrl = String.Format("{0}{1}", TFSUrl, "_apis/wit/workitems?ids=" + combindedWorkItemId + "&api-version=2.0");
                var workItemDetails = BasicAuthentication.client.DownloadString(workItemDetailsUrl);
                rootObject lists = JsonConvert.DeserializeObject<rootObject>(workItemDetails);
                var groupByWorkItems = lists.value.Where(t => t.fields != null).GroupBy(t => t.fields.SystemWorkItemType);
                var featureData = groupByWorkItems.Where(t => t.Key == "Feature").
                    Select(g => new
                    {
                        TargetFP = g.Sum(a => a.fields.MicrosoftVSTSCommonBusinessValue),
                        closedFP = g.Sum(a => a.fields.SystemState == "Done" ? a.fields.MicrosoftVSTSCommonBusinessValue : 0),
                        inProgressFP = g.Sum(a => a.fields.SystemState == "In Progress" ? a.fields.MicrosoftVSTSCommonBusinessValue : 0),
                        newFP = g.Sum(a => a.fields.SystemState == "New" ? a.fields.MicrosoftVSTSCommonBusinessValue : 0),
                        RemovedFP = g.Sum(a => a.fields.SystemState == "Removed" ? a.fields.MicrosoftVSTSCommonBusinessValue : 0)
                    }).ToList();
                var bugData = groupByWorkItems.Where(t => t.Key == "Bug").Select(g => new { BugRaised = g.Sum(a => 1), BugClosed = g.Sum(a => a.fields.SystemState == "Done" ? 1 : 0) }).ToList();
                var userStoriesData = groupByWorkItems.Where(t => t.Key == "User Story").
                    Select(g => new
                    {
                        TargetUs = g.Sum(a => a.fields.MicrosoftVSTSSchedulingStoryPoints),
                        closedUs = g.Sum(a => a.fields.SystemState == "Done" ? a.fields.MicrosoftVSTSSchedulingStoryPoints : 0),
                        resolvedUs = g.Sum(a => a.fields.SystemState == "Resolved" ? a.fields.MicrosoftVSTSSchedulingStoryPoints : 0),
                        activeUs = g.Sum(a => a.fields.SystemState == "Active" ? a.fields.MicrosoftVSTSSchedulingStoryPoints : 0),
                        newUs = g.Sum(a => a.fields.SystemState == "New" ? a.fields.MicrosoftVSTSSchedulingStoryPoints : 0)
                    }).ToList();
                if (featureData.Count > 0)
                {
                    closedFeaturePoint += featureData[0].closedFP;
                    inProgressFeaturePoint += featureData[0].inProgressFP;
                    newFeaturePoint += featureData[0].newFP;
                    removedFeaturePoint += featureData[0].RemovedFP;
                }
                if (bugData.Count > 0)
                {
                    bugRaised += bugData[0].BugRaised;
                    bugClosed += bugData[0].BugClosed;
                }
                if (userStoriesData.Count > 0)
                {
                    closedStoryPoint += userStoriesData[0].closedUs;
                    resolvedStoryPoint += userStoriesData[0].resolvedUs;
                    newStoryPoint += userStoriesData[0].newUs;
                    activeStoryPoint += userStoriesData[0].activeUs;
                }
            }

            Dictionary<string, int> ExpectedData = new Dictionary<string, int>();
            ExpectedData.Add("inProgressFeaturePoint", inProgressFeaturePoint);
            ExpectedData.Add("ClosedFeaturePoint", closedFeaturePoint);
            ExpectedData.Add("newFeaturePoint", newFeaturePoint);
            ExpectedData.Add("removedFeaturePoint", removedFeaturePoint);
            ExpectedData.Add("resolvedStoryPoint", targetStoryPoint);
            ExpectedData.Add("ClosedStoryPoint", closedStoryPoint);
            ExpectedData.Add("newStoryPoint", newStoryPoint);
            ExpectedData.Add("activeStoryPoint", activeStoryPoint);
            ExpectedData.Add("BugRaised", bugRaised);
            ExpectedData.Add("BugClosed", bugClosed);
            var Expectedjson = JsonConvert.SerializeObject(ExpectedData);
            return Expectedjson;
        }
    }
}




