namespace WorkforceManager.Models.Responses.RequestsResponseModels
{
    using System.Text;

    public class RejectedRequestResponseModel
    {
        public int RequestId { get; set; }
        public int RequesterId { get; set; }
        public string RequesterFirstName { get; set; }
        public string RequesterLastName { get; set; }
        public string RequestType { get; set; }
        public string RequestStatusState { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(new string('-', 40));
            sb.Append(new string(' ', 5));
            sb.Append($"Request '{this.RequestId}' update!");
            sb.Append(new string(' ', 5));
            sb.AppendLine(new string('-', 40));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"Employee Id: '{this.RequesterId}'.");
            sb.AppendLine($"First Name: '{this.RequesterFirstName}'.");
            sb.AppendLine($"Last Name: '{this.RequesterLastName}'.");
            sb.AppendLine($"Request type: '{this.RequestType}'.");
            sb.AppendLine($"Status: '{this.RequestStatusState}'.");

            return sb.ToString().Trim();
        }
    }

}

