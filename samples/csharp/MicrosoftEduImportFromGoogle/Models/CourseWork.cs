using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftEduImportFromGoogle.Models
{
    internal class CourseWork
    {
        public string CourseId { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public List<Material> Materials { get; set; }
        public int MaxPoints { get; set; }
        public string TopicId { get; set; }
        public string GradeCategory { get; set; }


    }
}
