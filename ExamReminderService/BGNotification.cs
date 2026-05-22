using System.Windows.Forms;

public class NotificationWorker : BackgroundService
{
    private readonly ExamRepository _repo;
    private NotifyIcon _notifyIcon;

    public NotificationWorker(ExamRepository repo)
    {
        _repo = repo;

        _notifyIcon = new NotifyIcon()
        {
            Icon = System.Drawing.SystemIcons.Information,
            Visible = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var exams = _repo.GetAll()
                .OrderBy(x => x.TargetDate ?? DateTime.MaxValue);

            foreach (var exam in exams)
            {
                if (exam.TargetDate == null) continue;

                var msg = GetDateDifference(exam.TargetDate.Value);

                ShowBalloon(exam.KeyName, msg);

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // spacing
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // interval
        }
    }

    private void ShowBalloon(string title, string message)
    {
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(5000);
    }

    public static string GetDateDifference(DateTime targetDate)
    {
        var diff = targetDate - DateTime.Now;
        if (diff.TotalDays < 0) return "Expired";

        int days = (int)diff.TotalDays;
        return $"{days} days to go";
    }
}