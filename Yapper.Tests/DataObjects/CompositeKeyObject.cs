using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yapper.Mappers;

namespace Yapper.Tests.DataObjects
{
    [Table("COMPOSITE_KEY_OBJECT")]
    public class CompositeKeyObject
    {
        [Key, Column("parent_id")]
        public string ParentID { get; set; }

        [Key, Column("this_id")]
        public string ThisID { get; set; }

        [Column]
        public string Name { get; set; }
    }
}
