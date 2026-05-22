public class ExamData
{
    public required string KeyName { get; set; }
    public required string ExamUrl { get; set; }
    public DateTime? TargetDate { get; set; }
    public bool IsReNew { get; set; }
    public bool IsTentative { get; set; }
}