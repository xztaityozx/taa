using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace taa {
    public class Element {
        public string Name { get; set; }
        public Dictionary<double,double> Values { get; }

        /// <summary>
        /// TextElementからElementクラスを作る
        /// </summary>
        /// <param name="te"></param>
        public Element(TextElement te) {
            Values=new Dictionary<double, double>();
            var lines = te.Text.Split("\n");

            Name = lines[0].Trim('#');
            foreach (var s in lines.Skip(1).Where(item=>item.Length!=0)) {
                var kv = s.Trim(' ').Split(',');
                var key = double.Parse(kv[0]);
                var value = double.Parse(kv[1]);

                Values.Add(key,value);
            }
        }
    }

    public class TextElement {
        public string Text { get; }
        public TextElement(string s) => Text = s;
    }
}
