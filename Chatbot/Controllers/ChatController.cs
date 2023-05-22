using System;
using Microsoft.AspNetCore.Mvc;
using Chatbot.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Data;
using System.Reflection.Metadata;

namespace Chatbot.Controllers
{
    public class ChatController : Controller
    {
        private static List<Message> chatHistory = new List<Message>();

        public IActionResult Index()
        {
            return View("Chat", chatHistory);
        }



        [HttpPost]
        [HttpPost]
        public async Task<JsonResult> Send(string content)
        {
            var userMessage = new Message { User = "User", Content = content, Timestamp = DateTime.Now };
            chatHistory.Add(userMessage);

            Console.WriteLine("About to call GetGpt3Response with content: " + content);

            string aiResponse = await GetGpt3Response(content);
            if (aiResponse != null)
            {
                Console.WriteLine("Received response from GetGpt3Response: " + aiResponse);
                chatHistory.Add(new Message { User = "AI", Content = aiResponse, Timestamp = DateTime.Now });
            }
            else
            {
                Console.WriteLine("Received null response from GetGpt3Response.");
            }

            return Json(chatHistory);
        }



        private async Task<string> GetGpt3Response(string message)
        {
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            string apiKey = "<api key>";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var jsonObject = new
                {
                    model= "gpt-3.5-turbo",
                    messages = new List<object>
        {
            new { role = "system", content = "You are a helpful assistant."},
            new { role = "user", content = message }
        },
                    n = 1 
                };

                string jsonContent = JsonConvert.SerializeObject(jsonObject);

                HttpResponseMessage response = await client.PostAsync(
                    apiUrl,
                    new StringContent(jsonContent, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response content: " + content);
                    dynamic result = JsonConvert.DeserializeObject(content);

                    if (result != null && result.completions != null && result.completions.Count > 0)
                    {
                        string aiResponse = result.completions[0].text.ToString().Trim();
                        Console.WriteLine("AI response: " + aiResponse);
                        return aiResponse;
                    }
                    else
                    {
                        return "I'm sorry, but I was unable to generate a response. Please try again later.";
                    }
                }
                else
                {
                    Console.WriteLine("API response status: " + response.StatusCode);
                    Console.WriteLine("API response content: " + await response.Content.ReadAsStringAsync());
                    return $"I'm sorry, but I encountered an error while generating a response: {response.StatusCode}. Please try again later.";
                }
            }
        }


    }
}
