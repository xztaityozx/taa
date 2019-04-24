using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace taa {
    public class WvParser {
        public string Path { get; }

        public WvParser(string p) {
            Path = p;
        }

        public WvCSV Parse() => new WvCSV(GetDocument());
        

        private Document GetDocument() {

            using (var sr = new StreamReader(this.Path)) {
                var str = "";
                while (sr.Peek() > 1) str += sr.ReadLine();

                var blocks = str.Split('#');

                return new Document(blocks);
            }

        }
    }
    public class WvCSV {
        public IReadOnlyList<Element> Data { get; }
        public Header Header { get; }

        public int Length => Data.Count;

        public WvCSV(Document d) {
            Header = d.Header;
            Data = d.GetElements().ToList();
        }
    }

    public struct Header {
        public readonly string Format, SavedTime, Signal;
        public Header(string text) {
            var lines = text.Split('\n');
            Format = lines[0].Replace("#format ", "");
            SavedTime=lines[1].Replace("#[Custom WaveView] saved ", "");
            Signal = lines[2].Replace("TIME ,", "");
        }
    }

    public class Document {
        public Header Header { get; }
        public List<TextElement> TextElements { get; }

        public Document(IReadOnlyList<string> blocks) {
            this.Header=new Header(blocks[1]+blocks[2]);
            this.TextElements=new List<TextElement>();
            foreach (var item in blocks.Skip(3)) {
                TextElements.Add(new TextElement(item));
            }
        }

        public IEnumerable<Element> GetElements() => TextElements.Select(item => new Element(item));
    }
}
