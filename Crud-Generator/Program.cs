using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter the name of the entity (e.g., Product): ");
        string entityName = Console.ReadLine();

        var basePath = GetProjectBasePath(string.Empty);

        GenerateModel(entityName);
        var type = LoadEntityType(GetProjectBasePath($"{entityName}.cs"));
        GenerateDtoForEntity(type);

        GenerateController(entityName);
        GenerateRepository(entityName);

        GenerateAutoMapperProfile(type);
        GenerateDbContextConfiguration(entityName);

        Console.WriteLine($"{entityName} CRUD files created successfully.");
    }

    public void ReadModel(string entityNameInput)
    {

        string entityName = "YourEntity"; // Replace with your entity name
        string filePath = Path.Combine("API", "Entities", $"{entityName}.cs");

        if (File.Exists(filePath))
        {
            // Load the assembly containing the entity
            var assembly = Assembly.GetExecutingAssembly(); // Adjust if your entity is in another assembly

            // Get the type of the entity
            var entityType = assembly.GetTypes().FirstOrDefault(t => t.Name == entityName);

            if (entityType != null)
            {
                // Get the properties of the entity
                var properties = entityType.GetProperties();

                Console.WriteLine($"Properties of {entityName}:");
                foreach (var property in properties)
                {
                    Console.WriteLine($"{property.Name} ({GetPrimitiveTypeName(property.PropertyType)})");
                }
            }
            else
            {
                Console.WriteLine($"Entity {entityName} not found in the assembly.");
            }
        }
        else
        {
            Console.WriteLine($"Entity file not found at: {filePath}");
        }
    }

    static void GenerateModel(string entityName)
    {
        string modelContent = $@"
public class {entityName}
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }} // Add more properties as needed
}}";

        File.WriteAllText(GetProjectBasePath($"{entityName}.cs"), modelContent);
    }

    static void GenerateController(string entityName)
    {
        string controllerContent = $@"
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

[Route(""api/[controller]"")]
[ApiController]
public class {entityName}Controller : ControllerBase
{{
    private readonly I{entityName}Repository _repository;
    private readonly IMapper _mapper;

    public {entityName}Controller(I{entityName}Repository repository, IMapper mapper)
    {{
        _repository = repository;
        _mapper = mapper;
    }}

    [HttpGet]
    public IActionResult GetAll()
    {{
        var entities = _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<{entityName}>>(entities));
    }}

    [HttpGet(""{{id}}"")]
    public IActionResult Get(int id)
    {{
        var entity = _repository.Get(id);
        if (entity == null) return NotFound();
        return Ok(_mapper.Map<{entityName}>(entity));
    }}

    [HttpPost]
    public IActionResult Create({entityName} entity)
    {{
        _repository.Create(entity);
        return CreatedAtAction(nameof(Get), new {{ id = entity.Id }}, entity);
    }}

    [HttpPut(""{{id}}"")]
    public IActionResult Update(int id, {entityName} entity)
    {{
        if (id != entity.Id) return BadRequest();
        _repository.Update(entity);
        return NoContent();
    }}

    [HttpDelete(""{{id}}"")]
    public IActionResult Delete(int id)
    {{
        _repository.Delete(id);
        return NoContent();
    }}
}}";

        File.WriteAllText(GetProjectBasePath($"{entityName}Controller.cs"), controllerContent);
    }


    public static void GenerateDtoForEntity(Type? entityType)
    {
        // Create DTO class name by appending "Dto" to the entity name
        var dtoClassName = $"{entityType.Name}Dto";
        var dtoFilePath = Path.Combine(GetProjectBasePath(string.Empty), $"{dtoClassName}.cs");

        using (var writer = new StreamWriter(dtoFilePath))
        {
            writer.WriteLine("using System;");
            writer.WriteLine($"public class {dtoClassName}");
            writer.WriteLine("{");

            // Generate properties for the DTO
            foreach (var property in entityType.GetProperties())
            {
                writer.WriteLine($"    public {GetPrimitiveTypeName(property.PropertyType)} {property.Name} {{ get; set; }}");
            }

            writer.WriteLine("}");
        }

        Console.WriteLine($"DTO file generated: {dtoFilePath}");
    }

    public static void GenerateRepository(string entityName)
    {
        string repositoryContent = $@"
public interface I{entityName}Repository
{{
    IEnumerable<{entityName}> GetAll();
    {entityName} Get(int id);
    void Create({entityName} entity);
    void Update({entityName} entity);
    void Delete(int id);
}}

public class {entityName}Repository : I{entityName}Repository
{{
    // Implement the methods here
    public IEnumerable<{entityName}> GetAll() => throw new NotImplementedException();
    public {entityName} Get(int id) => throw new NotImplementedException();
    public void Create({entityName} entity) => throw new NotImplementedException();
    public void Update({entityName} entity) => throw new NotImplementedException();
    public void Delete(int id) => throw new NotImplementedException();
}}";

        File.WriteAllText(GetProjectBasePath($"{entityName}Repository.cs"), repositoryContent);
    }






    private static void GenerateAutoMapperProfile(Type? entityType)
    {
        var dtoType = GetDtoType(entityType); // Implement this method to get corresponding DTO type
        var profileName = $"{entityType.Name}Profile.cs";

        var _outputPath = GetProjectBasePath(string.Empty);
        var profilePath = Path.Combine(_outputPath, profileName);
        File.WriteAllText(profilePath, null);
        try
        {
            using (var writer = new StreamWriter(profilePath))
            {
                writer.WriteLine("using AutoMapper;");
                writer.WriteLine($"public class {entityType.Name}Profile : Profile");
                writer.WriteLine("{");
                writer.WriteLine("    public " + $"{entityType.Name}Profile()");
                writer.WriteLine("    {");

                // Generate mapping for each property
                foreach (var property in entityType.GetProperties())
                {
                    var dtoProperty = dtoType?.GetProperty(property.Name);
                    if (dtoProperty != null)
                    {
                        writer.WriteLine($"        CreateMap<{entityType.Name}, {dtoType.Name}>()");
                        writer.WriteLine($"            .ForMember(x => x.{dtoProperty.Name}, opt => opt.MapFrom(src => src.{property.Name}));");
                    }
                }

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }

        Console.WriteLine($"AutoMapper profile generated: {profilePath}");
    }



    public static Type GetDtoType(Type? entityType)
    {
        if (entityType == null)
        {
            throw new ArgumentNullException(nameof(entityType), "Entity type cannot be null.");
        }

        // Assuming the DTO is named <EntityName>Dto
        string dtoName = $"{entityType.Name}";









        // Get the assembly containing the entity type
        var assembly = Assembly.GetAssembly(entityType);
        if (assembly == null)
        {
            throw new InvalidOperationException("Could not retrieve assembly for the entity type.");
        }

        var ll = assembly.GetTypes().Select(t => t.Name).ToList();
        // Search for the DTO type in the assembly
        var dtoType = assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Equals(dtoName, StringComparison.OrdinalIgnoreCase));

        if (dtoType == null)
        {
            Console.WriteLine($"DTO type '{dtoName}' not found for entity '{entityType.Name}'.");
        }
        else
        {
            Console.WriteLine($"Found DTO type: {dtoType.FullName}");
        }

        return dtoType;
    }



    static void GenerateDbContextConfiguration(string entityName)
    {
        string dbContextContent = $@"
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{{
    public DbSet<{entityName}> {entityName}s {{ get; set; }}

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {{ }}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        modelBuilder.Entity<{entityName}>()
            .HasKey(e => e.Id); // Configure primary key if needed
        // Additional configurations can be added here
    }}
}}";

        File.WriteAllText(GetProjectBasePath($"ApplicationDbContext.cs"), dbContextContent);
    }

    static string GetProjectBasePath(string fileName)
    {
        string currentProjectDirectory = AppContext.BaseDirectory;

        // Navigate to the solution directory
        string solutionDirectory = currentProjectDirectory;

        while (Directory.GetParent(solutionDirectory) != null &&
               !File.Exists(Path.Combine(solutionDirectory, "MyIdentitySolution.sln"))) // Replace with your solution name
        {
            solutionDirectory = Directory.GetParent(solutionDirectory).FullName;
        }

        // Construct the path to the target project's Entities folder (ProjectB)
        string targetProjectEntitiesDirectory = Path.Combine(solutionDirectory, "MyApiProject", "Entities");

        CreatedirectoriesIfNotExist(targetProjectEntitiesDirectory);

        return Path.Combine(targetProjectEntitiesDirectory, fileName);


    }

    static void CreatedirectoriesIfNotExist(string path)
    {

        try
        {
            // Create the directory and all subdirectories if they do not exist
            Directory.CreateDirectory(path);
            Console.WriteLine($"Successfully created or verified: {path}");
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., permission issues)
            Console.WriteLine($"Error creating directories: {ex.Message}");
        }

    }

    public static Type LoadEntityType(string filePath)
    {

        try
        {
            // Read the source code from the specified file
            string code = File.ReadAllText(filePath);

            // Create a syntax tree from the code
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            // Set up the references needed for compilation
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            // Create a compilation object
            CSharpCompilation compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(filePath) + "Assembly",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // Emit the assembly to a stream
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    // Handle compilation errors
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.WriteLine(diagnostic.ToString());
                    }
                    throw new InvalidOperationException("Compilation failed.");
                }

                // Load the compiled assembly
                ms.Seek(0, SeekOrigin.Begin);
                var assembly = System.Reflection.Assembly.Load(ms.ToArray());

                // Retrieve the type of the entity (assuming it's the only public class in the file)
                var entityType = assembly.GetTypes().FirstOrDefault(t => t.IsClass && t.IsPublic);

                if (entityType == null)
                {
                    throw new InvalidOperationException("No public class found in the specified file.");
                }

                return (entityType as Type);
            }


        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied: {ex.Message}");
            throw; // Optionally rethrow or handle accordingly
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error: {ex.Message}");
            throw; // Optionally rethrow or handle accordingly
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            throw; // Optionally rethrow or handle accordingly
        }
    }

    private static string GetPrimitiveTypeName(Type type)
    {


        // Map non-primitive types to their primitive equivalents
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(short)) return "short";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(char)) return "char";
        if (type == typeof(string)) return "string";

        // If it's not a primitive type, return the original type name
        return type.Name;
    }


}
