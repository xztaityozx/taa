using System;
using System.Collections.Generic;
using taa.Model;

namespace taa.Repository {

    public interface IRepository {
        void BulkWrite(IEnumerable<RecordModel> list);
        RecordModel[] Get(Func<RecordModel, bool> filter);
    }
    public class Repository {
    }
}