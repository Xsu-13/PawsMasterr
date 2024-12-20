using AutoMapper;
using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Backend.Services
{
    public class SelectionService
    {
        private readonly IMongoCollection<Selection> _selections;
        private readonly IMapper _mapper;

        public SelectionService(
            IOptions<MongoDBSettings> settings,
            IMapper mapper)
        {
            MongoClient client = new MongoClient(settings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(settings.Value.DatabaseName);
            _selections = database.GetCollection<Selection>(settings.Value.SelectionCollection);
            _mapper = mapper;
        }
        public async Task<SelectionDto> GetRecipes(string selectionId)
        {
            return _mapper.Map<SelectionDto>( await _selections.Find(selection => selection.id.ToString() == selectionId).FirstOrDefaultAsync());
        }
    }
}
