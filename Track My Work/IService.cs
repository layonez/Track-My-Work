using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Track_My_Work
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebGet]
        Service.Response GetDayInfo(int day);

        [OperationContract]
        [WebGet]
        IEnumerable<DayInfo> GetDatasource();

        [OperationContract]
        [WebGet]
        StatInfoDto GetStatInfo();
    }
}
