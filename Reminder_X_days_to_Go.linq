<Query Kind="Program">
  <Namespace>System.Windows</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <RuntimeVersion>6.0</RuntimeVersion>
</Query>

//[STAThread]
async Task Main()
{
	//Passed exams can be reneweed 6 months prior to expirary.
	var exams = new List<ExamData>() {	
	  	new ExamData("Google-GAIL",new DateTime(2026,05,29,17,00,0),"https://www.credly.com/org/google-cloud/badge/generative-ai-leader-certification"),
		new ExamData("Google-DevOps",new DateTime(2026,06,12,16,00,0)),	
		new ExamData("Google-PDevloper",new DateTime(2026,07,11,16,00,0)),	
		
		//new ExamData("SC-300",new DateTime(2026,04,24,07,0,0),null,false,true),
		//new ExamData("ISC2-CC",new DateTime(2026,03,14,14,00,0)),
		//https://www.linkedin.com/posts/chidinma-umejike-41846522a_cybersecurity-isc2-certifiedincybersecurity-activity-7332531484374761472-kwMf?utm_source=share&utm_medium=member_desktop&rcm=ACoAAAYBB3EBB83Puf_RHYbFAaX1YIiIvbv55eY
		
		
		new ExamData("DP-700",new DateTime(2026,07,18,07,0,0),null,false),
		//new ExamData("DP-100",null),
		new ExamData("AZ-500",new DateTime(2026,06,26,07,0,0),null,false),
		new ExamData("AZ-700",null),
		new ExamData("AZ-204",new DateTime(2027,06,01),"https://learn.microsoft.com/en-us/credentials/certifications/azure-developer/renew",true),
		new ExamData("AI-102",new DateTime(2027,07,14),"https://learn.microsoft.com/en-us/credentials/certifications/azure-ai-engineer/renew/", true),
		new ExamData("DP-600",new DateTime(2027,01,01),"https://learn.microsoft.com/en-us/credentials/certifications/fabric-analytics-engineer-associate/renew/", true),
		new ExamData("AZ-104",new DateTime(2027,04,26),"https://learn.microsoft.com/en-us/credentials/certifications/azure-administrator/renew/",true),
		new ExamData("AZ-305",new DateTime(2027,03,01,12,00,0),null,false),
		new ExamData("AZ-400",new DateTime(2027,02,15,14,00,0),null,false)
	};

	//Print in Sorted Order recent ones first and Null at last.
	exams.OrderBy(x=> x.TargetDate == null).ThenBy(x=> x.TargetDate).Dump();

	ParallelOptions parallelOptions =
		new() { MaxDegreeOfParallelism = 1};
	bool breakFlag = false;
	await Parallel.ForEachAsync(
	exams.OrderBy(x=> x.TargetDate == null).ThenBy(x=> x.IsReNew).ThenBy(x=> x.TargetDate).TakeWhile(_ => !Volatile.Read(ref breakFlag)),
	parallelOptions,
	async (x, token) =>
	{
		breakFlag = !(await Reminder_X_Days_To_Go(x, token));		
	})
	.WaitAsync(TimeSpan.FromMinutes(5));
}

// You can define other methods, fields, classes and namespaces here
private async static Task<bool> Reminder_X_Days_To_Go(ExamData examInfo, CancellationToken token)
{
	var result = await Reminder_X_Days_To_Go(examInfo.KeyName + " |=> " + examInfo.TargetDate?.Date.ToShortDateString(), examInfo.DaysRemaining, examInfo.ExamUrl, token);
	return result;
}


private async static Task<bool> Reminder_X_Days_To_Go(string title, string days_remaining, string Url, CancellationToken token)
{
	
	MessageBoxResult res = System.Windows.MessageBox.Show($"{days_remaining} for {title}", title, MessageBoxButton.YesNoCancel);
	if (res == MessageBoxResult.Yes)
	{
		Url = Url.Replace("&", "^&");
		await Task.Run(() => Process.Start(new ProcessStartInfo("cmd", $"/c start {Url}") { CreateNoWindow = true }))
		.WaitAsync(TimeSpan.FromMinutes(5), token);
		return true;
	}
	
	if (res == MessageBoxResult.No)
	{
		return true;
	}
	return false;
}

public static string GetDateDifference(DateTime? targetDate)
{
	if (targetDate == null)
	{
		return "TBD days to go";
	}
		
	DateTime currentDate = DateTime.Now;
	TimeSpan difference = targetDate.Value - currentDate;

	if (difference.TotalDays < 0)
	{
		return "The date has already passed. =>| {targetDate?.Date}";
	}

	int days = (int)difference.TotalDays;
	int weeks = (int)days/7;
	int months = days / 30;
	
	

	if (months > 0)
	{
		return months == 1 ? $"1 month ({weeks} weeks) to go " : $"{months} months ({weeks} weeks) to go ";		
	}
	else if (weeks > 0)
	{	
		weeks = (days % 30) / 7;
		return weeks == 1 ? $"1 week ({days} days) to go " : $"{weeks} weeks ({days} days) to go ";
	}
	else
	{
		int hours = (int)difference.Hours;
		days = days % 7;
		return days <= 1 ? $"{days} day ({hours} hours) to go " : $"{days} days to go ";
	}
}

public class ExamData //<strKey,strUrl,dtTarget>
{
	public ExamData(string key, DateTime? targetDate, string Url = null, bool isReNew = false, bool isTentative = false)
	{
		KeyName = key;
		ExamUrl = !string.IsNullOrEmpty(Url) ? Url : $"https://learn.microsoft.com/en-us/credentials/certifications/exams/{KeyName}/";
		IsReNew = isReNew;
		TargetDate = isReNew && targetDate.HasValue ? targetDate.Value.AddMonths(-6): targetDate;
		DaysRemaining = TargetDate.HasValue ? UserQuery.GetDateDifference(TargetDate.Value) : "TBD days to go";
		IsTentative = isTentative;
	}

	public string KeyName { get; set; }
	public string ExamUrl { get; set; }
	public DateTime? TargetDate { get; set; }
	public bool IsReNew { get; set; }
	public string DaysRemaining {get; private set;}
	public bool IsTentative { get; set; }
}
