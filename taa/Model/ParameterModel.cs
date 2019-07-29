using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace taa.Model {
    public class ParameterModel {
        [Key]
        public long Id { get; set; }
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
    }
}
