
public interface IrtRepository
{
    IEnumerable<rt> GetAll();
    rt Get(int id);
    void Create(rt entity);
    void Update(rt entity);
    void Delete(int id);
}

public class rtRepository : IrtRepository
{
    // Implement the methods here
    public IEnumerable<rt> GetAll() => throw new NotImplementedException();
    public rt Get(int id) => throw new NotImplementedException();
    public void Create(rt entity) => throw new NotImplementedException();
    public void Update(rt entity) => throw new NotImplementedException();
    public void Delete(int id) => throw new NotImplementedException();
}