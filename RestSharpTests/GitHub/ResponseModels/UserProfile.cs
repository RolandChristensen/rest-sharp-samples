using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharpTests.GitHub.ResponseModels
{
    internal class UserProfile
    {
        public string Login { get; set; }
        public int id { get; set; }
        public string Name { get; set; }
    }
}
