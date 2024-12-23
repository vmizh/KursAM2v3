using AutoMapper;
using Data;
using KursDomain.IDocuments.NomenklReturn;

namespace KursAM2
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            //Provide all the Mapping Configuration
            var config = new MapperConfiguration(cfg =>
            {
                //Configuring Employee and EmployeeDTO
                cfg.CreateMap<INomenklReturnOfClient, NomenklReturnOfClient>().ForMember(_ => _.Id, opt => opt.Ignore());
                cfg.CreateMap<INomenklReturnOfClientRow, NomenklReturnOfClient>().ForMember(_ => _.Id, opt => opt.Ignore());
                cfg.CreateMap<NomenklReturnOfClient, INomenklReturnOfClient>();
                cfg.CreateMap<NomenklReturnOfClient,INomenklReturnOfClientRow>();

            });
            //Create an Instance of Mapper and return that Instance
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}
