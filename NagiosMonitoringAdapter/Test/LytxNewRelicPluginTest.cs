using System;
using NUnit.Framework;


namespace LytxNewRelicPluginTest
{
    using LytxNewRelicPluginCore;
    using Moq;

    [TestFixture]
    public class LytxNewRelicPluginTest
    {
        ClsRESTCall restTest;

        [Test]
        public void ValidInputParams()
        {
            restTest = new ClsRESTCall();
            const string outputvalue = "\nError in parameter validation method.";
            restTest.Inputparams = null; //"APDEX,19824596,5,1.0,.90,0.75,1".Split(new char[] { ',' });
            Assert.IsFalse(restTest.ValidateInputAndAssignToParams(), outputvalue);
            
            
            restTest.Inputparams = "APDEX,5,1.0,.90,0.75,1".Split(new char[] { ',' });
            Assert.IsFalse(restTest.ValidateInputAndAssignToParams(), outputvalue);


            restTest.Inputparams = "APDEX,19824596,5,1.0,.90,0.75,1".Split(new char[] { ',' });
            Assert.IsTrue(restTest.ValidateInputAndAssignToParams(), outputvalue);

        }

        [Test]
        public void TestValidIndex()
        {
            const string outputvalue = "\nFunction IsGivenIndexValid has some errors.";
            restTest = new ClsRESTCall();
            restTest.GoodIndex = (decimal)1;
            restTest.WarningIndex = (decimal) 1.90;
            restTest.CriticalIndex = (decimal)0.75;            
            Assert.IsFalse(restTest.IsGivenIndexValid(), outputvalue);

            restTest.GoodIndex = (decimal)1;
            restTest.WarningIndex = (decimal)1.90;
            restTest.CriticalIndex = (decimal)0.75;               
            Assert.IsFalse(restTest.IsGivenIndexValid(), outputvalue);

            restTest.GoodIndex = (decimal)1;
            restTest.WarningIndex = (decimal)0.90;
            restTest.CriticalIndex = (decimal)1.75;   
            Assert.IsFalse(restTest.IsGivenIndexValid(), outputvalue);

            restTest.GoodIndex = (decimal)1;
            restTest.WarningIndex = (decimal)0.90;
            restTest.CriticalIndex = (decimal)0.75;              
            Assert.IsTrue(restTest.IsGivenIndexValid(), outputvalue);

        }

        [Test]
        public void TestStringParse()
        {
            const string outputvalue = "\nFunction CheckStringAvailable has some errors.";
            char[] delimiter = new char[]{','};
            restTest = new ClsRESTCall();
            Assert.IsFalse(restTest.CheckStringAvailable(null, 2), outputvalue);

            Assert.IsFalse(restTest.CheckStringAvailable("APDEX,19824596".Split(delimiter), 2), outputvalue);
            Assert.IsFalse(restTest.CheckStringAvailable("APDEX,19824596,5,1.0,.90,0.75,1".Split(delimiter), 8), outputvalue);
            Assert.IsTrue(restTest.CheckStringAvailable("APDEX,19824596,5,1.0,.90,0.75,1".Split(delimiter), 6), outputvalue);
        }

        [Test]
        public void TestGetResult()
        {
            const string outputvalue = "\nFunction GetResult has some errors.";
            restTest = new ClsRESTCall();
            restTest.GoodIndex = (decimal)1;
            restTest.WarningIndex = (decimal)0.90;
            restTest.CriticalIndex = (decimal)0.75;
            Assert.IsTrue(APDexResult.OK == restTest.GetResult(1), outputvalue);

            Assert.IsTrue(APDexResult.OK == restTest.GetResult((decimal)1.5), outputvalue);

            Assert.IsTrue(APDexResult.Warning == restTest.GetResult((decimal)0.9), outputvalue);

            Assert.IsTrue(APDexResult.Warning == restTest.GetResult((decimal)0.95), outputvalue);

            Assert.IsTrue(APDexResult.Critical == restTest.GetResult((decimal)0.84), outputvalue);

            Assert.IsTrue(APDexResult.Unknown == restTest.GetResult((decimal)0.74), outputvalue);
        }

