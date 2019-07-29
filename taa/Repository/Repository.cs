using System;
using System.Collections.Generic;
using taa.Model;

namespace taa.Repository {

    public interface IRepository {
        void BulkWrite(Parameter.Parameter parameter, IList<RecordModel> list);
        RecordModel[] Get(Parameter.Parameter parameter, Func<RecordModel, bool> filter);
    }
    public class Repository {
    }
}