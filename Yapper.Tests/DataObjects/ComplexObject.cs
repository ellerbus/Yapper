using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yapper.Mappers;

namespace Yapper.Tests.DataObjects
{
    [Table("COMPLEX_OBJECT")]
    public class ComplexObject
    {
            [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ID { get; set; }
            
            [Column]
            public int? NullableID { get; set; }
            
            [Column]
            public string Name { get; set; }
            
            [Column, BooleanValueMap("Y", "N")]
            public bool YesNo { get; set; }

            [Column, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public string Computed { get; set; }
            
            [Column]
            public string ReadOnly { get { return ""; } }
    }
}
