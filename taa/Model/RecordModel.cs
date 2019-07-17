using System;

namespace taa.Model {
    public class RecordModel {
        public string Parameter { get; set; }
        public long Sweep { get; set; }
        public decimal Value { get; set; }
    }

    public static class RecordFactory {
        public static RecordModel[] Build(Document.Document doc) {
            throw new NotImplementedException();
        }
    }
}
