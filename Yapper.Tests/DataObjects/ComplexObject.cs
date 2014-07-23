using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yapper.Mappers;

namespace Yapper.Tests.DataObjects
{
    public enum Status
    {
        [EnumMap("A")]
        Active,
        [EnumMap("I")]
        Inactive
    }

    public enum Order { First, Second }

    [Table("COMPLEX_OBJECT")]
    public class ComplexObject
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column]
        public int? NullableID { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public bool YesNo { get; set; }

        [Column, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string Computed { get; set; }

        [Column]
        public Status Status { get; set; }

        [Column]
        public Order Order { get; set; }

        [Column]
        public string ReadOnly { get { return ""; } }
    }
}
