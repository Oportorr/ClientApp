using System;
using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models
{
    public class Record
    {
      
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime DateIn { get; set; }
            public DateTime DateOut { get; set; }
            public string Status { get; set; }
            public string Location { get; set; }


        // DTOs
        public class RecordDto
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public DateTime DateIn { get; set; }

            [Required]
            public DateTime DateOut { get; set; }

            [Required]
            public string Status { get; set; }

            [Required]
            public string Location { get; set; }
        }

        public class SearchCriteria
        {
            public string Name { get; set; }
            public string Status { get; set; }
            public DateTime? DateFrom { get; set; }
            public DateTime? DateTo { get; set; }
            public string Location { get; set; }
        }

    }
}
