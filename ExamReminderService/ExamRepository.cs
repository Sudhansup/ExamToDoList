using System.Text.Json;

public class ExamRepository
{
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "exams.json");
    private readonly object _lock = new();

    public List<ExamData> GetAll()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath)) return new();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<ExamData>>(json) ?? new();
        }
    }

    public ExamData? Get(string key) =>
        GetAll().FirstOrDefault(x => x.KeyName == key);

    public void Add(ExamData exam)
    {
        var list = GetAll();
        list.Add(exam);
        Save(list);
    }

    public bool Update(string key, ExamData updated)
    {
        var list = GetAll();
        var index = list.FindIndex(x => x.KeyName == key);
        if (index < 0) return false;

        list[index] = updated;
        Save(list);
        return true;
    }

    public bool Delete(string key)
    {
        var list = GetAll();
        var removed = list.RemoveAll(x => x.KeyName == key) > 0;
        if (removed) Save(list);
        return removed;
    }

    private void Save(List<ExamData> exams)
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(exams, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}