using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

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
        StatInfo GetStatInfo();
    }
}
