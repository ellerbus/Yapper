using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yapper.Mappers;

namespace Yapper.Tests.DataObjects
{
    [Table("IDENTITY_OBJECT")]
    public class IdentityObject
    {
        [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdentityID { get; set; }

        [Column]
        public string Name { get; set; }
    }
}
