using EShopAdminApplication.Models;
using GemBox.Document;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EShopAdminApplication.Controllers
{
    public class OrderController : Controller
    {

        public OrderController()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        }


        public IActionResult Index()
        {
            HttpClient client = new HttpClient();


            string URI = "https://localhost:44309/api/Admin/GetOrders";

            HttpResponseMessage responseMessage = client.GetAsync(URI).Result;

            var result = responseMessage.Content.ReadAsAsync<List<Order>>().Result;

            return View(result);
        }

        public IActionResult Details(Guid id)
        {
            HttpClient client = new HttpClient();


            string URI = "https://localhost:44309/api/Admin/GetDetailsForProduct";

            var model = new
            {
                Id = id
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = client.PostAsync(URI, content).Result;


            var result = responseMessage.Content.ReadAsAsync<Order>().Result;


            return View(result);
        }

        public FileContentResult CreateInvoice(Guid id)
        {
            HttpClient client = new HttpClient();


            string URI = "https://localhost:44309/api/Admin/GetDetailsForProduct";

            var model = new
            {
                Id = id
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = client.PostAsync(URI, content).Result;


            var result = responseMessage.Content.ReadAsAsync<Order>().Result;

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Invoice.docx");
            var document = DocumentModel.Load(templatePath);


            document.Content.Replace("{{OrderNumber}}", result.Id.ToString());
            document.Content.Replace("{{UserName}}", result.User.UserName);

            StringBuilder sb = new StringBuilder();

            var totalPrice = 0.0;

            foreach (var item in result.ProductInOrders)
            {
                totalPrice += item.Quantity * item.OrderedProduct.ProductPrice;
                sb.AppendLine(item.OrderedProduct.ProductName + " with quantity of: " + item.Quantity + " and price of: " + item.OrderedProduct.ProductPrice + "$");
            }


            document.Content.Replace("{{ProductList}}", sb.ToString());
            document.Content.Replace("{{TotalPrice}}", totalPrice.ToString() + "$");


            var stream = new MemoryStream();

            document.Save(stream, new PdfSaveOptions());

            return File(stream.ToArray(), new PdfSaveOptions().ContentType, "ExportInvoice.pdf");
        }
    }
}
