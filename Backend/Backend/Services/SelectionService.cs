using AutoMapper;
using Backend.Models;

namespace Backend.Services
{
    public class SelectionService
    {
        private readonly YdbService _ydbService;
        private readonly IMapper _mapper;

        public SelectionService(YdbService ydbService, IMapper mapper)
        {
            _ydbService = ydbService;
            _mapper = mapper;
        }
        
        public async Task<SelectionDto> GetRecipes(string selectionId)
        {
            var selection = await _ydbService.GetDocumentAsync<Selection>("selections", selectionId);
            return _mapper.Map<SelectionDto>(selection);
        }
    }
}