        [Test]
        public void TestCheckApdex()
        {
            RestSharp.Deserializers.JsonDeserializer d = new RestSharp.Deserializers.JsonDeserializer();

            RestSharp.RestResponse app1browser97response = new RestSharp.RestResponse {
                Content = 
"{" +
"      \"metrics\" : [" +
"         {" +
"            \"name\" : \"Apdex\"," +
"            \"timeslices\" : [" +
"               {" +
"                  \"to\" : \"2016-03-01T00:00:00+00:00\"," +
"                  \"from\" : \"2016-03-01T00:30:00+00:00\"," +
"                  \"values\" : {" +
"                     \"score\" : 1" +
"                  }" +
"               }" +
"            ]" +
"         }," +
"         {" +
"            \"name\" : \"EndUser/Apdex\"," +
"            \"timeslices\" : [" +
"               {" +
"                  \"to\" : \"2016-03-01T00:00:00+00:00\"," +
"                  \"from\" : \"2016-03-01T00:30:00+00:00\"," +
"                  \"values\" : {" +
"                     \"score\" : 0.97" +
"                  }" +
"               }" +
"            ]" +
"         }" +
"      ]" +
"}"
            };
            var app1browser97metrics = d.Deserialize<MetricStub>(app1browser97response);

            RestSharp.RestResponse app9browser7response = new RestSharp.RestResponse
            {
                Content =
"{" +
"      \"metrics\" : [" +
"         {" +
"            \"name\" : \"Apdex\"," +
"            \"timeslices\" : [" +
"               {" +
"                  \"to\" : \"2016-03-01T00:00:00+00:00\"," +
"                  \"from\" : \"2016-03-01T00:30:00+00:00\"," +
"                  \"values\" : {" +
"                     \"score\" : 0.9" +
"                  }" +
"               }" +
"            ]" +
"         }," +
"         {" +
"            \"name\" : \"EndUser/Apdex\"," +
"            \"timeslices\" : [" +
"               {" +
"                  \"to\" : \"2016-03-01T00:00:00+00:00\"," +
"                  \"from\" : \"2016-03-01T00:30:00+00:00\"," +
"                  \"values\" : {" +
"                     \"score\" : 0.7" +
"                  }" +
"               }" +
"            ]" +
"         }" +
"      ]" +
"}"
            };
            var app9browser7metrics = d.Deserialize<MetricStub>(app9browser7response);


            var mockClient = new Mock<INewRelicClient>();
            mockClient.Setup(client => client.GetApdex("1", 30)).Returns(app1browser97metrics.Metrics);
            mockClient.Setup(client => client.GetApdex("2", 30)).Returns(app9browser7metrics.Metrics);
            mockClient.Setup(client => client.GetApdex("3", 30)).Throws(new Exception("Unknown error"));

            Plugin plugin = new Plugin();

            var r1 = plugin.Run(mockClient.Object, new Options {
                AppID = "1",
                CriticalThreshold = .5,
                WarnThreshold = .9,
                APIKey = "123",
                WindowMinutes = 30,
            });
            Assert.That(r1.ExitCode, Is.EqualTo(0));
            Assert.That(r1.Message, Is.StringStarting("OK "));

            r1 = plugin.Run(mockClient.Object, new Options
            {
                AppID = "2",
                CriticalThreshold = .7,
                WarnThreshold = .8,
                APIKey = "123",
                WindowMinutes = 30,
            });
            Assert.That(r1.ExitCode, Is.EqualTo(2));
            Assert.That(r1.Message, Is.StringStarting("CRITICAL "));

            r1 = plugin.Run(mockClient.Object, new Options
            {
                AppID = "2",
                CriticalThreshold = .4,
                WarnThreshold = .8,
                APIKey = "123",
                WindowMinutes = 30,
            });
            Assert.That(r1.ExitCode, Is.EqualTo(1));
            Assert.That(r1.Message, Is.StringStarting("WARNING "));

            Assert.That(delegate()
            {
                plugin.Run(mockClient.Object, new Options
                {
                    AppID = "3",
                    CriticalThreshold = .4,
                    WarnThreshold = .8,
                    APIKey = "123",
                    WindowMinutes = 30,
                });
            }, Throws.TypeOf<Exception>());

        }


        class MetricStub
        {
            public System.Collections.Generic.List<Metric> Metrics { get; set; }
        }

        
    }
}
