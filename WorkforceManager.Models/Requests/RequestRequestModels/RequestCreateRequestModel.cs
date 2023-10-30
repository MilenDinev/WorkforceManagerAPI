using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkforceManager.Models.Requests.RequestRequestModels
{
    public class RequestCreateRequestModel : IValidatableObject
    {
        [Required(ErrorMessage = "Start date is required!")]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "End date is required!")]
        public string EndDate { get; set; }
        
        [Required(ErrorMessage = "You are required to add a description for your request!")]
        [MinLength(16, ErrorMessage = "The description must be atleas 16 symbols long!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Type of request is required!")]
        [RegularExpression("(^(?i)paid$|^unpaid$|^sick$)",
            ErrorMessage ="Type must be \"Paid\", \"Unpaid\" or \"Sick\"")]
        public string Type { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();
            var isStartDateTime = DateTime.TryParse(StartDate, out DateTime startDate);
            var isEndDateTime = DateTime.TryParse(EndDate, out DateTime endDate);

            if (!isStartDateTime)
            {
                result.Add(new ValidationResult("StartDate must be a valid date in following format: 'MM/dd/yyyy'!", new string[] { "Date" }));
            }
            if (!isEndDateTime)
            {
                result.Add(new ValidationResult("EndDate must be a valid date in following format: 'MM/dd/yyyy'!", new string[] { "Date" }));
            }

            if (result.Count > 0) 
            {
                return result; // To not check anymore if one of the dates is not valid
            }

            if (startDate <= DateTime.Now) // to be discussed
            {
                result.Add(new ValidationResult("Start date should not be in the past! Please type valid date in following format: 'MM/dd/yyyy'.", new string[] { "Date" }));
            }

            if (startDate >= endDate)
            {
                result.Add(new ValidationResult("Start date cannot be after or equal to End date! Please type valid date in following format: 'MM/dd/yyyy'.", new string[] { "Date" }));
            }

            return result;
        }

    }
}
