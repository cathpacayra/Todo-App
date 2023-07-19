using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace Todo_App.Application.TodoLists.Queries.GetTodos;

public record GetTodosQuery : IRequest<TodosVm>
{
    public string? Text { get; init; }
    public string? Priority { get; init; }
}

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

        return new TodosVm
        {
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                .Cast<PriorityLevel>()
                .Select(p => new PriorityLevelDto { Value = (int)p, Name = p.ToString() })
                .ToList(),

            Lists = await _context.TodoLists
            .AsNoTracking()
            .ProjectTo<TodoListDto>(_mapper.ConfigurationProvider)
            .Select(s => new TodoListDto
                {
                    Colour = s.Colour,
                    Id = s.Id,
                    Title = s.Title,
                    Items = s.Items.Where(x => 
                            (request.Text == null || request.Text.Equals("none") || x.Title.Contains(request.Text)) && 
                            (request.Priority == null || request.Priority.Equals("none") || x.Priority == Convert.ToInt32(request.Priority)))
                    .OrderByDescending(o => o.Priority).ToList()
                })
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken)
         };


    }
}
