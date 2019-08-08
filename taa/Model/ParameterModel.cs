using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using taa.Parameter;

namespace taa.Model {
    public class ParameterModel {
        public virtual Transistor Vtn { get; set; }
        public virtual Transistor Vtp { get; set; }
        public string DbName { get; set; }

        public ParameterModel(Transistor vtn, Transistor vtp) {
            Vtn = vtn;
            Vtp = vtp;
            DbName = $"vtn:{vtn},vtp:{vtp}";
        }
        public ParameterModel() { }
    }
}
