
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

[Route("api/[controller]")]
[ApiController]
public class rtController : ControllerBase
{
    private readonly IrtRepository _repository;
    private readonly IMapper _mapper;

    public rtController(IrtRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var entities = _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<rt>>(entities));
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var entity = _repository.Get(id);
        if (entity == null) return NotFound();
        return Ok(_mapper.Map<rt>(entity));
    }

    [HttpPost]
    public IActionResult Create(rt entity)
    {
        _repository.Create(entity);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, rt entity)
    {
        if (id != entity.Id) return BadRequest();
        _repository.Update(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _repository.Delete(id);
        return NoContent();
    }
}