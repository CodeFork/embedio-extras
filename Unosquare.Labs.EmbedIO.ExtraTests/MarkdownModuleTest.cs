﻿namespace Unosquare.Labs.EmbedIO.ExtraTests
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Unosquare.Labs.EmbedIO.ExtraTests.Properties;
    using Unosquare.Labs.EmbedIO.Markdown;

    public class MarkdownModuleTest
    {
        protected string RootPath;
        protected WebServer WebServer;
        protected TestConsoleLog Logger = new TestConsoleLog();

        [SetUp]
        public void Init()
        {
            RootPath = TestHelper.SetupStaticFolder();

            WebServer = new WebServer(Resources.ServerAddress, Logger);
            WebServer.RegisterModule(new MarkdownStaticModule(RootPath));
            WebServer.RunAsync();
        }

        [Test]
        public void GetIndex()
        {
            var request = (HttpWebRequest)WebRequest.Create(Resources.ServerAddress);

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "Status Code OK");

                var html = new StreamReader(response.GetResponseStream()).ReadToEnd();

                Assert.AreEqual(html, Resources.indexhtml, "Same content index.html");
            }
        }

        [TearDown]
        public void Kill()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            WebServer.Dispose();
        }
    }
}