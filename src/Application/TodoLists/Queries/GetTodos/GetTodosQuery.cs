using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.TodoLists.Queries.GetTodos;

public record GetTodosQuery : IRequest<TodosVm>;

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, TodosVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTodosQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        List<ColorDto> value_2 = new()
        {
            new ColorDto { colour = "White", colorCode = "#FFFFFF" },
            new ColorDto { colour = "Red", colorCode = "#FF5733" },
            new ColorDto { colour = "Orange", colorCode = "#FFC300" },
            new ColorDto { colour = "Yellow", colorCode = "#FFFF66" },
            new ColorDto { colour = "Green", colorCode = "#CCFF99" },
            new ColorDto { colour = "Blue", colorCode = "#6666FF" },
            new ColorDto { colour = "Purple", colorCode = "#9966CC" },
            new ColorDto { colour = "Grey", colorCode = "#999999" },
        };

        return new TodosVm
        {
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
            .Cast<PriorityLevel>()
            .Select(p => new PriorityLevelDto { Value = (int)p, Name = p.ToString() })
            .ToList(),
            Colours = value_2,

            Lists = await _context.TodoLists
            .AsNoTracking()
            .ProjectTo<TodoListDto>(_mapper.ConfigurationProvider)
            .Where(t => t.IsDeletedList == false)
            .Select(s => new TodoListDto
            {
                Colour = s.Colour,
                Id = s.Id,
                IsDeletedList = s.IsDeletedList,
                Title = s.Title,
                Items = s.Items.Where(c => c.IsDeleted == false).ToList()
            })
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken)
        };
    }
}
