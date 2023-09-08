namespace MicrosoftEduImportFromGoogle.Models
{
    internal class CourseWorkMaterials
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } 
        public string AlternateLink { get; set; }
        public string State { get; set; }
        public string TopicId { get; set; }
        public List<Material> Materials { get; set; }
    }
}
