namespace StudentApi.Models
{
    public class QuestionNode
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }              // present only on leaf nodes
        public List<QuestionNode> SubQuestions { get; set; } // present only on non-leaf nodes
    }
}
