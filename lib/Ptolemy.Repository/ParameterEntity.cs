using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ptolemy.Repository {
    public class ParameterEntity {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required] public string Vtn { get; set; }
        [Required] public string Vtp { get; set; }
        [Required] public string NetListName { get; set; }
        [Required] public string DatabaseName { get; set; }
    }
}