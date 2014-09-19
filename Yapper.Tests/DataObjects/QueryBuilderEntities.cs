using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yapper.Tests.DataObjects
{
    [Table("SIMPLE_OBJECTS")]
    public class SimpleObject
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }

    [Table("COMPLEX_OBJECT")]
    public class ComplexObject
    {
        [Key, Column("parent_id")]
        public string ParentID { get; set; }

        [Key, Column("this_id")]
        public string ThisID { get; set; }

        [Column]
        public string Name { get; set; }

        [Column, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string Computed { get; set; }

        public IEnumerable<string> Strings { get; set; }
    }
}