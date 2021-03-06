﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects;
using Unosquare.Labs.EmbedIO.LiteLibWebApi;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Networking;

namespace Unosquare.Labs.EmbedIO.Extra.Tests
{
    [TestFixture]
    public class LiteLibModuleTest
    {
        protected string RootPath;
        protected string ApiPath = "dbapi";
        protected string WebServerUrl;
        protected WebServer WebServer;

        [SetUp]
        public void Init()
        {
            RootPath = TestHelper.SetupStaticFolder();

            WebServerUrl = Resources.GetServerAddress();
            WebServer = new WebServer(WebServerUrl);
            WebServer.RegisterModule(new LiteLibModule<TestDbContext>(new TestDbContext(), "/" + ApiPath + "/"));
            WebServer.RunAsync();
        }

        [Test]
        public async Task GetAllLiteLib()
        {
            var response = await JsonClient.GetString(WebServerUrl + ApiPath + "/order");

            Assert.IsNotNull(response);

            var orders = Json.Deserialize<List<Order>>(response);

            Assert.Greater(orders.Count, 0, "Orders count is greater than 0");
        }

        [Test]
        public async Task GetFirstItemLiteLib()
        {
            var response = await JsonClient.GetString(WebServerUrl + ApiPath + "/order/1");

            Assert.IsNotNull(response);

            var orders = Json.Deserialize<Order>(response);

            Assert.AreEqual(1, orders.RowId, "Order's RowId equals 1");
        }

        [Test]
        public async Task AddLiteLib()
        {
            var getAllResponse = await JsonClient.GetString(WebServerUrl + ApiPath + "/order");
            var orders = (Json.Deserialize<List<Order>>(getAllResponse)).Count;

            Order newOrder = new Order()
            {
                CustomerName = "UnoLabs",
                ShipperCity = "GDL",
                ShippedDate = "2017-03-20",
                Amount = 100,
                IsShipped = false
            };

            await JsonClient.Post(WebServerUrl + ApiPath + "/order", newOrder);

            getAllResponse = await JsonClient.GetString(WebServerUrl + ApiPath + "/order");
            var ordersPlusOne = (Json.Deserialize<List<Order>>(getAllResponse)).Count;

            Assert.AreEqual(orders + 1, ordersPlusOne);
        }

        [Test]
        public async Task PutLiteLib()
        {
            Order order = new Order()
            {
                CustomerName = "UnoLabs",
                ShipperCity = "Zapopan",
                ShippedDate = "2017-03-22",
                Amount = 200,
                IsShipped = true
            };

            await JsonClient.Put(WebServerUrl + ApiPath + "/order/1", order);

            var response = await JsonClient.GetString(WebServerUrl + ApiPath + "/order/1");
            var edited = (Json.Deserialize<Order>(response));

            Assert.AreEqual(order.ShipperCity, "Zapopan");

            Assert.AreEqual(order.Amount, 200);

            Assert.AreEqual(order.ShippedDate, "2017-03-22");

            Assert.AreEqual(order.IsShipped, true);
        }

        [Test]
        public async Task DeleteLiteLib()
        {
            var response = await JsonClient.GetString(WebServerUrl + ApiPath + "/order");

            Assert.IsNotNull(response);

            var order = (Json.Deserialize<List<Order>>(response));
            var last = order.Last().RowId;
            var count = order.Count();

            var request = (HttpWebRequest)WebRequest.Create(WebServerUrl + ApiPath + "/order/" + last);
            request.Method = "DELETE";

            using (var webResponse = (HttpWebResponse)await request.GetResponseAsync())
            {
                Assert.AreEqual(webResponse.StatusCode, HttpStatusCode.OK, "Status Code OK");
            }

            response = await JsonClient.GetString(WebServerUrl + ApiPath + "/order");
            var newCount = (Json.Deserialize<List<Order>>(response)).Count();

            Assert.AreEqual(newCount, count - 1);
        }
    }
}
