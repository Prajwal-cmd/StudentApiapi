using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentApi.Models;

namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly string _jsonPath;

        public ChatbotController(IHostEnvironment env)
        {
            // questions.json lives in /Data
            _jsonPath = Path.Combine(env.ContentRootPath, "Data", "questions.json");
        }

        private List<QuestionNode> LoadTree()
        {
            if (!System.IO.File.Exists(_jsonPath))
                return new List<QuestionNode>();

            var json = System.IO.File.ReadAllText(_jsonPath);
            return JsonConvert.DeserializeObject<List<QuestionNode>>(json) ?? new List<QuestionNode>();
        }

        // GET: /api/chatbot/questions
        // Returns top-level questions (id + text)
        [HttpGet("questions")]
        public IActionResult GetTopQuestions()
        {
            var tree = LoadTree();
            var payload = tree.Select(n => new { id = n.Id, text = n.Text }).ToList();
            return Ok(payload);
        }

        // GET: /api/chatbot/question/{id}
        // Returns either:
        //   { answer: "..." }  (if leaf)
        // or
        //   [{ id, text }, ...] (sub-questions)
        [HttpGet("question/{id}")]
        public IActionResult GetNode(string id)
        {
            var tree = LoadTree();

            // BFS (iterative) to find any node by id
            var q = new Queue<QuestionNode>(tree);
            while (q.Count > 0)
            {
                var node = q.Dequeue();
                if (string.Equals(node.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(node.Answer))
                        return Ok(new { id = node.Id, text = node.Text, answer = node.Answer });

                    var children = node.SubQuestions ?? new List<QuestionNode>();
                    return Ok(children.Select(c => new { id = c.Id, text = c.Text }));
                }

                if (node.SubQuestions != null)
                    foreach (var child in node.SubQuestions)
                        q.Enqueue(child);
            }

            return NotFound(new { error = $"Question '{id}' not found." });
        }
    }
}
