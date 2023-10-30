using AutoMapper;
using Moq;
using WorkforceManager.Services.MappingConfiguration;

namespace WorkforceManager.UnitTests.Mocks
{
    public class MapperMock
    {
        public static IMapper Instance
        {
            get
            {
                var mapperConfiguration = new MapperConfiguration(config =>
                {
                    config.AddProfile<UserMappingProfile>();
                    config.AddProfile<TeamMappingProfile>();
                    config.AddProfile<RequestMappingProfile>();
                });

                var mapper = new Mock<Mapper>(mapperConfiguration);
                return mapper.Object;
            }
        }
    }
}
