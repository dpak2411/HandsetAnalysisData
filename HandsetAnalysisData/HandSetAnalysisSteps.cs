using System;
using System.Collections.Generic;
using AlteryxGalleryAPIWrapper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace HandsetAnalysisData
{
    [Binding]
    public class HandSetAnalysisSteps
    {

        private string alteryxurl;
        private string _sessionid;
        private string _appid;
        private string _userid;
        private string _appName;
        private string jobid;
        private string outputid;
        private string validationId;

        private Client Obj = new Client("https://gallery.alteryx.com/api");

       
        private RootObject jsString = new RootObject();

        [Given(@"alteryx running at""(.*)""")]
        public void GivenAlteryxRunningAt(string url)
        {
            alteryxurl = url;
        }
        
        [Given(@"I am logged in using ""(.*)"" and ""(.*)""")]
        public void GivenIAmLoggedInUsingAnd(string user, string password)
        {
            // Authenticate User and Retreive Session ID
            _sessionid = Obj.Authenticate(user, password).sessionId;
        }
        
        [Given(@"I publish the application ""(.*)""")]
        public void GivenIPublishTheApplication(string p0)
        {
            //Publish the app & get the ID of the app
            string apppath = @"..\..\docs\Handset Performance Analysis.yxzp";
            Action<long> progress = new Action<long>(Console.Write);
            var pubResult = Obj.SendAppAndGetId(apppath, progress);
            _appid = pubResult.id;
            validationId = pubResult.validation.validationId;
            ScenarioContext.Current.Set(Obj, System.Guid.NewGuid().ToString());
        }
        
        [Given(@"I check if the application is ""(.*)""")]
        public void GivenICheckIfTheApplicationIs(string status)
        {
            // validate a published app can be run 
            // two step process. First, GetValidationStatus which indicates when validation disposition is available. 
            // Second, GetValidation, which gives actual status Valid, UnValid, etc.

            //String validStatus = "";
            //while (validStatus != "Completed")
            //{
            //    var validationStatus = Obj.GetValidationStatus(validationId); // url/api/apps/jobs/{VALIDATIONID}/
            //    validStatus = validationStatus.status;
            //    string disposition = validationStatus.disposition;
            //}
            int count = 0;
            String validStatus = "";

            var validationStatus = Obj.GetValidationStatus(validationId);
            validStatus = validationStatus.status;

        CheckValidate:
            System.Threading.Thread.Sleep(100);
            if (validStatus == "Completed" && count < 5)
            {
                string disposition = validationStatus.disposition;
            }
            else if (count < 5)
            {
                count++;
                var reCheck = Obj.GetValidationStatus(validationId);
                validStatus = reCheck.status;
                goto CheckValidate;
            }

            else
            {

                throw new Exception("Complete Status Not found");

            }
            var finalValidation = Obj.GetValidation(_appid, validationId); // url/api/apps/{APPID}/validation/{VALIDATIONID}/
            var finaldispostion = finalValidation.validation.disposition;
            StringAssert.IsMatch(status, finaldispostion.ToString());
        }

        [When(@"I run the handset performance app with sample data")]
        public void WhenIRunTheHandsetPerformanceAppWithSampleData()
        {
            //url + "/apps/studio/?search=" + appName + "&limit=20&offset=0"
            //Search for App & Get AppId & userId 

            string response = Obj.SearchApps("performance");
            var appresponse =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    response);
            int count = appresponse["recordCount"];
            //for (int i = 0; i <= count - 1; i++)
            //{
            //  _appid = appresponse["records"][0]["id"];
            _userid = appresponse["records"][0]["owner"]["id"];
            _appName = appresponse["records"][0]["primaryApplication"]["fileName"];
            //}       
            jsString.appPackage.id = _appid;
          //  jsString.appPackage.id = "52c15e7120aaf90df8ad1db5";
            jsString.userId = _userid;
            jsString.appName = _appName;

            //url +"/apps/" + appPackageId + "/interface/
            //Get the app interface - not required
            string appinterface = Obj.GetAppInterface(_appid);
            dynamic interfaceresp = JsonConvert.DeserializeObject(appinterface);

            //Construct the payload to be posted.
            string header = String.Empty;
            string payatbegin = String.Empty;
            List<JsonPayload.Question> questionAnsls = new List<JsonPayload.Question>();
            questionAnsls.Add(new JsonPayload.Question("Sample", "true"));
            questionAnsls.Add(new JsonPayload.Question("Your Data", "false"));
            questionAnsls.Add(new JsonPayload.Question("Limit", "false"));
            questionAnsls.Add(new JsonPayload.Question("Record Limit", "50"));
            //questionAnsls.Add(new JsonPayload.Question("LoanAmount", principle.ToString()));

            var solve = new List<JsonPayload.datac>();
            solve.Add(new JsonPayload.datac() { key = "2G", value = "{\"fileId\":\"\",\"fieldMap\":[]}"});
            var solve1 = new List<JsonPayload.datac>();
            solve1.Add(new JsonPayload.datac() { key = "3G", value = "{\"fileId\":\"\",\"fieldMap\":[]}" });
            var solve2 = new List<JsonPayload.datac>();
            solve2.Add(new JsonPayload.datac() { key = "LTE", value = "{\"fileId\":\"\",\"fieldMap\":[]}" });
            //var payat = new List<JsonPayload.datac>();
            //payat.Add(new JsonPayload.datac() { key = "0", value = "true" });
            string SolveFor = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(solve);
            string SolveFor1 = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(solve1);
            string SolveFor2 = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(solve2);

            //string PayAtBegin = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(payat);


            for (int i = 0; i <4; i++)
            {

                if (i == 0)
                {
                    JsonPayload.Question questionAns = new JsonPayload.Question();
                    questionAns.name = "2G";
                    questionAns.answer = SolveFor;
                    jsString.questions.Add(questionAns);
                }
                else if (i == 1)
                {
                    JsonPayload.Question questionAns = new JsonPayload.Question();
                    questionAns.name = "3G";
                    questionAns.answer = SolveFor1;
                    jsString.questions.Add(questionAns);
                }
                else if (i == 2)
                {
                    JsonPayload.Question questionAns = new JsonPayload.Question();
                    questionAns.name = "LTE";
                    questionAns.answer = SolveFor2;
                    jsString.questions.Add(questionAns);
                }

                else
                {
                    jsString.questions.AddRange(questionAnsls);
                }
            }
            jsString.jobName = "Job Name";


            // Make Call to run app

            var postData = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(jsString);
            string postdata = postData.ToString();
            string resjobqueue = Obj.QueueJob(postdata);

            var jobqueue =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    resjobqueue);
            jobid = jobqueue["id"];


            //Get the job status

            string status = "";
            while (status != "Completed")
            {
                string jobstatusresp = Obj.GetJobStatus(jobid);
                var statusresp =
                    new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                        jobstatusresp);
                status = statusresp["status"];
            }

        }

        [Then(@"I see output contains ""(.*)""")]
        public void ThenISeeOutputContains(string result)
        {
            //url + "/apps/jobs/" + jobId + "/output/"
            string getmetadata = Obj.GetOutputMetadata(jobid);
            dynamic metadataresp = JsonConvert.DeserializeObject(getmetadata);

            // outputid = metadataresp[0]["id"];
            int count = metadataresp.Count;
            for (int j = 0; j <= count - 1; j++)
            {
                outputid = metadataresp[j]["id"];
            }

            string getjoboutput = Obj.GetJobOutput(jobid, outputid, "html");
            string htmlresponse = getjoboutput;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlresponse);
            string output = doc.DocumentNode.SelectSingleNode("//div[@class='DefaultText']").InnerText;
            
            StringAssert.Contains(result, output);
        }
        }

       

    }

