namespace BespokeSoftware.Models
{
    public class WeeklyOff
    {
        public int ID { get; set; }
        public string Day { get; set; }

        public List<WeeklyOff> WeeklyOffList { get; set; }
    }
}
