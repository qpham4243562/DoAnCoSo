using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
namespace DoAnCoSo.Controllers
{
    public class HomePageController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomePageController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:7233");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("api/User_Post");
                response.EnsureSuccessStatusCode(); // Throws exception if not successful
                var content = await response.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<User_Post>>(content);
                return View(data);
            }
            catch (HttpRequestException)
            {
                // Xử lý lỗi kết nối
                return View("ConnectionError");
            }
            catch (Exception)
            {
                // Xử lý lỗi khác
                return View("Error");
            }
        }
    }
}
