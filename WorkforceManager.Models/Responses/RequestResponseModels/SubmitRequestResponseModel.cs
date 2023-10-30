namespace WorkforceManager.Models.Responses.RequestsResponseModels
{
    using System;
    using System.Text;

    public class SubmitRequestResponseModel
    {
        public int RequestId { get; set; }
        public int RequesterId { get; set; }
        public int CreatorId { get; set; }
        public string RequesterFirstName { get; set; }
        public string RequesterLastName { get; set; }
        public string RequestType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string RequestStatusState { get; set; }
        public int TotalDaysRequested { get; set; }
        public int WorkingDays { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(new string('-', 40));
            sb.Append(new string(' ', 5));
            sb.Append($"Request with id '{this.RequestId}' has been submitted!");
            sb.Append(new string(' ', 5));
            sb.AppendLine(new string('-', 40));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"Employee Id: '{this.RequesterId}'.");
            sb.AppendLine($"Submitted by (Employye Id): '{this.CreatorId}'.");
            sb.AppendLine($"First Name: '{this.RequesterFirstName}'.");
            sb.AppendLine($"Last Name: '{this.RequesterLastName}'.");
            sb.AppendLine($"Request Type: '{this.RequestType}'.");
            sb.AppendLine($"Start Date: '{this.StartDate:D}'.");
            sb.AppendLine($"End Date: '{this.EndDate:D}'.");
            sb.AppendLine($"Status: '{this.RequestStatusState}'.");
            sb.AppendLine($"Total days requested: '{ this.TotalDaysRequested}'.");
            sb.AppendLine($"Working days: '{this.WorkingDays}'.");

            return sb.ToString().Trim();
        }
    }

}
