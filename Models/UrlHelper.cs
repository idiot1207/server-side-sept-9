using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFSApi.Models
{
    public class UrlHelper
    {
        public string TFSBaseUrl { get; set; }

        public string teamNameUrl { get; set; }

        public string ReleaseUrl { get; set; }

        public string TeamProjectUrl { get; set; }

        public string WIQLUrl { get; set; }

        public string IterationUrl { get; set; }

        public string CurrentIterationUrl { get; set; }


    }
}